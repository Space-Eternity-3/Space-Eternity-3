using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SC_fun : MonoBehaviour
{
    public Transform Communtron1;
    public Material[] M = new Material[128];
    public Texture[] Item = new Texture[128];
    public Texture Item20u;
    public float volume;
    public int seed;
    public bool halloween_theme;
    public int smooth_size;
    public string[] GenLists = new string[2];
    public float biome_sector_size;
    public bool[] pushed_markers = new bool[9];
    public Transform[] structures;
    public Transform[] structures2;
    public SC_control SC_control;
    public SC_data SC_data;
    public SC_long_strings SC_long_strings;

    public void SeedSet(int world)
    {
        if(world!=100)
        {
            if(!Directory.Exists("../../saves/Universe"+world+"/"))
            {
                UnityEngine.Debug.LogError("Not existing universe is openned.");
                SC_control.MenuReturn();
            }
            if(SC_data.seed!="") seed=int.Parse(SC_data.seed);
            else
            {
                seed=UnityEngine.Random.Range(0,10000000);
                SC_data.seed=seed+"";
                SC_data.Save("seed");
            }
        }
        else
        {
            if(SC_data.TempFileConID[8]!="") seed=int.Parse(SC_data.TempFileConID[8]);
            else SC_control.MenuReturn();
        }
    }
    int MakeID(int ID,int sed)
    {
        int sid=sed*2;
        return ID+sid;
    }
    public int CheckID(int X,int Y)
    {
        int ID=0,P;
        if(Mathf.Abs(X)>Mathf.Abs(Y))
		{
			P=Mathf.Abs(X);
		}
		else
        {
			P=Mathf.Abs(Y);
		}
		ID=4*(P*P-P)+1;

		X=X+P+1;
		Y=Y+P+1;

		if(X==(2*P+1)&&Y!=1) //first
		{
			ID=ID+Y-1;
		} else
		if(Y==(2*P+1)) //second
		{
			ID=ID+4*P+1-X;
		} else
		if(X==1) //third
		{
			ID=ID+6*P+1-Y;
		} else
		if(Y==1) //fourth
		{
			ID=ID+6*P+X-1;
		}
        return ID;
    }
    public void GenListAdd(int ID,int legsID)
    {
        GenLists[legsID]=GenLists[legsID]+ID+";";
    }
    public void GenListRemove(int ID,int legsID)
    {
        if(GenListContains(ID,legsID))
        {
            string[] list = GenLists[legsID].Split(';');
            int i,lngt=list.Length-1;
            string newString=""; bool found=false;
            for(i=0;i<lngt;i++)
            {
                if(int.Parse(list[i])!=ID) newString=newString+list[i]+";";
                else found=true;
            }
            if(!found) UnityEngine.Debug.Log("Game crashed! Element not found.");
            else GenLists[legsID]=newString;
        }
    }
    public bool GenListContains(int ID,int legsID)
    {
        string[] list = GenLists[legsID].Split(';');
        int i,lngt=list.Length-1;
        for(i=0;i<lngt;i++)
        {
            if(int.Parse(list[i])==ID) return true;
        }
        return false;
    }
    public bool AsteroidCheck(int ID)
    {
        if(GenListContains(ID,0)) return false;
        int IDm=MakeID(ID,seed);
        if(ID==1) return false;
        if(IDm%2==0) return false;
        char it=SC_long_strings.AsteroidBase[(IDm%65536-1)/2];
        if(it=='V'||it=='R') return true;
        else return false;
    }
    public char AsteroidChar(int ID)
    {
        int IDm=MakeID(ID,seed);
        return SC_long_strings.AsteroidBase[(IDm%65536-1)/2];
    }
    public bool StructureCheck(int ID)
    {
        if(GenListContains(ID,1)) return false;
        string gbs=GetBiomeString(ID);
        if(gbs!="") if(gbs[0]=='v') return true;
        return false;
    }
    public int GetSize(int ID)
    {
        switch(MakeID(ID,seed)%32)
        {
            case 1: return 4;
            case 3: return 5;
            case 5: return 6;
            case 7: return 7;
            case 9: return 7;
            case 11: return 8;
            case 13: return 8;
            case 15: return 9;
            case 17: return 4;
            case 19: return 5;
            case 21: return 10;
            case 23: return 7;
            case 25: return 6;
            case 27: return 4;
            case 29: return 5;
            case 31: return 6;
            default: return 4;
        }
    }
    public string GetMove(int X,int Y)
    {
        return LocalMove(CheckID(X,Y));
    }
    public string LocalMove(int ID)
    {
        int ird=(MakeID(ID,seed)*2)%18;
        switch(ird)
        {
            case 0: return "0;0";
            case 4: return "0;1";
            case 16: return "0;2";
            case 6: return "1;0";
            case 14: return "1;1";
            case 2: return "1;2";
            case 10: return "2;0";
            case 8: return "2;1";
            case 12: return "2;2";
            default: return "0;0"; 
        }
    }

//-----------------------------------------------------------------------------------------------

    public float GetBiomeSize(int ulam)
    {
        string typ=GetBiomeString(ulam);
        if(typ=="") return 0f;
        if(typ=="void1") return 30f;
        else
        {
            if(ulam%3==0) return 60f;
            if(ulam%3==1) return 70f;
            if(ulam%3==2) return 80f;
            UnityEngine.Debug.Log("Game crashed! Wrong divider: "+ulam+";"+(ulam%3));
            return 60f;
        }
    }
    public Vector3 GetBiomeMove(int ulam)
    {
        float size=GetBiomeSize(ulam);
        string red=LocalMove(ulam);
        float dX=(int.Parse(red.Split(';')[0])-1)*(biome_sector_size/2f-size);
        float dY=(int.Parse(red.Split(';')[1])-1)*(biome_sector_size/2f-size);
        return new Vector3(dX,dY,0f);
    }
    public string GetBiomeString(int ulam)
    {
        int IDm=ulam+seed;
        char cha=SC_long_strings.BiomeBase[(IDm%32768-1)];
        if(ulam==1||cha=='O') return "";

        if(cha=='A') return "b1";
        if(cha=='B') return "b2";
        if(cha=='C') return "b3";
        if(cha=='V') return "void1";

        UnityEngine.Debug.Log("Game crashed! Char '"+cha+"' doesn't exists in code.");
        return "";
    }
}
