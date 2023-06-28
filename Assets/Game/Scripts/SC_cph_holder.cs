using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_cph_holder : MonoBehaviour
{
    public Transform boss_model;
    public Transform effect;
    public int delay;
    public int required_state;
    public bool parenting;
    public int cooldown = 50;
    public SC_boss SC_boss;

    void FixedUpdate()
    {
        if(cooldown>0) cooldown--;
        else
        {
            int state_timer = SC_boss.dataID[19] - SC_boss.dataID[17];
            int current_state = 5*SC_boss.type + SC_boss.dataID[18];
            if(state_timer >= delay && current_state == required_state)
            {
                cooldown = 50;
                Transform trn = Instantiate(effect,boss_model.position,Quaternion.identity);
                if(parenting) trn.parent = boss_model;
            }
        }
    }
}
