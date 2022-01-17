using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class NodeOptions : MonoBehaviour
{
    public GameObject nodeGroupPrefab;
    public GameObject nodeGroupRowPrefab;
    public GameObject nodeRowPrefab;
    private GameObject optionsMenu;
    private GameObject colorPicker;
    private List<InputField> nodeGroupRowFields = new List<InputField>();
    private List<Dropdown> nodeRowDrops = new List<Dropdown>();
    private List<Text> nodeRowNames = new List<Text>();
    private List<GameObject> allNodes = new List<GameObject>();
    private List<GameObject> allNodeGroups;
    private GameObject nodeGroupViewerContent;
    private GameObject nodeListViewerContent;
    private float updateWhen;
    private float updateInterval = 0.5f;

    void Start(){
        updateWhen=Time.fixedTime+updateInterval;
        optionsMenu = GameObject.Find("OptionsMenu(Clone)");
        colorPicker = transform.Find("FlexibleColorPicker").gameObject;
        optionsMenu.SetActive(false);
        float nodeScale = -1.0f;
        GameObject[] nodeGroups = GameObject.FindGameObjectsWithTag("NodeGroup");
        allNodeGroups = nodeGroups.ToList();
        allNodes = new List<GameObject>();
        List<string> nodeGroupNames = new List<string>();
        nodeGroupViewerContent = transform.Find("NodeGroupWindow/NodeGroupViewer/Viewport/Content").gameObject;
        nodeGroupViewerContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f,40.0f*nodeGroups.Length+10.0f);
        for (int i=0;i<nodeGroups.Length;i++){
            GameObject newGroupRow = (GameObject) Instantiate(nodeGroupRowPrefab,Vector3.zero,Quaternion.identity);
            newGroupRow.transform.SetParent(nodeGroupViewerContent.transform,false);
            newGroupRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f,-40.0f*i-10.0f);
            nodeGroupNames.Add(nodeGroups[i].name);
            InputField inpf = newGroupRow.transform.Find("InputField").gameObject.GetComponent<InputField>();
            inpf.text = nodeGroups[i].name;
            nodeGroupRowFields.Add(inpf);
            GameObject firstNode = nodeGroups[i].transform.GetChild(0).gameObject;
            if (GameObject.Find(firstNode.name+"_label").transform.position.x < float.MaxValue){
                newGroupRow.transform.Find("LabelToggle").gameObject.GetComponent<Toggle>().isOn = true;
            } else {
                newGroupRow.transform.Find("LabelToggle").gameObject.GetComponent<Toggle>().isOn = false;
            }
            GameObject cpb = newGroupRow.transform.Find("ColorPicker/Button").gameObject;
            cpb.GetComponent<Image>().color = firstNode.GetComponent<MeshRenderer>().material.color;
            cpb.GetComponent<Button>().onClick.AddListener(delegate{ColorPicker(cpb);});
            if (nodeScale<0.0f){
                nodeScale=firstNode.transform.localScale.x;
                transform.Find("NodeSizeSlider/Slider").gameObject.GetComponent<Slider>().value = nodeScale;
            }
            foreach (Transform childNode in nodeGroups[i].transform){
                allNodes.Add(childNode.gameObject);
            }
        }
        nodeListViewerContent = transform.Find("NodeListWindow/NodeListViewer/Viewport/Content").gameObject;
        nodeListViewerContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f,40.0f*allNodes.Count+10.0f);
        for (int i=0;i<allNodes.Count;i++){
            GameObject newNodeRow = (GameObject) Instantiate(nodeRowPrefab,Vector3.zero,Quaternion.identity);
            newNodeRow.transform.SetParent(nodeListViewerContent.transform,false);
            newNodeRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f,-40.0f*i-10.0f);
            Dropdown dd = newNodeRow.transform.Find("Dropdown").gameObject.GetComponent<Dropdown>();
            dd.AddOptions(nodeGroupNames);
            dd.value = nodeGroupNames.IndexOf(allNodes[i].transform.parent.gameObject.name);
            nodeRowDrops.Add(dd);
            Text t = newNodeRow.transform.Find("NodeName").gameObject.GetComponent<Text>();
            t.text = allNodes[i].name;
            nodeRowNames.Add(t);
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
        RectTransform rt = nodeGroupViewerContent.GetComponent<RectTransform>();
        GameObject newGroupRow = (GameObject) Instantiate(nodeGroupRowPrefab,Vector3.zero,Quaternion.identity);
        newGroupRow.transform.SetParent(nodeGroupViewerContent.transform,false);
        newGroupRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f,-rt.sizeDelta.y);
        GameObject cpb = newGroupRow.transform.Find("ColorPicker/Button").gameObject;
        cpb.GetComponent<Button>().onClick.AddListener(delegate{ColorPicker(cpb);});
        rt.sizeDelta = new Vector2(0.0f,rt.sizeDelta.y+40.0f);
        InputField inpf = newGroupRow.transform.Find("InputField").gameObject.GetComponent<InputField>();
        inpf.text = "NodeGroup_"+nodeGroupRowFields.Count.ToString();
        nodeGroupRowFields.Add(inpf);
    }

    public void RenameNodesButton(){
        string path = EditorUtility.OpenFilePanel("Choose new name CSV...","","csv");
        string[] lines=File.ReadAllLines(path);
        Dictionary<string,string> nameDict = new Dictionary<string,string>();
        foreach(string line in lines){
            string cleanline=line.Trim('\n');
            string[] parts=cleanline.Split(',');
            
            nameDict[parts[0]]=parts[1];
        }

        foreach(Text name in nodeRowNames){
            string nameText = name.text;
            if (nameDict.ContainsKey(nameText)){
                name.text = nameDict[nameText];
            }
        } 
    }

    public void ColorPicker(GameObject button){
        if (colorPicker.transform.localScale.x == 0.0f){
            colorPicker.transform.SetParent(button.transform);
            Vector2 buttonPos = button.GetComponent<RectTransform>().anchoredPosition;
            colorPicker.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonPos.x-10.0f,buttonPos.y);
            colorPicker.transform.localScale = new Vector3(0.7f,0.7f,0.7f);
        } else {
            button.GetComponent<Image>().color = colorPicker.GetComponent<FlexibleColorPicker>().color;
            colorPicker.transform.localScale = Vector3.zero;
        }
    }

    public void Cancel(){
        optionsMenu.SetActive(true);
        Destroy(gameObject);
    }

    struct NodeGroupParams{
        public bool labelActive;
        public Color nodeColor;
        
    }

    public void ApplyChanges(){
        float nodeScale = transform.Find("NodeSizeSlider/Slider").gameObject.GetComponent<Slider>().value;
        List<NodeGroupParams> nodeGroupParamList = new List<NodeGroupParams>();
        for (int i = 0; i<nodeGroupViewerContent.transform.childCount; i++){
            Transform nodeGroupRow = nodeGroupViewerContent.transform.GetChild(i);
            NodeGroupParams ngp = new NodeGroupParams();
            ngp.labelActive = nodeGroupRow.Find("LabelToggle").gameObject.GetComponent<Toggle>().isOn;
            ngp.nodeColor = nodeGroupRow.Find("ColorPicker/Button").gameObject.GetComponent<Image>().color;
            nodeGroupParamList.Add(ngp);
            string groupName = nodeGroupRow.Find("InputField").gameObject.GetComponent<InputField>().text;
            if (i<allNodeGroups.Count){
                allNodeGroups[i].name = groupName;
            } else {
                GameObject newNodeGroup = (GameObject) Instantiate(nodeGroupPrefab,Vector3.zero,Quaternion.identity);
                newNodeGroup.name = groupName;
                allNodeGroups.Add(newNodeGroup);
            }
        }

        for (int i = 0; i<allNodes.Count; i++){
            GameObject node = allNodes[i];
            node.transform.localScale = new Vector3(nodeScale,nodeScale,nodeScale);
            string originalName = node.name;
            GameObject label = GameObject.Find(originalName+"_label");
            string newName = nodeRowNames[i].text;
            if (originalName != newName){
                if (GameObject.Find(newName) != null){
                    int n = 1;
                    while (GameObject.Find(newName+"_"+n.ToString()) != null){
                        n++;
                    }
                    newName = newName+"_"+n.ToString();
                }
                node.name=newName;
                label.name = newName+"_label";
                label.GetComponent<Text>().text = newName;
            }

            int stateIndex = nodeRowDrops[i].value;
            node.transform.SetParent(allNodeGroups[stateIndex].transform);
            NodeGroupParams ngp = nodeGroupParamList[stateIndex];

            if (ngp.labelActive){
                label.transform.position = new Vector3(0.0f,0.0f,0.0f);
            } else {
                label.transform.position = new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
            }
            //label.SetActive(ngp.labelActive);
            Material newMaterial = new Material(Shader.Find("Standard"));
            newMaterial.color=ngp.nodeColor;
            newMaterial.EnableKeyword("_EMISSION");
            newMaterial.SetColor("_EmissionColor",new Color(ngp.nodeColor.r/2,ngp.nodeColor.g/2,ngp.nodeColor.b/2,ngp.nodeColor.a));
            foreach (Transform child in allNodeGroups[stateIndex].transform){
                MeshRenderer renderer = child.gameObject.GetComponent<MeshRenderer>();
                renderer.material = newMaterial;
            }
        }

        foreach(GameObject nodeG in allNodeGroups){
            if (nodeG.transform.childCount == 0){
                Destroy(nodeG);
            }
        }

        optionsMenu.SetActive(true);
        Destroy(gameObject);
    }
}
