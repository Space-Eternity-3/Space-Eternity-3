using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_structure : MonoBehaviour
{
	public SC_fun SC_fun;
	public Transform legs;
	public Transform Communtron1;
	public Transform asteroid;
	public Transform stwall;
    public int X=0,Y=0,ID=1;
	public string[] st_commands = new string[50];
	bool mother = true;
	
	void Start()
	{
		if(transform.position.z<=100f) mother = false; else return;
		transform.position += SC_fun.GetBiomeMove(ID);
		
		int i,lngt=st_commands.Length;
		for(i=0;i<lngt;i++) CommandDo(st_commands[i],i);
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
	void CommandDo(string cmd, int index)
	{
		string[] cmds = cmd.Split(' ');
		
		if(cmds[0]=="gen")
		{
			if(cmds[1]=="ast")
			{
				SC_asteroid ast = Instantiate(asteroid,transform.position + new Vector3(float.Parse(cmds[2]),float.Parse(cmds[3]),0f),Quaternion.identity).GetComponent<SC_asteroid>();
				ast.GetComponent<Transform>().parent = transform;
				
				int[] sXY = BuildXY(index);
				ast.proto = true; ast.X=sXY[0]; ast.Y=sXY[1]; ast.ID=SC_fun.CheckID(ast.X,ast.Y);
				ast.protsize = float.Parse(cmds[4]);
				ast.prottype = int.Parse(cmds[5]);
			}
			if(cmds[1]=="wal")
			{
				Transform trn = Instantiate(stwall,transform.position + new Vector3(float.Parse(cmds[2]),float.Parse(cmds[3]),0f),Quaternion.Euler(0f,0f,float.Parse(cmds[4])));
				float scaleX = float.Parse(cmds[5].Split(';')[0]);
				float scaleY = float.Parse(cmds[5].Split(';')[1]);
				float scaleZ = float.Parse(cmds[5].Split(';')[2]);
				trn.localScale = new Vector3(scaleX,scaleY,scaleZ);
				trn.parent = transform;
				trn.GetComponent<Renderer>().material = SC_fun.st_materials[int.Parse(cmds[6])];
			}
		}
	}
}
