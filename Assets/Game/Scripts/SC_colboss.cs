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
                if(!(bul.is_unstable && SC_boss.type==6) && !(bul.type==15 && SC_boss.type==4))
                {
                    float damage_modifier = 1;
                    SC_boss.DamageSGP(bul.normal_damage*damage_modifier);
                }
                if(bul.type==14)
                {
                    float wind_force = float.Parse(SC_control.SC_data.Gameplay[123]) / 50f;
                    Vector3 force_vector = SC_boss.bossModels.position - bul.transform.position;
                    force_vector = new Vector3(force_vector.x,force_vector.y,0f);
                    float dist = Mathf.Sqrt(force_vector.x*force_vector.x + force_vector.y*force_vector.y);
                    if(dist!=0)
                    {
                        float wx = wind_force * force_vector.x / dist;
                        float wy = wind_force * force_vector.y / dist;
                        SC_boss.AddForceToBoss(wx,wy);
                    }
                }
                if(bul.type==15 && SC_boss.type!=4 && !(SC_boss.type==1 && SC_boss.dataID[18]==2))
                {
                    if(bul.upgrade_boost > SC_boss.dataID[25] || SC_boss.dataID[24]==0)
                        SC_boss.dataID[25] = bul.upgrade_boost;
                    SC_boss.dataID[24] = (int)(float.Parse(SC_control.SC_data.Gameplay[37])+1) * 50;
                }
            }
        }
    }
}
