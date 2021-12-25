using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SC_backpack : MonoBehaviour
{
    public Button[] buttons = new Button[21];
    public Transform[] darknero = new Transform[21];
    Vector3[] darkPos = new Vector3[21];

    public SC_fun SC_fun;
    public SC_upgrades SC_upgrades;
    public SC_control SC_control;
    public SC_data SC_data;
    public SC_slots SC_slots;
	public SC_artefacts SC_artefacts;

    public Transform Communtron4;

    string worldDIR;
    public bool destroyLock=false;

    public void Select(int n,bool all)
    {
        int xx = SC_slots.BackpackX[n];
        int yy = SC_slots.BackpackY[n];
		if(yy==0) return;

        if(yy>0&&!all) yy=1;
        
		if(n!=15)
		{
			if(yy!=0&&SC_slots.InvHaveB(xx,1,true,false,true,0)&&SC_slots.InvHaveB(xx,-yy,false,true,true,0))
			{
				int slI = SC_slots.InvChange(xx,yy,true,false,true);
				int slB = SC_slots.InvChange(xx,-yy,false,true,true);
				
				if((int)Communtron4.position.y==100) SC_control.SendMTP("/Backpack "+SC_control.connectionID+" "+xx+" "+yy+" "+slI+" "+slB);
			}
			else SC_slots.InvHaveB(-1,-1,true,true,false,2);
		}
		else
		{
			if(yy!=0&&SC_slots.InvHaveB(xx,1,true,false,true,0)&&SC_slots.BackpackX[15]==xx&&SC_slots.BackpackYA[15]>0)
			{
				int slI = SC_slots.InvChange(xx,yy,true,false,true);
				int slB = 15+9;
				
				SC_slots.BackpackX[15] = xx;
				SC_slots.BackpackY[15] -= yy; //sureMTP
				SC_slots.BackpackYA[15] -= yy;
				SC_slots.BackpackYB[15] -= yy;
				SC_slots.PopInv(15+9,-1);
				
				if((int)Communtron4.position.y==100) SC_control.SendMTP("/Backpack "+SC_control.connectionID+" "+xx+" "+yy+" "+slI+" "+slB);
			}
			else SC_slots.InvHaveB(-1,-1,true,true,false,2);
		}
    }
    public void Import(int n,bool all)
    {
        destroyLock=true;

        int xx = SC_slots.SlotX[n-1];
        int yy = SC_slots.SlotY[n-1];

        if(yy>0&&!all) yy=1;
        
        if(yy!=0&&SC_slots.InvHaveB(xx,1,false,true,true,0)&&SC_slots.InvHaveB(xx,-yy,true,false,true,0))
        {
            int slI = SC_slots.InvChange(xx,-yy,true,false,true);
            int slB = SC_slots.InvChange(xx,yy,false,true,true);
            
			if((int)Communtron4.position.y==100) SC_control.SendMTP("/Backpack "+SC_control.connectionID+" "+xx+" "+(-yy)+" "+slI+" "+slB);
        }
    }
	public void ImportArt(int n)
	{
		destroyLock=true;
		
		int xx = SC_slots.SlotX[n-1];
		int yy = SC_slots.SlotY[n-1];
		
		if(yy>0) yy=1;
		
		if(yy==1 && SC_upgrades.MTP_levels[4]>=5 && SC_slots.InvHaveB(xx,-yy,true,false,true,0) && SC_slots.BackpackY[15]==0 && SC_artefacts.IsArtefact(xx))
		{
			int slI = SC_slots.InvChange(xx,-yy,true,false,true);
			int slB = 15+9;
			
			SC_slots.BackpackX[15] = xx;
			SC_slots.BackpackY[15] += yy; //sureMTP
			SC_slots.BackpackYA[15] += yy;
			SC_slots.BackpackYB[15] += yy;
			SC_slots.PopInv(15+9,1);
			
			if((int)Communtron4.position.y==100) SC_control.SendMTP("/Backpack "+SC_control.connectionID+" "+xx+" "+(-yy)+" "+slI+" "+slB);
		}
	}
    void LateUpdate()
    {
        int i,lim=Lim();
        for(i=0;i<21;i++)
        {
			//virtual mode: one "if part" deleted to simplify code
            if((i<lim||i==15)&&SC_slots.BackpackY[i]!=0&&SC_slots.InvHaveB(SC_slots.BackpackX[i],1,true,false,false,0)) buttons[i].interactable=true;
            else buttons[i].interactable=false;

            if(i<lim||i==15) darknero[i].localPosition=new Vector3(10000f,0f,0f);
            else darknero[i].localPosition=darkPos[i];
        }
    }
    void Start()
    {
        int i;
        for(i=0;i<21;i++) darkPos[i]=darknero[i].localPosition;
    }
    public void MTP_loadBackpack(string s)
    {
        string[] ss = s.Split(';');
        int i;
        for(i=0;i<21;i++)
        {
            SC_slots.BackpackX[i]=int.Parse(ss[2*i]);
            SC_slots.BackpackY[i]=int.Parse(ss[2*i+1]);
        }
    }
    void Update()
    {
        if(Input.GetMouseButtonUp(0)) destroyLock=false;
    }
    public int Lim()
    {
        int lvl = SC_upgrades.MTP_levels[4];
        if(lvl<5) return 3*(lvl+1);
        else return 3*5;
    }
}

//Add and Remove backpack functions -> return used slot
