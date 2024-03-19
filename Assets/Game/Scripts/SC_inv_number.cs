using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SC_inv_number : MonoBehaviour {

	public Text number;
	public int SlotID;
	Vector3 normal_pos;
	Vector3 hide_pos=new Vector3(0f,10000f,0f);
	public int smallerIN=0;
	public Transform img;
	public Transform Communtron1;
	public Transform Communtron4;
	public float scaleMultiplier=1f;

	public bool only_jump;
	public SC_backpack SC_backpack;
	public SC_data SC_data;
	public SC_slots SC_slots;
	public SC_fun SC_fun;

	void Start()
	{
		if(!only_jump)
		{
			normal_pos=transform.localPosition;
			transform.localPosition=hide_pos;

			if((int)Communtron4.position.y!=100)
			{
				SC_slots.SlotX[SlotID-1] = Parsing.IntE(SC_data.inventory[SlotID-1,0]);
				SC_slots.SlotY[SlotID-1] = Parsing.IntE(SC_data.inventory[SlotID-1,1]);
				SC_slots.SlotYA[SlotID-1] = SC_slots.SlotY[SlotID-1];
				SC_slots.SlotYB[SlotID-1] = SC_slots.SlotY[SlotID-1];
			}
		}
		else
		{
			img=gameObject.GetComponent<Transform>();
		}
	}
	void Update()
	{
		if(!only_jump)
		{
			if(SC_slots.SlotY[SlotID-1]==0) transform.localPosition=hide_pos;
			else transform.localPosition=normal_pos;

			number.text=SC_slots.SlotY[SlotID-1]+"";
		}
	}
	void FixedUpdate()
	{
		if(!only_jump) if(SC_fun.pushed_markers[SlotID-1])
		{
			smallerIN=0;
			SC_fun.pushed_markers[SlotID-1]=false;
		}
		//------------------------------
		if(smallerIN>0&&smallerIN<=3)
		{
			smallerIN--;
			img.localScale=new Vector3(1.15f,1.15f,0f) * scaleMultiplier;
		}
		if(smallerIN>3)
		{
			smallerIN--;
			img.localScale=new Vector3(1.3f,1.3f,0f) * scaleMultiplier;
		}
		if(smallerIN==0)
		{
			img.localScale=new Vector3(1f,1f,0f) * scaleMultiplier;
		}
		if(smallerIN<0&&smallerIN>=-3)
		{
			smallerIN++;
			img.localScale=new Vector3(0.9f,0.9f,0f) * scaleMultiplier;
		}
		if(smallerIN<-3)
		{
			smallerIN++;
			img.localScale=new Vector3(0.8f,0.8f,0f) * scaleMultiplier;
		}
	}
}
