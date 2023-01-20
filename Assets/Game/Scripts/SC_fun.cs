using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public struct BiomeOfUlam
{
	public int ulam;
	public string memories;

	public Vector3 Move;
	public float Size;
	public string Tag;
	public string String;
}

public class SC_fun : MonoBehaviour
{
    public Transform Communtron1;
    public Material[] M = new Material[128];
    public Texture[] Item = new Texture[128];
    public Texture Item20u, Item55u;
	
    public float volume;
    public int seed;
    public bool halloween_theme;
	public List<int> GenListsB0 = new List<int>();
	public List<int> GenListsB1 = new List<int>();
	public List<int> GenListsB2 = new List<int>();
	BiomeOfUlam[] mems = new BiomeOfUlam[36];
	int currentBOU=0;
    public float biome_sector_size;

	public float[] boss_damages = new float[16];
	public float[] boss_damages_cyclic = new float[16];
	public float[] other_bullets_colliders = new float[16];

    public bool[] pushed_markers = new bool[9];
	
	public int[] bW = new int[32];
	public int[] bH = new int[32];
	public int[] bMI = new int[32];
	public int[] bMA = new int[32];
	public int[] bD = new int[32];
	public int[] bP = new int[32];
	public int[] bC = new int[32];
	
	public bool[,] bbW = new bool[32,81];
	public bool[,] bbH = new bool[32,81];
	
    public Transform[] structures;
    public Transform[] structures2;
	public Transform biomeCAN;
    public SC_control SC_control;
    public SC_data SC_data;
    public SC_long_strings SC_long_strings;

	void Awake()
	{
		int i;
		for(i=0;i<36;i++) {
			mems[i].memories="0000";
			mems[i].ulam=-1;
		}
	}
    public void SeedSet(int world)
    {
        if(world!=100)
        {
            if(!Directory.Exists(SC_data.worldDIR))
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
	public Vector3 Skop(Vector3 V, float F)
	{
		float pita = SC_control.Pitagoras(V);
		if(pita!=0) return V*F/SC_control.Pitagoras(V);
		else return V*F;
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
	public int LogListSearch(int search,List<int> list)
	{
		int i,smin=0,smax=list.Count-1;
		while(true)
		{
			i = smin + (smax-smin)/2;
			
			if(search < list[i]) smax = i-1;
			else if(search > list[i]) smin = i+1;
			else return i;

			if(smin>smax) return -smin;
		}
	}
    public void GenListAdd(int ID,int legsID)
    {
		if(legsID==0) {
			int searched = LogListSearch(ID,GenListsB0);
			if(searched>=0) return;
			GenListsB0.Insert(-searched,ID);
		}
		else if(legsID==1) {
			int searched = LogListSearch(ID,GenListsB1);
			if(searched>=0) return;
			GenListsB1.Insert(-searched,ID);
		}
		else if(legsID==2) {
			int searched = LogListSearch(ID,GenListsB2);
			if(searched>=0) return;
			GenListsB2.Insert(-searched,ID);
		}
    }
    public void GenListRemove(int ID,int legsID)
    {
		if(legsID==0) {
			int searched = LogListSearch(ID,GenListsB0);
			if(searched<0) return;
			GenListsB0.RemoveAt(searched);
		}
		else if(legsID==1) {
			int searched = LogListSearch(ID,GenListsB1);
			if(searched<0) return;
			GenListsB1.RemoveAt(searched);
		}
		else if(legsID==2) {
			int searched = LogListSearch(ID,GenListsB2);
			if(searched<0) return;
			GenListsB2.RemoveAt(searched);
		}
    }
    public bool GenListContains(int ID,int legsID)
    {
		if(legsID==0) return (LogListSearch(ID,GenListsB0)>=0);
		if(legsID==1) return (LogListSearch(ID,GenListsB1)>=0);
		if(legsID==2) return (LogListSearch(ID,GenListsB2)>=0);
		return false;
    }
	public int StructureCheck(int ulam)
	{
		if(GenListContains(ulam,1) || GetBiomeSize(ulam)==-1f) return 0;
		string tags = GetBiomeTag(ulam);
		int bint=int.Parse(GetBiomeString(ulam).Split('b')[1]);
		if(!TagContains(tags,"structural")) return 0;
		return bC[bint];
	}
    public bool AsteroidCheck(int ID)
    {
        int IDm=MakeID(ID,seed);
        if(ID==1) return false;
        if(IDm%2==0) return false;
        //int it = System.Text.Encoding.ASCII.GetBytes(SC_long_strings.AsteroidBase[(IDm%65536-1)/2]+"")[0];
		int it = (int)SC_long_strings.AsteroidBase[(IDm%65536-1)/2];
		int ia = 28; //from 28th ASCII char
		int inu = it - ia;
		
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
			if(CheckAppearing(bid,distance,size)==TagContains(tags,"swap"))
				return false;
		}
		else locD = bD[0];
		
		if(inu < locD) return true;
		return false;
    }
	public bool CheckAppearing(int bid, float distance, float size)
	{
		int from;
		bool bpW, bpH;
		
		//Checking outer (W)
		from = (int)Mathf.Round(size-distance);
		if(from<0) from=0; if(from>80) from=80;
		bpW = bbW[bid,from];
		
		//Checking inner (H)
		from = (int)Mathf.Round(distance);
		if(from<0) from=0; if(from>80) from=80;
		bpH = bbH[bid,from];
		
		return (!bpW && !bpH);
	}
    public char AsteroidChar(int ID)
    {
        int IDm=MakeID(ID,seed);
        return SC_long_strings.AsteroidBase[(IDm%65536-1)/2];
    }
    public int GetSize(int ID)
    {
		int IDm=MakeID(ID,seed);
		return (int)SC_long_strings.AsteroidSizeBase[(IDm%65536-1)/2]-48;
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
	public int[] UlamToXY(int ulam)
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

	public int pseudoRandom100(int prSeed)
	{
		prSeed = prSeed % 15000;
		string psInt = (SC_long_strings.BiomeNewBase[2*prSeed+0]+"") + (SC_long_strings.BiomeNewBase[2*prSeed+1]+"");
		return int.Parse(psInt);
	}
	public int pseudoRandom1000(int prSeed)
	{
		prSeed = prSeed % 10000;
		string psInt = (SC_long_strings.BiomeNewBase[3*prSeed+0]+"") + (SC_long_strings.BiomeNewBase[3*prSeed+1]+"") + (SC_long_strings.BiomeNewBase[3*prSeed+2]+"");
		return int.Parse(psInt);
	}
	public int pseudoRandom10e4(int prSeed)
	{
		prSeed = prSeed % 7500;
		string psInt = (SC_long_strings.BiomeNewBase[4*prSeed+0]+"") + (SC_long_strings.BiomeNewBase[4*prSeed+1]+"") + (SC_long_strings.BiomeNewBase[4*prSeed+2]+"") + (SC_long_strings.BiomeNewBase[4*prSeed+3]+"");
		return int.Parse(psInt);
	}
	public int pseudoRandom10e5(int prSeed)
	{
		prSeed = prSeed % 6000;
		string psInt = (SC_long_strings.BiomeNewBase[5*prSeed+0]+"") + (SC_long_strings.BiomeNewBase[5*prSeed+1]+"") + (SC_long_strings.BiomeNewBase[5*prSeed+2]+"") + (SC_long_strings.BiomeNewBase[5*prSeed+3]+"") + (SC_long_strings.BiomeNewBase[5*prSeed+4]+"");
		return int.Parse(psInt);
	}
	public Vector3 GetBiomeMove(int ulam)
	{
		Vector3 got_now;
		int i;
		for(i=0;i<36;i++) {
			if(mems[i].ulam==ulam)
			{
				if(mems[i].memories[0]=='1') {
					return mems[i].Move;
				}
				else {
					string str = mems[i].memories;
					mems[i].memories="1"+str[1]+str[2]+str[3];
					got_now = GetBiomeMoveR(ulam);
					mems[i].Move = got_now;
					return got_now;
				}
			}
		}

		got_now = GetBiomeMoveR(ulam);
		mems[currentBOU].ulam = ulam;
		mems[currentBOU].memories = "1000";
		mems[currentBOU].Move = got_now;

		currentBOU++;
		if(currentBOU==36) currentBOU=0;

		return got_now;
	}
	public float GetBiomeSize(int ulam)
	{
		float got_now;
		int i;
		for(i=0;i<36;i++) {
			if(mems[i].ulam==ulam)
			{
				if(mems[i].memories[1]=='1') {
					return mems[i].Size;
				}
				else {
					string str = mems[i].memories;
					mems[i].memories=str[0]+"1"+str[2]+str[3];
					got_now = GetBiomeSizeR(ulam);
					mems[i].Size = got_now;
					return got_now;
				}
			}
		}

		got_now = GetBiomeSizeR(ulam);
		mems[currentBOU].ulam = ulam;
		mems[currentBOU].memories = "0100";
		mems[currentBOU].Size = got_now;

		currentBOU++;
		if(currentBOU==36) currentBOU=0;

		return got_now;
	}
	public string GetBiomeTag(int ulam)
	{
		string got_now;
		int i;
		for(i=0;i<36;i++) {
			if(mems[i].ulam==ulam)
			{
				if(mems[i].memories[2]=='1') {
					return mems[i].Tag;
				}
				else {
					string str = mems[i].memories;
					mems[i].memories=str[0]+str[1]+"1"+str[3];
					got_now = GetBiomeTagR(ulam);
					mems[i].Tag = got_now;
					return got_now;
				}
			}
		}

		got_now = GetBiomeTagR(ulam);
		mems[currentBOU].ulam = ulam;
		mems[currentBOU].memories = "0010";
		mems[currentBOU].Tag = got_now;

		currentBOU++;
		if(currentBOU==36) currentBOU=0;

		return got_now;
	}
	public string GetBiomeString(int ulam)
	{
		string got_now;
		int i;
		for(i=0;i<36;i++) {
			if(mems[i].ulam==ulam)
			{
				if(mems[i].memories[3]=='1') {
					return mems[i].String;
				}
				else {
					string str = mems[i].memories;
					mems[i].memories=str[0]+str[1]+str[2]+"1";
					got_now = GetBiomeStringR(ulam);
					mems[i].String = got_now;
					return got_now;
				}
			}
		}

		got_now = GetBiomeStringR(ulam);
		mems[currentBOU].ulam = ulam;
		mems[currentBOU].memories = "0001";
		mems[currentBOU].String = got_now;

		currentBOU++;
		if(currentBOU==36) currentBOU=0;

		return got_now;
	}
	public Vector3 GetBiomeMoveR(int ulam)
    {
		string bstr=GetBiomeString(ulam);
        float size=GetBiomeSize(ulam);
		string tags=GetBiomeTag(ulam);
		if(size==900f || TagContains(tags,"centred")) return new Vector3(0f,0f,0f);
		
		int bint=int.Parse(GetBiomeString(ulam).Split('b')[1]);
        string red=LocalMove(ulam);
        float dX=0f, dY=0f;
		float maxD=(biome_sector_size*2-20f)/2f;
		
		int[] ulXY = UlamToXY(ulam);
		bool[] alXY = new bool[2]; alXY[0]=true; alXY[1]=true;
		
		if(true)
		{
			if(alXY[0]) dX = (int.Parse(red.Split(';')[0])-1)*(maxD-20f*Mathf.Ceil(size/20f));
			if(alXY[1]) dY = (int.Parse(red.Split(';')[1])-1)*(maxD-20f*Mathf.Ceil(size/20f));
		}
		else
		{
			if(alXY[0]) dX = (int.Parse(red.Split(';')[0])-1)*(maxD-size);
			if(alXY[1]) dY = (int.Parse(red.Split(';')[1])-1)*(maxD-size);
		}
		
        return new Vector3(dX,dY,0f);
    }
	public float GetBiomeSizeR(int ulam)
    {
        string typ=GetBiomeString(ulam);
		string tags=GetBiomeTag(ulam);
		
		if(TagContains(tags,"full")) return 900f;
		
		int[] ulXY = UlamToXY(ulam);
        if(typ=="b0") return -1f;
		if(TagContains(tags,"structural") && !(ulXY[0]%2==0 && ulXY[1]%2==0)) return -1f;
		
		int i,j;
		float min, max;
		
		min = bMI[int.Parse(typ.Split('b')[1])];
		max = bMA[int.Parse(typ.Split('b')[1])];
		
		if(min>max)
		{
			float pom=min;
			min=max; max=pom;
		}
		
		int ps_rand = pseudoRandom10e4(ulam+seed) % (((int)max-(int)min)+1);
		float ret = min + ps_rand;
		return ret;
    }
	public string GetBiomeTagR(int ulam)
	{
		return SC_data.BiomeTags[int.Parse(GetBiomeString(ulam).Split('b')[1])];
	}
    public string GetBiomeStringR(int ulam)
    {
		int i;
		if(ulam==1)
		for(i=1;i<=31;i++)
		{
			string tags = SC_data.BiomeTags[i];
			if(TagContains(tags,"spawn")) return ("b"+i);
		}

		if(ulam>=1 && ulam<=9) return "b0";
		if(ulam%2==0) return "b0";
		
        int IDm=ulam+seed;
		int pr=pseudoRandom1000(IDm);
		string[] bcSourced = SC_data.biomeChances.Split(';');
		int lngt=bcSourced.Length/3;
		
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
	public bool TagContains(string tags, string tag)
	{
		return (Array.IndexOf(tags.Replace('[','_').Replace(']','_').Replace('_',',').Split(','),tag)>-1);
	}
	bool HasBadNeighbor(int ulam, Vector3[] udels)
	{
		int[] getXY = UlamToXY(ulam);
		int uX = getXY[0];
		int uY = getXY[1];
		int i;
		
		for(i=0;i<9;i++)
		{
			int ulam2 = CheckID(uX+(int)udels[i].x,uY+(int)udels[i].y);
			string tags = GetBiomeTag(ulam2);
			if(TagContains(tags,"structural") && GetBiomeString(ulam2)!="b0")
			{
				if(pseudoRandom100(ulam2+seed) > pseudoRandom100(ulam+seed)) return true;
				if(pseudoRandom100(ulam2+seed) == pseudoRandom100(ulam+seed) && ulam2>ulam) return true;
			}
		}
		
		return false;
	}
	public int TrueBiomeUlam(Vector3 cenPos, Vector3 astPos)
	{
		int ux = (int)(cenPos.x/biome_sector_size);
		int uy = (int)(cenPos.y/biome_sector_size);
		int i;
		
		Vector3[] udels = new Vector3[9];
		udels[0] = new Vector3(-1f,1f,0f);
		udels[1] = new Vector3(0f,1f,0f);
		udels[2] = new Vector3(1f,1f,0f);
		udels[3] = new Vector3(-1f,0f,0f);
		udels[4] = new Vector3(0f,0f,0f);
		udels[5] = new Vector3(1f,0f,0f);
		udels[6] = new Vector3(-1f,-1f,0f);
		udels[7] = new Vector3(0f,-1f,0f);
		udels[8] = new Vector3(1f,-1f,0f);
		
		int[] ulams = new int[9];
		for(i=0;i<9;i++) ulams[i] = CheckID(ux+(int)udels[i].x,uy+(int)udels[i].y);
		
		bool[] insp = new bool[9];
		for(i=0;i<9;i++) insp[i] = ((SC_control.Pitagoras(cenPos+GetBiomeMove(ulams[i])+biome_sector_size*udels[i]-astPos) < GetBiomeSize(ulams[i])));
	
		int proper = 0;
		int prr = 0;
		for(i=0;i<9;i++)
		{
			if(insp[i])
			{
				int locP = bP[int.Parse(GetBiomeString(ulams[i]).Split('b')[1])];
				if(locP > prr)
				{
					proper = i;
					prr = locP;
				}
				else if(locP == prr)
				{
					if(pseudoRandom100(ulams[i]+seed) > pseudoRandom100(ulams[proper]+seed))
					{
						proper = i;
						prr = locP;
					}
					else if(pseudoRandom100(ulams[i]+seed) == pseudoRandom100(ulams[proper]+seed))
					{
						if(ulams[i] > ulams[proper])
						{
							proper = i;
							prr = locP;
						}
					}
				}
			}
		}
		
		if(proper==0 && !insp[0]) return 1; //guaranted empty sector
		return ulams[proper];
	}
	public float[] GetBiomeDAU(int ID)
	{
		float[] retu = new float[2];
		
		//Ulam segment
		int[] astXY = UlamToXY(ID);
		Vector3 astPos = new Vector3(astXY[0]*10f,astXY[1]*10f,0f);
		Vector3 BS = biome_sector_size * new Vector3(Mathf.Round(astPos.x/biome_sector_size),Mathf.Round(astPos.y/biome_sector_size),0f);
		int ulam = TrueBiomeUlam(BS,astPos);

		//Distance segment
		int[] tupXY = UlamToXY(ulam);
		
		BS = biome_sector_size * new Vector3(tupXY[0],tupXY[1],0f);
		BS += GetBiomeMove(ulam);
		
		float dX = astPos.x - BS.x;
		float dY = astPos.y - BS.y;
		float distance = Mathf.Sqrt(dX*dX+dY*dY);
		
		retu[0]=distance; retu[1]=ulam;
		return retu;
	}
	public int GetBiomePriority(int ulam)
	{
		return bP[int.Parse(GetBiomeString(ulam).Split('b')[1])];
	}
	public void BTPT()
	{
		//BIOME TAG PRE TRANSLATE
		int i,j;
		bool truing;
		
		for(i=0;i<32;i++)
		{
			bW[i] = 81; //wide
			bH[i] = -1; //hole
			bMI[i] = 65; //min size
			bMA[i] = 80; //max size
			bD[i] = 60; //denity [%]
			bP[i] = 16; //priority
			bC[i] = 0; //structure ID
			
			for(j=0;j<=80;j++) bbW[i,j] = false;
			for(j=0;j<=80;j++) bbH[i,j] = false;
			
			string tags = SC_data.BiomeTags[i];
			
			for(j=0;j<=80;j++)
				if(TagContains(tags,"ring.outer.change->"+j))
				{
					bbW[i,j]=true;
				}
			for(j=0;j<=80;j++)
				if(TagContains(tags,"ring.inner.change->"+j))
				{
					bbH[i,j]=true;
				}
			
			truing = false;
			for(j=0;j<=80;j++)
			{
				if(TagContains(tags,"full"))
				{
					bbW[i,j] = false;
					continue;
				}
				if(bbW[i,j]) truing = !truing;
				bbW[i,j] = truing;
			}
			truing = false;
			for(j=0;j<=80;j++)
			{
				if(bbH[i,j]) truing = !truing;
				bbH[i,j] = truing;
			}
			
			for(j=0;j<=80;j++)
				if(TagContains(tags,"min="+j)) bMI[i]=j;
			for(j=0;j<=80;j++)
				if(TagContains(tags,"max="+j)) bMA[i]=j;
			for(j=0;j<=100;j++)
				if(TagContains(tags,"radius="+j)) {bMI[i]=j; bMA[i]=j;}
			
			for(j=0;j<=100;j++)
				if(TagContains(tags,"density="+j+"%")) bD[i]=j;
			for(j=1;j<=31;j++)
				if(TagContains(tags,"priority="+j)) bP[i]=j;
			for(j=1;j<=31;j++)
				if(TagContains(tags,"struct="+j)) bC[i]=-j;
			
			if(TagContains(tags,"structural")) bP[i]=32;
			if(TagContains(tags,"arena")) bC[i]=1;
			if(TagContains(tags,"cementery")) bC[i]=2;
			if(TagContains(tags,"catastrophic.arena")) bC[i]=3;
		}
		
		bP[0] = 0;
	}
}
