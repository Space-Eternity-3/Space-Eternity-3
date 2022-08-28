using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_structure : MonoBehaviour
{
	public SC_fun SC_fun;
	public Transform legs;
	public Transform Communtron1;
	
	public Transform asteroid;
	public Transform stwall;
	public Transform arcen;
	public Transform gatepart;
	public Transform stsphere;
	public Transform stboss;
	public Transform emptyobject;
	
    public int X=0,Y=0,ID=1,overrand=0;
	public string actual_state = "default";
	public int scaling_blocker = 0;
	[TextArea(15,20)]
	public string SeonField = "";
	public Transform[] st_structs = new Transform[1024];

	int counter_to_destroy = 200;
	bool mother = true;
	
	string TxtToSeonArray(string str)
	{
		//All separators to space
		int i,lngt=str.Length;
		string effect = "";
		for(i=0;i<lngt;i++)
		{
			if(str[i]!='\t' && str[i]!='\r' && str[i]!='\n' && str[i]!='[' && str[i]!=']')
				effect += str[i];
			else
				effect += ' ';
		}
		
		//Remove multispace
		str = effect+"x"; effect="";
		for(i=0;i<lngt;i++)
		{
			if((str[i]==' ' && str[i+1]!=' ') || str[i]!=' ')
				effect += str[i];
		}

		//All . to ,
		str = effect; effect="";
		lngt = str.Length;
		for(i=0;i<lngt;i++)
		{
			if(str[i]!='.') effect+=str[i];
			else effect+=',';
		}	
		return effect;
	}
	bool isMulti(string chhh) {
		return (chhh=="children");
	}
	void Start()
	{
		if(transform.position.z<=100f) mother = false; else return;
		transform.position += SC_fun.GetBiomeMove(ID);

		//SEON
		overrand = SC_fun.pseudoRandom10e5(ID+SC_fun.seed);
		string final_seon = TxtToSeonArray(SeonField);
		SeonGenerate(final_seon);
	}
	void FixedUpdate()
	{
		if(!mother)
		{
			//Optimalize
			float ssX=X;
			float ssY=Y;
			float llX=Mathf.Round(legs.position.x/100f);
			float llY=Mathf.Round(legs.position.y/100f);
			float distance=Mathf.Sqrt((ssX-llX)*(ssX-llX)+(ssY-llY)*(ssY-llY));
			if(distance>2f&&Communtron1.position.z==0f)
			{
				if(counter_to_destroy==0)
				{
					SC_fun.GenListRemove(ID,1);
					Destroy(gameObject);
				}
				else counter_to_destroy--;
			}
			else counter_to_destroy = 200;
		}
	}
	string uzup20(string str)
	{
		if(str=="x"||str=="X") str=",";
		int junk;
		string[] strs = str.Split(',');
		int i,lngt=strs.Length;
		for(i=0;i<lngt;i++)
			if(strs[i]!="")
				junk = int.Parse(strs[i]);
		for(i=lngt;i<20;i++)
			str+=",";

		lngt=str.Length;
		string effect="";
		for(i=0;i<lngt;i++)
		{
			if(str[i]!=',') effect+=str[i];
			else effect+=';';
		}
		return effect;
	}
	int[] BuildXY(int id) //<0;49>
	{
		int[] retu = new int[2];
		
		int sX = 2*(id%5);
		int sY = id/5;
		
		if(sY%2==0) sX++;
		
		retu[0] = 10*X + sX;
		retu[1] = 10*Y + sY;
		
		return retu;
	}
	int PercConvert(string str)
	{
		int i,lngt=str.Length;
		string effect="";
		if(str[0]=='%')
		for(i=1;i<lngt;i++)
			effect+=str[i];
		return int.Parse(effect);
	}
	int HashConvert(string str)
	{
		int i,lngt=str.Length;
		string effect="";
		if(str[0]=='#')
		for(i=1;i<lngt;i++)
			effect+=str[i];
		return int.Parse(effect);
	}
	int DollarConvert(string str)
	{
		int i,lngt=str.Length;
		string effect="";
		if(str[0]=='$')
		for(i=1;i<lngt;i++)
			effect+=str[i];
		return int.Parse(effect);
	}
	void SeonGenerate(string seon_string)
	{
		string[] arg = (seon_string+" ").Split(' ');
		int i,lngt=arg.Length;
		int pr1=-1, pr2=-3, lockedI=-1, preCurrent=-1;
		int setrandom=-1, ifrandom=-1;
		int current=-1, findex=-1;
		int min_findex=0, max_findex=19;
		for(i=0;i<lngt;i++)
		{
			//Force resetting scr position
			if(st_structs[0]!=null)
				if(st_structs[0].GetComponent<SC_boss>()!=null)
				{
					st_structs[0].position = transform.position;
					st_structs[0].rotation = Quaternion.identity;
					st_structs[0].parent = transform;
					st_structs[0].localScale = new Vector3(1f,1f,1f);
				}

			//From #A to #B segment
			if(pr1<=pr2)
			{
				i = lockedI;
				current = pr1;
				pr1++;
			}
			else if(pr1==pr2+1)
			{
				current = preCurrent;
				pr1++;
			}

			try {

				//Technical commands
				if(arg[i]=="setrandom")
				{
					i++;
					int prper = PercConvert(arg[i]);

					setrandom = overrand % prper;
				}
				else if(arg[i]=="ifrandom")
				{
					i++;
					if(arg[i]=="break")
					{
						ifrandom = -1;
					}
					else
					{
						int prran = int.Parse(arg[i]);
						ifrandom = prran;
					}
				}
				else if(!(ifrandom==-1||ifrandom==setrandom))
				{
					i++;
					throw(new Exception());
				}
				else if(arg[i]=="from")
				{
					i++;
					int prr1 = HashConvert(arg[i]);
					if(prr1<0) prr1=0;

					i++;
					if(arg[i]!="to") throw(new Exception());

					i++;
					int prr2 = HashConvert(arg[i]);
					if(prr2>=1024) prr2=1023;

					if(prr2<prr1) {
						int pprr = prr1;
						prr1 = prr2;
						prr2 = pprr;
					}

					pr1 = prr1;
					pr2 = prr2;
					lockedI = i+1;
					preCurrent = current;
				}

				//Catching commands
				else if(arg[i]=="summon")
				{
					i++;
					int pre_current = current;
					current = HashConvert(arg[i]);
					if(st_structs[current]!=null)
					{
						current = pre_current;
						throw(new Exception());
					}

					i++;
					if(arg[i]=="asteroid")
					{
						if(current<0 || current>49)
						{
							current = pre_current;
							throw(new Exception());
						}

						i++;
						int prsize = int.Parse(arg[i]);
						if(prsize<4 || prsize>10) prsize = 4;

						i++;
						int prtype = int.Parse(arg[i]);
						if(prtype<0||prtype>=64) throw(new Exception());

						i++;
						string prfobs = uzup20(arg[i]);

						SC_asteroid ast = Instantiate(asteroid,transform.position,Quaternion.identity).GetComponent<SC_asteroid>();
						ast.GetComponent<Transform>().parent = transform;
						st_structs[current] = ast.GetComponent<Transform>();
						ast.strucutral_parent = transform;
						scaling_blocker++;
				
						int[] sXY = BuildXY(current);
						ast.proto = true; ast.X=sXY[0]; ast.Y=sXY[1]; ast.ID=SC_fun.CheckID(ast.X,ast.Y);
						ast.protsize = prsize;
						ast.prottype = prtype;
						ast.fobCode = prfobs;
					}
					else if(arg[i]=="wall")
					{
						i++;
						int prmaterial = int.Parse(arg[i]);
						if(prmaterial<0 || prmaterial>15) prmaterial = 0;
				
						Transform wal = Instantiate(stwall,transform.position,Quaternion.identity);
						wal.parent = transform;
						st_structs[current] = wal;
						if(wal.GetComponent<SC_drill>()!=null)
							wal.GetComponent<SC_drill>().type=prmaterial;

						wal.GetChild(0).GetComponent<Renderer>().material = wal.GetComponent<SC_material>().Materials[prmaterial];
					}
					else if(arg[i]=="sphere")
					{
						i++;
						float prsize = float.Parse(arg[i]);

						i++;
						int prmaterial = int.Parse(arg[i]);
						if(prmaterial<0 || prmaterial>15) prmaterial = 0;
				
						Transform ras = Instantiate(stsphere,transform.position,Quaternion.identity);
						ras.parent = transform;
						st_structs[current] = ras;
						if(ras.GetComponent<SC_drill>()!=null)
							ras.GetComponent<SC_drill>().type=prmaterial;

						ras.localScale = new Vector3(prsize,prsize,0.75f*prsize);

						if(prmaterial==0 || prmaterial==1) ras.GetChild(0).GetComponent<Renderer>().material = ras.GetComponent<SC_material>().Materials2[UnityEngine.Random.Range(0,3)];
						else ras.GetChild(0).GetComponent<Renderer>().material = ras.GetComponent<SC_material>().Materials[prmaterial];
					}
					else if(arg[i]=="piston")
					{
						Transform gat = Instantiate(gatepart,transform.position,Quaternion.identity);
						gat.parent = transform;
						st_structs[current] = gat;
					}
					else if(arg[i]=="boss")
					{
						if(current!=0)
						{
							current = pre_current;
							throw(new Exception());
						}

						i++;
						int prboss = int.Parse(arg[i]);
						if(prboss<0 || prboss>6) prboss=0;

						SC_boss bos = Instantiate(stboss,transform.position,Quaternion.identity).GetComponent<SC_boss>();
						bos.GetComponent<Transform>().parent = transform;
						st_structs[current] = bos.GetComponent<Transform>();

						bos.SC_structure = transform.GetComponent<SC_structure>();

						int[] sXY = BuildXY(current);
						bos.bX=sXY[0]; bos.bY=sXY[1]; bos.bID=SC_fun.CheckID(bos.bX,bos.bY); bos.sID=ID;
						bos.type = prboss;

						bos.StartFromStructure();
					}
					else if(arg[i]=="empty")
					{
						Transform emp = Instantiate(emptyobject,transform.position,Quaternion.identity);
						emp.parent = transform;
						st_structs[current] = emp;
					}
					else if(arg[i]=="respblock")
					{
						i++;
						float prradius = float.Parse(arg[i]);

						Transform arc = Instantiate(arcen,transform.position,Quaternion.identity);
						arc.parent = transform;
						st_structs[current] = arc;

						arc.GetComponent<SC_resp_blocker>().radius = prradius;
					}
					else throw(new Exception());
				}
				else if(arg[i]=="catch")
				{
					i++;
					if(arg[i]=="child")
					{
						i++;
						findex = DollarConvert(arg[i]);
					}
					else if(arg[i]=="children")
					{
						i++;
						int prdolar1 = DollarConvert(arg[i]);
						if(prdolar1<0 || prdolar1>19) throw(new Exception());

						i++;
						int prdolar2 = DollarConvert(arg[i]);
						if(prdolar2<0 || prdolar2>19) throw(new Exception());

						min_findex = prdolar1;
						max_findex = prdolar2;
					}
					else
					{
						current = HashConvert(arg[i]);
					}
				}

				//Transform commands
				else if(arg[i]=="move")
				{
					i++;
					if(arg[i]=="child" || arg[i]=="children")
					{
						int k=findex,kngt=findex+1;
						if(isMulti(arg[i])) {
							k=min_findex; kngt=max_findex+1;
						}
						int preI = i;
						for(;k<kngt;k++) {
						//===================

						i++;
						float dX = float.Parse(arg[i]);

						i++;
						float dY = float.Parse(arg[i]);

						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();

						float angle = -ast.fobInfoRot[k];
						angle *= 3.14159f/180f;

						if(!ast.fobCenPos[k])
						{
							ast.fobInfoPos[k] = st_structs[current].position;
							ast.fobCenPos[k] = true;
						}
						ast.fobInfoPos[k] += new Vector3(
							Mathf.Cos(angle)*dX + Mathf.Cos(angle+3.14159f/2)*dY,
							Mathf.Sin(angle)*dX + Mathf.Sin(angle+3.14159f/2)*dY,
							0f
						);

						//===================
						i = preI; }
					}
					else
					{
						float dX = float.Parse(arg[i]);

						i++;
						float dY = float.Parse(arg[i]);

						float angle = st_structs[current].eulerAngles.z;
						angle *= 3.14159f/180f;
						st_structs[current].position += new Vector3(
							Mathf.Cos(angle)*dX + Mathf.Cos(angle+3.14159f/2)*dY,
							Mathf.Sin(angle)*dX + Mathf.Sin(angle+3.14159f/2)*dY,
							0f
						);
					}
				}
				else if(arg[i]=="rotate")
				{
					i++;
					if(arg[i]=="child" || arg[i]=="children")
					{
						int k=findex,kngt=findex+1;
						if(isMulti(arg[i])) {
							k=min_findex; kngt=max_findex+1;
						}
						int preI = i;
						for(;k<kngt;k++) {
						//===================

						i++;
						float dA = float.Parse(arg[i]);

						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();
						
						if(!ast.fobCenRot[k])
						{
							ast.fobInfoRot[k] = -st_structs[current].eulerAngles.z;
							ast.fobCenRot[k] = true;
						}
						ast.fobInfoRot[k] -= dA;

						//===================
						i = preI; }
					}
					else
					{
						float dA = float.Parse(arg[i]);

						float angle = st_structs[current].eulerAngles.z;
						st_structs[current].rotation = Quaternion.Euler(0f,0f,dA+angle);
					}
				}
				else if(arg[i]=="scale")
				{
					i++;
					if(arg[i]=="child" || arg[i]=="children")
					{
						int k=findex,kngt=findex+1;
						if(isMulti(arg[i])) {
							k=min_findex; kngt=max_findex+1;
						}
						int preI = i;
						for(;k<kngt;k++) {
						//===================

						i++;
						float nX = float.Parse(arg[i]);

						i++;
						float nY = float.Parse(arg[i]);

						i++;
						float nZ = float.Parse(arg[i]);

						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();

						if(!ast.fobCenScale[k])
						{
							ast.fobCenScale[k] = true;
						}
						ast.fobInfoScale[k] = new Vector3(nX,nY,nZ);

						//===================
						i = preI; }
					}
					else
					{
						float nX = float.Parse(arg[i]);

						i++;
						float nY = float.Parse(arg[i]);

						i++;
						float nZ = float.Parse(arg[i]);

						st_structs[current].localScale = new Vector3(nX,nY,nZ);
					}
				}
				else if(arg[i]=="layer")
				{
					i++;
					if(arg[i]=="child" || arg[i]=="children")
					{
						int k=findex,kngt=findex+1;
						if(isMulti(arg[i])) {
							k=min_findex; kngt=max_findex+1;
						}
						int preI = i;
						for(;k<kngt;k++) {
						//===================

						i++;
						float nZ = float.Parse(arg[i]);
						st_structs[current].GetComponent<SC_asteroid>().fobInfoPosZ[k] = new Vector3(0f,0f,nZ);

						//===================
						i = preI; }
					}
					else
					{
						float nZ = float.Parse(arg[i]);
						st_structs[current].position += new Vector3(0f,0f,nZ);
					}
				}
				else if(arg[i]=="reset")
				{
					i++;
					if(arg[i]=="child" || arg[i]=="children")
					{
						int k=findex,kngt=findex+1;
						if(isMulti(arg[i])) {
							k=min_findex; kngt=max_findex+1;
						}
						int preI = i;
						for(;k<kngt;k++) {
						//===================

						i++;
						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();
						if(arg[i]=="position")
						{
							ast.fobCenPos[k] = true;
							ast.fobInfoPos[k] = transform.position;
						}
						else if(arg[i]=="rotation")
						{
							ast.fobCenRot[k] = true;
							ast.fobInfoRot[k] = -st_structs[current].eulerAngles.z;
						}
						else if(arg[i]=="localpos")
						{
							ast.fobCenPos[k] = true;
							ast.fobInfoPos[k] = st_structs[current].position;
						}
						else throw(new Exception());

						//===================
						i = preI; }
					}
					else if(arg[i]=="position")
					{
						st_structs[current].position = transform.position;
					}
					else if(arg[i]=="rotation")
					{
						st_structs[current].rotation = st_structs[current].parent.rotation;
					}
					else if(arg[i]=="localpos")
					{
						st_structs[current].localPosition = new Vector3(0f,0f,0f);
					}
					else throw(new Exception());
				}
				else if(arg[i]=="parent")
				{
					i++;
					if(arg[i]=="set")
					{
						i++;
						int prparent = HashConvert(arg[i]);
						st_structs[current].parent = st_structs[prparent];
					}
					else if(arg[i]=="remove")
					{
						st_structs[current].parent = transform;
					}
					else throw(new Exception());
				}

				//Script commands
				else if(arg[i]=="drill")
				{
					i++;
					if(arg[i]=="remove")
					{
						Destroy(st_structs[current].GetComponent<SC_drill>());
					}
					else if(arg[i]=="set")
					{
						i++;
						int prdrill = int.Parse(arg[i]);

						SC_drill drl;
						if(st_structs[current].GetComponent<SC_drill>() == null)
						{
							drl = st_structs[current].gameObject.AddComponent<SC_drill>();
							SC_drill dst = asteroid.GetComponent<SC_drill>();

							//Required public variables
							drl.rHydrogenParticles = dst.rHydrogenParticles;
							drl.Communtron1 = dst.Communtron1;
							drl.Communtron3 = dst.Communtron3;
							drl.Communtron4 = dst.Communtron4;
							drl.CommuntronM1 = dst.CommuntronM1;
							drl.SC_slots = dst.SC_slots;
							drl.SC_control = dst.SC_control;
							drl.SC_upgrades = dst.SC_upgrades;
							drl.SC_data = dst.SC_data;
							drl.SC_asteroid = dst.SC_asteroid;
						}
						else drl = st_structs[current].GetComponent<SC_drill>();

						drl.type = prdrill;
						drl.freeze = true;
					}
					else throw(new Exception());
				}
				else if(arg[i]=="list")
				{
					i++;
					SC_seon_remote ssr;
					if(st_structs[current].GetComponent<SC_seon_remote>() == null)
					{
						ssr = st_structs[current].gameObject.AddComponent<SC_seon_remote>();
						ssr.SC_structure = transform.GetComponent<SC_structure>();
					}
					else ssr = st_structs[current].GetComponent<SC_seon_remote>();

					if(arg[i]=="hidden")
					{
						i++;
						if(arg[i]=="undone")
						{
							i++;
							string[] arr = arg[i].Split(',');
							int j,jngt=arr.Length;
							for(j=0;j<jngt;j++) ssr.HideStateSet(arr[j],0);
						}
						else if(arg[i]=="doing")
						{
							i++;
							string[] arr = arg[i].Split(',');
							int j,jngt=arr.Length;
							for(j=0;j<jngt;j++) ssr.HideStateSet(arr[j],1);
						}
						else if(arg[i]=="done")
						{
							i++;
							string[] arr = arg[i].Split(',');
							int j,jngt=arr.Length;
							for(j=0;j<jngt;j++) ssr.HideStateSet(arr[j],2);
						}
						else if(arg[i]=="undoing")
						{
							i++;
							string[] arr = arg[i].Split(',');
							int j,jngt=arr.Length;
							for(j=0;j<jngt;j++) ssr.HideStateSet(arr[j],3);
						}
						else throw(new Exception());
					}
					else if(arg[i]=="extended")
					{
						i++;
						if(arg[i]=="undone")
						{
							i++;
							string[] arr = arg[i].Split(',');
							int j,jngt=arr.Length;
							for(j=0;j<jngt;j++) ssr.ExtendedStateSet(arr[j],0);
						}
						else if(arg[i]=="doing")
						{
							i++;
							string[] arr = arg[i].Split(',');
							int j,jngt=arr.Length;
							for(j=0;j<jngt;j++) ssr.ExtendedStateSet(arr[j],1);
						}
						else if(arg[i]=="done")
						{
							i++;
							string[] arr = arg[i].Split(',');
							int j,jngt=arr.Length;
							for(j=0;j<jngt;j++) ssr.ExtendedStateSet(arr[j],2);
						}
						else if(arg[i]=="undoing")
						{
							i++;
							string[] arr = arg[i].Split(',');
							int j,jngt=arr.Length;
							for(j=0;j<jngt;j++) ssr.ExtendedStateSet(arr[j],3);
						}
						else throw(new Exception());
					}
					else throw(new Exception());
				}
				else if(arg[i]=="extension")
				{
					i++;
					SC_seon_remote ssr;
					if(st_structs[current].GetComponent<SC_seon_remote>() == null)
					{
						ssr = st_structs[current].gameObject.AddComponent<SC_seon_remote>();
						ssr.SC_structure = transform.GetComponent<SC_structure>();
					}
					else ssr = st_structs[current].GetComponent<SC_seon_remote>();

					float dX = float.Parse(arg[i]);

					i++;
					float dY = float.Parse(arg[i]);

					i++;
					int dT = int.Parse(arg[i]);
					if(dT<1) dT=1;

					ssr.extending_time = dT;
					ssr.extension = new Vector3(dX,dY,0f);
				}
				else if(arg[i]=="hidesmooth")
				{
					i++;
					SC_seon_remote ssr;
					if(st_structs[current].GetComponent<SC_seon_remote>() == null)
					{
						ssr = st_structs[current].gameObject.AddComponent<SC_seon_remote>();
						ssr.SC_structure = transform.GetComponent<SC_structure>();
					}
					else ssr = st_structs[current].GetComponent<SC_seon_remote>();

					float dZ = float.Parse(arg[i]);

					i++;
					int dT = int.Parse(arg[i]);
					if(dT<1) dT=1;

					ssr.hiding_time = dT;
					ssr.hidevector = new Vector3(0f,0f,dZ);
				}
				else if(arg[i]=="steal")
				{
					i++;
					int pramount = int.Parse(arg[i]);
					if(pramount > 10) pramount = 10;

					i++;
					if(arg[i]=="from")
					{
						i++;
						int prsteal;
						if(arg[i]=="delta")
						{
							i++;
							prsteal = current + int.Parse(arg[i]);
						}
						else
						{
							prsteal = HashConvert(arg[i]);
						}

						st_structs[prsteal].position = st_structs[current].position;
						st_structs[prsteal].eulerAngles = st_structs[current].eulerAngles + new Vector3(0f,0f,-90f);

						st_structs[prsteal].GetComponent<Renderer>().enabled = false;
						st_structs[prsteal].GetComponent<SphereCollider>().enabled = false;

						float ddY = st_structs[current].localScale.x;
						float ddX = st_structs[current].localScale.y;
						float angle = st_structs[prsteal].eulerAngles.z;

						SC_asteroid ast = st_structs[prsteal].GetComponent<SC_asteroid>();

						int j;
						float dxom = -0.85f*(pramount-1);
						for(j=0;j<pramount;j++)
						{
							ast.fobCenPos[2*j+0] = true;
							ast.fobCenPos[2*j+1] = true;
							ast.fobInfoPos[2*j+0] = ast.transform.position + new Vector3(0f,0f,0f);
							ast.fobInfoPos[2*j+1] = ast.transform.position + new Vector3(0f,0f,0f);

							ast.fobCenRot[2*j+0] = true;
							ast.fobCenRot[2*j+1] = true;
							ast.fobInfoRot[2*j+0] = -angle;
							ast.fobInfoRot[2*j+1] = -angle + 180f;

							float dX,dY,ankle;
							
							ankle = angle * 3.14159f/180f;
							dX = (dxom + 2f*0.85f*j);
							dY = ddY*1.5f/st_structs[prsteal].localScale.y;
							ast.fobInfoPos[2*j+0] += new Vector3(
								Mathf.Cos(ankle)*dX + Mathf.Cos(ankle+3.14159f/2)*dY,
								Mathf.Sin(ankle)*dX + Mathf.Sin(ankle+3.14159f/2)*dY,
								0f
							);

							ankle = angle * 3.14159f/180f;
							dX = (dxom + 2f*0.85f*j);
							dY = -ddY*1.5f/st_structs[prsteal].localScale.y;
							ast.fobInfoPos[2*j+1] += new Vector3(
								Mathf.Cos(ankle)*dX + Mathf.Cos(ankle+3.14159f/2)*dY,
								Mathf.Sin(ankle)*dX + Mathf.Sin(ankle+3.14159f/2)*dY,
								0f
							);
						}
						for(j=pramount;j<10;j++)
						{
							ast.fobCenPos[2*j+0] = true;
							ast.fobCenPos[2*j+1] = true;
							ast.fobInfoPos[2*j+0] = ast.transform.position + new Vector3(0f,0f,-500f);
							ast.fobInfoPos[2*j+1] = ast.transform.position + new Vector3(0f,0f,-500f);
						}
					}
					else throw(new Exception());
				}
				else if(arg[i]=="asteroid")
				{
					i++;
					if(arg[i]=="hide")
					{
						st_structs[current].GetComponent<Renderer>().enabled = false;
						st_structs[current].GetComponent<SphereCollider>().enabled = false;
					}
					else if(arg[i]=="show")
					{
						st_structs[current].GetComponent<Renderer>().enabled = true;
						st_structs[current].GetComponent<SphereCollider>().enabled = true;
					}
					else if(arg[i]=="blocker")
					{
						i++;
						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();

						if(arg[i]=="enable")
						{
							ast.permanent_blocker = true;
						}
						else if(arg[i]=="disable")
						{
							ast.permanent_blocker = false;
						}
						else throw(new Exception());
					}
					else throw(new Exception());
				}
			}
			catch(Exception) {
				i--;
				continue;
			}
		}
		for(i=0;i<1024;i++)
		{
			if(st_structs[i]!=null)
				if(
					st_structs[i].GetComponent<SC_asteroid>()!=null &&
					st_structs[i].GetComponent<SC_seon_remote>()==null &&
					st_structs[i].parent.GetComponent<SC_seon_remote>()!=null
				){
					SC_seon_remote ssr = st_structs[i].gameObject.AddComponent<SC_seon_remote>();
					ssr.SC_structure = transform.GetComponent<SC_structure>();
				}
		}
	}
}
