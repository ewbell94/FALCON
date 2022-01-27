using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        //string path = EditorUtility.OpenFilePanel("Choose interaction file...","","sif");
        NetworkBuilder n = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
        FileBrowserSpawner fbs = GameObject.Find("FileBrowserSpawner").GetComponent<FileBrowserSpawner>();
        fbs.SpawnLoader((paths)=>{n.BuildNetwork(paths[0]); Destroy(gameObject);},
                        ()=>{gameObject.SetActive(true);},
                        "Choose interaction file...",".sif",transform.position,transform.rotation);
        gameObject.SetActive(false);
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
