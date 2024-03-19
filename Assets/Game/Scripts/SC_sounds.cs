using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_sounds : MonoBehaviour
{
    public AudioSource[] FobBreak, FobPlace, Other;
    public Transform Camera;
    public SC_control SC_control;
    public SC_data SC_data;
    public float MHD; //max hearing distance

    public float GetVolume(Vector3 pos,float nvl,float deeprng)
    {
        float dN = nvl;
		float dG = SC_data.global_volume;
        float dS = Parsing.FloatE(SC_data.volume); 
        return GetVolumeDeep(pos,deeprng) * dS * dN * dG;
    }
    public float GetVolumeDeep(Vector3 pos, float deeprng)
    {
        //Linear system
        float dD = SC_control.Pitagoras((Camera.position - new Vector3(0f,0f,Camera.position.z)) - pos) - deeprng;
        if(dD<0f) dD=0f;
        dD=(MHD-dD)/MHD;
        if(dD<0f) dD=0f;
        return dD;
    }
    public float deeprange = 0;
    public void PlaySound(Vector3 pos,int group,int ID)
    {
        AudioSource ads = new AudioSource();
        if(group==0) ads=Instantiate(FobBreak[ID],pos,new Quaternion(0f,0f,0f,0f));
        if(group==1) ads=Instantiate(FobPlace[ID],pos,new Quaternion(0f,0f,0f,0f));
        if(group==2) ads=Instantiate(Other[ID],pos,new Quaternion(0f,0f,0f,0f));

        SC_snd snd = ads.GetComponent<SC_snd>();
        snd.deeprange = deeprange;
        snd.Active();

        deeprange = 0;
    }
}
