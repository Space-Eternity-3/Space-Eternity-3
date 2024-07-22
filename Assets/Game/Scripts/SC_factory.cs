using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_factory : MonoBehaviour
{
    public GameObject Off, On;
    public SC_cog[] LocalCogs;
    public SC_particle_transition[] LocalParticles;
    public SC_pulse SC_pulse; //locally set
    public Transform UpgradeParticles;
    public float BaseSimulationSpeed = 2.5f;
    public int internal_rotation_delta = 0;
    public bool mother;

    bool production = false;
    int diamonds_before = -1;

    public SC_data SC_data;

    void Start()
    {
        mother = transform.parent==null;

        foreach(SC_cog SC_cog in LocalCogs)
        {
            SC_cog.configuration = UnityEngine.Random.Range(1,3);
        }
    }
    void Update()
    {
        if(mother) return;

        production = false;
        foreach(Transform trn in transform.parent)
        {
            SC_fobs fob = trn.GetComponent<SC_fobs>();
            if(fob!=null)
            {
                if(fob.ObjID==81)
                    if(fob.GetComponent<SC_tbase>().nbt1 != 5)
                    {
                        production = true;
                        break;
                    }
            }
        }
        SetActivation(production);

        int diamonds_now = CountVisualDiamonds();
        if(diamonds_before != -1 && diamonds_before < diamonds_now) {
            //Transform prt = Instantiate(UpgradeParticles,transform.position,Quaternion.identity);
            //prt.SetParent(transform,true);
            SC_pulse.MakePulse();
        }
        diamonds_before = diamonds_now;
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
            var ps = SC_particle_transition.GetComponent<ParticleSystem>().main;
            ps.simulationSpeed = BaseSimulationSpeed * Mathf.Pow(Parsing.FloatU(SC_data.Gameplay[131]),CountVisualDiamonds());
        }
        foreach(Transform trn in transform.parent)
        {
            SC_fobs fob = trn.GetComponent<SC_fobs>();
            if(fob!=null)
            {
                bool local_want;
                bool fob82_blocked = false;
                if(fob.ObjID==82) {
                    if(fob.GetComponent<SC_dbase>().nbt1==0) fob82_blocked = true;
                }
                local_want = want_true && !fob82_blocked;

                foreach(Transform trn2 in fob.ActivatorObjects)
                {
                    //cog
                    if(fob.ActivatorType==1)
                        trn2.GetComponent<SC_cog>().SC_factory = this; //only reference, calculations in this script

                    //particle transition
                    if(fob.ActivatorType==2)
                        trn2.GetComponent<SC_particle_transition>().active = local_want;

                    //gameObject
                    if(fob.ActivatorType==3)
                        trn2.gameObject.SetActive(local_want);
                }
            }
        }
    }
    int CountVisualDiamonds()
    {
        int counted = 0;
        foreach(Transform trn in transform.parent)
        {
            SC_fobs fob = trn.GetComponent<SC_fobs>();
            if(fob!=null)
            {
                if(fob.ObjID==82 && trn.GetComponent<SC_dbase>().nbt1==1)
                    counted++;
            }
        }
        return counted;
    }
}
