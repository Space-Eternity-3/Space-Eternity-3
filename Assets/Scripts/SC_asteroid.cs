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
	public Transform player;
	public Image FuelBar;
	public Renderer asteroidR;
	public int asteroid_lim = 16;
	public Material[] texture = new Material[16];
	public Material[] textureStone;
	public Material textureERROR;
	public GameObject[] GenPlaceT = new GameObject[128];
	public GameObject[] GenPlaceM = new GameObject[18];
	
	public SC_resp_blocker SC_resp_blocker;
	public SC_control SC_control;
	public SC_fun SC_fun;
	public SC_upgrades SC_upgrades;
	public SC_data SC_data;
	public SC_snd_start SC_snd_start;
	public SC_slots SC_slots;

	int type=0;
	bool mother = true;
	public int ID=1,X=0,Y=0;
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
	public string generation_code="";
	public float upg3down, upg3up, upg3hugity;
	
	public bool proto = false;
	public float protsize = 4;
	public int prottype = 0;
	public string fobCode = ";;;;;;;;;;;;;;;;;;;";
	
	public bool[] fobCenPos = new bool[20];
	public bool[] fobCenRot = new bool[20];
	public Vector3[] fobInfoPos = new Vector3[20];
	public Vector3[] fobInfoPosrel = new Vector3[20];
	public float[] fobInfoRot = new float[20];

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
		string[] prefobs = fobCode.Split(';');
		
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
			if(prefobs[i]!="")
			{
				objectID[i]=int.Parse(prefobs[i]);
				continue;	
			}
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
		
		if(!proto)
		{
			int B=0;
			B = int.Parse(biome.Split('b')[1]);
			int I=(int)size-4 + B*7;

			string ts = SC_data.TypeSet[I];
			if(ts=="") ts="0;0;999";
			string[] dGet = ts.Split(';');
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
					if(type<0||type>=16) type=0;
					break;
				}
			}
		}
		else type = int.Parse(generation_code.Split('t')[1]);
	}
	void Move(float size, string Mg)
	{
		if(ID==1) return;
		string tags = SC_data.BiomeTags[int.Parse(biome.Split('b')[1])];
		if(SC_fun.TagContains(tags,"grid")) return;
		
		int r121 = SC_fun.pseudoRandom1000(ID+SC_fun.seed) % 121;
		float dE = 5f - (size+2f)*0.35f;
		float dZ = (r121/11-5) * dE * 0.2f;
		float dW = (r121%11-5) * dE * 0.2f;
		transform.position += new Vector3(dW-dZ,dW+dZ,0f);
	}
	void GetBiome()
	{
		float[] dau = SC_fun.GetBiomeDAU(ID);
		float distance = dau[0];
		int ulam = (int)dau[1];
		
		if(distance < SC_fun.GetBiomeSize(ulam)) biome=SC_fun.GetBiomeString(ulam);
		else biome="b0";
	}
	void StructureReplace(int st)
	{
		Debug.Log(ID+" replaced to: "+st);
		Destroy(gameObject);
	}
	void Start()
	{
		if(transform.position.z<100f || proto)
			mother = false;
		
		//UNIVERSAL asteroid generator (UAG)
		if(ID!=1) GetBiome();
		float size;
		if(!proto) size=SC_fun.GetSize(ID);
		else size=protsize;
		suze=(int)size;
		saze=size;
		string Mg=SC_fun.GetMove(X,Y);
		if(!proto) Move(size,Mg);
		transform.localScale=new Vector3(size,size,size*0.75f);
		SC_resp_blocker.radius = size/2f + 3f;
		if(!proto) generation_code = suze + biome;
		else generation_code = suze + "t" + prottype + "t" + fobCode;
		
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
				SC_control.SendMTP("/AsteroidData "+ID+" "+generation_code+" "+SC_control.connectionID);
			}
		}
	}
	public void onMSG(string eData)
	{
		string[] arg = eData.Split(' ');
		if(mother) return;

		if(arg[0]=="/RetAsteroidData"&&int.Parse(arg[1])==ID)
		{
			int i;
			string[] astDats=arg[2].Split(';');
			type=int.Parse(astDats[0]);
			if(type<0||type>=16) type=0;
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
			}
			int rand,ii;
			for(ii=0;ii<times;ii++)
			{
				float beta = ii*alpha-transform.rotation.eulerAngles.z;
				float gamma = -beta-fobInfoRot[ii];
				float gammaR = gamma*(3.14159f/180f);
				
				GameObject gobT = gameObject;
				float uuX=Mathf.Sin(beta*(3.14159f/180f))*(size/2f);
				float uuY=Mathf.Cos(beta*(3.14159f/180f))*(size/2f);
				
				fobInfoPos[ii].x += Mathf.Cos(gammaR)*fobInfoPosrel[ii].x + Mathf.Sin(gammaR)*fobInfoPosrel[ii].y;
				fobInfoPos[ii].y += Mathf.Sin(gammaR)*fobInfoPosrel[ii].x + Mathf.Cos(gammaR)*fobInfoPosrel[ii].y;
				
				Quaternion quat_angle=new Quaternion(0f,0f,0f,0f);
				quat_angle.eulerAngles=new Vector3(0f,0f,gamma);
				Vector3 modvec = new Vector3(uuX,uuY,0f);
				Vector3 rotation_place=transform.position+modvec+fobInfoPos[ii];
				if(fobCenRot[ii]) quat_angle.eulerAngles=new Vector3(0f,0f,-fobInfoRot[ii]);
				if(fobCenPos[ii]) rotation_place=fobInfoPos[ii];

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
				gobT.GetComponent<SC_fobs>().index = ii;
			}
		}
	}
	void Update()
	{
		if(!mother&&UUTCed&&!proto)
		{	
			//Optimalize
			float ssX=X;
			float ssY=Y;
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
		if(Communtron1.localScale==new Vector3(2f,2f,2f)&&Mining&&Communtron1.position.z==0f&&(Communtron3.position.y==0f||CommuntronM1.position.x==1f)&&Input.GetMouseButton(0))
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
		if((!(Communtron1.localScale==new Vector3(2f,2f,2f) && Input.GetMouseButton(0)))&&Communtron1.position.z==0f)
		{
			if(AD_particleID==0)
			{
				rHydrogenParticles.localPosition=new Vector3(0f,1.9f,-1000f);
				CommuntronM1.position=new Vector3(0f,0f,0f);
			}
		}
	}
}
