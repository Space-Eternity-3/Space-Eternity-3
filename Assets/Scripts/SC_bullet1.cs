using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_bullet1 : MonoBehaviour {

	public float F;
	public Rigidbody playerR;
	public Rigidbody bulletR;
	public int Lifetime;
	public int Jednotime;
	public float drag;
	int counter;
	Vector3 starter;
	bool multiplayer=false;
	public SC_control SC_control;
	public Transform Communtron4;
	public Material type2, type3;
	public Renderer bulletRE;
	public Transform type3effect, unstableExpl;
	public bool turn_used = false;
	bool mother=true;

	public int type;
	public int mode;
	public float mX,mY;
	public int id;
	public Vector3 velo;
	
	public SC_snd_loop SC_snd_loop;
	public SC_sounds SC_sounds;
	int loopSndID = -1;

	void Start()
	{
		if((int)Communtron4.position.y==100) multiplayer=true;
		if(transform.position.z<100f) mother=false;
		if(!mother)
		{
			bulletR.velocity=new Vector3(mX*F/Mathf.Sqrt(mX*mX+mY*mY),mY*F/Mathf.Sqrt(mX*mX+mY*mY),0f)+velo;
			if(type==2) bulletRE.material=type2;
			if(type==3)
			{
				bulletRE.material=type3;
				Transform trn = Instantiate(type3effect,transform.position,transform.rotation);
				trn.parent = transform;
			}
			
			if(type!=3)
			{
				SC_sounds.PlaySound(transform.position,2,1);
			}
			else
			{
				SC_sounds.PlaySound(transform.position,2,17);
			}
		}
		starter = bulletR.velocity;
		
		if(!mother && type==3) loopSndID = SC_snd_loop.AddToLoop(2,transform.position);
	}
	void FixedUpdate()
	{
		if(transform.position.z<100f)
		if(counter==Lifetime) Make2Destroy(gameObject);
		else counter++;
		
		if(loopSndID!=-1) SC_snd_loop.sound_pos[loopSndID] = transform.position;
		
		if(counter>=Jednotime)
		bulletR.velocity-=drag*starter;
		if(Mathf.Round(bulletR.velocity.x*10)/10==0f&&Mathf.Round(bulletR.velocity.y*10)/10==0f&&transform.position.z<100f) Make2Destroy(gameObject);
	}
	public void MakeDestroy(string neme)
	{
		switch(mode)
		{
			case 0: //main
				if(neme!="pseudoBody")
				{
					if(multiplayer) SC_control.SendMTP("/BulletDestroy "+id);
					Make2Destroy(gameObject);
				}
				break;
			case 1: //projection
				if(neme=="Asteroid(Clone)")
				{
					Make2Destroy(gameObject);
				}
				break;
			}
	}
	void Make2Destroy(GameObject gob)
	{
		if(loopSndID!=-1)
		{
			SC_snd_loop.RemoveFromLoop(loopSndID);
			loopSndID = -1;
		}
		if(type==3 && !turn_used)
		{
			SC_sounds.PlaySound(transform.position,2,15);
			Instantiate(unstableExpl,transform.position,new Quaternion(0f,0f,0f,0f));
		}
		Destroy(gob);
	}
	public void cmdDoo(string eData)
	{
		if(eData.Split(' ')[0]=="/RetBulletDestroy"&&int.Parse(eData.Split(' ')[1])==id&&!mother)
			Make2Destroy(gameObject);
	}
	public void serverExitDestroy()
	{
		if(!mother&&multiplayer)
		SC_control.SendMTP("/BulletDestroy "+id);
	}
	void OnTriggerEnter(Collider collision)
	{
		string neme=collision.gameObject.name;
		if(neme!="Body"&&
		neme!="Body_back"&&
		neme!="Engine"&&
		neme!="Drill"&&
		neme!="Drill2"&&
		neme!="Drill3"&&
		neme!="Drill4"&&
		neme[0]!='S'&&
		neme!="Asteroid_pre(Clone)"&&
		neme!="FOB0_Empty(Clone)"&&
		neme!="Fob12(Clone)"&&
		neme!="Fob14(Clone)"&&
		neme!="Engine1"&&
		neme!="Engine2"&&
		neme!="Engine1 (1)"&&
		neme!="Engine2 (1)"&&
		neme!="Bullet1(Clone)"&&
		neme!="wind_active"&&
		neme!="Fob48(Clone)"&&
		neme!="cactus_kill_zone"&&
		neme!="Fob41(Clone)"&&
		neme!="Fob42(Clone)"&&
		neme!="Fob43(Clone)"&&
		neme!="Fob44(Clone)"&&
		neme!="Fob45(Clone)"&&
		neme!="Fob46(Clone)"&&
		neme!="Fob47(Clone)"&&
		neme!="in0collider"&&
		neme!="Fob49 (bedrock carpet)(Clone)"&&
		neme!="respawn_blocker"&&
		!mother&&
		collision.gameObject.transform.root.gameObject.name[0]!='P')
		{
			counter=Lifetime;
		}
	}
}
