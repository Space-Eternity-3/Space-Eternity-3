using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_colcont : MonoBehaviour
{
    public SC_control SC_control;
    public SC_effect SC_effect;
    public SC_fun SC_fun;

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.name=="Bullet(Clone)" && (int)SC_control.Communtron4.position.y!=100)
        {
            SC_bullet bul = collision.gameObject.GetComponent<SC_bullet>();
            if(bul.controller && !bul.boss_damaged && bul.gun_owner!=0)
            {
                bul.boss_damaged = true;
                int effector_time = SC_fun.bullet_effector[bul.type];
                if(effector_time!=0) SC_effect.SetEffect(bul.type,effector_time);
                if(!bul.is_unstable || SC_control.SC_artefacts.GetArtefactID()!=6) SC_control.DamageFLOAT(bul.normal_damage);
                if(!SC_control.SC_fun.bullet_air_consistence[bul.type])
                {
                    bul.block_graphics = true;
                    bul.MakeDestroy(false);
                }
            }
        }
    }
}
