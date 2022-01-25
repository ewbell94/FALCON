using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GlobalOptions : MonoBehaviour
{
    public GameObject nodeMenuPrefab;
    public GameObject edgeMenuPrefab;
    public void SpawnNodeMenu(){
        GameObject nodeMenu = (GameObject) Instantiate(nodeMenuPrefab,transform.position,transform.rotation);
        nodeMenu.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void SpawnEdgeMenu(){
        GameObject edgeMenu = (GameObject) Instantiate(edgeMenuPrefab,transform.position,transform.rotation);
        edgeMenu.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void ReadNewNetwork(){
        string path = EditorUtility.OpenFilePanel("Choose interaction file...","","sif");
        NetworkBuilder n = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
        n.BuildNetwork(path);
        Destroy(gameObject);
    }

    public void QuitProgram(){
        Application.Quit();
    }

    public void ReinitializeProgram(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CloseOptions(){
        MovementController xrMove= GameObject.Find("XR Rig").GetComponent<MovementController>();
        xrMove.movementActive=true;
        Destroy(gameObject);
    }
}
