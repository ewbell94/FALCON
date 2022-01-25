using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random=UnityEngine.Random;

public class NetworkBuilder : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject edgePrefab;
    public GameObject labelPrefab; 
    public GameObject nodeGroupPrefab; 
    
    //Data structure for storing network states
    private struct Network{
        public string[] nodeNames;
        public string[] nodeGroups;
        public float[,] edgeGraph;
        public Vector3[] nodePositions;
    }

    private int stateIndex = -1; //Index of the current network state in states
    private List<Network> states=new List<Network>(); //List of all the loaded network states

    //Given a SIF file, this function builds the network state and changes the viewer to that new state
    public void BuildNetwork(string filename){
        (string[] nodeNames, float[,] edgeGraph) = ReadSIF(filename);
        float t0=Time.realtimeSinceStartup;
        Vector3[] nodePositions= GDCoords(edgeGraph);
        float t1=Time.realtimeSinceStartup;
        Debug.Log(t1-t0);
        string[] nodeGroups = AssessNodeGrouping(nodeNames);
        Network thisState=new Network(){nodeNames=nodeNames,nodeGroups=nodeGroups,edgeGraph=edgeGraph,nodePositions=nodePositions};
        states.Add(thisState);
        ChangeState(true);
        MovementController xrMove= GameObject.Find("XR Rig").GetComponent<MovementController>();
        xrMove.movementActive=true;
    }

    //Given a SIF file, extract the node connectivity array and node names
    private (string[],float[,]) ReadSIF(string fname){
        Dictionary<string,List<string>> pairings = new Dictionary<string,List<string>>();
        string[] lines=File.ReadAllLines(fname);
        foreach (string line in lines){
            string[] parts=line.Split(' ');
            if (!pairings.ContainsKey(parts[0])){
                pairings[parts[0]] = new List<string>();
            }
            for (int prot=2; prot<parts.Length;prot++){
                pairings[parts[0]].Add(parts[prot]);

                if (!pairings.ContainsKey(parts[prot])){
                    pairings[parts[prot]] = new List<string>();
                }
                pairings[parts[prot]].Add(parts[0]);
            }
        }
        
        int nodeCount = pairings.Keys.Count;
        string[] nodeNames = new string[nodeCount];
        int i=0;
        foreach(string name in pairings.Keys){
            nodeNames[i]=name;
            i++;
        }

        float[,] edgeGraph = new float[nodeCount,nodeCount];
        for(i=0;i<nodeCount;i++){
            foreach(string partner in pairings[nodeNames[i]]){
                int j=Array.IndexOf(nodeNames,partner);
                edgeGraph[i,j]=0.1f;
            }
        }
        return (nodeNames,edgeGraph);
    }
    
    //GDCoords uses adam optimization to calculate coordinates for all nodes given a network connectivity
    private Vector3[] GDCoords(float[,] edgeGraph){
        int nodeCount=edgeGraph.GetLength(0);
        float d=2.0f; //Ideal internode distance
        float mu=1.0f; //Base learning rate
        float beta=0.9f; //Smoothing constant for gradient
        float gamma=0.9f; //Smoothing constant for cache
        float epsilon=0.00000001f; //Keeps denominator nonzero
        float k=1.0f; //Spring constant/weight for spring energy term
        float tol=0.1f; //Tolerance for stopping GD
        Vector3[] coords=new Vector3[nodeCount];
        Vector3[] m = new Vector3[nodeCount];
        Vector3[] c = new Vector3[nodeCount];
        for (int i=0;i<nodeCount;i++){
            coords[i]=new Vector3(Random.Range(0,nodeCount),Random.Range(0,nodeCount),Random.Range(0,nodeCount));
        }
        Debug.Log(nodeCount);
        Vector3 center=CenterOfMass(coords);
        float totalE=float.PositiveInfinity;
        float deltaE=float.PositiveInfinity;
        while (deltaE > tol){
            Vector3[] newCoords=new Vector3[nodeCount];
            Parallel.For(0,nodeCount,n=>{
                Vector3 pointn=coords[n];
                Vector3 grad=2*(pointn-center)/nodeCount;
                for (int i=0;i<nodeCount;i++){
                    if (i==n){
                        continue;
                    }
                    Vector3 pointi=coords[i];
                    float vecdist=Vector3.Distance(pointn,pointi);
                    grad-=2*(pointn-pointi)/Mathf.Pow(vecdist,4.0f);
                    if (edgeGraph[n,i] > 0.0f){
                        float dterm=1.0f-d/vecdist;
                        grad+=2*k*(pointn-pointi)*dterm;
                    }
                }
                m[n]=beta*m[n]+(1.0f-beta)*grad;
                c[n]=gamma*c[n]+(1.0f-gamma)*Vector3.Scale(grad,grad);
                Vector3 newPoint = new Vector3();
                newPoint.x=pointn.x-mu*m[n].x/(Mathf.Pow(c[n].x,0.5f)+epsilon);
                newPoint.y=pointn.y-mu*m[n].y/(Mathf.Pow(c[n].y,0.5f)+epsilon);
                newPoint.z=pointn.z-mu*m[n].z/(Mathf.Pow(c[n].z,0.5f)+epsilon);
                newCoords[n]=newPoint;
            });
            coords=newCoords;
            center=CenterOfMass(coords);
            float[] nodeE=new float[nodeCount];
            Parallel.For(0,nodeCount,n=>{
                for (int i=0;i<nodeCount;i++){
                    if (i==n){
                        continue;
                    }
                    float vecdist=Vector3.Distance(coords[n],coords[i]);
                    nodeE[n]+=Mathf.Pow(Vector3.Distance(coords[n],center),2.0f)/nodeCount;
                    nodeE[n]+=1/Mathf.Pow(vecdist,2.0f);
                    if (edgeGraph[n,i] > 0.0f){
                        nodeE[n]+=k*Mathf.Pow(vecdist-d,2.0f);
                    }
                }
            });
            float newE=0.0f;
            foreach (float E in nodeE){
                newE+=E;
            }
            deltaE=totalE-newE;
            totalE=newE;
            Debug.Log(new Vector2(newE,deltaE));
        }
        coords=RegularizeCoords(coords);
        return coords;
    }

    private Vector3 CenterOfMass(Vector3[] coords){
        Vector3 com = Vector3.zero;
        for (int i=0;i<coords.Length;i++){
            com+=coords[i];
        }
        return com/coords.Length;
    }
    
    //Given a set of coordinates, scale them to ensure node separation and center them
    private Vector3[] RegularizeCoords(Vector3[] coords){
        float minDist=1.0f;

        int nodeCount=coords.Length;
        float mind = float.PositiveInfinity;
        Vector3 minvec=new Vector3(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity);
        Vector3 maxvec=new Vector3(float.NegativeInfinity,float.NegativeInfinity,float.NegativeInfinity);

        for (int i=0;i<nodeCount;i++){
            if (coords[i].x < minvec.x){
                minvec.x=coords[i].x;
            }
            if (coords[i].x > maxvec.x){
                maxvec.x=coords[i].x;
            }
            if (coords[i].y < minvec.y){
                minvec.y=coords[i].y;
            }
            if (coords[i].y > maxvec.y){
                maxvec.y=coords[i].y;
            }
            if (coords[i].z < minvec.z){
                minvec.z=coords[i].z;
            }
            if (coords[i].z > maxvec.z){
                maxvec.z=coords[i].z;
            }

            for (int j=i+1;j<nodeCount;j++){
                float pointd=Vector3.Distance(coords[i],coords[j]);
                if (pointd<mind && pointd>1){
                    mind=pointd;
                }
            }
        }
        float scale=minDist/mind;

        Vector3[] newCoords=new Vector3[nodeCount];
        for (int i=0;i<nodeCount;i++){
            newCoords[i]=coords[i];
            newCoords[i]-=maxvec-(maxvec-minvec)/2;
            newCoords[i]*=scale;
        }

        return newCoords;
    }

    //Based on the network states in the state list, move forward one state or backward one state
    public void ChangeState(bool forward){
        if (forward){
            if (stateIndex+1 >= states.Count){
                return;
            } else {
                stateIndex++;
            }
        } else {
            if (stateIndex-1 < 0){
                return;
            } else {
                stateIndex--;
            }
        }

        Network newState = states[stateIndex];
        string[] nodeNames = newState.nodeNames;
        float[,] edgeGraph = newState.edgeGraph;
        Vector3[] nodePositions = newState.nodePositions;
        
        GameObject[] newNodes= new GameObject[nodePositions.Length];
        Transform canvas=GameObject.Find("NodeLabelCanvas").transform;
        Transform edgeParent = GameObject.Find("Edges").transform;
        Color oldColor;
        if (edgeParent.childCount > 0){
            oldColor = edgeParent.GetChild(0).gameObject.GetComponent<LineRenderer>().startColor;
        } else {
            oldColor = edgePrefab.GetComponent<LineRenderer>().startColor;
        }
        
        foreach (Transform child in edgeParent) {
            GameObject.Destroy(child.gameObject);
        }
        GameObject nodeParent = GameObject.Find("Nodes");
        for (int i=0; i<nodePositions.Length; i++){
            newNodes[i]=GameObject.Find(nodeNames[i]);
            if (newNodes[i] == null){ //If the node is non-existent, create it
                newNodes[i]=(GameObject) Instantiate(nodePrefab,nodePositions[i],Quaternion.identity);
                if (nodeParent == null){
                    nodeParent = (GameObject) Instantiate(nodeGroupPrefab,Vector3.zero,Quaternion.identity);
                    nodeParent.name = "Nodes";
                }
                newNodes[i].transform.SetParent(nodeParent.transform,true);
                newNodes[i].name=nodeNames[i];
                GameObject label = (GameObject) Instantiate(labelPrefab,Vector3.zero,Quaternion.identity);
                label.name=nodeNames[i]+"_label";
                Text t = label.GetComponent<Text>();
                t.text=nodeNames[i];
                TextMover tm = label.GetComponent<TextMover>();
                tm.Target = newNodes[i].transform;
                label.transform.SetParent(canvas,false);
            } else {
                int oldStateIndex;
                if (forward){
                    oldStateIndex = stateIndex - 1;
                } else {
                    oldStateIndex = stateIndex + 1;
                }
                int nodeIndex = Array.IndexOf(states[oldStateIndex].nodeNames,nodeNames[i]);
                states[oldStateIndex].nodePositions[nodeIndex] = newNodes[i].transform.position;

                newNodes[i].transform.position = nodePositions[i];
            }

            for (int j=0; j<i; j++){
                if (edgeGraph[i,j] > 0.0f){
                    GameObject newEdge = (GameObject) Instantiate(edgePrefab,Vector3.zero,Quaternion.identity);
                    newEdge.transform.SetParent(edgeParent,true);
                    EdgeConnector newEdgeConnector = newEdge.GetComponent<EdgeConnector>();
                    newEdgeConnector.NodeA = newNodes[i];
                    newEdgeConnector.NodeB = newNodes[j];
                    LineRenderer lr = newEdge.GetComponent<LineRenderer>();
                    lr.startColor = oldColor;
                    lr.endColor = oldColor;
                    lr.startWidth = edgeGraph[i,j];
                    lr.endWidth = edgeGraph[i,j];
                }
            }
        }
    }
    
    //If the node names/groupings are changed in NodeOptions, this is called to change them in the state list
    public void SetNodeNames(string[] oldNames, string[] newNames, string[] newGroups){
        for (int i = 0; i < oldNames.Length; i++){
            foreach (Network state in states){
                int nodeIndex = Array.IndexOf(state.nodeNames,oldNames[i]);
                state.nodeNames[nodeIndex] = newNames[i];
                state.nodeGroups[nodeIndex] = newGroups[i];
            }
        }
    }

    //If the edge weights are changed in EdgeOptions, this is called to change them in the state list
    public void SetEdgeWeights(string[] nodesA, string[] nodesB, float[] weights){
        return;
    }

    //Returns a node group array for BuildNetwork to create its network state
    private string[] AssessNodeGrouping(string[] nodeNames){
        string[] nodeGroups = new string[nodeNames.Length];
        if (stateIndex >= 0){
            for (int i = 0; i<nodeGroups.Length; i++){
                int nodeIndex = Array.IndexOf(states[stateIndex].nodeNames,nodeNames[i]);
                if (nodeIndex >= 0){
                    nodeGroups[i] = states[stateIndex].nodeGroups[nodeIndex];
                } else {
                    nodeGroups[i] = "Nodes";
                }
            }
        } else {
            for (int i = 0; i<nodeGroups.Length; i++){
                nodeGroups[i]="Nodes";
            }
        }

        return nodeGroups;
    }
}
