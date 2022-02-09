using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private Dropdown dd;

    void Start(){
        dd = GameObject.Find("Dropdown").GetComponent<Dropdown>();
    }
    public void LaunchScene(){
        if (dd.value == 0){
            SceneManager.LoadScene("VRScene");
        } else {
            SceneManager.LoadScene("MKScene");
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}
