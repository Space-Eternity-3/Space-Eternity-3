using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_backT : MonoBehaviour
{
    public SC_fun SC_fun;
    public SC_inv_mover SC_inv_mover;
    public SC_backpack SC_backpack;
    public SC_bp_upg SC_bp_upg;
    public SC_slots SC_slots;
    public Transform Communtron2;
    public int index;
    
    public Vector3 startPos = new Vector3(0f,0f,0f);
    
    void Start()
    {
        startPos=transform.localPosition;
    }
    void LateUpdate()
    {
        if(SC_slots.SlotY[index-1]==0||!SC_slots.InvHaveB(SC_slots.SlotX[index-1],1,false,true,false,0)||
        !SC_inv_mover.active||SC_bp_upg.state!=1) transform.localPosition = new Vector3(10000f,0f,0f);
        else transform.localPosition=startPos;
    }
}
