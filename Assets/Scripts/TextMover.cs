using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;
using UnityEngine.UI;

//Controls the behavior of node labels so that they attach themselves to the
public class TextMover : MonoBehaviour
{
    public Transform Target;
    private int[] sizeBounds={12,150}; //These are in font size pts, I really wish I could do scale but that breaks the positioning
    private float moveCoef=33.0f; //How long it takes for the label to move to the node from its previous spot i.e. "smoothing"
    private RectTransform rt;
    private RectTransform parent;

    void Start(){
        rt=GetComponent<RectTransform>();
        parent=transform.parent.GetComponent<RectTransform>();
    }
    
    void Update(){
        rt.sizeDelta = new Vector2(parent.sizeDelta.x,parent.sizeDelta.y); //Unfortunately the parent isn't ready at Start so we have to do this in Update
        if (transform.position.x < float.MaxValue){
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(Target.position);
            if (screenPoint.z>0.0f){ //If the label's in front of you
                int newSize=(int)(sizeBounds[1]/screenPoint.z);
                Text t = GetComponent<Text>();
                if (newSize>sizeBounds[0]){ //If the label is close enough to you to be rendered, rescale based on its distance to you
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
