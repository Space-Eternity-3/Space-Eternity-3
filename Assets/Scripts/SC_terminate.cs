using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_terminate : MonoBehaviour
{
    public int timer;
    public bool disabled;
    public bool loopSound;
    public bool partgey;
    bool mother=true;

    public ParticleSystem ps;
    public SC_snd SC_snd;
    public SC_fobs SC_fobs;
    
    void Start()
    {
        if(transform.position.z<100f) mother=false;
    }
    void FixedUpdate()
    {
        if(!mother&&!disabled)
        {
            timer--;
            if(timer<=50 && loopSound) SC_snd.terminated = true;
            if(timer<=0) Destroy(gameObject);
        }
        if(partgey && SC_fobs.mother)
        {
            disabled=false;
            particleDisable();
        }
    }
    public void particleDisable()
    {
        var emission = ps.emission;
        emission.enabled=false;
    }
}
