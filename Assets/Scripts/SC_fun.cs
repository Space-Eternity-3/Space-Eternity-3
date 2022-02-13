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
	
	public int[] bW = new int[32];
	public int[] bMI = new int[32];
	public int[] bMA = new int[32];
	public int[] bD = new int[32];
	
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
		
		float[] dau = GetBiomeDAU(ID);
		float distance = dau[0];
		int ulam = (int)dau[1];
		
		string biost = GetBiomeString(ulam);
		float size = GetBiomeSize(ulam);
		string tags = GetBiomeTag(ulam);
		int bid = int.Parse(biost.Split('b')[1]);
		int locD = bD[bid];
		
		if((distance<size) && biost!="b0")
		{
			if(TagContains(tags,"circle") && !TagContains(tags,"full") && distance<size-GetCircleWide(biost)) return false;
		}
		else locD = bD[0];
		
		if(it=='a' && locD>=1) return true;
		if(it=='b' && locD>=2) return true;
		if(it=='c' && locD>=3) return true;
		if(it=='d' && locD>=4) return true;
		if(it=='e' && locD>=5) return true;
		if(it=='f' && locD>=6) return true;
		if(it=='g' && locD>=7) return true;
		if(it=='h' && locD>=8) return true;
		if(it=='i' && locD>=9) return true;
		if(it=='j' && locD>=10) return true;
		return false;
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
	int[] UlamToXY(int ulam)
	{
		int[] retu = new int[2];
		
		int sqrt=(int)Mathf.Sqrt(ulam);
		if(sqrt%2==0) sqrt--;
		int x=sqrt/2+1,y=-sqrt/2-1;
		int pot=sqrt*sqrt;
		int delta=ulam-pot;
		int cwr=delta/(sqrt+1);
		int dlt=delta%(sqrt+1);
		if(cwr==0&&dlt==0)
		{
			retu[0]=x-1; retu[1]=y+1;
			return retu;
		}
  
		if(cwr>0) y+=(sqrt+1);
		if(cwr>1) x-=(sqrt+1);
		if(cwr>2) y-=(sqrt+1);

		if(cwr==0) y+=dlt;
		if(cwr==1) x-=dlt;
		if(cwr==2) y-=dlt;
		if(cwr==3) x+=dlt;

		retu[0]=x; retu[1]=y;
		return retu;
	}

//-----------------------------------------------------------------------------------------------

    public float GetCircleWide(string typ)
	{
		int i;
		return bW[int.Parse(typ.Split('b')[1])];
	}
	public float GetBiomeSize(int ulam)
    {
        string typ=GetBiomeString(ulam);
		string tags=GetBiomeTag(ulam);
		
        if(typ=="b0") return 0f;
		if(TagContains(tags,"full")) return 10000f;
		
		int i,j;
		float min, max;
		
		min = bMI[int.Parse(typ.Split('b')[1])];
		max = bMA[int.Parse(typ.Split('b')[1])];
		
		if(min>max)
		{
			float pom=min;
			min=max; max=pom;
		}
		
		int ps_rand = (ulam+seed) % (((int)max-(int)min)+1);
		float ret = min + ps_rand;
		return ret;
    }
    public Vector3 GetBiomeMove(int ulam)
    {
        float size=GetBiomeSize(ulam);
		if(size>1000f) return new Vector3(0f,0f,0f);
        string red=LocalMove(ulam);
        float dX=(int.Parse(red.Split(';')[0])-1)*(80f-size);
        float dY=(int.Parse(red.Split(';')[1])-1)*(80f-size);
        return new Vector3(dX,dY,0f);
    }
	public int pseudoRandom1000(int prSeed)
	{
		prSeed = prSeed % 10000;
		string psInt = (SC_long_strings.BiomeNewBase[3*prSeed+0]+"") + (SC_long_strings.BiomeNewBase[3*prSeed+1]+"") + (SC_long_strings.BiomeNewBase[3*prSeed+2]+"");
		return int.Parse(psInt);
	}
    public string GetBiomeString(int ulam)
    {
		if(ulam==1) return "b0";
		
        int IDm=ulam+seed;
		int pr=pseudoRandom1000(IDm);
		string[] bcSourced = SC_data.biomeChances.Split(';');
		int i,lngt=bcSourced.Length/3;
		
		for(i=0;i<lngt;i++)
		{
			int min = int.Parse(bcSourced[3*i+1]);
			int max = int.Parse(bcSourced[3*i+2]);
			if(pr>=min && pr<=max)
			{
				int bb = int.Parse(bcSourced[3*i]);
				if(bb>=1 && bb<=31) return "b"+bb;
				else return "b0";
			}
		}
		
		return "b0";
    }
	public string GetBiomeTag(int ulam)
	{
		return SC_data.BiomeTags[int.Parse(GetBiomeString(ulam).Split('b')[1])];
	}
	public bool TagContains(string tags, string tag)
	{
		return (Array.IndexOf(tags.Split('_'),tag)>-1);
	}
	public float[] GetBiomeDAU(int ID)
	{
		float[] retu = new float[2];
		
		int[] astXY = UlamToXY(ID);
		Vector3 astPos = new Vector3(astXY[0]*10f,astXY[1]*10f,0f);
		
		Vector3 BS = new Vector3(Mathf.Round(astPos.x/biome_sector_size),Mathf.Round(astPos.y/biome_sector_size),0f);
		int ulam = CheckID((int)BS.x,(int)BS.y);
		BS = BS * biome_sector_size;

		BS += GetBiomeMove(ulam);
		float dX = astPos.x - BS.x;
		float dY = astPos.y - BS.y;
		float distance = Mathf.Sqrt(dX*dX+dY*dY);
		
		retu[0]=distance; retu[1]=ulam;
		return retu;
	}
	public void BTPT()
	{
		//BIOME TAG PRE TRANSLATE
		int i,j;
		for(i=0;i<32;i++)
		{
			bW[i] = 20;
			bMI[i] = 65;
			bMA[i] = 80;
			bD[i] = 6;
			
			string tags = SC_data.BiomeTags[i];
			
			for(j=0;j<=80;j++)
				if(TagContains(tags,"wide="+j)) bW[i]=j;
			for(j=0;j<=80;j++)
				if(TagContains(tags,"min="+j)) bMI[i]=j;
			for(j=0;j<=80;j++)
				if(TagContains(tags,"max="+j)) bMA[i]=j;
			for(j=0;j<=80;j++)
				if(TagContains(tags,"radius="+j)) {bMI[i]=j; bMA[i]=j;}
			
			for(j=0;j<=10;j++)
				if(TagContains(tags,"density="+j)) bD[i]=j;
		}
	}
}
