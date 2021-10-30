using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_sounds : MonoBehaviour
{
    public AudioSource[] FobBreak, FobPlace, Other;
    public Transform camera;
    public SC_control SC_control;
    public SC_data SC_data;
    public float MHD; //max hearing distance

    public float GetVolume(Vector3 pos,float nvl)
    {
        int xd = 1;
        if(xd > 0) return 0f;

        //Linear system
        float dD,dN,dS;
        dD=(MHD-SC_control.Pitagoras((camera.position - new Vector3(0f,0f,camera.position.z)) - pos))/MHD;
        if(dD<0f) dD=0f;
        dS=float.Parse(SC_data.volume);
        dN=nvl;
        return dD*dN*dS;
    }
    public void PlaySound(Vector3 pos,int group,int ID)
    {
        AudioSource ads = new AudioSource();
        if(group==0) ads=Instantiate(FobBreak[ID],pos,new Quaternion(0f,0f,0f,0f));
        if(group==1) ads=Instantiate(FobPlace[ID],pos,new Quaternion(0f,0f,0f,0f));
        if(group==2) ads=Instantiate(Other[ID],pos,new Quaternion(0f,0f,0f,0f));

        SC_snd snd = ads.GetComponent<SC_snd>();
        snd.volume = GetVolume(pos,snd.naturalVolume);
        snd.Active();
    }
    public AudioSource StartLoop(GameObject parent,int group,int ID)
    {
        Vector3 pos=parent.transform.position;
        
        AudioSource ads = new AudioSource();
        if(group==0) ads=Instantiate(FobBreak[ID],pos,new Quaternion(0f,0f,0f,0f));
        if(group==1) ads=Instantiate(FobPlace[ID],pos,new Quaternion(0f,0f,0f,0f));
        if(group==2) ads=Instantiate(Other[ID],pos,new Quaternion(0f,0f,0f,0f));

        SC_snd snd = ads.GetComponent<SC_snd>();
        snd.updator=true;
        snd.Active();
        snd.GetComponent<Transform>().SetParent(parent.transform);

        return ads.GetComponent<AudioSource>();
    }
}
