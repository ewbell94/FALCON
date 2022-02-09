using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MKUIWrapper : MonoBehaviour
{
    public void WrapUI(GameObject element){
        Vector2 uiSize = element.GetComponent<RectTransform>().sizeDelta + new Vector2(100.0f,100.0f);
        element.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = element.GetComponent<CanvasScaler>();
        cs.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = uiSize;
        if (uiSize.x > uiSize.y){
            cs.matchWidthOrHeight = 0.0f;
        } else {
            cs.matchWidthOrHeight = 1.0f;
        }
        foreach (Canvas c in element.GetComponentsInChildren<Canvas>()){
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.gameObject.AddComponent<GraphicRaycaster>();
        }


    }
}
