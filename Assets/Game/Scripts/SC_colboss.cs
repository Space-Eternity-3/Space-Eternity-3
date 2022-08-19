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
            if(bul.controller)
            {
                int mdo = bul.type;
                float base_damage=0f;
                if(mdo==1) base_damage = float.Parse(SC_control.SC_data.Gameplay[3]);
                if(mdo==2) base_damage = float.Parse(SC_control.SC_data.Gameplay[27]);
                if(mdo==3) base_damage = float.Parse(SC_control.SC_data.Gameplay[28]);
                if(mdo!=3) base_damage *= Mathf.Pow(1.08f,SC_control.SC_upgrades.MTP_levels[3]);
                SC_boss.DamageSGP(base_damage);
            }
        }
    }
}
