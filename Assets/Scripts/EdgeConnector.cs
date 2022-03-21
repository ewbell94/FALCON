using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ensures that edges stay connected even when nodes are moved
public class EdgeConnector : MonoBehaviour
{
    public GameObject NodeA;
    public GameObject NodeB;
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();   
        line.material.renderQueue=2999;  //Ensures that edges don't overlap the UI
    }

    void Update(){
        line.SetPosition(0,NodeA.transform.position);
        line.SetPosition(1,NodeB.transform.position);
    }
}
