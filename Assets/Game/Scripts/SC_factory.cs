using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_factory : MonoBehaviour
{
    public GameObject Off, On;
    public SC_cog[] LocalCogs;
    public SC_particle_transition[] LocalParticles;
    public bool production;
    public int internal_rotation_delta = 0;
    public bool mother;

    void Start()
    {
        mother = transform.parent==null;

        foreach(SC_cog SC_cog in LocalCogs)
        {
            SC_cog.configuration = UnityEngine.Random.Range(1,3);
        }

        //temporary
        int rand = UnityEngine.Random.Range(0,2);
        production = rand==1;
    }
    void Update()
    {
        if(mother) return;

        SetActivation(production);
    }
    void FixedUpdate()
    {
        if(mother) return;

        if(!production)
            internal_rotation_delta-=3;
    }
    void SetActivation(bool want_true)
    {
        Off.SetActive(!want_true);
        On.SetActive(want_true);
        foreach(SC_cog SC_cog in LocalCogs)
        {
            SC_cog.active = want_true;
        }
        foreach(SC_particle_transition SC_particle_transition in LocalParticles)
        {
            SC_particle_transition.active = want_true;
        }
        foreach(Transform trn in transform.parent)
        {
            SC_fobs fob = trn.GetComponent<SC_fobs>();
            if(fob!=null)
            foreach(Transform trn2 in fob.ActivatorObjects)
            {
                //cog
                if(fob.ActivatorType==1)
                    trn2.GetComponent<SC_cog>().SC_factory = this; //only reference, calculations here

                //particle transition
                if(fob.ActivatorType==2)
                    trn2.GetComponent<SC_particle_transition>().active = want_true;

                //gameObject
                if(fob.ActivatorType==3)
                    trn2.gameObject.SetActive(want_true);
            }
        }
    }
}
