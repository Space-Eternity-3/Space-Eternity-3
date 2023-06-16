using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_loop_holder : MonoBehaviour
{
    public int lsid;
    public SC_snd_loop SC_snd_loop;
    int loopSndID;
    bool mother = true;
    
    public void Start()
    {
        mother = transform.position.z > 100f;
        if(mother) return;

        loopSndID = SC_snd_loop.AddToLoop(lsid,transform.position);
    }
    public void FixedUpdate()
    {
        if(mother) return;

        SC_snd_loop.sound_pos[loopSndID] = transform.position;
    }
    public void OnDestroy()
    {
        if(mother) return;
        
        try{
            SC_snd_loop.RemoveFromLoop(loopSndID);
        }catch(Exception e) {}
    }
}
