using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SC_inventory_manager : MonoBehaviour {

	public RawImage sImg; //Slot Image
	public int sNum; //Slot Number

	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron3;
	public Transform Communtron4;
	public Transform NumParent;
	public Text NumValue;
	Vector3 NumParDef;
	public SC_fun SC_fun;
	public SC_backpack SC_backpack;
	public SC_slots SC_slots;
	public SC_data SC_data;
	public SC_craft3 SC_craft3;

	int pvNum=0;

	void Start()
	{
		if(sNum>9) NumParDef=NumParent.localPosition;
	}
	void Update()
	{
		if(sNum<=9)
		{
			if(SC_slots.SlotY[sNum-1]==0) pvNum=0;
			else pvNum=(int)Mathf.Round(SC_slots.SlotX[sNum-1]);
		}

		if(sNum>9&&sNum<100&&(int)Communtron2.position.z>=0)
		{
			if(sNum<=50)
			{
				pvNum=SC_data.imgID[sNum-16+(21*SC_craft3.selected_page)];
				NumValue.text=""+SC_data.imgCO[sNum-16+(21*SC_craft3.selected_page)];
			}
			else
			{
				pvNum=SC_data.imgID[18+(21*((int)Mathf.Round(Communtron2.position.z)*6+sNum-51))];
				NumValue.text="1";
			}
		}

		if(sNum>100)
		{
			pvNum=SC_slots.BackpackX[sNum-101];
			NumValue.text=""+SC_slots.BackpackY[sNum-101];
		}

		if(sNum>9)
		{
			if(NumValue.text=="0")
			{
				pvNum=0;
				NumParent.localPosition=new Vector3(10000f,0f,0f);
			}
			else NumParent.localPosition=NumParDef;
		}

		try{sImg.texture=SC_fun.Item[(int)pvNum];}catch(Exception e){sImg.texture=SC_fun.Item[0];}

		if(gameObject.name!="ItemImageO"&&gameObject.name!="ItemImageB"&&gameObject.name!="ItemImageOU")
		{
			if(pvNum==20f)
			if(Communtron3.position.x==0f&&Communtron3.position.y==0f&&Communtron3.position.z==0f&&Communtron2.position.x==0f&&Communtron1.position.y==sNum&&!SC_fun.SC_control.SC_invisibler.invisible)
			{
				sImg.texture=SC_fun.Item20u;
			}
		}
	}
}
