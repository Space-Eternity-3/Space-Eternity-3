using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_effect : MonoBehaviour
{
    public int cycle_period;
    
    public int effect = 0;
    int damage_cycle_timer = 0;

    public SC_seeking eS;

    public SC_fun SC_fun;
    public SC_control SC_control;

    public void SetEffect(int type, int cycles)
    {
        if(effect==8 && type!=8) return;
        effect = type;
        damage_cycle_timer = cycles * cycle_period + 49;
    }
    public void Remove()
    {
        damage_cycle_timer = 0;
    }
    public void OneFrameDamage()
    {
        if(damage_cycle_timer>0)
            SC_control.DamageFLOAT((damage_cycle_timer/cycle_period)*SC_fun.boss_damages_cyclic[effect]);
        damage_cycle_timer = 0;
    }
    void FixedUpdate()
    {
        if(damage_cycle_timer==0) effect = 0;
        else
        {
            if(damage_cycle_timer % cycle_period == 0)
            {
                SC_control.DamageFLOAT(SC_fun.boss_damages_cyclic[effect]);
            }
            damage_cycle_timer--;
        }

        eS.offset = new Vector3(0f,0f,-450f*effect);
    }
}
