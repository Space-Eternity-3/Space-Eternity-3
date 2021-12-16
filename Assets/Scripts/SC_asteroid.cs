using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;

public class SC_asteroid : MonoBehaviour {

	public Transform legs;
	public Transform rFuelParticles;
	public Transform rHydrogenParticles;
	public Transform Drilled_effect;
	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron3;
	public Transform Communtron4;
	public Transform CommuntronM1;
	public Light fela_lune;
	public Transform player;
	public Image FuelBar;
	public Renderer asteroidR;
	public int asteroid_lim = 16;
	public Material[] texture = new Material[16];
	public Material[] textureStone;
	public Material textureERROR;
	public GameObject[] GenPlaceT = new GameObject[128];
	public GameObject[] GenPlaceM = new GameObject[18];
	public SC_control SC_control;
	public SC_fun SC_fun;
	public SC_upgrades SC_upgrades;
	public SC_data SC_data;
	public SC_snd_start SC_snd_start;
	public SC_slots SC_slots;

	int type=0;
	public int ID=0,X=0,Y=0;
	int[] objectID = new int[32];
	string[] objectData = new string[32];
	string[] objectGrow = new string[32];

	bool Mining=false;
	bool AD_FuelRich=false;
	float AD_fuelAmount=0.00065f;
	int AD_particleID=0;
	int AD_loottableID=0;
	int counter=-1;
	int suze;
	bool stille=false;
	bool UUTCed=false;
	float saze;
	bool pseudoF1=false;
	string biome="";
	public float upg3down, upg3up, upg3hugity;

	string worldDIR="";
	int worldID=1;
	string datapackDIR="./Datapacks/";

	public int SetLoot(int typp, bool only_one)
	{
		int i,rand,lngt;
		int[] idn = new int[2048];
		int[] min = new int[2048];
		int[] max = new int[2048];
		
		string[] dGet = SC_data.DrillLoot[typp].Split(';');
		lngt=dGet.Length/3;

		for(i=0;i<lngt&&i<2048;i++)
		{
			idn[i]=int.Parse(dGet[i*3]);
			min[i]=int.Parse(dGet[i*3+1]);
			max[i]=int.Parse(dGet[i*3+2]);
		}
		
		rand=UnityEngine.Random.Range(0,1000);
		for(i=0;i<lngt;i++)
		{
			if(rand>=min[i]&&rand<=max[i])
			{
				if(!only_one || i==0) return idn[i];
				else return 0;
			}
		}
		return 0;
	}
	void settings(int typpe)
	{
		if(typpe<=1) AD_loottableID=1;
		else AD_loottableID=typpe;
	}
	int longg(float S)
	{
		int size=(int)S;
		return size*2;
	}
	void generate_free()
	{
		int i,j,rand,lngt;
		int[] idn = new int[2048];
		int[] min = new int[2048];
		int[] max = new int[2048];
		
		string[] dGet = SC_data.FobGenerate[type].Split(';');
		lngt=dGet.Length/3;

		for(i=0;i<lngt&&i<2048;i++)
		{
			idn[i]=int.Parse(dGet[i*3]);
			min[i]=int.Parse(dGet[i*3+1]);
			max[i]=int.Parse(dGet[i*3+2]);
		}
		
		for(i=0;i<20;i++)
		{
			rand=UnityEngine.Random.Range(0,1000);
			for(j=0;j<lngt;j++)
			{
				if(rand>=min[j]&&rand<=max[j])
				{
					objectID[i]=idn[j];
					break;
				}
			}
		}
	}
	void TypeSet(float size)
	{
		int i,rand,lngt;
		int[] idn = new int[2048];
		int[] min = new int[2048];
		int[] max = new int[2048];
		
		int B=0;
		if(biome=="b1") B=1; 
		if(biome=="b2") B=2;
		if(biome=="b3") B=3;
		int I=(int)size+B*7-4;

		string[] dGet = SC_data.TypeSet[I].Split(';');
		lngt=dGet.Length/3;

		for(i=0;i<lngt&&i<2048;i++)
		{
			idn[i]=int.Parse(dGet[i*3]);
			min[i]=int.Parse(dGet[i*3+1]);
			max[i]=int.Parse(dGet[i*3+2]);
		}
		
		rand=UnityEngine.Random.Range(0,1000);
		for(i=0;i<lngt;i++)
		{
			if(rand>=min[i]&&rand<=max[i])
			{
				type=idn[i];
				if(type<0||type>16) type=0;
				break;
			}
		}
	}
	void Move(float size, string Mg)
	{
		int pX=int.Parse(Mg.Split(';')[0]);
		int pY=int.Parse(Mg.Split(';')[1]);
		float R=size/2;
		float P=5f-R;
		switch(pX)
		{
			case 0: transform.position += new Vector3(-P,0f,0f); break;
			case 2: transform.position += new Vector3(P,0f,0f); break;
		}
		switch(pY)
		{
			case 0: transform.position += new Vector3(0f,-P,0f); break;
			case 2: transform.position += new Vector3(0f,P,0f); break;
		}
	}
	void GetBiome()
	{
		Vector3 BS = new Vector3(Mathf.Round(transform.position.x/SC_fun.biome_sector_size),Mathf.Round(transform.position.y/SC_fun.biome_sector_size),0f);
		int ulam=SC_fun.CheckID((int)BS.x,(int)BS.y);
		BS=BS*SC_fun.biome_sector_size;
		float biomeSize=SC_fun.GetBiomeSize(ulam);
		if(biomeSize==0f) return;

		BS+=SC_fun.GetBiomeMove(ulam);
		float dX=transform.position.x-BS.x;
		float dY=transform.position.y-BS.y;
		float distance=Mathf.Sqrt(dX*dX+dY*dY);
		
		if(distance<biomeSize) biome=SC_fun.GetBiomeString(ulam);
	}
	void StructureReplace(int st)
	{
		Debug.Log(ID+" replaced to: "+st);
		Destroy(gameObject);
	}
	void Awake()
	{
		ID=SC_fun.CheckID((int)(transform.position.x/10f),(int)(transform.position.y/10f));
		if(ID!=1)
		{
			GetBiome();
			if(biome!="") if(biome[0]=='v')
			{
				SC_fun.GenListRemove(ID,0);
				Destroy(gameObject);
				return;
			}
		}
	}
	void Start()
	{
		worldID=(int)Communtron4.position.y;
		worldDIR="../../saves/UniverseData"+worldID+"/WorldData/";
		if(worldID!=100){
			datapackDIR="../../saves/UniverseData"+worldID+"/Datapacks/";
		}
		else{
			datapackDIR="../../saves/ServerData/Datapacks/";
		}

		//UNIVERSAL asteroid generator (UAG)
		float size=SC_fun.GetSize(ID);
		suze=(int)size;
		saze=size;
		X=(int)(transform.position.x/10f);
		Y=(int)(transform.position.y/10f);
		string Mg=SC_fun.GetMove(X,Y);
		Move(size,Mg);
		transform.localScale=new Vector3(size,size,size*0.75f);
		
		//DATA downloader (singleplayer, multiplayer)
		if(ID!=1)
		{
			if((int)Communtron4.position.y!=100) //If singleplayer
			{
				string[] uAst = SC_data.GetAsteroid(X,Y).Split(';');
				int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]),i;
				if(SC_data.World[a,0,c]=="")
				{
					//Not exists
					TypeSet(size);
					generate_free();
					SC_data.World[a,0,c]=type+"";
					for(i=0;i<longg(size);i++){
						SC_data.World[a,i+1,c]=objectID[i]+"";
					}
					if(uAst[2]=="T") SC_data.SaveAsteroid(c);
				}
				else
				{
					//Exists
					type=int.Parse(SC_data.World[a,0,c]);
					for(i=0;i<longg(size);i++){
						try{
						objectID[i]=int.Parse(SC_data.World[a,i+1,c]);
						}catch(Exception){objectID[i] = 0;}
					}
				}
				UUTC(size);
			}
			else //If multiplayer
			{
				SC_control.SendMTP("/AsteroidData "+ID+" "+suze+biome+" "+SC_control.connectionID);
			}
		}
	}
	public void onMSG(string eData)
	{
		string[] arg = eData.Split(' ');
		if(transform.position.z>100f) return;

		if(arg[0]=="/RetAsteroidData"&&int.Parse(arg[1])==ID)
		{
			int i;
			string[] astDats=arg[2].Split(';');
			type=int.Parse(astDats[0]);
			if(type<0||type>16) type=0;
			for(i=0;i<longg(saze);i++)
			{
				try{
					objectID[i]=int.Parse(astDats[i+1]);
				}catch(Exception)
				{
					objectID[i] = 0;
				}
				
				try{
					objectData[i]=arg[3+i];
				}catch(Exception)
				{
					if(objectID[i]==21||objectID[i]==2) objectData[i] = "0;0";
				}
			}
			UUTC(saze);
		}
	}
	void UUTC(float size)
	{
		if(UUTCed) return;
		UUTCed=true;
		
		if(ID!=1)
		{
			//UNIVERSAL Unity translate code (UUTC)
			int S=(int)size;
			float alpha=180f/S;
			int times=2*S;
			settings(type);
			try{
				if(type>1) asteroidR.material=texture[type];
				else
				{
					int rand2=UnityEngine.Random.Range(0,textureStone.Length);
					asteroidR.material=textureStone[rand2];
				}
			}
			catch(Exception){
				asteroidR.material=textureERROR;
				Debug.Log("Game crashed! Asteroid type "+type+" doesn't exists.");
			}
			int rand,ii;
			for(ii=0;ii<times;ii++)
			{
				GameObject gobT = gameObject;
				Quaternion quat_angle=new Quaternion(0f,0f,0f,0f);
				float sinX=Mathf.Sin(ii*alpha*(3.14159f/180f))*(size/2-0.02f);
				float cosY=Mathf.Cos(ii*alpha*(3.14159f/180f))*(size/2-0.02f);
				quat_angle.eulerAngles=new Vector3(0f,0f,-ii*alpha);
				Vector3 rotation_place=transform.position+new Vector3(sinX,cosY,(ii+1)/100000f);

				int tid=objectID[ii]; //tid -> Physical ID

				if((tid<8||tid>11)&&tid!=16&&tid!=30)
				{
					if(tid==21||tid==2) GenPlaceT[tid].name=objectData[ii]+";";
					try{gobT=Instantiate(GenPlaceT[tid],rotation_place,quat_angle);}catch(Exception)
					{
						gobT=Instantiate(GenPlaceT[0],rotation_place,quat_angle);
					}
				}
				else
				{
					int tud=0;
					rand=UnityEngine.Random.Range(0,3);
					if(tid>=8&&tid<=11) tud=tid-8;
					if(tid==16) tud=4;
					if(tid==30) tud=5;
					gobT = Instantiate(GenPlaceM[tud*3+rand],rotation_place,quat_angle);
				}
				gobT.transform.parent = gameObject.transform;
			}
		}
	}
	void Update()
	{
		if(transform.position.z<100&&UUTCed)
		{	
			//Optimalize
			float ssX=Mathf.Round(transform.position.x/10f);
			float ssY=Mathf.Round(transform.position.y/10f);
			float llX=Mathf.Round(legs.position.x/10f);
			float llY=Mathf.Round(legs.position.y/10f);
			float distance=Mathf.Sqrt((ssX-llX)*(ssX-llX)+(ssY-llY)*(ssY-llY));
			if(distance>7f&&Communtron1.position.z==0f)
			{
				SC_fun.GenListRemove(ID,0);
				Destroy(gameObject);
			}
		}
	}
	void OnTriggerEnter(Collider collision)
	{
		if(collision.gameObject.name=="Drill3")
		{
			Mining=true;
		}
	}
	void OnTriggerExit(Collider collision)
	{
		if(collision.gameObject.name=="Drill3")
		{
			Mining=false;
		}
	}
	int GetTimeDrill()
	{
		int down=(int)(upg3down/Mathf.Pow(upg3hugity,SC_upgrades.MTP_levels[2]+float.Parse(SC_data.Gameplay[2])));
		int up=(int)(upg3up/Mathf.Pow(upg3hugity,SC_upgrades.MTP_levels[2]+float.Parse(SC_data.Gameplay[2])));
		return UnityEngine.Random.Range(down,up);
	}
	void FixedUpdate()
	{
		int mined;
		if(!Input.GetMouseButton(0)) counter=0;
		counter++;
		if(counter==1) counter=-GetTimeDrill();
		
		//Particles
		if(Communtron1.localScale==new Vector3(2f,2f,2f)&&Mining&&Communtron1.position.z==0f&&Communtron3.position.y==0&&Input.GetMouseButton(0))
		{
			if(AD_particleID==0)
			{
				rHydrogenParticles.localPosition=new Vector3(0f,1.9f,0f);
				CommuntronM1.position=new Vector3(1f,0f,0f);
			}
			mined=0;
			if(counter==0) mined=SetLoot(type,false);			
			if(mined>0 && SC_slots.InvHaveB(mined,1,true,true,true,0))
			{
				int slot = SC_slots.InvChange(mined,1,true,true,true);
				if((int)Communtron4.position.y == 100) SC_control.SendMTP("/InventoryChange "+SC_control.connectionID+" "+mined+" 1 "+slot);
			}
		}
		if((Communtron1.localScale!=new Vector3(2f,2f,2f)||Communtron3.position.y!=0f||!Input.GetMouseButton(0))&&Communtron1.position.z==0f)
		{
			if(AD_particleID==0)
			{
				rHydrogenParticles.localPosition=new Vector3(0f,1.9f,-1000f);
				CommuntronM1.position=new Vector3(0f,0f,0f);
			}
		}

		//Drill singleplayer sounds
		int G;
		if(rHydrogenParticles.localPosition.z==0f) G=1;
		else G=0;
		if(SC_snd_start.drMode!=G)
        {
            SC_snd_start.drMode=G;
            if(G!=0) SC_snd_start.ManualStartLoop(0);
            else SC_snd_start.ManualEndLoop();
        }

		if(transform.position.z<50f&&Communtron1.position.z==0f)
		{
			//Respawn scared
			float rtX=transform.position.x;
			float rtY=transform.position.y;
			float atX=player.position.x;
			float atY=player.position.y;
			if(Mathf.Sqrt((rtX-atX)*(rtX-atX)+(rtY-atY)*(rtY-atY))<suze/2+3f)
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
}
