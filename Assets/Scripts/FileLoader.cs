using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using SimpleFileBrowser;

public class FileLoader : MonoBehaviour
{
    public GameObject fileBrowserPrefab;
    /*
    void Start(){
        LoadExample();

        string[] groupNames={"Human","Covid"};
        string[][] nodeMembers=new string[groupNames.Length][];
        List<string> humanProts = new List<string>();
        List<string> covidProts = new List<string>();
        GameObject firstGroup=GameObject.Find("Nodes");
        foreach(Transform child in firstGroup.transform){
            string nodeName = child.gameObject.name;
            int nodeNumber = Int32.Parse(nodeName.Substring(4));
            if (nodeNumber > 20600){
                covidProts.Add(nodeName);
            } else {
                humanProts.Add(nodeName);
            }
        }
        nodeMembers[0] = new string[humanProts.Count];
        for (int i=0;i<humanProts.Count;i++){
            nodeMembers[0][i] = humanProts[i];
        }
        nodeMembers[1] = new string[covidProts.Count];
        for (int i=0;i<covidProts.Count;i++){
            nodeMembers[1][i] = covidProts[i];
        }

        GameObject optionsMenu = GameObject.Find("OptionsMenu");
        GlobalOptions go = optionsMenu.GetComponent<GlobalOptions>();
        go.NewNodeOrder(groupNames,nodeMembers);
        go.RenameNodes("names.csv");
        GameObject nodeParent=GameObject.Find("Human");
        NodeConfig nc = nodeParent.GetComponent<NodeConfig>();
        nc.SetColor(Color.blue);
        nc.Labels(false);
        GameObject edgeParent = GameObject.Find("Edges");
        EdgeConfig ec = edgeParent.GetComponent<EdgeConfig>();
        ec.SetColor(Color.green);
    }
    */
    public void OpenSIF(){
        NetworkBuilder n = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
        string path = EditorUtility.OpenFilePanel("Choose interaction file...","","sif");
        n.BuildNetwork(path);
        /*
        GameObject fBrowser = (GameObject) Instantiate(fileBrowserPrefab,transform.position,Quaternion.identity);
        Canvas c = fBrowser.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;
        FileBrowser.ShowLoadDialog((paths) => {n.BuildNetwork(paths[0]);},() => {Debug.Log("Canceled");},FileBrowser.PickMode.Files,false,"Choose interaction file...","Select");
        */
        Destroy(gameObject);
    }

    public void LoadExample(){
        string path = "covid.sif";
        NetworkBuilder n = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
        n.BuildNetwork(path);
        Destroy(gameObject);
    }

    public void QuitProgram(){
        Application.Quit();
    }
}
