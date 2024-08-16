using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_ge_cou : MonoBehaviour
{
    public SC_fobs SC_fobs;
    public string activer1, activer2;

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.name==activer1 || collision.gameObject.name==activer2) SC_fobs.inGeyzer++;
    }
    void OnTriggerExit(Collider collision)
    {
        if(collision.gameObject.name==activer1 || collision.gameObject.name==activer2)
        {
            SC_fobs.inGeyzer--;
            if(SC_fobs.inGeyzer==0) SC_fobs.GeyzerTime=0;
        }
    }
}
