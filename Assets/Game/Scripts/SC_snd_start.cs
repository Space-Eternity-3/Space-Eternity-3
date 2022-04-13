using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_snd_start : MonoBehaviour
{
    public SC_sounds SC_sounds;
    public int type,id;
    public bool updator;
    public bool dontStart;

    public int enMode=0;
    public int drMode=0;

    bool exists=false;
    AudioSource sndObject;

    int nanoCounter=-1;

    void Start()
    {
        if(transform.position.z<100f)
        {
            if(!updator)
            {
                SC_sounds.PlaySound(transform.position,type,id);
            }
            else if(!dontStart)
            {
                nanoCounter=Random.Range(1,15);
            }
        }
    }
    void FixedUpdate()
    {
        nanoCounter--;
        if(nanoCounter==0)
        {
            ManualStartLoop(0);
        }
    }
    public void ManualStartLoop(int deltaID)
    {
        if(exists) ManualEndLoop();
        exists=true;
        sndObject=SC_sounds.StartLoop(gameObject,type,id+deltaID);
    }
    public void ManualEndLoop()
    {
        nanoCounter=-1;
        exists=false;
        sndObject.GetComponent<SC_terminate>().disabled=false;
    }
}
