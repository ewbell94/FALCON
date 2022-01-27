using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

public class EdgeOptions : MonoBehaviour
{
    private GameObject colorPicker;
    private GameObject optionsMenu;
    private float minWidth = 0.01f;
    private float maxWidth = 0.5f;
    private Dictionary<string,float> edgeWeights;
    // Start is called before the first frame update
    void Start(){
        colorPicker = transform.Find("FlexibleColorPicker").gameObject;
        optionsMenu = GameObject.Find("OptionsMenu(Clone)");
        optionsMenu.SetActive(false);
        GameObject firstEdge = GameObject.Find("Edges").transform.GetChild(0).gameObject;
        transform.Find("EdgeColorPart/Button").gameObject.GetComponent<Image>().color = firstEdge.GetComponent<LineRenderer>().startColor;
        edgeWeights = new Dictionary<string, float>();
    }

    public void ColorPicker(GameObject button){
        if (colorPicker.transform.localScale.x == 0.0f){
            colorPicker.transform.SetParent(button.transform);
            Vector2 buttonPos = button.GetComponent<RectTransform>().anchoredPosition;
            colorPicker.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonPos.x-10.0f,buttonPos.y);
            colorPicker.transform.localScale = new Vector3(1.4f,1.4f,1.4f);
        } else {
            button.GetComponent<Image>().color = colorPicker.GetComponent<FlexibleColorPicker>().color;
            colorPicker.transform.localScale = Vector3.zero;
        }
    }

    public void WeightEdgesButton(){
        FileBrowserSpawner fbs = GameObject.Find("FileBrowserSpawner").GetComponent<FileBrowserSpawner>();
        fbs.SpawnLoader((paths)=>{WeightEdges(paths[0]); gameObject.SetActive(true);},
                        ()=>{gameObject.SetActive(true);},
                        "Choose weight edges CSV...",".csv",transform.position,transform.rotation);
        gameObject.SetActive(false);
    }

    private void WeightEdges(string path){
        edgeWeights = new Dictionary<string, float>(); //If the file is opened again without applying, clean out the old weights
        string[] lines=File.ReadAllLines(path);
        List<float> weights = new List<float>();
        List<string> chainPairs = new List<string>();
        float minWeight = Single.PositiveInfinity;
        float maxWeight = Single.NegativeInfinity;
        foreach(string line in lines){
            string cleanline=line.Trim('\n');
            string[] parts=cleanline.Split(',');
            string[] chains=parts[0].Split('-');
            float weight = float.Parse(parts[1]);
            if (weight < minWeight){
                minWeight = weight;
            }
            if (weight > maxWeight){
                maxWeight = weight;
            }
            chainPairs.Add(chains[0]+","+chains[1]);
            weights.Add(weight);
            chainPairs.Add(chains[1]+","+chains[0]);
            weights.Add(weight);
        }
        
        float m;
        float b;
        
        if (maxWeight != minWeight){
            m = (maxWidth-minWidth)/(maxWeight-minWeight);
            b = minWidth-m*minWeight;
        } else {
            m = 0.0f;
            b = 0.1f;
        }

        for (int i = 0; i<weights.Count; i++){
            float newWeight = m*weights[i]+b;
            edgeWeights.Add(chainPairs[i],newWeight);
        }
    }

    public void Cancel(){
        optionsMenu.SetActive(true);
        Destroy(gameObject);
    }

    public void ApplyChanges(){
        Color finalColor = transform.Find("EdgeColorPart/Button").gameObject.GetComponent<Image>().color;
        int edgeCount = GameObject.Find("Edges").transform.childCount;
        string[] nodesA = new string[edgeCount];
        string[] nodesB = new string[edgeCount];
        float[] weights = new float[edgeCount];
        int i = 0;

        foreach (Transform edgeT in GameObject.Find("Edges").transform){
            GameObject edge = edgeT.gameObject;
            LineRenderer lr = edge.GetComponent<LineRenderer>();
            lr.startColor = finalColor;
            lr.endColor = finalColor;
            if (edgeWeights.Count > 0){
                EdgeConnector ec = edge.GetComponent<EdgeConnector>();
                nodesA[i] = ec.NodeA.name;
                nodesB[i] = ec.NodeB.name;
                if (edgeWeights.ContainsKey(ec.NodeA.name + "," + ec.NodeB.name)){
                    float width=edgeWeights[ec.NodeA.name+","+ec.NodeB.name];
                    lr.startWidth=width;
                    lr.endWidth=width;
                    weights[i] = width;
                } else {
                    lr.startWidth=0.1f;
                    lr.endWidth=0.1f;
                    weights[i] = 0.1f;
                }
                i++;
            }
        }

        if (i > 0){
            GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>().SetEdgeWeights(nodesA, nodesB, weights);
        }
        optionsMenu.SetActive(true);
        Destroy(gameObject);
    }
}
