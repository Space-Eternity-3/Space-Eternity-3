using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_loop_holder : MonoBehaviour
{
    public int lsid;
    public bool use_source_pointer;
    public Transform SourcePointer;
    public Transform SourceParentPointer;
    public bool scale_must_be;
    public SC_snd_loop SC_snd_loop;
    
    int loopSndID;
    bool mother = true;
    
    Vector3 GetTpos()
    {
        if(!use_source_pointer) return transform.position;

        if(SourceParentPointer.gameObject.activeSelf && (SourcePointer.localScale.x>0.4f || !scale_must_be)) return SourcePointer.position;
        else return new Vector3(0f,0f,10000f);
    }
    public void Start()
    {
        mother = transform.position.z > 100f;
        if(mother) return;

        loopSndID = SC_snd_loop.AddToLoop(lsid,GetTpos());
    }
    public void FixedUpdate()
    {
        if(mother) return;

        SC_snd_loop.sound_pos[loopSndID] = GetTpos();
    }
    public void OnDestroy()
    {
        if(mother) return;
        
        try{
            SC_snd_loop.RemoveFromLoop(loopSndID);
        }catch(Exception e) {}
    }
}
