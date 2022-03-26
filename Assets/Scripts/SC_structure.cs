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
	public Transform[] st_structs = new Transform[50];
	bool mother = true;
	
	void Start()
	{
		if(transform.position.z<=100f) mother = false; else return;
		transform.position += SC_fun.GetBiomeMove(ID);
		
		int i,lngt=st_commands.Length;
		for(i=0;i<lngt;i++) CommandDo((ConvCmd(st_commands[i])+"          ").Split(' '),i);
	}
	void Update()
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
				SC_fun.GenListRemove(ID,1);
				Destroy(gameObject);
			}
		}
	}
	string uzup20(string str)
	{
		string[] strs = str.Split(';');
		int i,lngt=strs.Length;
		for(i=lngt;i<20;i++) str+=";";
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
				st_structs[index] = arc;
			}
			if(cmds[4]=="gat"||cmds[4]=="gar")
			{
				Transform gat = Instantiate(gatepart,transform.position+dpos+curpos,Quaternion.Euler(0f,0f,zr));
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
