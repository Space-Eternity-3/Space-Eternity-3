using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_particle_transition : MonoBehaviour
{
    public bool active = true;

    void Start()
    {
        ParticleSystem ps = transform.GetComponent<ParticleSystem>();
        if(!active)
        {
            ps.Stop();
        }
    }
    void LateUpdate()
    {
        ParticleSystem ps = transform.GetComponent<ParticleSystem>();
        if(ps.isPlaying != active)
        {
            if(active) ps.Play();
            else ps.Stop();
        }
    }
}
