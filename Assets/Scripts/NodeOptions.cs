using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class NodeOptions : MonoBehaviour
{
    public GameObject nodeGroupPrefab;
    public GameObject nodeGroupRowPrefab;
    public GameObject nodeRowPrefab;

    private GameObject optionsMenu;
    private List<InputField> nodeGroupRowFields;
    private List<Dropdown> nodeRowDrops;
    private float updateWhen;
    private float updateInterval = 0.5f;

    void Start(){
        updateWhen=Time.fixedTime+updateInterval;
        optionsMenu = GameObject.Find("OptionsMenu(Clone)");
        optionsMenu.SetActive(false);
        nodeGroupRowFields = new List<InputField>();
        nodeRowDrops = new List<Dropdown>();
        GameObject[] nodeGroups= GameObject.FindGameObjectsWithTag("NodeGroup");
        List<GameObject> allNodes = new List<GameObject>();
        GameObject nodeGroupViewerContent = transform.Find("NodeGroupWindow/NodeGroupViewer/Viewport/Content").gameObject;
        nodeGroupViewerContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f,40.0f*nodeGroups.Length+10.0f);
        for (int i=0;i<nodeGroups.Length;i++){
            GameObject newGroupRow = (GameObject) Instantiate(nodeGroupRowPrefab,Vector3.zero,Quaternion.identity);
            newGroupRow.transform.SetParent(nodeGroupViewerContent.transform,false);
            newGroupRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f,-40.0f*i-10.0f);
            InputField inpf = newGroupRow.transform.Find("InputField").gameObject.GetComponent<InputField>();
            inpf.text = nodeGroups[i].name;
            nodeGroupRowFields.Add(inpf);
            GameObject firstNode = nodeGroups[i].transform.GetChild(0).gameObject;
            newGroupRow.transform.Find("LabelToggle").gameObject.GetComponent<Toggle>().isOn = GameObject.Find(firstNode.name+"_label").activeSelf;
            newGroupRow.transform.Find("ColorPicker/Button").gameObject.GetComponent<Image>().color = firstNode.GetComponent<MeshRenderer>().material.color;
            foreach (Transform childNode in nodeGroups[i].transform){
                allNodes.Add(childNode.gameObject);
            }
        }
        GameObject nodeListViewerContent = transform.Find("NodeListWindow/NodeListViewer/Viewport/Content").gameObject;
        nodeListViewerContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f,40.0f*allNodes.Count+10.0f);
        for (int i=0;i<allNodes.Count;i++){
            GameObject newNodeRow = (GameObject) Instantiate(nodeRowPrefab,Vector3.zero,Quaternion.identity);
            newNodeRow.transform.SetParent(nodeListViewerContent.transform,false);
            newNodeRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f,-40.0f*i-10.0f);
            Dropdown dd = newNodeRow.transform.Find("Dropdown").gameObject.GetComponent<Dropdown>();
            nodeRowDrops.Add(dd);
            newNodeRow.transform.Find("InputField").gameObject.GetComponent<InputField>().text = allNodes[i].name;
        }
    }

    
    void FixedUpdate(){
        if (Time.fixedTime >= updateWhen){
            List<string> nodeGroupNames = new List<string>();
            foreach (InputField nodeGroupRowField in nodeGroupRowFields){
                nodeGroupNames.Add(nodeGroupRowField.text);
            }
            foreach (Dropdown nodeRowDrop in nodeRowDrops){
                for (int i = 0; i<nodeRowDrop.options.Count;i++){
                    nodeRowDrop.options[i].text=nodeGroupNames[i];
                }
                
                if (nodeGroupNames.Count > nodeRowDrop.options.Count){
                    List<string> toAdd = nodeGroupNames.GetRange(nodeRowDrop.options.Count,nodeGroupNames.Count-nodeRowDrop.options.Count);
                    nodeRowDrop.AddOptions(toAdd);
                }
                
            }
            updateWhen=Time.fixedTime+updateInterval;
        }
    }
    public void SpawnNewGroupRow(){
        GameObject nodeGroupViewerContent = transform.Find("NodeGroupWindow/NodeGroupViewer/Viewport/Content").gameObject;
        RectTransform rt = nodeGroupViewerContent.GetComponent<RectTransform>();
        GameObject newGroupRow = (GameObject) Instantiate(nodeGroupRowPrefab,Vector3.zero,Quaternion.identity);
        newGroupRow.transform.SetParent(nodeGroupViewerContent.transform,false);
        newGroupRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f,-rt.sizeDelta.y);
        rt.sizeDelta = new Vector2(0.0f,rt.sizeDelta.y+40.0f);
        InputField inpf = newGroupRow.transform.Find("InputField").gameObject.GetComponent<InputField>();
        inpf.text = "NodeGroup_"+nodeGroupRowFields.Count.ToString();
        nodeGroupRowFields.Add(inpf);
    }
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
    public void NewNodeOrder(string[] groupNames, string[][] nodeMembers){
        for(int i=0;i<groupNames.Length;i++){
            GameObject nodeGroup = GameObject.Find(groupNames[i]);
            if (nodeGroup == null){
                nodeGroup = (GameObject) Instantiate(nodeGroupPrefab,Vector3.zero,Quaternion.identity);
                nodeGroup.name=groupNames[i];
            } 
            foreach(string nodeName in nodeMembers[i]){
                GameObject node = GameObject.Find(nodeName);
                node.transform.SetParent(nodeGroup.transform,true);
            }
        }

        foreach(GameObject nodeG in GameObject.FindGameObjectsWithTag("NodeGroup")){
            if (nodeG.transform.childCount == 0){
                Destroy(nodeG);
            }
        }
    }

    public void RenameNodesButton(){
        string path = EditorUtility.OpenFilePanel("Choose new name CSV...","","csv");
        string[] lines=File.ReadAllLines(path);
        foreach(string line in lines){
            string cleanline=line.Trim('\n');
            string[] parts=cleanline.Split(',');
            
            GameObject node = GameObject.Find(parts[0]);
            if (node == null){
                continue;
            } 

            if (parts[0]!=parts[1] && GameObject.Find(parts[1]) != null){
                int i = 1;
                while (GameObject.Find(parts[1]+"_"+i.ToString()) != null){
                    i++;
                }
                parts[1]=parts[1]+"_"+i.ToString();
            }
            node.name=parts[1];

            GameObject label = GameObject.Find(parts[0]+"_label");
            label.name=parts[1]+"_label";
            label.GetComponent<Text>().text=parts[1];
        }
    }

    public void Cancel(){
        optionsMenu.SetActive(true);
        Destroy(gameObject);
    }
}
