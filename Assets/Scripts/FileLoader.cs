using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

//Attached to the startup window
public class FileLoader : MonoBehaviour
{
    //Opens a network in SIF format and builds it in the scene
    public void OpenSIF(){
        NetworkBuilder n = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
        FileBrowserSpawner fbs = GameObject.Find("FileBrowserSpawner").GetComponent<FileBrowserSpawner>();
        fbs.SpawnLoader((paths)=>{n.BuildNetwork(paths[0]); Destroy(gameObject);},
                        ()=>{gameObject.SetActive(true);},
                        "Choose interaction file...",".sif",transform.position,transform.rotation);
        gameObject.SetActive(false);
    }

    //Loads the example network from Example.sif
    public void LoadExample(){
        string path = Application.streamingAssetsPath + "/Example.sif";
        NetworkBuilder n = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
        n.BuildNetwork(path);
        Destroy(gameObject);
    }

    public void QuitProgram(){
        Application.Quit();
    }
}
