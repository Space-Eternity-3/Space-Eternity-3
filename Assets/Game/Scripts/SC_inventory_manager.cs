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
	public SC_resp_blocker SC_resp_blocker;

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
			if(Communtron3.position.y==0f&&Communtron3.position.z==0f&&Communtron2.position.x==0f&&Communtron1.position.y==sNum)
			{
				if(pvNum==20f)
				if(SC_resp_blocker.IsAllowing() && !SC_fun.SC_control.SC_invisibler.invisible && !SC_fun.SC_control.impulse_enabled)
					sImg.texture=SC_fun.Item20u;

				if(!SC_fun.SC_control.impulse_enabled)
				{
					if(pvNum==55f) if(SC_fun.SC_control.HealBalanceGood())
						sImg.texture=SC_fun.Item55u;
					if(pvNum==57f) if(SC_fun.SC_control.turbo_V<0.95f)
						sImg.texture=SC_fun.Item57u;
					if(pvNum==59f) if(SC_fun.SC_control.power_V<0.95f && !SC_fun.SC_control.SC_invisibler.invisible && (SC_fun.SC_control.SC_artefacts.GetArtefactID()==2 || SC_fun.SC_control.SC_artefacts.GetArtefactID()==3))
						sImg.texture=SC_fun.Item59u;
					if(pvNum==61f) if(SC_fun.SC_control.SC_effect.effect!=0 || SC_fun.SC_control.HealBalanceGood())
						sImg.texture=SC_fun.Item61u;
					if(pvNum==63f)
						sImg.texture=SC_fun.Item63u;
				}
			}
		}
	}
}
