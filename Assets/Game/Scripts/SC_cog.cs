using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_cog : MonoBehaviour
{
    public Transform Hold;
    public Transform Body;
    public SC_fobs SC_fobs;
    public SC_control SC_control;

    void Update()
    {
        if(SC_fobs.index % 2 == 0)
        {
            Body.localRotation = Quaternion.Euler(0,-SC_control.cog_global_rot,0);
        }
        else
        {
            Body.localRotation = Quaternion.Euler(0,SC_control.cog_global_rot+15f,0);
        }
    }
}
