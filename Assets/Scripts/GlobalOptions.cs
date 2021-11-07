using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GlobalOptions : MonoBehaviour
{
    public GameObject nodeGroupPrefab;

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

    public void RenameNodes(string nameCsv){
        string[] lines=File.ReadAllLines(nameCsv);
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
}
