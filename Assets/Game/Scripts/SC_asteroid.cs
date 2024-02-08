using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;

public class SC_asteroid : MonoBehaviour
{
	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron4;
	public Transform CommuntronM1;
	public Transform player;
	public Renderer asteroidR;
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
	public bool UUTCed=false;
	public string generation_code="";
	public float upg3down, upg3up, upg3hugity;
	int type=0, size;
	public bool temporary_blocker = false;
	
	//Seon asteroid data holders
	public int protbiome = -1;
	public int protsize = 4;
	public int prottype = 0;
	public string fobCode = ",,,,,,,,,,,,,,,,,,,";
	public Vector3[] fobInfoPos = new Vector3[20];
	public float[] fobInfoRot = new float[20];

	//Script references
	public SC_control SC_control;
	public SC_fun SC_fun;
	public SC_upgrades SC_upgrades;
	public SC_data SC_data;
	public SC_slots SC_slots;

	void OnDestroy()
	{
		SC_control.SC_lists.RemoveFrom_SC_asteroid(this);
	}
	void Start()
	{
		if(transform.position.z<100f) mother = false;
		if(mother) return;
		SC_control.SC_lists.AddTo_SC_asteroid(this);
		
		//UNIVERSAL asteroid generator (UAG)
		size = protsize;
		if(protbiome==-1) generation_code = size + "t" + prottype + "t" + fobCode;
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
				Quaternion quat_angle = new Quaternion(0f,0f,0f,0f);
				quat_angle.eulerAngles = new Vector3(0f,0f,fobInfoRot[i]);
				Vector3 rotation_place = fobInfoPos[i];

				int tid=objectID[i]; //tid -> Physical ID

				GameObject gobT = gameObject;
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
			}
		}
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
}
