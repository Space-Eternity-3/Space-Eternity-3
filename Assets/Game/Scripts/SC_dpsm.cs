using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_dpsm : MonoBehaviour
{
    //Drill Particles Sound Manager
    public SC_snd_loop SC_snd_loop;
    int loopSndID = -1;
    bool active = false;

    void Update()
    {
        if(transform.position.z > -100f && transform.position.z < 100f)
        {
            if(!active)
            {
                active = true;
                loopSndID = SC_snd_loop.AddToLoop(0,transform.position);
            }
        }
        else
        {
            if(active)
            {
                active = false;
                SC_snd_loop.RemoveFromLoop(loopSndID);
            }
        }

        if(active) SC_snd_loop.sound_pos[loopSndID] = transform.position;
    }
    void OnDestroy()
    {
        try{
            if(active) SC_snd_loop.RemoveFromLoop(loopSndID);
        }catch(Exception) {}
    }
}
