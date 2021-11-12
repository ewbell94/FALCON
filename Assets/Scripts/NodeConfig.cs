using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeConfig : MonoBehaviour
{
    public void SetColor(Color nodeColor){
        Material newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.color=nodeColor;
        newMaterial.EnableKeyword("_EMISSION");
        newMaterial.SetColor("_EmissionColor",new Color(nodeColor.r/2,nodeColor.g/2,nodeColor.b/2,nodeColor.a));
        foreach (Transform child in transform){
            MeshRenderer renderer = child.gameObject.GetComponent<MeshRenderer>();
            renderer.material = newMaterial;
        }
    }

    public void Labels(bool labelState){
        foreach (Transform child in transform){
            string name = child.gameObject.name;
            GameObject.Find(name+"_label").SetActive(labelState);
        }
    }
}
