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
	public Material type2;
	public Renderer bulletRE;
	bool mother=true;

	public int type;
	public int mode;
	public float mX,mY;
	public int id;
	public Vector3 velo;

	void Start()
	{
		if((int)Communtron4.position.y==100) multiplayer=true;
		if(transform.position.z<100f) mother=false;
		if(!mother)
		{
			bulletR.velocity=new Vector3(mX*F/Mathf.Sqrt(mX*mX+mY*mY),mY*F/Mathf.Sqrt(mX*mX+mY*mY),0f)+velo;
			if(type==2) bulletRE.material=type2;
		}
		starter=bulletR.velocity;
	}
	void FixedUpdate()
	{
		if(transform.position.z<100f)
		if(counter==Lifetime) Destroy(gameObject);
		else counter++;
		if(counter>=Jednotime)
		bulletR.velocity-=drag*starter;
		if(Mathf.Round(bulletR.velocity.x*10)/10==0f&&Mathf.Round(bulletR.velocity.y*10)/10==0f&&transform.position.z<100f) Destroy(gameObject);
	}
	public void cmdDoo(string eData)
	{
		if(eData.Split(' ')[0]=="/RetBulletDestroy"&&int.Parse(eData.Split(' ')[1])==id&&!mother)
			Destroy(gameObject);
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
		neme!="cactus_kill_zone"&&
		!mother&&
		collision.gameObject.transform.root.gameObject.name[0]!='P')
		{
			switch(mode)
			{
				case 0: //main
					if(neme!="pseudoBody")
					{
						if(multiplayer) SC_control.SendMTP("/BulletDestroy "+id);
						Destroy(gameObject);
					}
					break;
				case 1: //projection
					if(neme=="Asteroid(Clone)")
					{
						Destroy(gameObject);
					}
					break;
			}
		}
	}
}
