using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Behavior for the options menu
public class GlobalOptions : MonoBehaviour
{
    public GameObject nodeMenuPrefab;
    public GameObject edgeMenuPrefab;
    private MKUIWrapper uiWrapper = null;

    void Start() {
        GameObject fpsController = GameObject.Find("FPSController");
        if (fpsController != null){  //This will be null in VR mode
            uiWrapper = fpsController.GetComponent<MKUIWrapper>();
        }
    }

    public void SpawnNodeMenu(){
        GameObject nodeMenu = (GameObject) Instantiate(nodeMenuPrefab,transform.position,transform.rotation);
        nodeMenu.GetComponent<Canvas>().worldCamera = Camera.main;
        if (uiWrapper != null){
            uiWrapper.WrapUI(nodeMenu);
        }
    }

    public void SpawnEdgeMenu(){
        GameObject edgeMenu = (GameObject) Instantiate(edgeMenuPrefab,transform.position,transform.rotation);
        edgeMenu.GetComponent<Canvas>().worldCamera = Camera.main;
        if (uiWrapper != null){
            uiWrapper.WrapUI(edgeMenu);
        }
    }

    //Loads in a new network state in SIF format
    public void ReadNewNetwork(){
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

    //Closes the options menu and gives control back to the player controller
    public void CloseOptions(){
        MovementController move;
        if (GameObject.Find("XR Rig") != null){
            move = GameObject.Find("XR Rig").GetComponent<MovementControllerVR>();
        } else {
            move = GameObject.Find("FPSController").GetComponent<MovementControllerMK>();
        }
        move.movementActive=true;
        Destroy(gameObject);
    }
}
