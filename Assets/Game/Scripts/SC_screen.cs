using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_screen : MonoBehaviour
{
    public static bool already_started = false;
    public static bool fullS;

    public static int defX;
    public static int defY;

    public static int reload_counter = 0;

    void Start()
    {
        if(!already_started)
        {
            defX = Screen.currentResolution.width;
            defY = Screen.currentResolution.height;
            fullS = true;
            already_started = true;
        }
        ReloadScreen();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) ReloadScreen();

        if(fullS) {
            if(reload_counter==2) SetFull2();
            if(reload_counter==1) SetFull();
        }
        else {
            if(reload_counter==2) SetWindowed2();
            if(reload_counter==1) SetWindowed();
        }

        if(reload_counter==0)
        {
            if(Input.GetKeyDown(KeyCode.F11)) fullS = !fullS;
            if(fullS && !Screen.fullScreen) SetFull();
            if(!fullS && Screen.fullScreen) SetWindowed();
        }
        else reload_counter--;
    }

    void SetFull()      { Screen.SetResolution(defX,defY,true); }
    void SetWindowed()  { Screen.SetResolution(1280,720,false); }
    void SetFull2()     { Screen.SetResolution(defX-1,defY-1,true); }
    void SetWindowed2() { Screen.SetResolution(1280-1,720-1,false); }

    void ReloadScreen()
    {
        reload_counter = 2;
    }
}
