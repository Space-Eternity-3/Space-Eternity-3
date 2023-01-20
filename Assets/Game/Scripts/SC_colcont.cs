using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_colcont : MonoBehaviour
{
    public SC_control SC_control;

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.name=="Bullet(Clone)" && (int)SC_control.Communtron4.position.y!=100)
        {
            SC_bullet bul = collision.gameObject.GetComponent<SC_bullet>();
            if(bul.controller && !bul.boss_damaged && bul.gun_owner!=0)
            {
                bul.boss_damaged = true;
                if(!bul.is_unstable || SC_control.SC_artefacts.GetArtefactID()!=6) SC_control.DamageFLOAT(bul.normal_damage);
                if(!bul.is_unstable)
                {
                    bul.block_graphics = true;
                    bul.MakeDestroy(false);
                }
            }
        }
    }
}
