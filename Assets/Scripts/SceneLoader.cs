using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;

//Behavior on startup
public class SceneLoader : MonoBehaviour
{
    public Sprite[] controlSprites; //Sprites for the control how tos for VR and MK
    private Dropdown dd;
    private Image controls;
    void Start(){
        dd = GameObject.Find("Dropdown").GetComponent<Dropdown>();
        controls = GameObject.Find("Controls").GetComponent<Image>();
    }

    public void LaunchScene(){
        if (dd.value == 0){
            SceneManager.LoadScene("VRScene");
        } else {
            SceneManager.LoadScene("MKScene");
            XRGeneralSettings.Instance.Manager.DeinitializeLoader(); //So the program doesn't freak out if there's no VR headset
        }
    }

    void Update(){
        controls.sprite = controlSprites[dd.value]; //Ensures the control how to matches the selected mode
    }
}
