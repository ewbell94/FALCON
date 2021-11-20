using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Accord.Math.Decompositions;
using Random=UnityEngine.Random;

public class NetworkBuilder : MonoBehaviour
{
    public GameObject nodePrefab; //Prefab for the node object
    public GameObject edgePrefab; //Prefab for the edge object
    public GameObject labelPrefab; //Prefab for node labels
    
    private struct Network{
        public string[] nodeNames;
        public bool[,] edgeGraph;
        public Vector3[] nodePositions;
    }
    private List<Network> states=new List<Network>();
    public void BuildNetwork(string filename){
        (string[] nodeNames, bool[,] edgeGraph) = ReadSIF(filename);
        float t0=Time.realtimeSinceStartup;
        Vector3[] nodePositions= GDCoords(edgeGraph);
        float t1=Time.realtimeSinceStartup;
        Debug.Log(t1-t0);
        Network thisState=new Network(){nodeNames=nodeNames,edgeGraph=edgeGraph,nodePositions=nodePositions};
        states.Add(thisState);
        DrawNetwork(nodePositions,edgeGraph,nodeNames);
        MovementController xrMove= GameObject.Find("XR Rig").GetComponent<MovementController>();
        xrMove.movementActive=true;
    }

    //Given a SIF file, extract the node connectivity array and node names
    private (string[],bool[,]) ReadSIF(string fname){
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

        bool[,] edgeGraph = new bool[nodeCount,nodeCount];
        for(i=0;i<nodeCount;i++){
            foreach(string partner in pairings[nodeNames[i]]){
                int j=Array.IndexOf(nodeNames,partner);
                edgeGraph[i,j]=true;
            }
        }
        return (nodeNames,edgeGraph);
    }
    
    private Vector3[] GDCoords(bool[,] edgeGraph){
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
                    if (edgeGraph[n,i]){
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
                    if (edgeGraph[n,i]){
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
    //Given the adjacency matrix, calculate the laplacian and determine coordinates based on its principal components
    /*
    private Vector3[] PCACoords(bool[,] edgeGraph){
        int nodeCount=edgeGraph.GetLength(0);
        Vector3[] coords = new Vector3[nodeCount];
        float[,] edgeLap = new float[nodeCount,nodeCount];
        for (int i=0;i<nodeCount;i++){
            float degree=0.0f;
            for (int j=0;j<nodeCount;j++){
                if (edgeGraph[i,j]){
                    degree++;
                    edgeLap[i,j]=-1.0f;
                }
            }
            edgeLap[i,i]=degree;
        }
        SingularValueDecompositionF result= new SingularValueDecompositionF(edgeLap);
        float[,] lsv=result.LeftSingularVectors;
        float[] svals=result.Diagonal;
        //Debug.Log(result.Ordering);
        //Debug.Log(result.IsSingular);
        
        for (int i=0;i<nodeCount;i++){
            float xval=svals[0]*lsv[i,0];
            float yval=svals[1]*lsv[i,1];
            float zval=svals[2]*lsv[i,2];
            Vector3 point = new Vector3(svals[0]*lsv[i,0],svals[1]*lsv[i,1],svals[2]*lsv[i,2]);
            coords[i]=point;
        }

        coords = RegularizeCoords(coords);
        return coords;
    }
    */
    //Given a set of coordinates, scale them to ensure node separation, raise them off the ground, and center them
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

    //Given a set of node positions and node connectivity, instantiate all objects of the network
    private void DrawNetwork(Vector3[] nodePositions, bool[,] edgeGraph, string[] nodeNames){
        GameObject[] newNodes= new GameObject[nodePositions.Length];
        Transform canvas=GameObject.Find("NodeLabelCanvas").transform;
        Transform edgeParent = GameObject.Find("Edges").transform;
        Transform nodeParent = GameObject.Find("Nodes").transform;
        for (int i=0; i<nodePositions.Length; i++){
            newNodes[i]=(GameObject) Instantiate(nodePrefab,nodePositions[i],Quaternion.identity);
            newNodes[i].transform.SetParent(nodeParent,true);
            newNodes[i].name=nodeNames[i];
            GameObject label = (GameObject) Instantiate(labelPrefab,Vector3.zero,Quaternion.identity);
            label.name=nodeNames[i]+"_label";
            Text t = label.GetComponent<Text>();
            t.text=nodeNames[i];
            TextMover tm = label.GetComponent<TextMover>();
            tm.Target = newNodes[i].transform;
            label.transform.SetParent(canvas,false);
            
            for (int j=0; j<i; j++){
                if (edgeGraph[i,j]){
                    GameObject newEdge = (GameObject) Instantiate(edgePrefab,Vector3.zero,Quaternion.identity);
                    newEdge.transform.SetParent(edgeParent,true);
                    EdgeConnector newEdgeConnector = newEdge.GetComponent<EdgeConnector>();
                    newEdgeConnector.NodeA = newNodes[i];
                    newEdgeConnector.NodeB = newNodes[j];
                }
            }
        }
    }
    
}
