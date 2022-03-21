using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Behavior for the error/warning log that flashes on screen
public class LogPrompt : MonoBehaviour
{
    private float fadeLength = 0.5f; //How long it takes to fade out
    private float fadeWhen = 0.0f;
    Text t;
    Color initialColor;

    void Start(){
        t = gameObject.GetComponent<Text>();
        initialColor = t.color;
    }

    void Update()
    {
        if (Time.time > fadeWhen && t.color.a > 0.0f){
            t.color = Color.Lerp(initialColor, new Color(initialColor.r, initialColor.g, initialColor.b, 0.0f), (Time.time-fadeWhen)/fadeLength);
        }
    }

    //Shows the error prompt text, using color textColor, for showLength seconds
    public void SetText(string text, Color textColor, float showLength){
        t.text = text;
        t.color = textColor;
        initialColor = textColor;
        fadeWhen = Time.time + showLength;
    }
}
