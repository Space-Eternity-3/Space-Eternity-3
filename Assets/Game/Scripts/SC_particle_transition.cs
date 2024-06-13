using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_particle_transition : MonoBehaviour
{
    public bool active = true;
    bool locally_active = true;

    void Awake()
    {
        ParticleSystem ps = transform.GetComponent<ParticleSystem>();
        if(!active)
        {
            locally_active = false;
            ps.Stop();
        }
    }
    void LateUpdate()
    {
        ParticleSystem ps = transform.GetComponent<ParticleSystem>();
        if(locally_active != active)
        {
            if(active) ps.Play();
            else ps.Stop();
            locally_active = active;
        }
    }
}
