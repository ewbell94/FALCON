using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeConfig : MonoBehaviour
{
    public void SetColor(Color edgeColor){
        foreach (Transform child in transform){
            LineRenderer renderer = child.gameObject.GetComponent<LineRenderer>();
            renderer.SetColors(edgeColor,edgeColor);
        }
    }
}
