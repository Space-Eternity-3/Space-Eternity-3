using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_cog : MonoBehaviour
{
    public Transform Hold;
    public Transform Body;
    public SC_fobs SC_fobs;
    public SC_control SC_control;
    public int configuration = 0; // 0:auto 1:right 2:left

    void Update()
    {
        bool is_right;
        if(configuration==0) is_right = SC_fobs.index % 2 == 0;
        else is_right = configuration==1;

        if(is_right)
            Body.localRotation = Quaternion.Euler(0,-SC_control.cog_global_rot,0);
        else
            Body.localRotation = Quaternion.Euler(0,SC_control.cog_global_rot+15f,0);
    }
}
