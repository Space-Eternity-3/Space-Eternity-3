using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_impulse_action : MonoBehaviour
{
    public Transform player;
    public SC_control SC_control;

    void OnTriggerEnter(Collider collision)
    {
        if(transform.parent.parent.parent.GetComponent<SC_seeking>().seek!=player)
        if(collision.gameObject.name=="pseudoBody")
        {
            //SC_control.DamageINT(5);
        }
    }
}
