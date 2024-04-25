using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_screen : MonoBehaviour
{
    public static bool fullS;
    public static bool already_started = false;

    void Start()
    {
        if(!already_started)
        {
            fullS = true;
            already_started = true;
        }
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F11)) fullS = !fullS;
        if(fullS && !Screen.fullScreen) SetFull();
        if(!fullS && Screen.fullScreen) SetWindowed();
    }
    void SetFull()
    {
        Screen.SetResolution(Screen.currentResolution.width,Screen.currentResolution.height,true);
        Screen.fullScreen=true;
    }
    void SetWindowed()
    {
        Screen.SetResolution(1280,720,true);
        Screen.fullScreen=false;
    }
}
