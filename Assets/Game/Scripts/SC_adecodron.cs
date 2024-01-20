using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_adecodron : MonoBehaviour
{
    public int cooldown = 0;
    public float push_force;
    public float enter_damage;
    public Transform player;
    public SC_control SC_control;
    public SC_boss SC_boss;
    public bool detached;

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.name=="pseudoBody" && (detached || SC_boss.dataID[2]==2))
        {
            Vector3 cent = transform.position;
            Vector3 plar = player.position;
            Vector3 force = new Vector3(0f,0f,0f);
            if(plar!=cent) force = Vector3.Normalize(plar-cent) * push_force;
            SC_control.playerR.velocity = force;
            if(cooldown<25) SC_control.DamageFLOAT(enter_damage * float.Parse(SC_control.SC_data.Gameplay[32]) * SC_control.SC_fun.difficulty);
            cooldown = 50;
        }
    }
    void FixedUpdate()
    {
        if(cooldown > 0) cooldown--;
    }
}
