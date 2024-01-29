using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;

public class SC_asteroid : MonoBehaviour {

	public Transform legs;
	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron4;
	public Transform CommuntronM1;
	public Transform player;
	public Renderer asteroidR;
	public Transform strucutral_parent;
	public Material[] texture = new Material[16];
	public Material[] textureStone;
	public Material[] textureDark;
	public Material textureERROR;
	public GameObject[] GenPlaceT = new GameObject[128];
	public GameObject[] GenPlaceM = new GameObject[18];

	bool mother = true;
	public int ID=1,X=0,Y=0;
	int[] objectID = new int[32];
	string[] objectData = new string[32];
	int counter_to_destroy = 200;
	public bool UUTCed=false;
	string biome="";
	public string generation_code="";
	public float upg3down, upg3up, upg3hugity;
	int type=0, size;
	
	//Seon asteroid data holders
	public bool proto = false;
	public Vector3 asteroidScale = new Vector3(1f,1f,1f);
	public Vector3 normalScale = new Vector3(1f,1f,1f);
	public bool reward_blocker = false;
	public bool temporary_blocker = false;
	public int protbiome = -1;
	public int protsize = 4;
	public int prottype = 0;
	public string fobCode = ";;;;;;;;;;;;;;;;;;;";
	
	//Seon fob data holders
	public bool[] fobCenPos = new bool[20];
	public bool[] fobCenRot = new bool[20];
	public bool[] fobCenScale = new bool[20];
	public Vector3[] fobInfoPos = new Vector3[20];
	public Vector3[] fobInfoPosZ = new Vector3[20];
	public Vector3[] fobInfoPosrel = new Vector3[20];
	public Vector3[] fobInfoScale = new Vector3[20];
	public float[] fobInfoRot = new float[20];

	//Script references
	public SC_control SC_control;
	public SC_fun SC_fun;
	public SC_upgrades SC_upgrades;
	public SC_data SC_data;
	public SC_slots SC_slots;

	void Move(int S, string Mg)
	{
		if(ID==1) return;
		string tags = SC_data.BiomeTags[int.Parse(biome.Split('b')[1])];
		if(SC_data.TagContains(tags,"grid")) return;
		
		int r121 = Deterministics.Random10e3(ID+Generator.seed) % 121;
		float dE = 5f - (S+2f)*0.35f;
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
	void OnDestroy()
	{
		SC_control.SC_lists.RemoveFrom_SC_asteroid(this);
	}
	void Start()
	{
		if(transform.position.z<100f || proto)
			mother = false;

		SC_control.SC_lists.AddTo_SC_asteroid(this);
		if(proto) SC_fun.GenListAdd(ID,0);
		
		//UNIVERSAL asteroid generator (UAG)
		if(ID!=1) GetBiome();
		if(!proto) size = SC_fun.GetSize(ID);
		else size = protsize;
		if(!proto) Move(size,SC_fun.LocalMove(ID));

		normalScale = transform.localScale;
		asteroidScale = new Vector3(1f,1f,0.75f)*size;
		if(ID!=1) transform.localScale = new Vector3(
			normalScale.x * asteroidScale.x,
			normalScale.y * asteroidScale.y,
			normalScale.z * asteroidScale.z
		);
		
		if(!proto) generation_code = size + biome;
		else if(protbiome==-1) generation_code = size + "t" + prottype + "t" + fobCode;
		else generation_code = size + "b" + protbiome + "b" + fobCode;
		
		//DATA downloader (singleplayer, multiplayer)
		if(ID!=1)
		{
			if((int)Communtron4.position.y!=100) //If singleplayer
			{
				int i;
				WorldData.Load(X,Y);
				int supertype = WorldData.GetType();
				if(!(supertype>=0 && supertype<=63)) //NOT EXISTS
				{
					WorldData.DataGenerate(generation_code);
					supertype = WorldData.GetType();
				}

				type = supertype % 16;
				for(i=0;i<=19;i++) {
					objectID[i] = WorldData.GetFob(i+1);
				}

				UUTC();
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
			if(type<0) type=0;
			else type = type % 16;
			for(i=0;i<2*size;i++)
			{
				try{
					objectID[i]=int.Parse(astDats[i+1]);
				}catch(Exception) {
					objectID[i] = 72;
				}
				
				try{
					objectData[i]=arg[3+i];
				}catch(Exception) {
					if(objectID[i]==21||objectID[i]==2||objectID[i]==52) objectData[i] = "0;0";
				}
			}
			UUTC();
		}
	}
	void UUTC()
	{
		if(UUTCed) return;
		UUTCed=true;
		
		if(ID!=1)
		{
			//UNIVERSAL Unity translate code (UUTC)
			if(transform.GetComponent<SC_drill>() != null)
				if(!transform.GetComponent<SC_drill>().freeze)
					transform.GetComponent<SC_drill>().type = type;

			float alpha=180f/size;
			try{
				if(type>1) asteroidR.material=texture[type];
				else if(type==0)
				{
					int rand2=UnityEngine.Random.Range(0,textureStone.Length);
					asteroidR.material=textureStone[rand2];
				}
				else if(type==1)
				{
					int rand2=UnityEngine.Random.Range(0,textureDark.Length);
					asteroidR.material=textureDark[rand2];
				}
			}
			catch(Exception){
				asteroidR.material=textureERROR;
			}
			int rand,i;
			for(i=0;i<2*size;i++)
			{
				transform.localScale = asteroidScale;

				float beta = i*alpha-transform.rotation.eulerAngles.z;
				float gamma = -beta-fobInfoRot[i];
				float gammaR = gamma*(3.14159f/180f);
				
				GameObject gobT = gameObject;
				float uuX=Mathf.Sin(beta*(3.14159f/180f))*(size/2f);
				float uuY=Mathf.Cos(beta*(3.14159f/180f))*(size/2f);
				
				fobInfoPos[i].x += Mathf.Cos(gammaR)*fobInfoPosrel[i].x + Mathf.Sin(gammaR)*fobInfoPosrel[i].y;
				fobInfoPos[i].y += Mathf.Sin(gammaR)*fobInfoPosrel[i].x + Mathf.Cos(gammaR)*fobInfoPosrel[i].y;
				
				Quaternion quat_angle=new Quaternion(0f,0f,0f,0f);
				quat_angle.eulerAngles=new Vector3(0f,0f,gamma);
				Vector3 modvec = new Vector3(uuX,uuY,0f);
				Vector3 rotation_place=transform.position+modvec+fobInfoPos[i];

				if(fobCenRot[i]) quat_angle.eulerAngles = new Vector3(0f,0f,-fobInfoRot[i]);
				if(fobCenPos[i]) rotation_place = fobInfoPos[i];
				rotation_place += fobInfoPosZ[i];

				int tid=objectID[i]; //tid -> Physical ID

				if((tid<8||tid>11)&&tid!=16&&tid!=30&&tid!=50&&tid!=51&&tid!=66)
				{
					if(tid==21||tid==2||tid==52) GenPlaceT[tid].name=objectData[i]+";";
					try{gobT=Instantiate(GenPlaceT[tid],rotation_place,quat_angle);}catch(Exception)
					{
						gobT=Instantiate(GenPlaceT[72],rotation_place,quat_angle);
					}
				}
				else
				{
					int tud=0;
					rand=UnityEngine.Random.Range(0,3);
					if(tid>=8&&tid<=11) tud=tid-8;
					if(tid==16) tud=4;
					if(tid==30) tud=5;
					if(tid==50) tud=6;
					if(tid==51)
					{
						tud=7;
						rand=UnityEngine.Random.Range(0,2);
					}
					if(tid==66) tud=8;
					gobT = Instantiate(GenPlaceM[tud*3+rand],rotation_place,quat_angle);
				}
				gobT.transform.parent = gameObject.transform;
				gobT.GetComponent<SC_fobs>().index = i;

				transform.localScale = new Vector3(
					normalScale.x * asteroidScale.x,
					normalScale.y * asteroidScale.y,
					normalScale.z * asteroidScale.z
				);

				if(!fobCenScale[i])
					fobInfoScale[i] = new Vector3(1f,1f,1f);

				gobT.transform.localScale = new Vector3(
					gobT.transform.localScale.x / normalScale.x,
					gobT.transform.localScale.y / normalScale.y,
					gobT.transform.localScale.z / normalScale.z
				);			
				gobT.GetComponent<SC_fobs>().transportScale = new Vector3(
					fobInfoScale[i].x * normalScale.x,
					fobInfoScale[i].y * normalScale.y,
					fobInfoScale[i].z * normalScale.z
				);
			}
		}
		if(strucutral_parent!=null)
			strucutral_parent.GetComponent<SC_structure>().scaling_blocker--;
	}
	public int SetLoot(int typp)
	{
		if(typp<0 || typp>15) return 0;

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
				return idn[i];
		}
		return 0;
	}
	void FixedUpdate()
	{
		if(!mother&&UUTCed&&!proto)
		{	
			//Optimalize
			float ssX=X;
			float ssY=Y;
			float llX=Mathf.Round(legs.position.x/10f);
			float llY=Mathf.Round(legs.position.y/10f);
			float distance=Mathf.Sqrt((ssX-llX)*(ssX-llX)+(ssY-llY)*(ssY-llY));
			if(distance>12f&&Communtron1.position.z==0f)
			{
				if(counter_to_destroy==0)
				{
					SC_fun.GenListRemove(ID,0);
					Destroy(gameObject);
				}
				else counter_to_destroy--;
			}
			else counter_to_destroy = 200;
		}
	}
}
