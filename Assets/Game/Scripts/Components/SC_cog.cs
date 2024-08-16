using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_cog : MonoBehaviour
{
    public Transform Hold;
    public Transform Body;
    public SC_fobs SC_fobs;
    public SC_control SC_control;
    public SC_factory SC_factory; //automatically assigned
    public int configuration = 0; // 0:auto 1:right 2:left
    public int cog_local_rot = 0;
    public bool active = true;

    void FixedUpdate()
    {
        if(!active)
            cog_local_rot-=3;
    }
    void LateUpdate()
    {
        bool is_right;
        if(configuration==0) is_right = SC_fobs.index % 2 == 0;
        else is_right = configuration==1;

        int external_delta = 0;
        if(SC_factory != null) external_delta = SC_factory.internal_rotation_delta;

        if(is_right)
            Body.localRotation = Quaternion.Euler(0,-(SC_control.cog_global_rot + cog_local_rot + external_delta),0);
        else
            Body.localRotation = Quaternion.Euler(0,(SC_control.cog_global_rot + cog_local_rot + external_delta)+15f,0);
    }
}
