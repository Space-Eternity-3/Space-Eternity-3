using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_aspect : MonoBehaviour
{
    public Camera Camera;
    public CanvasScaler CanvasScaler;
    public bool menu;
    public float zoomMultiplier;
    
    Vector3 defaultCameraPosition = new Vector3(0f,0f,0f);

    void Start()
    {
        if(menu)
            defaultCameraPosition = Camera.transform.position;
    }
    void Update()
    {
        float aspect = Camera.aspect;
        if(aspect > 1.777777f) CanvasScaler.matchWidthOrHeight = 1f;
        else CanvasScaler.matchWidthOrHeight = 0f;

        if(menu)
        {
            if(CanvasScaler.matchWidthOrHeight==1f)
                Camera.transform.position = defaultCameraPosition + new Vector3(0f,0f,(aspect-1.777777f)*zoomMultiplier);
            else
                Camera.transform.position = defaultCameraPosition;
        }
    }
}
