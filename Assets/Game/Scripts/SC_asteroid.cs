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
	public Transform strucutral_parent;
	public int asteroid_lim = 16;
	public Material[] texture = new Material[16];
	public Material[] textureStone;
	public Material textureERROR;
	public GameObject[] GenPlaceT = new GameObject[128];
	public GameObject[] GenPlaceM = new GameObject[18];
	
	//public SC_resp_blocker SC_resp_blocker;
	public SC_control SC_control;
	public SC_fun SC_fun;
	public SC_upgrades SC_upgrades;
	public SC_data SC_data;
	public SC_slots SC_slots;

	int type=0, supertype=0;
	bool mother = true;
	public int ID=1,X=0,Y=0;
	int[] objectID = new int[32];
	string[] objectData = new string[32];
	string[] objectGrow = new string[32];

	bool Mining=false;
	int counter=-1;
	int counter_to_destroy = 200;
	int suze;
	bool stille=false;
	public bool UUTCed=false;
	float saze;
	bool pseudoF1=false;
	string biome="";
	public string generation_code="";
	public float upg3down, upg3up, upg3hugity;
	
	public Vector3 asteroidScale = new Vector3(1f,1f,1f);
	public Vector3 normalScale = new Vector3(1f,1f,1f);
	public bool permanent_blocker = false;
	public bool temporary_blocker = false;
	
	public bool proto = false;
	public float protsize = 4;
	public int prottype = 0;
	public string fobCode = ";;;;;;;;;;;;;;;;;;;";
	
	public bool[] fobCenPos = new bool[20];
	public bool[] fobCenRot = new bool[20];
	public bool[] fobCenScale = new bool[20];
	public Vector3[] fobInfoPos = new Vector3[20];
	public Vector3[] fobInfoPosZ = new Vector3[20];
	public Vector3[] fobInfoPosrel = new Vector3[20];
	public Vector3[] fobInfoScale = new Vector3[20];
	public float[] fobInfoRot = new float[20];

	string worldDIR="";
	int worldID=1;

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
		
		string[] dGet = SC_data.FobGenerate[supertype].Split(';');
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
					if(type<0) {
						supertype=0;
						type=0;
					}
					else {
						supertype = type;
						type = type % 16;
					}
					break;
				}
			}
		}
		else
		{
			supertype = int.Parse(generation_code.Split('t')[1]);
			type = supertype % 16;
		}
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
		float size;
		if(!proto) size=SC_fun.GetSize(ID);
		else size=protsize;
		suze=(int)size;
		saze=size;
		string Mg=SC_fun.GetMove(X,Y);
		if(!proto) Move(size,Mg);

		normalScale = transform.localScale;
		asteroidScale = new Vector3(1f,1f,0.75f)*size;
		if(ID!=1) transform.localScale = new Vector3(
			normalScale.x * asteroidScale.x,
			normalScale.y * asteroidScale.y,
			normalScale.z * asteroidScale.z
		);
		//SC_resp_blocker.radius = size/2f + 3f;
		if(!proto) generation_code = suze + biome;
		else generation_code = suze + "t" + prottype + "t" + fobCode;
		
		//DATA downloader (singleplayer, multiplayer)
		if(ID!=1)
		{
			if((int)Communtron4.position.y!=100) //If singleplayer
			{
				string[] uAst = SC_data.GetAsteroid(X,Y).Split(';');
				int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]),i;
				
				if(SC_data.World[a,0,c]=="" || int.Parse(SC_data.World[a,0,c])>=1024)
				{
					//Not exists
					TypeSet(size);
					generate_free();
					SC_data.World[a,0,c]=supertype+"";
					for(i=0;i<longg(size);i++){
						SC_data.World[a,i+1,c]=objectID[i]+"";
					}
				}
				else
				{
					//Exists
					supertype = int.Parse(SC_data.World[a,0,c]);
					if(supertype<0) supertype=0;
					type = supertype % 16;
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
			if(type<0) type=0;
			else type = type % 16;
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
					if(objectID[i]==21||objectID[i]==2||objectID[i]==52) objectData[i] = "0;0";
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
			if(transform.GetComponent<SC_drill>() != null)
				if(!transform.GetComponent<SC_drill>().freeze)
					transform.GetComponent<SC_drill>().type = type;

			int S=(int)size;
			float alpha=180f/S;
			int times=2*S;
			try{
				if(type>0) asteroidR.material=texture[type];
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
				transform.localScale = asteroidScale;

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

				if(fobCenRot[ii]) quat_angle.eulerAngles = new Vector3(0f,0f,-fobInfoRot[ii]);
				if(fobCenPos[ii]) rotation_place = fobInfoPos[ii];
				rotation_place += fobInfoPosZ[ii];

				int tid=objectID[ii]; //tid -> Physical ID

				if((tid<8||tid>11)&&tid!=16&&tid!=30&&tid!=50&&tid!=51)
				{
					if(tid==21||tid==2||tid==52) GenPlaceT[tid].name=objectData[ii]+";";
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
					if(tid==50) tud=6;
					if(tid==51)
					{
						tud=7;
						rand=UnityEngine.Random.Range(0,2);
					}
					gobT = Instantiate(GenPlaceM[tud*3+rand],rotation_place,quat_angle);
				}
				gobT.transform.parent = gameObject.transform;
				gobT.GetComponent<SC_fobs>().index = ii;

				transform.localScale = new Vector3(
					normalScale.x * asteroidScale.x,
					normalScale.y * asteroidScale.y,
					normalScale.z * asteroidScale.z
				);

				if(!fobCenScale[ii])
					fobInfoScale[ii] = new Vector3(1f,1f,1f);

				gobT.transform.localScale = new Vector3(
					gobT.transform.localScale.x / normalScale.x,
					gobT.transform.localScale.y / normalScale.y,
					gobT.transform.localScale.z / normalScale.z
				);			
				gobT.GetComponent<SC_fobs>().transportScale = new Vector3(
					fobInfoScale[ii].x * normalScale.x,
					fobInfoScale[ii].y * normalScale.y,
					fobInfoScale[ii].z * normalScale.z
				);
			}
		}
		if(strucutral_parent!=null)
			strucutral_parent.GetComponent<SC_structure>().scaling_blocker--;
	}
	public int SetLoot(int typp, bool only_one)
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
			{
				if(!only_one || i==0) return idn[i];
				else return 0;
			}
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
			if(distance>7f&&Communtron1.position.z==0f)
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
