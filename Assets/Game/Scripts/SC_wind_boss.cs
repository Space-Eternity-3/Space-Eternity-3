using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_wind_boss : MonoBehaviour
{
    public float force;
    public Transform center;
    public Transform player;
    public Rigidbody playerR;
    public SC_boss SC_boss;

    void OnTriggerStay(Collider collision)
    {
        //Like FixedUpdate
        if(collision.gameObject.name=="pseudoBody" && SC_boss.InArena("range"))
        {
            Vector3 unm = player.position - center.position;
            unm -= new Vector3(0f,0f,unm.z);
            playerR.velocity += 0.02f * force * Vector3.Normalize(unm);
        }
    }
}
