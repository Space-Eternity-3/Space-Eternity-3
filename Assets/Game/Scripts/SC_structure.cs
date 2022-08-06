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
	
    public int X=0,Y=0,ID=1;
	public float c_multiplier, zwalnum;
	public string[] st_commands = new string[50];
	[TextArea(15,20)]
	public string SeonField = "";
	public Transform[] st_structs = new Transform[1024];

	int counter_to_destroy = 200;
	bool mother = true;
	
	string TxtToSeonArray(string str)
	{
		int i,lngt=str.Length;
		string effect = "";
		for(i=0;i<lngt;i++)
		{
			if(str[i]!='\t' && str[i]!='\r' && str[i]!='\n')
				effect += str[i];
			else
				effect += ' ';
		}
		str = effect+"x"; effect="";
		for(i=0;i<lngt;i++)
		{
			if((str[i]==' ' && str[i+1]!=' ') || str[i]!=' ')
				effect += str[i];
		}
		return effect;
	}
	void Start()
	{
		if(transform.position.z<=100f) mother = false; else return;
		transform.position += SC_fun.GetBiomeMove(ID);
		
		//OLD SEON
		int i,lngt=st_commands.Length;
		for(i=0;i<lngt;i++) CommandDo((ConvCmd(st_commands[i])+"          ").Split(' '),i);

		//SEON
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
		if(str=="x"||str=="X") str=";";
		int junk;
		string[] strs = str.Split(';');
		int i,lngt=strs.Length;
		for(i=0;i<lngt;i++)
			if(strs[i]!="")
				junk = int.Parse(strs[i]);
		for(i=lngt;i<20;i++)
			str+=";";
		return str;
	}
	string ConvCmd(string cmd)
	{
		string ret = "";
		int spaces = 0;
		
		int i,j,lngt=cmd.Length;
		for(i=0;i<lngt;i++)
		{
			if(cmd[i]==' ') spaces++;
			if(cmd[i]=='|')
			{
				for(j=spaces%7;j<7;j++)
				{
					ret+=" ";
					spaces++;
				}
				continue;
			}
			ret+=cmd[i];
		}
		
		return ret;
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
		string[] arg = seon_string.Split(' ');
		int i,lngt=arg.Length;
		int current=-1, findex=-1;
		for(i=0;i<lngt;i++)
		{
			try {

				//Catching commands
				if(arg[i]=="summon")
				{
					i++;
					current = HashConvert(arg[i]);

					i++;
					if(arg[i]=="asteroid")
					{
						i++;
						int prsize = int.Parse(arg[i]);
						if(prsize<4 || prsize>10) prsize = 4;

						i++;
						int prtype = int.Parse(arg[i]);

						i++;
						string prfobs = uzup20(arg[i]);

						SC_asteroid ast = Instantiate(asteroid,transform.position,Quaternion.identity).GetComponent<SC_asteroid>();
						ast.GetComponent<Transform>().parent = transform;
						st_structs[current] = ast.GetComponent<Transform>();
				
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

						wal.GetComponent<Renderer>().material = SC_fun.st_materials[prmaterial];
					}
					else if(arg[i]=="piston")
					{
						Transform gat = Instantiate(gatepart,transform.position,Quaternion.identity);
						gat.parent = transform;
						st_structs[current] = gat;
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
					else
					{
						current = HashConvert(arg[i]);
					}
				}

				//Transform commands
				else if(arg[i]=="move")
				{
					i++;
					if(arg[i]=="child")
					{
						i++;
						float dX = float.Parse(arg[i]);

						i++;
						float dY = float.Parse(arg[i]);

						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();

						float angle = -ast.fobInfoRot[findex];
						angle *= 3.14159f/180f;

						if(!ast.fobCenPos[findex])
						{
							ast.fobInfoPos[findex] = st_structs[current].position;
							ast.fobCenPos[findex] = true;
						}
						ast.fobInfoPos[findex] += new Vector3(
							Mathf.Cos(angle)*dX + Mathf.Cos(angle+3.14159f/2)*dY,
							Mathf.Sin(angle)*dX + Mathf.Sin(angle+3.14159f/2)*dY,
							0f
						);
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
					if(arg[i]=="child")
					{
						i++;
						float dA = float.Parse(arg[i]);

						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();
						
						if(!ast.fobCenRot[findex])
						{
							ast.fobInfoRot[findex] = 0f;
							ast.fobCenRot[findex] = true;
						}
						ast.fobInfoRot[findex] -= dA;
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
					if(arg[i]=="child")
					{
						i++;
						float nX = float.Parse(arg[i]);

						i++;
						float nY = float.Parse(arg[i]);

						i++;
						float nZ = float.Parse(arg[i]);

						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();

						if(!ast.fobCenScale[findex])
						{
							ast.fobCenScale[findex] = true;
						}
						ast.fobInfoScale[findex] = new Vector3(nX,nY,nZ);
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
					if(arg[i]=="child")
					{
						i++;
						float nZ = float.Parse(arg[i]);
						st_structs[current].GetComponent<SC_asteroid>().fobInfoPosZ[findex] = new Vector3(0f,0f,nZ);
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
					if(arg[i]=="child")
					{
						i++;
						SC_asteroid ast = st_structs[current].GetComponent<SC_asteroid>();
						if(arg[i]=="position")
						{
							ast.fobCenPos[findex] = true;
							ast.fobInfoPos[findex] = transform.position;
						}
						else if(arg[i]=="rotation")
						{
							ast.fobCenRot[findex] = true;
							ast.fobInfoRot[findex] = 0f;
						}
						else if(arg[i]=="localpos")
						{
							ast.fobCenPos[findex] = true;
							ast.fobInfoPos[findex] = st_structs[current].position;
						}
						else throw(new Exception());
					}
					else if(arg[i]=="position")
					{
						st_structs[current].position = transform.position;
					}
					else if(arg[i]=="rotation")
					{
						st_structs[current].rotation = Quaternion.identity;
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
			}
			catch(Exception) {
				i--;
				continue;
			}
		}
	}
	//-------------------------
	//RELICT BUT STILL WORKING AND REQUIRED
	//-------------------------
	void CommandDo(string[] cmds, int index)
	{
		//gen [pX;pY;pZ] [rZ] [sX;sY;sZ] -> (...)
			// (...) ast/asr [size;type] [fobCode?] <cmdn>
			// (...) wal [material] - <cmdn>
			
		//cmdns:
		//fob [index] -> (...)
			// (...) mov [X;Y;Z] - - - <cmdn>
			// (...) rot [rZ] - - - <cmdn>
		
		//example:
		//gen 10;10;0 0 1;1;1 ast 6;10 - - fob 1 rot st 90 - - fob 1 mov ch 10;0;0 - - fob 1 rrt - - - - fob 1 set 42 - - -
		
		if(cmds.Length==0) return;
		if(cmds[0]=="gen")
		{
			float[] zp = new float[3];
			float[] zs = new float[3];
			float zr;
			
			if(cmds[1][0]!='c'){
				zp[0] = float.Parse(cmds[1].Split(';')[0]);
				zp[1] = float.Parse(cmds[1].Split(';')[1]);
				zp[2] = float.Parse(cmds[1].Split(';')[2]);
			}
			else switch(cmds[1])
			{
				case "c1":
					zp[0]=c_multiplier;
					zp[1]=c_multiplier;
					zp[2]=0f;
					break;
				case "c2":
					zp[0]=-c_multiplier;
					zp[1]=c_multiplier;
					zp[2]=0f;
					break;
				case "c3":
					zp[0]=-c_multiplier;
					zp[1]=-c_multiplier;
					zp[2]=0f;
					break;
				case "c4":
					zp[0]=c_multiplier;
					zp[1]=-c_multiplier;
					zp[2]=0f;
					break;
				default:
					zp[0]=0f;
					zp[1]=0f;
					zp[2]=0f;
					break;
			}
			
			zr = float.Parse(cmds[2]);
			
			zs[0] = float.Parse(cmds[3].Split(';')[0]);
			zs[1] = float.Parse(cmds[3].Split(';')[1]);
			zs[2] = float.Parse(cmds[3].Split(';')[2]);
			
			Vector3 dpos, curpos;
			dpos = new Vector3(zp[0],zp[1],zp[2]);
			float radangle = (zs[0]*3.14159f)/180f;
			
			if(cmds[4]!="asr" && cmds[4]!="war" && cmds[4]!="arr" && cmds[4]!="gar") curpos = new Vector3(0f,0f,0f);
			else curpos = zs[1] * new Vector3(Mathf.Cos(radangle),Mathf.Sin(radangle),0f);
			
			if(cmds[4]=="ast"||cmds[4]=="asr")
			{
				int[] astdat = new int[2];
				astdat[0] = int.Parse(cmds[5].Split(';')[0]);
				astdat[1] = int.Parse(cmds[5].Split(';')[1]);
				
				SC_asteroid ast = Instantiate(asteroid,transform.position+dpos+curpos,Quaternion.Euler(0f,0f,zr)).GetComponent<SC_asteroid>();
				ast.GetComponent<Transform>().parent = transform;
				st_structs[index] = ast.GetComponent<Transform>();
				
				int[] sXY = BuildXY(index);
				ast.proto = true; ast.X=sXY[0]; ast.Y=sXY[1]; ast.ID=SC_fun.CheckID(ast.X,ast.Y);
				ast.protsize = astdat[0];
				ast.prottype = astdat[1];
				ast.fobCode = uzup20(cmds[6]);
			}
			if(cmds[4]=="wal"||cmds[4]=="war")
			{
				int waldata0 = int.Parse(cmds[5]);
				
				Transform trn = Instantiate(stwall,transform.position+dpos+curpos,Quaternion.Euler(0f,0f,zr));
				trn.parent = transform;
				st_structs[index] = trn;
				
				trn.GetComponent<Renderer>().material = SC_fun.st_materials[waldata0];
			}
			if(cmds[4]=="arc"||cmds[4]=="arr")
			{
				Transform arc = Instantiate(arcen,transform.position+dpos+curpos,Quaternion.Euler(0f,0f,zr));
				arc.parent = transform;
				st_structs[index] = arc;
			}
			if(cmds[4]=="gat"||cmds[4]=="gar")
			{
				Transform gat = Instantiate(gatepart,transform.position+dpos+curpos,Quaternion.Euler(0f,0f,zr));
				gat.parent = transform;
				st_structs[index] = gat;
			}
		}
		else if(cmds[0]=="fob")
		{
			int findex = int.Parse(cmds[1]);
			SC_asteroid ast = st_structs[index].GetComponent<SC_asteroid>();
			
			if(cmds[2]=="mov")
			{
				float[] zp = new float[3];
				
				if(cmds[3][0]!='c'){
				zp[0] = float.Parse(cmds[3].Split(';')[0]);
				zp[1] = float.Parse(cmds[3].Split(';')[1]);
				zp[2] = float.Parse(cmds[3].Split(';')[2]);
				}
				else switch(cmds[3])
				{
					case "c1":
						zp[0]=c_multiplier;
						zp[1]=c_multiplier;
						zp[2]=0f;
						break;
					case "c2":
						zp[0]=-c_multiplier;
						zp[1]=c_multiplier;
						zp[2]=0f;
						break;
					case "c3":
						zp[0]=-c_multiplier;
						zp[1]=-c_multiplier;
						zp[2]=0f;
						break;
					case "c4":
						zp[0]=c_multiplier;
						zp[1]=-c_multiplier;
						zp[2]=0f;
						break;
					default:
						zp[0]=0f;
						zp[1]=0f;
						zp[2]=0f;
						break;
				}
				
				if(cmds[4]=="cen")
				{
					ast.fobCenPos[findex] = true;
					ast.fobInfoPos[findex] = transform.position;
					ast.fobInfoPos[findex] += new Vector3(zp[0],zp[1],zp[2]);
				}
				else if(cmds[4]=="cer")
				{
					float[] zsr = new float[2];
					zsr[0] = float.Parse(cmds[5].Split(';')[0]);
					zsr[1] = float.Parse(cmds[5].Split(';')[1]);
					zsr[0] *= 3.14159f/180f;
					
					Vector3 curposr = zsr[1] * new Vector3(Mathf.Cos(zsr[0]),Mathf.Sin(zsr[0]),0f);
					
					ast.fobCenPos[findex] = true;
					ast.fobInfoPos[findex] = transform.position;
					ast.fobInfoPos[findex] += new Vector3(zp[0],zp[1],zp[2]) + curposr;
				}
				else if(cmds[4]=="rel") ast.fobInfoPosrel[findex] += new Vector3(zp[0],zp[1],zp[2]);
				else ast.fobInfoPos[findex] += new Vector3(zp[0],zp[1],zp[2]);
			}
			if(cmds[2]=="rot")
			{
				float zr = float.Parse(cmds[3]);
				
				ast.fobInfoRot[findex] = zr;
				if(cmds[4]=="cen") ast.fobCenRot[findex] = true;
			}
		}
		else return;
		
		//CommandDo other
		int i, lngt = cmds.Length;
		string[] narray = new string[2048];
		for(i=7;i<lngt;i++){
			narray[i-7] = cmds[i];
		}
		if(i!=7) CommandDo(narray,index);
	}
}
