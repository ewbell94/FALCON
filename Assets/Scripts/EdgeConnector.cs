using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeConnector : MonoBehaviour
{
    public GameObject NodeA;
    public GameObject NodeB;
    private LineRenderer line;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();   
        line.material.renderQueue=2999;
    }

    void Update(){
        line.SetPosition(0,NodeA.transform.position);
        line.SetPosition(1,NodeB.transform.position);
    }
}
