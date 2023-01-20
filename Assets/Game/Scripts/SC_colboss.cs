using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_colboss : MonoBehaviour
{
    SphereCollider collid;
    public SC_control SC_control;
    public SC_boss SC_boss;
    public bool impulse_used = false;

    void Start()
    {
        collid = gameObject.GetComponent<SphereCollider>();
    }
    void FixedUpdate()
    {
        if(SC_control.impulse_enabled) collid.enabled = false;
        else collid.enabled = true;
    }
    void OnTriggerStay(Collider collision)
    {
        if(collision.gameObject.name=="impulseBody" && SC_control.impulse_enabled && !impulse_used && !SC_boss.multiplayer)
        {
            impulse_used = true;
            SC_boss.DamageSGP(float.Parse(SC_control.SC_data.Gameplay[29]));
        }
    }
    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.name=="Bullet(Clone)" && !SC_boss.multiplayer)
        {
            SC_bullet bul = collision.gameObject.GetComponent<SC_bullet>();
            if(bul.controller && !bul.boss_damaged && bul.gun_owner==0)
            {
                bul.boss_damaged = true;
                if(!bul.is_unstable || SC_boss.type!=6) SC_boss.DamageSGP(bul.normal_damage);
            }
        }
    }
}
