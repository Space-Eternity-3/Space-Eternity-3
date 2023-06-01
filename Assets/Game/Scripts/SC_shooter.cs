using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_shooter : MonoBehaviour
{
    public Transform emptyObject;

    public SC_boss SC_boss;
    public GameObject Clothe;
    public int type;
    public int prime_id;
    public string when;
    public bool alive;

    public int steps;
    public Vector3 deltaPosition;
    public Vector3 framePosition;
    public Vector3 normalPosition;
    public Vector3 hiddenPosition;

    public void DeclareAssignment(SC_boss bos, string whe, int prim)
    {
        Transform gob = Instantiate(emptyObject,transform.position,transform.rotation);
        gob.parent = transform.parent;
        transform.parent = gob;

        SC_boss = bos;
        prime_id = prim;
        when = whe;
        normalPosition = transform.localPosition;
        hiddenPosition = normalPosition + deltaPosition;
        framePosition = deltaPosition / steps;

        alive = true;
    }
    void FixedUpdate()
    {
        if(!alive) return;

        int current_step = 0;
        int reduced_steps = SC_boss.dataID19_client-SC_boss.dataID17_client;
        if(reduced_steps>steps) reduced_steps = steps;

        bool before_extended = (when[SC_boss.dataID21_client]=='1');
        bool now_extended = (when[SC_boss.dataID18_client]=='1');

        if(before_extended && now_extended) current_step = 0;
        if(!before_extended && !now_extended) current_step = steps;
        if(before_extended && !now_extended) current_step = reduced_steps;
        if(!before_extended && now_extended) current_step = steps - reduced_steps;

        transform.localPosition = normalPosition + current_step*framePosition;
        if(Clothe!=null) Clothe.SetActive(current_step==0 && SC_boss.dataID18_client!=0 && SC_boss.dataID20_client%prime_id!=0);
    }
}
