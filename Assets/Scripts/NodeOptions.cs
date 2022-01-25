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
    private NetworkBuilder networkBuilder; //Needed for calling SetNodeNames
    private List<InputField> nodeGroupRowFields = new List<InputField>(); //All entry fields for node group names
    private List<Dropdown> nodeRowDrops = new List<Dropdown>(); //All dropdown menus for individual nodes
    private List<Text> nodeRowNames = new List<Text>(); //All text fields for node names
    private List<GameObject> allNodes = new List<GameObject>(); //All node objects present in the network
    private List<GameObject> allNodeGroups; //All node groups present in the network
    private GameObject nodeGroupViewerContent; //The viewport for the node groups panel
    private GameObject nodeListViewerContent; //The viewport for the individual nodes panel
    private float updateWhen; //Used for updating less frequently than the Update loop
    private float updateInterval = 0.5f; //Time separation between updates

    //Called on menu instantiation
    void Start(){
        updateWhen=Time.fixedTime+updateInterval;
        optionsMenu = GameObject.Find("OptionsMenu(Clone)");
        networkBuilder = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
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
 
    //Called every frame
    void FixedUpdate(){

        //If the node group names are changed, update all the dropdown menus
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

    //When "new group" is pressed, this function adds in a new group to the window
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

    //Opens a CSV file full of "oldName,newName" lines and renames the nodes in the window accordingly
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

    //Opens and closes the color picker when clicked
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

    //Discards all changes through the "cancel" button
    public void Cancel(){
        optionsMenu.SetActive(true);
        Destroy(gameObject);
    }

    //Data structure to group together the parameters for each node group
    struct NodeGroupParams{
        public bool labelActive;
        public Color nodeColor;
        
    }

    //Applies changes made to the nodes/node groups to the scene
    public void ApplyChanges(){
        float nodeScale = transform.Find("NodeSizeSlider/Slider").gameObject.GetComponent<Slider>().value;
        //Pull node group parameters from node group window and create any new needed groups
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

        //Apply changes to each individual node according to their node group parameters and changes in the node window
        string[] oldNames = new string[allNodes.Count];
        string[] newNames = new string[allNodes.Count];
        string[] newGroups = new string[allNodes.Count];

        for (int i = 0; i<allNodes.Count; i++){
            GameObject node = allNodes[i];
            node.transform.localScale = new Vector3(nodeScale,nodeScale,nodeScale);
            string originalName = node.name;
            oldNames[i] = originalName;
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
                node.name = newName;
                label.name = newName+"_label";
                label.GetComponent<Text>().text = newName;
            }
            newNames[i] = newName;

            int stateIndex = nodeRowDrops[i].value;
            node.transform.SetParent(allNodeGroups[stateIndex].transform);
            newGroups[i] = allNodeGroups[stateIndex].name;
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
        networkBuilder.SetNodeNames(oldNames, newNames, newGroups); //changes node names/groups in the state list

        //Get rid of any empty node groups
        foreach(GameObject nodeG in allNodeGroups){
            if (nodeG.transform.childCount == 0){
                Destroy(nodeG);
            }
        }
        
        optionsMenu.SetActive(true);
        Destroy(gameObject);
    }
}
