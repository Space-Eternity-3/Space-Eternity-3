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
	public SC_artefacts SC_artefacts;
	public SC_backT SC2_backT; //partner button
    public Transform Communtron2;
    public int index;
	public bool special;
	public bool delta_accept = false;
	Vector3 delta_pos = new Vector3(0f,27.5f,0f);
	float startY1 = -123.75f; //up pos
	float startY2 = -151.25f; //down pos
    
    public Vector3 startPos = new Vector3(0f,0f,0f);
    
    void Start()
    {
        if(!special) startPos = transform.localPosition;
		else startPos = transform.localPosition;
    }
    void LateUpdate()
    {
		if(special)
		{
			if(SC_slots.SlotY[index-1]==0 || !SC_slots.InvHaveB(SC_slots.SlotX[index-1],1,false,true,false,0)) startPos = new Vector3(startPos.x,startY2,startPos.z);
			else startPos = new Vector3(startPos.x,startY1,startPos.z);
			if(SC_slots.SlotY[index-1]==0||SC_slots.BackpackY[15]!=0||!SC_artefacts.IsArtefact(SC_slots.SlotX[index-1])||
			!SC_inv_mover.active||SC_bp_upg.state!=1) transform.localPosition = new Vector3(10000f,0f,0f);
			else transform.localPosition=startPos;
			SC2_backT.LaterUpdate();
		}
    }
	public void LaterUpdate()
	{
		//!special
		
		if(SC_slots.SlotY[index-1]!=0 && SC_inv_mover.active &&
		((SC_slots.InvHaveB(SC_slots.SlotX[index-1],1,false,true,false,0) && SC_bp_upg.state==1) ||
		(SC_bp_upg.state==2 && (SC_slots.BackpackY[16]==0 || SC_slots.BackpackX[16]==SC_slots.SlotX[index-1]))))
		{
			if(!delta_accept) transform.localPosition = startPos;
			else transform.localPosition = startPos + delta_pos;
		}
		else transform.localPosition = new Vector3(10000f,0f,0f);
	}
}
