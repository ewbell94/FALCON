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
        Instantiate(edgeMenuPrefab,transform.position,transform.rotation);
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
