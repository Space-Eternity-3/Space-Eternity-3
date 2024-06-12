using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_factory : MonoBehaviour
{
    public GameObject Off, On;
    public SC_cog[] LocalCogs;
    public bool production;
    public bool mother;

    void Start()
    {
        mother = transform.parent==null;

        //temporary
        int rand = UnityEngine.Random.Range(0,2);
        if(rand==0)
            production = true;
    }
    void Update()
    {
        if(mother) return;

        SetActivation(production);
    }
    void SetActivation(bool want_true)
    {
        Off.SetActive(!want_true);
        On.SetActive(want_true);
        foreach(SC_cog SC_cog in LocalCogs)
        {
            SC_cog.active = want_true;
        }
        foreach(Transform trn in transform.parent)
        {
            SC_fobs fob = trn.GetComponent<SC_fobs>();
            if(fob!=null)
            foreach(Transform trn2 in fob.ActivatorObjects)
            {
                //cog
                if(fob.ActivatorType==1)
                    trn2.GetComponent<SC_cog>().active = want_true;

                //gameObject
                if(fob.ActivatorType==2)
                    trn2.gameObject.SetActive(want_true);
            }
        }
    }
}
