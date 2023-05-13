using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_effect : MonoBehaviour
{
    public int cycle_period;
    
    public int effect = 0;
    public int damage_cycle_timer = 0;

    public SC_seeking eS;

    public SC_fun SC_fun;
    public SC_control SC_control;

    public void SetEffect(int type, int cycles)
    {
        if(cycles==0) return;
        if(type==10) type = 6;
        if(type==15) type = 5;
        if(effect==8 && type!=8) return;
        effect = type;
        int candidate = cycles * cycle_period + 49;
        if(candidate > damage_cycle_timer) damage_cycle_timer = candidate;
    }
    public void EffectClean()
    {
        effect = 0;
        damage_cycle_timer = 0;
    }
    public float GetSpeedMultiplier()
    {
        if(effect==8) return 0.2f;
        else if(effect==6) return 0.8f;
        else return 1f;
    }
    public void OneFrameDamage()
    {
        if(damage_cycle_timer>0)
            SC_control.DamageFLOAT((damage_cycle_timer/cycle_period)*SC_fun.boss_damages_cyclic[effect]*float.Parse(SC_fun.SC_data.Gameplay[36]));
        damage_cycle_timer = 0;
    }
    void FixedUpdate()
    {
        if(damage_cycle_timer<=0)
        {
            damage_cycle_timer = 0;
            effect = 0;
        }
        else
        {
            if(damage_cycle_timer % cycle_period == 0)
            {
                SC_control.DamageFLOAT(SC_fun.boss_damages_cyclic[effect]*float.Parse(SC_fun.SC_data.Gameplay[36]));
            }
            damage_cycle_timer--;
        }

        eS.offset = new Vector3(0f,0f,-450f*effect);
    }
}
