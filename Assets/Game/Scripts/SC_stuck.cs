using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_stuck : MonoBehaviour
{
    public SC_control SC_control;
    public SC_asteroid SC_asteroid;
    public SphereCollider SphereCollider;
    bool in_collider = false;
    public int counter_to_teleport = 150;
    
    Vector3 EscapePosition()
    {
        Vector3 deltapos = SC_control.transform.position - transform.position;
        if(SC_control.Pitagoras(deltapos)==0f) deltapos += new Vector3(1f,0f,0f);
        Vector3 ret = transform.position + SC_control.Skop(SC_asteroid.transform.localScale.x/2f+3f,deltapos);
        return ret - new Vector3(0f,0f,ret.z);
    }
    void OnTriggerStay(Collider c)
    {
        if(c.gameObject.name=="pseudoBody" && SphereCollider.enabled)
            in_collider = true;
    }
    void FixedUpdate()
    {
        if(!in_collider) counter_to_teleport = 150;
        else counter_to_teleport--;
        
        if(counter_to_teleport==0)
        {
            counter_to_teleport = 150;
            SC_control.transform.position = EscapePosition();
            SC_control.RemoveImpulse();
        }

        in_collider = false;
    }
}
