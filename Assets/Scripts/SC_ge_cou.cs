using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_ge_cou : MonoBehaviour
{
    public SC_fobs SC_fobs;

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.name=="wind_active") SC_fobs.inGeyzer++;
    }
    void OnTriggerExit(Collider collision)
    {
        if(collision.gameObject.name=="wind_active")
        {
            SC_fobs.inGeyzer--;
            if(SC_fobs.inGeyzer==0) SC_fobs.GeyzerTime=0;
        }
    }
}
