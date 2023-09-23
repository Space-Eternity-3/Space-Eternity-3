using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SC_craft2 : MonoBehaviour {

	public Button button;
	public Transform dark;
	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron3;
	public Transform Communtron4;
	public SC_control SC_control;
	public SC_fun SC_fun;
	public SC_data SC_data;
	public SC_slots SC_slots;
	public SC_bp_upg SC_bp_upg;
	public SC_craft3 SC_craft3;
	public SC_resp_blocker SC_resp_blocker;
	public Transform respawn_point;
	public Transform player;
	public Transform R_set_particles;
	public Transform R_destroy_particles;
	public int SpaceID;
	
	bool stille = false;
	bool bpkPg = false;
	Vector3 darkDef;

	int Data(int id,int mied)
	{
		return (3*(id-1)+mied);
	}
	bool AllowUse(int id, bool physical)
	{
		if(SC_data.imgID[Data(id,2)]==0) return false;
		int H0=SC_slots.InvHaveM(SC_data.imgID[Data(id,0)],-SC_data.imgCO[Data(id,0)],true,bpkPg,physical,0);
		int H1=SC_slots.InvHaveM(SC_data.imgID[Data(id,1)],-SC_data.imgCO[Data(id,1)],true,bpkPg,physical,0);
		if(SC_data.imgCO[Data(id,1)]==0) H1=1;
		if((H0>0&&H1>0&&SC_slots.InvHaveM(SC_data.imgID[Data(id,2)],2,true,bpkPg,physical,0)==1)||((H0>1||H1>1)&&(H0>0&&H1>0))) return true;
		else return false;
	}
	void Start()
	{
		darkDef=dark.localPosition;

		string[] dGet = SC_data.craftings.Split(';');
		int i,lngt=dGet.Length/2;

		if(SpaceID==1)
		for(i=0;i<lngt&&i<16384;i++)
		{
			SC_data.imgID[i]=int.Parse(dGet[i*2]);
			SC_data.imgCO[i]=int.Parse(dGet[i*2+1]);
		}
	}
	void Update()
	{
		if((int)Communtron2.position.z>=0) button.interactable=AllowUse(7*SC_craft3.selected_page+SpaceID,false);
		else button.interactable=false;
		if(button.interactable) dark.localPosition=new Vector3(10000f,0f,0f);
		else dark.localPosition=darkDef;

		if(SC_bp_upg.state == 1) bpkPg = true;
		else bpkPg = false;

		if(SpaceID==1)
		{
			//RESPAWN SET (strange place)
			if(Input.GetMouseButtonDown(1)&&Communtron3.position.y==0f&&Communtron3.position.z==0f&&Communtron2.position.x==0f&&SC_slots.InvHaving(20))
			if(!SC_control.impulse_enabled && !SC_control.SC_invisibler.invisible && SC_resp_blocker.IsAllowing())
			{
				int slot = SC_slots.InvChange(20,-1,true,false,true);
				if((int)Communtron4.position.y==100)
				{
					SC_control.SendMTP("/SetRespawn "+SC_control.connectionID+" "+slot+" 1 1");
					SC_control.SendMTP("/EmitParticles "+SC_control.connectionID+" 3 "+player.position.x+" "+player.position.y);
				}
				Instantiate(R_set_particles,player.position,player.rotation);
				respawn_point.position=player.position+new Vector3(0f,0f,1f);
			}
			else SC_control.InfoUp("Respawn blocked",380);
		}
	}
	void FixedUpdate()
	{
		if(SpaceID==1)
		{
			//Respawn scared
			float atX=player.position.x;
			float atY=player.position.y;
			if(Mathf.Sqrt(atX*atX+atY*atY)<3f)
			{
				if(!stille)
				{
					stille=true;
					Communtron3.position+=new Vector3(1f,0f,0f);
				}
			}
			else
			{
				if(stille)
				{
					stille=false;
					Communtron3.position-=new Vector3(1f,0f,0f);
				}
			}
		}
	}
	public void Crafted()
	{
		if(!AllowUse(7*SC_craft3.selected_page+SpaceID,true)) return;

		int id=7*SC_craft3.selected_page+SpaceID;
		int sl2;

		int id1 = SC_data.imgID[Data(id,0)];
		int co1 = -SC_data.imgCO[Data(id,0)];
		int sl1 = SC_slots.InvChange(id1,co1,true,bpkPg,true);

		int id2 = SC_data.imgID[Data(id,1)];
		int co2 = -SC_data.imgCO[Data(id,1)];
		if(co2==0) {id2=0; sl2=-1;}
		else sl2 = SC_slots.InvChange(id2,co2,true,bpkPg,true);

		int idE = SC_data.imgID[Data(id,2)];
		int coE = SC_data.imgCO[Data(id,2)];
		int slE = SC_slots.InvChange(idE,coE,true,bpkPg,true); 
		
		if((int)Communtron4.position.y==100)
			SC_control.SendMTP("/Crafting "+SC_control.connectionID+" "+(id-1)+" "+sl1+" "+sl2+" "+slE);
	}
	public void ResetSpawn()
	{
		if(SC_slots.InvHaveB(10,1,true,true,true,1))
		{
			int slot = SC_slots.InvChange(10,3,true,true,true);
			if((int)Communtron4.position.y==100)
			{
				SC_control.SendMTP("/SetRespawn "+SC_control.connectionID+" "+slot+" 0 0");
			}
			respawn_point.position=new Vector3(0f,0f,1f);
		}
	}
}
