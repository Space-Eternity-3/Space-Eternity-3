using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_bird : MonoBehaviour
{
    public RectTransform BadButton;
    public RectTransform GoodButton;
    public bool state;

    void Update()
    {
        if(state)
        {
            BadButton.localPosition = new Vector3(10000f,0f,0f);
            GoodButton.localPosition = new Vector3(0f,0f,0f);
        }
        else
        {
            BadButton.localPosition = new Vector3(0f,0f,0f);
            GoodButton.localPosition = new Vector3(10000f,0f,0f);
        }
    }
    public void SetValue()
    {
        state = !state;
    }
}
