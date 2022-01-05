using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SC_Fob21 : MonoBehaviour {

	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron3;
	public Transform Communtron4;
	public Transform player;
	public Material norm, high;
	public Renderer Ention;
	public Material Off,On;
	public Renderer[] oSim = new Renderer[35];

	public SC_control SC_control;
	public SC_fun SC_fun;
	public SC_data SC_data;
	public SC_slots SC_slots;
	public SC_fobs SC_fobs;
	public int max_count;
	
	int ID=0,uID=0,X,Y;
	public int ASAC_cooldown;
	public int item=0;
	public int count=0;
	public int pub_item=0;
	public int pub_count=0;
	string neme;
	bool naceled=false;

	bool pressed_0 = false;
	bool pressed_1 = false;

	int using_0 = 0;
	int using_1 = 0;

	string worldDIR="";
	int worldID=1;

	void CountTranslate()
	{
		try{
			Ention.material=SC_fun.M[item];
		}catch{Ention.material=SC_fun.M[23];}
		int i,lngt;

		for(i=0;i<max_count;i++){
			if(count>i) oSim[i].material=On;
			else oSim[i].material=Off;
		}
	}
	void Update()
	{
		CountTranslate();
	}
	void FixedUpdate()
	{
		if(transform.position.z>100f) return;
		if(count<=0) item=0;
		pub_item=item;
		pub_count=count;
		if(SC_control.livTime%2==0) ClickUpdate(-1);
	}
	void ClickUpdate(int inst)
	{
		if(pressed_0) using_0++;
		else using_0 = 0;
		
		if(pressed_1) using_1++;
		else using_1 = 0;

		int slot;

		if(using_0>=ASAC_cooldown || using_0==1 || inst==0)
		{
			if(inst==0) using_0 = 1;

			if(Req(0) && Input.GetMouseButton(0))
			if(SC_slots.InvHaveB(item,1,true,true,true,0))
			{
				slot = SC_slots.InvChange(item,1,true,true,false);
				count--;

				if((int)Communtron4.position.y!=100)
				{
					if(count==0) item=0;
					SaveSGP();
				}
				else
				{
					SC_control.SendMTP("/FobsDataChange "+SC_control.connectionID+" "+ID+" "+uID+" "+item+" -1 "+slot+" "+SC_fobs.ObjID);
					SC_control.InvisiblityPulseSend("none");
					if(count==0) item=0;
				}
			}
		}
		if((using_1>=ASAC_cooldown || using_1==1 || inst==1) && SC_fobs.ObjID!=2)
		{
			if(inst==1) using_1 = 1;

			if(Req(1) && Input.GetMouseButton(1))
			{
				int itemTym=SC_slots.SelectedItem();

				slot = SC_slots.InvChange(itemTym,-1,true,false,false);
				count++;
				item=itemTym;

				if((int)Communtron4.position.y!=100) SaveSGP();
				else
				{
					SC_control.SendMTP("/FobsDataChange "+SC_control.connectionID+" "+ID+" "+uID+" "+itemTym+" 1 "+slot+" "+SC_fobs.ObjID);
					SC_control.InvisiblityPulseSend("none");
				}
			}
		}
	}
	bool InDistance(float dist)
	{
		float dX=player.position.x-transform.position.x;
		float dY=player.position.y-transform.position.y;
		if(Mathf.Sqrt(dX*dX+dY*dY)<dist) return true;
		else return false;
	}
	void SaveSGP()
	{
		string[] uAst = SC_data.GetAsteroid(X,Y).Split(';');
        int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]);
		SC_data.World[a,21+uID*2,c]=item+"";
		SC_data.World[a,22+uID*2,c]=count+"";
		if(uAst[2]=="T") SC_data.SaveAsteroid(c);
	}
	bool Req(int mode)
	{
		if((int)Communtron1.position.z==0&&(int)Communtron3.position.y==0&&InDistance(15f)&&Communtron3.position.y==0)
		if(mode==0)
		{
			if(count>0)
			return true;
		}
		else if(mode==1)
		{
			int itemTym=SC_slots.SelectedItem();
			if((itemTym==item||item==0)&&itemTym!=0)
			if(count<max_count)
			return true;
		}
		return false;
	}
	void OnMouseExit()
	{
		pressed_0 = false;
		pressed_1 = false;
	}
	void OnMouseOver()
	{
		if(Req(0))
		{
			if(Input.GetMouseButtonDown(0))
			{
				if(SC_slots.InvHaveB(item,1,true,true,true,1))
				{
					pressed_0 = true;
					ClickUpdate(0);
				}
			}
			else if(!Input.GetMouseButton(0)) pressed_0 = false;
		}
		else pressed_0 = false;
		
		if(Req(1))
		{
			if(Input.GetMouseButtonDown(1))
			{
				pressed_1 = true;
				ClickUpdate(1);
			}
			else if(!Input.GetMouseButton(1)) pressed_1 = false;
		}
		else pressed_1 = false;
	}
	void Start()
	{
		worldID=(int)Communtron4.position.y;
		worldDIR="../../saves/UniverseData"+worldID+"/WorldData/";

		X=(int)Mathf.Round(transform.position.x/10f);
		Y=(int)Mathf.Round(transform.position.y/10f);
		ID=SC_fun.CheckID(X,Y);
		uID=(int)Mathf.Round(transform.position.z*100000f)-1;
		
		//NBT read
		if(transform.position.z<100f)
		if((int)Communtron4.position.y!=100)
		{
			//singleplayer load
			string[] uAst = SC_data.GetAsteroid(X,Y).Split(';');
            int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]);
			if(SC_data.World[a,21+uID*2,c]!="")
			{
				item=int.Parse(SC_data.World[a,21+uID*2,c]);
				count=int.Parse(SC_data.World[a,22+uID*2,c]);
			}
		}
		else
		{
			//multiplayer load
			item=int.Parse(gameObject.name.Split(';')[0]);
			count=int.Parse(gameObject.name.Split(';')[1]);
		}
		CountTranslate();
		pub_item=item;
		pub_count=count;
	}
	public void Fob2Drilled(int id)
	{
		if((int)Communtron4.position.y!=100)
		if((count==0 || item==id) && count<5)
		{
			item=id;
			count++;
		}
		SaveSGP();
	}
	void OnDestroy()
	{
		try{
			if(Mathf.Abs(player.position.x-transform.position.x)<=25f&&Mathf.Abs(player.position.y-transform.position.y)<=25f)
			{
				Communtron3.position-=new Vector3(0f,0f,1f);
			}
		}catch(Exception e){}
	}
}
