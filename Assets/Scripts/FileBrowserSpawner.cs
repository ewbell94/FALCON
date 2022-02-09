using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SimpleFileBrowser;
public class FileBrowserSpawner : MonoBehaviour
{
    public GameObject fileBrowserPrefab;
    private MKUIWrapper uiWrapper = null;

    void Start() {
        GameObject fpsController = GameObject.Find("FPSController");
        if (fpsController != null){
            uiWrapper = fpsController.GetComponent<MKUIWrapper>();
        }
    }

    public void SpawnLoader(FileBrowser.OnSuccess onSuccess, FileBrowser.OnCancel onCancel, string title, string fileExtension, Vector3 position, Quaternion rotation){
        GameObject fBrowser = (GameObject) Instantiate(fileBrowserPrefab,Vector3.zero,Quaternion.identity);
        if (uiWrapper == null){
            Canvas c = fBrowser.GetComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            c.worldCamera = Camera.main;
            fBrowser.transform.position = position;
            fBrowser.transform.rotation = rotation;
            fBrowser.transform.localScale = new Vector3(0.005f,0.005f,0.005f);
            FileBrowser.SingleClickMode = true;
        } else {
            uiWrapper.WrapUI(fBrowser);
        }
        FileBrowser.SetFilters(false, fileExtension);
        FileBrowser.ShowLoadDialog(onSuccess,onCancel,FileBrowser.PickMode.Files,false,title,"Select"); 
    }
}
