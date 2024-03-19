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
        int candidate = cycles * cycle_period + 49;
        if(candidate > damage_cycle_timer || effect != type) damage_cycle_timer = candidate;
        effect = type;
    }
    public void EffectClean()
    {
        effect = 0;
        damage_cycle_timer = 0;
    }
    public float GetSpeedMultiplier()
    {
        if(effect==8) return 0.2f;
        else return 1f;
    }
    public float GetSpeedFMultiplier()
    {
        if(effect==8) return 0.12f;
        else return 1f;
    }
    public float GetVacuumMultiplier()
    {
        if(effect==8) return 2f;
        else return 1f;
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
                SC_control.DamageFLOAT(SC_fun.boss_damages_cyclic[effect]*Parsing.FloatE(SC_fun.SC_data.Gameplay[36])*SC_fun.difficulty);
            }
            damage_cycle_timer--;
        }

        eS.offset = new Vector3(0f,0f,-450f*effect);
    }
}
