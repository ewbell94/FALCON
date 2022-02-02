using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class FileLoader : MonoBehaviour
{
   
    public void OpenSIF(){
        NetworkBuilder n = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
        //string path = EditorUtility.OpenFilePanel("Choose interaction file...","","sif");
        //n.BuildNetwork(path);
        FileBrowserSpawner fbs = GameObject.Find("FileBrowserSpawner").GetComponent<FileBrowserSpawner>();
        fbs.SpawnLoader((paths)=>{n.BuildNetwork(paths[0]); Destroy(gameObject);},
                        ()=>{gameObject.SetActive(true);},
                        "Choose interaction file...",".sif",transform.position,transform.rotation);
        gameObject.SetActive(false);
    }

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
