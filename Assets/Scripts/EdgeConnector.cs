using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeConnector : MonoBehaviour
{
    public GameObject NodeA;
    public GameObject NodeB;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 startPoint=NodeA.transform.position;
        Vector3 endPoint=NodeB.transform.position;

        LineRenderer line = GetComponent<LineRenderer>();
        line.SetPosition(0,startPoint);
        line.SetPosition(1,endPoint);
        line.material.renderQueue=2999;
    }
}
