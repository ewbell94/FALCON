using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;
using UnityEngine.UI;

public class TextMover : MonoBehaviour
{
    public Transform Target;
    private int[] sizeBounds={12,150};
    private float moveCoef=33.0f;
    private RectTransform rt;
    private RectTransform parent;

    void Start(){
        rt=GetComponent<RectTransform>();
        parent=transform.parent.GetComponent<RectTransform>();
    }
    
    void Update(){
        rt.sizeDelta = new Vector2(parent.sizeDelta.x,parent.sizeDelta.y);
        if (transform.position.x < float.MaxValue){
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(Target.position);
            if (screenPoint.z>0.0f){
                int newSize=(int)(sizeBounds[1]/screenPoint.z);
                Text t = GetComponent<Text>();
                if (newSize>sizeBounds[0]){
                    if (newSize<sizeBounds[1]){
                        t.fontSize=newSize;
                    }
                    if (transform.localScale.x < 1e-5){
                        transform.localScale=new Vector3(1.0f,1.0f,1.0f);
                        transform.localPosition=screenPoint;
                    } else {
                        transform.localPosition=Vector3.MoveTowards(transform.localPosition,screenPoint,Time.deltaTime*moveCoef*Vector3.Distance(transform.localPosition,screenPoint));
                    }
                } else {
                    transform.localScale=new Vector3(0.0f,0.0f,0.0f);
                }

            } else {
                transform.localScale=new Vector3(0.0f,0.0f,0.0f);
            }
        }
    }
}
