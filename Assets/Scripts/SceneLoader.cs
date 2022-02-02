using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (XRSettings.isDeviceActive){
            SceneManager.LoadScene("VRScene");
        } else {
            SceneManager.LoadScene("MKScene");
        }
    }
}
