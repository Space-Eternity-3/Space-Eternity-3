using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// RANDOMIZATION CLASS

public static class Deterministics
{
    public static string long1, long2, long3;

    //Fully deterministic functions
    public static int Random10e2(int sour)
    {
		sour = sour % 15000;
		string psInt = (long2[2*sour+0]+"") + (long2[2*sour+1]+"");
		return int.Parse(psInt);
	}
	public static int Random10e3(int sour) //long2 works best for 10e3
    {
		sour = sour % 10000;
		string psInt = (long2[3*sour+0]+"") + (long2[3*sour+1]+"") + (long2[3*sour+2]+"");
		return int.Parse(psInt);
	}
	public static int Random10e4(int sour)
    {
		sour = sour % 7500;
		string psInt = (long2[4*sour+0]+"") + (long2[4*sour+1]+"") + (long2[4*sour+2]+"") + (long2[4*sour+3]+"");
		return int.Parse(psInt);
	}
	public static int Random10e5(int sour)
    {
		sour = sour % 6000;
		string psInt = (long2[5*sour+0]+"") + (long2[5*sour+1]+"") + (long2[5*sour+2]+"") + (long2[5*sour+3]+"") + (long2[5*sour+4]+"");
		return int.Parse(psInt);
	}
    public static int AnyRandom(int div, int sour)
    {
        return Random10e5(sour) % div;
    }
    public static int CalculateFromString(string chance_string, int sour)
    {
        int decider = Random10e3(sour);
        string[] s_nums = chance_string.Split(';');
        int i,lngt = s_nums.Length/3, V,A,B;
        for(i=0;i<lngt;i++)
        {
            V = int.Parse(s_nums[3*i + 0]);
            A = int.Parse(s_nums[3*i + 1]);
            B = int.Parse(s_nums[3*i + 2]);
            if(decider>=A && decider<=B) return V;
        }
        return 0;
    }
}


// GENERATION LAYER 3 (inner) -> Fobs and files communication

public static class WorldData
{
    public static SC_data SC_data;
    private static int a=0; // line
    private static int c=0; // sector id
    private static int ulam_hold=0;

    //Technical methods
    public static void Load(int bX, int bY) //Loads X;Y data to memory
    {
        string[] uAst = SC_data.GetAsteroid(bX,bY).Split(';');
        c=int.Parse(uAst[0]);
        a=int.Parse(uAst[1]);
        ulam_hold = SC_data.SC_control.SC_fun.MakeUlam(bX,bY);
    }
    public static void DataGenerate(string gencode) //Generates data from gencode
    {
        int i;
        for(i=0;i<=60;i++) UpdateData(i,0);
        if(gencode=="BOSS")
        {
            UpdateType(1024);
            for(i=1;i<=60;i++) UpdateData(i,0);
        }
        else
        {
            /*
                Gencode:
                [size]b[biome]b[fobCode] -> biome based code, calculate type from biome
                [size]t[type]t[fobCode] -> type based code, type is given
            */

            //Gencode parse
            char sep = 'b'; if(gencode.Contains('t')) sep = 't';
            string[] elements = gencode.Split(sep);

            //Size parse
            int size = int.Parse(elements[0]);

            //Type parse
            int type;
            if(sep=='t') type = int.Parse(elements[1]);
            else type = Deterministics.CalculateFromString(SC_data.TypeSet[int.Parse(elements[1])*7 + size-4], ulam_hold + Generator.seed);

            //Gens parse
            int[] gens = new int[]{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}; //20 generation places
            if(elements.Length>2) {
                string[] s_gens = elements[2].Split(';');
                for(i=0;(i<s_gens.Length && i<20);i++)
                    if(!int.TryParse(s_gens[i], out gens[i]))
                        gens[i] = -1;
            }
            
            //Generate type and fobs
            UpdateType(type);
            for(i=1;i<=size*2;i++)
            {
                int gen = gens[i-1];
                if(gen==-1) gen = Deterministics.CalculateFromString(SC_data.FobGenerate[type], 20*((ulam_hold + Generator.seed) % 1000000)+i);
                UpdateFob(i,gen);
            }
        }
    }
    
    //Read methods
    public static int GetData(int place) //Returns the place data ("" -> 0)
    {
        string got = SC_data.World[a,place,c];
        int num = 0;
        if(int.TryParse(got, out num)) return num;
        else return 0;
    }
    public static int GetNbt(int place, int index) //Returns the fob nbt data ("" -> 0)
    {
        return GetData(21+index+2*(place-1));
    }
    public static int GetFob(int place) //Returns the fob (0-127)
    {
        string got = SC_data.World[a,place,c];
        int num = -1;
        if(int.TryParse(got, out num))
        {
            if(num>=0 && num<=127) return num;
            else return -1;
        }
        else return -1;
    }
    public static int GetType() //Returns data type (0-63 or 1024)
    {
        string got = SC_data.World[a,0,c];
        int num = -1;
        if(int.TryParse(got, out num))
        {
            if((num>=0 && num<=63) || num==1024) return num;
            else return -1;
        }
        else return -1;
    }

    //Write methods
    public static void UpdateData(int place, int data) //Updates data (0 -> "")
    {
        if(data!=0) SC_data.World[a,place,c] = data+"";
        else SC_data.World[a,place,c] = "";
    }
    public static void UpdateNbt(int place, int index, int data) //Updates nbt data (0 -> "")
    {
        UpdateData(21+index+2*(place-1),data);
    }
    public static void UpdateFob(int place, int data) //Updates the fob (0-127)
    {
        ResetNbt(place);
        if(data>=0 && data<=127) SC_data.World[a,place,c] = data+"";
        else SC_data.World[a,place,c] = "0";
    }
    public static void UpdateType(int data) //Updates data type (0-63 or 1024)
    {
        if((data>=0 && data<=63) || data==1024) SC_data.World[a,0,c] = data+"";
        else SC_data.World[a,0,c] = "";
    }

    //Private methods
    private static void ResetNbt(int place) //Resets fob nbt data ("")
    {
        UpdateNbt(place,0,0);
        UpdateNbt(place,1,0);
    }
}


// GENERATION LAYER 2 (middle) -> Gameplay objects create

// coming soon


// GENERATION LAYER 1 (outer) -> Biome and structure specify

public class CBiomeInfo
{
    public int biome;
    public int size;
    public Vector3 move;

    public CBiomeInfo(int ulam)
    {
        biome = Generator.GetBiome(ulam);
        size = Generator.GetBiomeSize(ulam,biome);
        move = Generator.GetBiomeMove(ulam,biome,size);
    }
}

public static class Generator
{
    public static SC_data SC_data;
    public static SC_fun SC_fun;

    public static int seed;

    public static int[] tag_min = new int[32];
    public static int[] tag_max = new int[32];
    public static int[] tag_density = new int[32];
    public static int[] tag_priority = new int[32];
    public static int[] tag_struct = new int[32];
    public static int[] tag_gradient = new int[32];
    public static bool[] tag_grid = new bool[32];
    public static bool[] tag_spawn = new bool[32];
    public static bool[] tag_centred = new bool[32];
    public static bool[] tag_structural = new bool[32];

    public const int max_dict_size = 64;
    public static Dictionary<int, CBiomeInfo> WorldMap = new Dictionary<int, CBiomeInfo>();

    //Initializes seed in generator, returns the actual seed string
    public static string SetSeed(string s_seed)
    {
        if(int.TryParse(s_seed, out seed)) return s_seed;
        else {
            seed = UnityEngine.Random.Range(0,10000000);
            return seed+"";
        }
    }

    //Converts all biome tags into arrays
    public static void TagNumbersInitialize()
    {
        int i,j;
        for(i=0;i<32;i++)
        {
            tag_min[i] = 65;
            tag_max[i] = 80;
            tag_density[i] = 60;
            tag_priority[i] = 16;
            tag_struct[i] = 0;
            tag_gradient[i] = 80;
            tag_grid[i] = false;
            tag_spawn[i] = false;
            tag_centred[i] = false;
            tag_structural[i] = false;

            string tags = SC_data.BiomeTags[i];

            for(j=0;j<=80;j++)
                if(SC_data.TagContains(tags,"min="+j)) tag_min[i]=j;
            for(j=0;j<=80;j++)
                if(SC_data.TagContains(tags,"max="+j)) tag_max[i]=j;
            for(j=0;j<=80;j++)
                if(SC_data.TagContains(tags,"radius="+j)) { tag_min[i]=j; tag_max[i]=j; }
            
            for(j=0;j<=100;j++)
                if(SC_data.TagContains(tags,"density="+j+"%")) tag_density[i]=j;
            for(j=1;j<=31;j++)
                if(SC_data.TagContains(tags,"priority="+j)) tag_priority[i]=j;
            for(j=1;j<=31;j++)
                if(SC_data.TagContains(tags,"struct="+j)) tag_struct[i]=j;

            for(j=0;j<=80;j++)
                if(SC_data.TagContains(tags,"gradient="+j)) tag_gradient[i]=j;

            if(SC_data.TagContains(tags,"grid")) tag_grid[i] = true;
            if(SC_data.TagContains(tags,"spawn")) tag_spawn[i] = true;
            if(SC_data.TagContains(tags,"centred")) tag_centred[i] = true;
            if(SC_data.TagContains(tags,"structural")) tag_structural[i] = true;

            if(tag_min[i] > tag_max[i])
            {
                int pom = tag_min[i];
                tag_min[i] = tag_max[i];
                tag_max[i] = pom;
            }

            if(tag_structural[i]) tag_priority[i] = 32;
            else tag_struct[i] = 0;
        }

        tag_priority[0] = 0;
        tag_struct[0] = 0;
        tag_structural[0] = false;
    }

    //Generator partly independent methods
    public static int MixID(int ID,int sed)
    {
        return ID+sed*2;
    }
    public static int[] BaseMove(int ID)
    {
        int ird = MixID(ID,seed) % 9;
        switch(ird)
        {
            case 0: return new int[]{0,0};
            case 2: return new int[]{0,1};
            case 8: return new int[]{0,2};
            case 3: return new int[]{1,0};
            case 7: return new int[]{1,1};
            case 1: return new int[]{1,2};
            case 5: return new int[]{2,0};
            case 4: return new int[]{2,1};
            case 6: return new int[]{2,2};
            default: return new int[]{0,0};
        }
    }
	public static int DeltaOfSize(int size)
	{
		if(size<=80 && size>=61) return 10;
		if(size<=60 && size>=40) return 30;
		if(size<=39 && size>=20) return 10;
		return 0;
	}
	public static bool IsBiggerPriority(int ulam1, int ulam2, int prio1, int prio2)
	{
		if(prio1 > prio2) return true;
		else if(prio1 == prio2) {
			int rnd1 = Deterministics.Random10e2(ulam1+seed);
			int rnd2 = Deterministics.Random10e2(ulam2+seed);
			if(rnd1 > rnd2) return true;
			else if(rnd1 == rnd2) {
				if(ulam1 > ulam2) return true;
			}
		}
        return false;
	}

    //Dictionary access
    public static CBiomeInfo GetBiomeData(int ulam)
    {
        if(!WorldMap.ContainsKey(ulam))
        {
            int lngt = WorldMap.Count;
            if(lngt >= max_dict_size) WorldMap.Clear();
            WorldMap.Add(ulam,new CBiomeInfo(ulam));
            return WorldMap[ulam];
        }
        else return WorldMap[ulam];
    }

    //Basement functions
    public static int GetBiome(int ulam)
    {
        //Unconditional biome 0
        if((ulam>=2 && ulam<=9) || ulam%2==0) return 0;

        //Memories check and generate
        int biome = SC_fun.FindBiome(ulam);
        if(biome==-1)
        {
            int i;
            if(ulam==1)
            {
                biome = 0;
                for(i=1;i<=31;i++)
                    if(tag_spawn[i]) biome = i;
            }
            else
            {
                biome = Deterministics.CalculateFromString(SC_data.biomeChances,ulam+seed);
            }
            SC_fun.InsertBiome(ulam,biome);
        }

        //Structures erase
        if(!tag_structural[biome]) return biome;
        else
        {
            int[] XY = SC_fun.UlamToXY(ulam);
            if(!(XY[0]%2==0 || XY[1]%2==0)) return 0;
            else return biome;
        }
    }
    public static int GetBiomeSize(int ulam, int biome)
    {
        if(biome==0) return -1;
		int ps_rand = Deterministics.Random10e4(ulam+seed) % ((tag_max[biome]-tag_min[biome])+1);
		return tag_min[biome] + ps_rand;
    }
    public static Vector3 GetBiomeMove(int ulam, int biome, int size)
    {
        if(tag_centred[biome]) return new Vector3(0f,0f,0f);
        int move_multiplier = DeltaOfSize(size);
        int[] move_raw = BaseMove(ulam);
        return move_multiplier * new Vector3(move_raw[0]-1,move_raw[1]-1,0f);
    }
}

public class SC_worldgen : MonoBehaviour
{
    public Transform player;
    void Update()
    {
        if(Input.GetKeyDown("p"))
        {
            int X = (int)Mathf.Round(player.position.x/100f);
            int Y = (int)Mathf.Round(player.position.y/100f);
            int ulam = Generator.SC_fun.MakeUlam(X,Y);
            CBiomeInfo bnf = Generator.GetBiomeData(ulam);
            UnityEngine.Debug.Log(
                "Ulam: "+ulam+" XY: "+X+";"+Y+"\n"+
                "Biome: "+bnf.biome+"\n"+
                "Size: "+bnf.size+"\n"+
                "Move: "+bnf.move+"\n"
            );
        }
    }
}
