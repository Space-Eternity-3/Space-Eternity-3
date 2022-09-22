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
                int mdo = bul.type;
                float base_damage=0f;
                if(mdo==1) base_damage = float.Parse(SC_control.SC_data.Gameplay[3]);
                if(mdo==2) base_damage = float.Parse(SC_control.SC_data.Gameplay[27]);
                if(mdo==3) base_damage = float.Parse(SC_control.SC_data.Gameplay[28]);
                if(mdo!=3 || SC_control.SC_artefacts.GetArtefactID()!=6) SC_control.DamageFLOAT(base_damage);
                bul.block_graphics = true;
                bul.MakeDestroy(false);
            }
        }
    }
}
