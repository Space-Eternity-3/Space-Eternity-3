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
		return Parsing.IntE(psInt);
	}
	public static int Random10e3(int sour) //long2 works best for 10e3
    {
		sour = sour % 10000;
		string psInt = (long2[3*sour+0]+"") + (long2[3*sour+1]+"") + (long2[3*sour+2]+"");
		return Parsing.IntE(psInt);
	}
	public static int Random10e4(int sour)
    {
		sour = sour % 7500;
		string psInt = (long2[4*sour+0]+"") + (long2[4*sour+1]+"") + (long2[4*sour+2]+"") + (long2[4*sour+3]+"");
		return Parsing.IntE(psInt);
	}
	public static int Random10e5(int sour)
    {
		sour = sour % 6000;
		string psInt = (long2[5*sour+0]+"") + (long2[5*sour+1]+"") + (long2[5*sour+2]+"") + (long2[5*sour+3]+"") + (long2[5*sour+4]+"");
		return Parsing.IntE(psInt);
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
            V = Parsing.IntE(s_nums[3*i + 0]);
            A = Parsing.IntE(s_nums[3*i + 1]);
            B = Parsing.IntE(s_nums[3*i + 2]);
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
    private static bool dont_update_bosbul=false;

    //Technical methods
    public static void Load(int bX, int bY) //Loads X;Y data to memory
    {
        string[] uAst = SC_data.GetAsteroid(bX,bY).Split(';');
        c=Parsing.IntE(uAst[0]);
        a=Parsing.IntE(uAst[1]);
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
            int size = Parsing.IntE(elements[0]);

            //Type parse
            int type;
            if(sep=='t') type = Parsing.IntE(elements[1]);
            else type = Deterministics.CalculateFromString(SC_data.TypeSet[Parsing.IntE(elements[1])*7 + size-4], ulam_hold + Generator.seed);

            //Gens parse
            int[] gens = new int[]{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}; //20 generation places
            if(elements.Length>2) {
                string[] s_gens = elements[2].Split(';');
                for(i=0;(i<s_gens.Length && i<20);i++)
                    if(Parsing.IntC(s_gens[i])) gens[i] = Parsing.IntU(s_gens[i]);
            }
            
            //Generate type and fobs & T-base proof
            int T_bases = 0, D_bases = 0;
            UpdateType(type);
            for(i=1;i<=size*2;i++)
            {
                int gen = gens[i-1];
                if(gen==-1) gen = Deterministics.CalculateFromString(SC_data.FobGenerate[type], 20*((ulam_hold + Generator.seed) % 1000000)+i);
                dont_update_bosbul = true;
                UpdateFob(i,gen);
                if(gen==81) T_bases++;
                if(gen==82) D_bases++;
            }
            if(T_bases==0)
            {
                string[] fob_gens = SC_data.FobGenerate[type].Split(";");
                for(i=0;3*i<fob_gens.Length;i++) if(fob_gens[3*i]=="81")
                {
                    int I = Deterministics.Random10e2((ulam_hold + Generator.seed) % 1000000) % (size*2) + 1;
                    dont_update_bosbul = true;
                    UpdateFob(I,81);
                    break;
                }
            }

            //D-base diamond probability
            for(i=1;i<=size*2;i++)
            {
                if(D_bases <= 1) break;
                if(WorldData.GetFob(i)==82)
                {
                    float diamonded_chance = Parsing.FloatU(SC_data.Gameplay[132]);
                    if((Deterministics.Random10e4((ulam_hold + Generator.seed + 153*i) % 1000000) + 1) / 10000f <= diamonded_chance) {
                        WorldData.UpdateNbt(i,0,1);
                        D_bases--;
                    }
                }
            }

            Bosbul.UpdateFobColliders(ulam_hold);
        }
    }
    
    //Read methods
    public static int GetData(int place) //Returns the place data ("" -> 0)
    {
        string got = SC_data.World[a,place,c];
        return Parsing.IntU(got);
    }
    public static int GetNbt(int place, int index) //Returns the fob nbt data ("" -> 0)
    {
        return GetData(21+index+2*(place-1));
    }
    public static int GetFob(int place) //Returns the fob (0-127)
    {
        string got = SC_data.World[a,place,c];
        if(Parsing.IntC(got))
        {
            int num = Parsing.IntU(got);
            if(num>=0 && num<=127) return num;
            else return -1;
        }
        else return -1;
    }
    public static int GetType() //Returns data type (0-63 or 1024)
    {
        string got = SC_data.World[a,0,c];
        if(Parsing.IntC(got))
        {
            int num = Parsing.IntU(got);
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
        
        if(!dont_update_bosbul) Bosbul.UpdateFobColliders(ulam_hold);
        else dont_update_bosbul = false;
    }
    public static void UpdateType(int data) //Updates data type (0-63 or 1024)
    {
        if((data>=0 && data<=63) || data==1024) SC_data.World[a,0,c] = data+"";
        else SC_data.World[a,0,c] = "";
    }

    //Other methods
    public static int GetCountOf(int data,int nbt1)
    {
        int ret_count = 0;
        for(int i=1;i<=20;i++)
        {
            if(GetFob(i) == data)
            if(GetNbt(i,0) == nbt1 || nbt1 == -1)
                ret_count++;
        }
        return ret_count;
    }

    //Private methods
    private static void ResetNbt(int place) //Resets fob nbt data ("")
    {
        UpdateNbt(place,0,0);
        UpdateNbt(place,1,0);
    }
}


// GENERATION LAYER 2 (middle) -> Gameplay objects create

public class CObjectInfo
{
    //Indicators
    public int ulam;
    public string obj = "unknown";
    public int animator = -1;
    public CObjectInfo animator_reference = null;

    //Transform
    public Vector3 default_position;
    public Vector3 position;
    public float rotation = 0f;
    public Vector3[] fob_positions = new Vector3[20];
    public float[] fob_rotations = new float[20];

    //Properties
    public int type = -1;
    public int size = -1;
    public int biome = -1;
    public float range = 0f;
    public float size1 = 0f;
    public float size2 = 0f;
    public bool hidden = false;
    public string fobcode = "";
    public string GetGencode()
    {
        if(biome==-1) return size + "t" + type + "t" + fobcode;
        else return size + "b" + biome + "b" + fobcode;
    }

    //Animations
    public int animation_type = 0;
    public Vector3 animation_size = new Vector3(0f,0f,0f);
    public string animation_when_doing = "";
    public string animation_when_done = "";
    public string animation_when_undoing = "";

    //Constructor
    public CObjectInfo(int p_ulam, Vector3 start_pos)
    {
        ulam = p_ulam;
        position = start_pos;
        default_position = start_pos;
    }

    //Summoners
    public void Asteroid(string p_size, string p_type, string p_fobcode)
    {
        size = Parsing.IntU(p_size);
        if(size<4 || size>10) size=4;

        type = Parsing.IntU(p_type);
        if(type<0 || type>63) type=0;

        string[] fob_array = (p_fobcode+",,,,,,,,,,,,,,,,,,,").Split(',');

        int i;
        for(i=0;i<20;i++)
        {
            int t = Universe.RangedIntParse(fob_array[i],-1,127);
            if(t!=-1) fobcode += t;
            if(i!=19) fobcode += ";";

            fob_rotations[i] = -(360f/(size*2))*i;
            fob_positions[i] = position + size/2f * new Vector3(
                Mathf.Sin(-fob_rotations[i]*Mathf.PI/180f),
                Mathf.Cos(-fob_rotations[i]*Mathf.PI/180f),
            0f);
        }
        obj = "asteroid";
    }
    public void Wall(string p_size1, string p_size2, string p_type)
    {
        size1 = Parsing.FloatU(p_size1);
        if(size1<0f) size1=0f;

        size2 = Parsing.FloatU(p_size2);
        if(size2<0f) size2=0f;
        
        type = Parsing.IntU(p_type);
        if(type<0 || type>15) type=0;

        obj = "wall";
    }
    public void Sphere(string p_size1, string p_type)
    {
        size1 = Parsing.FloatU(p_size1);
        if(size1<0f) size1=0f;

        type = Parsing.IntU(p_type);
        if(type<0 || type>15) type=0;

        obj = "sphere";
    }
    public void Piston(string p_size1, string p_size2, string p_type)
    {
        size1 = Parsing.FloatU(p_size1);
        if(size1<0f) size1=0f;

        size2 = Parsing.FloatU(p_size2);
        if(size2<0f) size2=0f;

        type = Parsing.IntU(p_type);
        if(type<0 || type>15) type=0;

        obj = "piston";
    }
    public void Ranger(string p_range, string p_obj)
    {
        range = Parsing.FloatU(p_range);
        if(range<0f) range=0f;

        obj = p_obj;
    }
    public void Spherical(string p_obj)
    {
        size1 = 1f;

        obj = p_obj;
    }
    public void Boss(string p_type)
    {
        type = Parsing.IntU(p_type);
        if(type<0 || type>6) type=0;

        animation_type = 1;
        animation_when_doing = "b1a1;b2a2;b3a3;";
        animation_when_done = "default;A1;A2;A3;R;b1a2;b2a3;b3r;";
        animation_when_undoing = "a1b1;a2b2;a3b3;";

        obj = "boss";
    }

    //Modifiers
    public void SetBiomeBase(string p_biome)
    {
        if(obj!="asteroid") return;

        biome = Parsing.IntU(p_biome);
        if(biome<0 || biome>31) biome=0;
        type=-1;
    }
    public void Move(string p_x, string p_y)
    {
        if(obj=="boss") return;

        float x=0f, y=0f;
        x = Parsing.FloatU(p_x);
        y = Parsing.FloatU(p_y);
        float[] get = SC_fun.RotateVector(x,y,rotation);
        position += new Vector3(get[0],get[1],0f);

        if(obj=="asteroid")
        {
            int i;
            for(i=0;i<20;i++)
            {
                fob_positions[i] += new Vector3(get[0],get[1],0f);
            }
        }
    }
    public void Rotate(string p_rot)
    {
        if(obj=="boss") return;

        float rot=0f;
        rot = Parsing.FloatU(p_rot);
        rotation += rot;
        
        if(obj=="asteroid")
        {
            int i;
            for(i=0;i<20;i++)
            {
                Vector3 raw_delta_pos = fob_positions[i] - position;
                float[] delta_pos = SC_fun.RotateVector(raw_delta_pos.x,raw_delta_pos.y,-fob_rotations[i]);
                ResetS(i,"position");
                RotateS(i,rot+"");
                MoveS(i,delta_pos[0]+"",delta_pos[1]+"");
            }
        }
    }
    public void Reset(string p_what)
    {
        if(obj=="boss") return;

        if(p_what=="position")
        {
            Vector3 delta_pos = position - default_position;
            position -= delta_pos;

            if(obj=="asteroid")
            {
                int i;
                for(i=0;i<20;i++)
                    fob_positions[i] -= delta_pos;
            }
        }
        if(p_what=="rotation") Rotate((-rotation)+"");
    }

    //Child modifiers
    public void MoveS(int S, string p_x, string p_y)
    {
        if(obj!="asteroid") return;

        float x=0f, y=0f;
        x = Parsing.FloatU(p_x);
        y = Parsing.FloatU(p_y);
        float[] get = SC_fun.RotateVector(x,y,fob_rotations[S]);
        fob_positions[S] += new Vector3(get[0],get[1],0f);
    }
    public void RotateS(int S, string p_rot)
    {
        if(obj!="asteroid") return;

        float rot=0f;
        rot = Parsing.FloatU(p_rot);
        fob_rotations[S] += rot;
    }
    public void ResetS(int S, string p_what)
    {
        if(obj!="asteroid") return;

        if(p_what=="position") fob_positions[S] = position;
        if(p_what=="rotation") fob_rotations[S] = rotation;
    }

    //Animation initializer
    public void AnimationCreate(string p_type, string p_when, string p_dx, string p_dy)
    {
        if(obj!="animator") return;

        animation_when_doing = "";
        animation_when_done = "";
        animation_when_undoing = "";

        int i;
        string[] given_states = p_when.Split(',');
        string[] st_array = {"R","A1","A2","A3","B1","B2","B3","default"};
        bool[] have = new bool[8];

        //Static states
        for(i=0;i<8;i++)
        {
            have[i] = (Array.IndexOf(given_states,st_array[i])>=0);
            if(have[i]) animation_when_done += st_array[i]+";";
        }

        //Activating & losing
        for(i=1;i<=3;i++)
        {
            if(have[i] && have[i+3]) {
                animation_when_done += "a"+i+"b"+i+";";
                animation_when_done += "b"+i+"a"+i+";";
            }
            if(have[i] && !have[i+3]) {
                animation_when_undoing += "a"+i+"b"+i+";";
                animation_when_doing += "b"+i+"a"+i+";";
            }
            if(!have[i] && have[i+3]) {
                animation_when_doing += "a"+i+"b"+i+";";
                animation_when_undoing += "b"+i+"a"+i+";";
            }
        }

        //Winning
        for(i=1;i<=2;i++)
        {
            if(have[i+1] && have[i+3]) animation_when_done += "b"+i+"a"+(i+1)+";";
            if(have[i+1] && !have[i+3]) animation_when_doing += "b"+i+"a"+(i+1)+";";
            if(!have[i+1] && have[i+3]) animation_when_undoing += "b"+i+"a"+(i+1)+";";
        }

        //Completing
        if(have[0] && have[6]) animation_when_done += "b3r;";
        if(have[0] && !have[6]) animation_when_doing += "b3r;";
        if(!have[0] && have[6]) animation_when_undoing += "b3r;";
        

        float dx=0f, dy=0f;
        if(p_type=="hiding")
        {
            animation_type = 1;
        }
        if(p_type=="extending")
        {
            animation_type = 2;
            dx = Parsing.FloatU(p_dx);
            dy = Parsing.FloatU(p_dy);
            animation_size = new Vector3(dx,dy,0f);
        }
    }
}

public static class Universe
{
    public static SC_fun SC_fun;

    public const int max_dict_size = 64;
    public static Dictionary<string, CObjectInfo[]> Sectors = new Dictionary<string, CObjectInfo[]>();

    //Public methods
    public static CObjectInfo GetObject(int ulam)
    {
        string sector_name = GetSectorNameByUlam(ulam);
        CObjectInfo[] sector = GetSector(sector_name);
        foreach(CObjectInfo obj in sector)
        {
            if(obj!=null) if(obj.ulam==ulam)
                return obj;
        }
        return null;
    }
    public static CObjectInfo[] GetSector(string sector_name)
    {
        if(Sectors.ContainsKey(sector_name)) return Sectors[sector_name];

        string[] spl = sector_name.Split('_');
        int X = Parsing.IntE(spl[1]);
        int Y = Parsing.IntE(spl[2]);
        int sX = X; if(X%2!=0) sX++;
        int sY = Y; if(Y%2!=0) sY++;

        if(X < -2000 || X >= 2000) return new CObjectInfo[0];
        if(Y < -2000 || Y >= 2000) return new CObjectInfo[0];

        int i;
        CObjectInfo[] Build;
        if(spl[0]=="B")
        {
            Build = new CObjectInfo[50];
            CObjectInfo[] LocalStructure = GetSector("S_"+sX+"_"+sY);
            List<CObjectInfo> Holes = new List<CObjectInfo>();
            foreach(CObjectInfo obj in LocalStructure)
            {
                if(obj!=null) if(obj.obj=="hole")
                    Holes.Add(obj);
            }
            for(i=0;i<50;i++)
            {
                int x = 10*X + i/5;
                int y = 10*Y + 2*(i%5);
                if(x%2!=0) y++;
                int ulam = SC_fun.MakeUlam(x,y);
                Build[i] = AsteroidBuild(ulam,Holes);
            }
        }
        else Build = StructureBuild(SC_fun.MakeUlam(sX,sY));

        if(Sectors.Count >= max_dict_size) Sectors.Clear();
        Sectors.Add(sector_name,Build);
        return Sectors[sector_name];
    }

    //Conversion methods
    public static string GetSectorNameByUlam(int ulam)
    {
        int[] xy = SC_fun.UlamToXY(ulam);
        int X,Y;

        if(xy[0]>=0) X = xy[0]/10;
        else X = -((-xy[0]-1)/10+1);

        if(xy[1]>=0) Y = xy[1]/10;
        else Y = -((-xy[1]-1)/10+1);

        if(ulam%2==0)
        {
            if(X%2!=0) X--;
            if(Y%2!=0) Y--;
            return "S_"+X+"_"+Y;
        }
        else return "B_"+X+"_"+Y;
    }
    public static Vector3 GetAsteroidMove(int ulam, int size, int biome)
    {
		if(Generator.tag_grid[biome]) return new Vector3(0f,0f,0f);
		int r121 = Deterministics.Random10e3(ulam+Generator.seed) % 121;
		float dE = 5f - (size+2f)*0.35f;
		float dZ = (r121/11-5) * dE * 0.2f;
		float dW = (r121%11-5) * dE * 0.2f;
		return new Vector3(dW-dZ,dW+dZ,0f);
    }
    public static int RangedIntParse(string str, int min, int max)
    {
        int n = min;
        if(Parsing.IntC(str)) n = Parsing.IntU(str);
        else return min;
        if(n < min || n > max) n = min;
        return n;
    }

    //Generator methods
    private static CObjectInfo AsteroidBuild(int ulam, List<CObjectInfo> Holes)
    {
        if(ulam==1 || ulam%2==0) return null;
        int ulam_mixed = Generator.MixID(ulam,Generator.seed);
        int r100 = (int)Deterministics.long1[(ulam_mixed%65536-1)/2] - 28;
        int s100 = (int)Deterministics.long3[(ulam_mixed%65536-1)/2] - 48;

        int[] xy = SC_fun.UlamToXY(ulam);
        Vector3 ast_pos = 10 * new Vector3(xy[0],xy[1],0f);
        int Ulam = LeadingBiomeUlam(ast_pos);
        CBiomeInfo Biome = Generator.GetBiomeData(Ulam);
        int[] XY = SC_fun.UlamToXY(Ulam);
        Vector3 biome_pos = 100 * new Vector3(XY[0],XY[1],0f) + Biome.move;
        bool is_in = (SC_fun.SC_control.Pitagoras(ast_pos-biome_pos) < Biome.size);

        int gen_biome = 0, gen_size = s100;
        if(is_in) gen_biome = Biome.biome;
        if(r100 >= Generator.tag_density[gen_biome]) return null;

        foreach(CObjectInfo obj in Holes)
        {
            if(SC_fun.SC_control.Pitagoras(ast_pos - obj.position) < obj.range)
                return null;
        }

        CObjectInfo Asteroid = new CObjectInfo(ulam, ast_pos);
        Asteroid.Asteroid(gen_size+"","0","");
        Asteroid.SetBiomeBase(gen_biome+"");
        Vector3 loc_mov = GetAsteroidMove(ulam,gen_size,gen_biome);
        Asteroid.Move(loc_mov.x+"",loc_mov.y+"");
        return Asteroid;
    }
    private static int LeadingBiomeUlam(Vector3 ast_pos)
    {
        Vector3 cen_pos = new Vector3(Mathf.Round(ast_pos.x/100f),Mathf.Round(ast_pos.y/100f),0f) * 100f;
        int X = (int)(cen_pos.x/100f);
		int Y = (int)(cen_pos.y/100f);
		
        int i;
		Vector3[] udels = new Vector3[]
        {
            new Vector3(-1f,-1f,0f), new Vector3(0f,-1f,0f), new Vector3(1f,-1f,0f),
            new Vector3(-1f,0f,0f), new Vector3(0f,0f,0f), new Vector3(1f,0f,0f),
            new Vector3(-1f,1f,0f), new Vector3(0f,1f,0f), new Vector3(1f,1f,0f)
        };
		int[] ulams = new int[9];
		bool[] insp = new bool[9];
        CBiomeInfo[] biomes = new CBiomeInfo[9];
		for(i=0;i<9;i++)
        {
            ulams[i] = SC_fun.MakeUlam(X+(int)udels[i].x, Y+(int)udels[i].y);
            biomes[i] = Generator.GetBiomeData(ulams[i]);
            insp[i] = (SC_fun.SC_control.Pitagoras(cen_pos + 100*udels[i] + biomes[i].move - ast_pos) < biomes[i].size);
        }
	
		int proper=0, prr=0;
		for(i=0;i<9;i++)
		{
			if(insp[i])
			{
				int locP = Generator.tag_priority[biomes[i].biome];
				if(Generator.IsBiggerPriority(ulams[i],ulams[proper],locP,prr))
				{
					proper = i;
					prr = locP;
				}
			}
		}
		if(proper==0 && !insp[0]) return 1;
		return ulams[proper];
    }
    private static CObjectInfo[] StructureBuild(int Ulam)
    {
        int[] XY = SC_fun.UlamToXY(Ulam);
        int X = XY[0];
        int Y = XY[1];
        
        CObjectInfo[] Build = new CObjectInfo[1000];
        CBiomeInfo biomeInfo = Generator.GetBiomeData(Ulam);
        int struct_id = Generator.tag_struct[biomeInfo.biome];
        if(struct_id==0) return Build;

        string[] SeonArgs = TxtToSeonArray(SC_fun.SC_data.CustomStructures[struct_id]);
        Vector3 base_position = 100 * new Vector3(X,Y,0f) + biomeInfo.move;
        int setrand=0, ifrand1=-1, ifrand2=-1, setrand_initializer = Ulam + Generator.seed;

        //Random processing
        List<string> args = new List<string>();
        int i,lngt = SeonArgs.Length;
        for(i=0;i<lngt;i++)
        {
            if(i<=lngt-2)
            {
                if(SeonArgs[i]=="setrand")
                {
                    int a = RangedIntParse(SeonArgs[i+1],1,1000);
                    setrand_initializer = Deterministics.Random10e5(setrand_initializer);
                    setrand = setrand_initializer % a;
                    i++; continue;
                }
                if(SeonArgs[i]=="ifrand")
                {
                    string[] ifrand_str = (SeonArgs[i+1]+"-").Split('-');
                    ifrand1 = RangedIntParse(ifrand_str[0],-1,999);
                    ifrand2 = RangedIntParse(ifrand_str[1],ifrand1,999);
                    i++; continue;
                }
            }
            if((ifrand1<=setrand && setrand<=ifrand2) || (ifrand1<=-1 && -1<=ifrand2))
                args.Add(SeonArgs[i]);
        }

        lngt = args.Count;
        bool started = false;
        string cmd = "";
        List<string> cmds = new List<string>();
        string[] key_words = new string[]{
            "summon",
            "move","rotate","reset",
            "setbiome","hide","steal",
            "setanimator","animate",
            "move$","rotate$","reset$"
        };
        int H1=0, H2=0, S1=0, S2=0;
        for(i=0;i<lngt;i++)
        {
            if(i<=lngt-3)
            {
                if(args[i]=="catch")
                {
                    if(args[i+1]=="#")
                    {
                        string[] m_spl = (args[i+2]+"-").Split('-');
                        H1 = RangedIntParse(m_spl[0],0,999);
                        H2 = RangedIntParse(m_spl[1],H1,999);
                        i+=2; continue;
                    }
                    if(args[i+1]=="$")
                    {
                        string[] m_spl = (args[i+2]+"-").Split('-');
                        S1 = RangedIntParse(m_spl[0],0,19);
                        S2 = RangedIntParse(m_spl[1],S1,19);
                        i+=2; continue;
                    }
                }
            }
            if(Array.IndexOf(key_words,args[i])>=0)
            {
                if(started) cmds.Add(cmd+"          ");
                started = true;
                cmd = H1+" "+H2+" "+S1+" "+S2+" "+args[i];
            }
            else cmd += " "+args[i];
        }
        if(started) cmds.Add(cmd+"          ");

        foreach(string line in cmds)
        {
            string[] arg = line.Split(' ');
            H1 = Parsing.IntE(arg[0]);
            H2 = Parsing.IntE(arg[1]);
            S1 = Parsing.IntE(arg[2]);
            S2 = Parsing.IntE(arg[3]);
            int H,S;
            for(H=H1;H<=H2;H++)
            {
                if(arg[4]=="summon")
                {
                    Build[H] = new CObjectInfo(BuildUlam(X,Y,H),base_position);

                    if(arg[5]=="wall") Build[H].Wall(arg[6],arg[7],arg[8]);
                    if(arg[5]=="piston") Build[H].Piston(arg[6],arg[7],arg[8]);
                    if(arg[5]=="sphere") Build[H].Sphere(arg[6],arg[7]);
                    if(arg[5]=="respblock") Build[H].Ranger(arg[6],arg[5]);
                    if(arg[5]=="hole") Build[H].Ranger(arg[6],arg[5]);
                    if(arg[5]=="animator") Build[H].obj = "animator";
                    if(arg[5]=="star") Build[H].Spherical(arg[5]);
                    if(arg[5]=="monster") Build[H].Spherical(arg[5]);

                    if(H>199) continue;
                    if(arg[5]=="asteroid") Build[H].Asteroid(arg[6],arg[7],arg[8]);

                    if(H>0) continue;
                    if(arg[5]=="boss") Build[H].Boss(arg[6]);
                }
                if(Build[H]==null) continue;
                if(arg[4]=="move")
                {
                    float a,b,c=0,d=0;
                    a = Parsing.FloatU(arg[5]);
                    b = Parsing.FloatU(arg[6]);
                    if(arg[7]=="mod") {
                        c = Parsing.FloatU(arg[8]);
                        d = Parsing.FloatU(arg[9]);
                    }
                    if(arg[7]=="offset") {
                        setrand_initializer = Deterministics.Random10e5(setrand_initializer);
                        int vrn = setrand_initializer % 9;
                        int x_vrn = vrn/3-1, y_vrn = vrn%3-1;
                        a*= x_vrn; b*= y_vrn;
                    }
                    int n = H-H1;
                    Build[H].Move((a+n*c)+"",(b+n*d)+"");
                }
                if(arg[4]=="rotate")
                {
                    float a,c=0;
                    a = Parsing.FloatU(arg[5]);
                    if(arg[6]=="mod") {
                        c = Parsing.FloatU(arg[7]);
                    }
                    int n = H-H1;
                    Build[H].Rotate((a+n*c)+"");
                }
                if(arg[4]=="reset") Build[H].Reset(arg[5]);
                if(arg[4]=="setbiome") Build[H].SetBiomeBase(arg[5]);
                if(arg[4]=="hide")
                {
                    if(Build[H].obj!="asteroid") continue;
                    Build[H].hidden = true;
                }
                if(arg[4]=="steal")
                {
                    if(Build[H].obj!="wall") continue;
                    int b = 0;
                    if(arg[5]=="fromhash") b = Parsing.IntU(arg[6]);
                    else if(arg[5]=="fromdelta")
                    {
                        b = Parsing.IntU(arg[6]);
                        b += H;
                    }
                    if(b<0 || b>999) continue;
                    if(Build[b]==null) continue;
                    if(Build[b].obj!="asteroid") continue;
                    
                    int a = Build[b].size;
                    Vector3 rel_pos = Build[H].position - Build[b].position;
                    Build[b].Reset("rotation");
                    Build[b].Move(rel_pos.x+"",rel_pos.y+"");
                    Build[b].hidden = true;

                    float start_dx = -1.7f*(a-1)/2f;
                    for(i=0;i<2*a;i++)
                    {
                        Build[b].ResetS(i,"position");
                        Build[b].ResetS(i,"rotation");
                    }
                    for(i=0;i<a;i++)
                    {
                        Build[b].RotateS(2*i,(Build[H].rotation-90f)+"");
                        Build[b].RotateS(2*i+1,(Build[H].rotation+90f)+"");
                        Build[b].MoveS(2*i,(start_dx + i*1.7f)+"",(1.5f*Build[H].size1)+"");
                        Build[b].MoveS(2*i+1,(start_dx + i*1.7f)+"",(1.5f*Build[H].size1)+"");
                    }
                }
                if(arg[4]=="setanimator")
                {
                    int a = RangedIntParse(arg[5],0,999);
                    if(Build[a]==null) continue;
                    if(Build[a].obj!="animator") continue;
                    if(Build[H].obj=="animator" || Build[H].obj=="boss") continue;
                    Build[H].animator = a;
                    Build[H].animator_reference = Build[a];
                }
                if(arg[4]=="animate")
                {
                    if(arg[6]=="when")
                        Build[H].AnimationCreate(arg[5],arg[7],arg[8],arg[9]);
                }
                if(Build[H].obj!="asteroid") continue;
                for(S=S1;S<=S2;S++)
                {
                    if(arg[4]=="move$")
                    {
                        float a,b,c=0,d=0;
                        a = Parsing.FloatU(arg[5]);
                        b = Parsing.FloatU(arg[6]);
                        if(arg[7]=="mod") {
                            c = Parsing.FloatU(arg[8]);
                            d = Parsing.FloatU(arg[9]);
                        }
                        int n = S-S1;
                        Build[H].MoveS(S,(a+n*c)+"",(b+n*d)+"");
                    }
                    if(arg[4]=="rotate$")
                    {
                        float a,c=0;
                        a = Parsing.FloatU(arg[5]);
                        if(arg[6]=="mod") {
                            c = Parsing.FloatU(arg[7]);
                        }
                        int n = S-S1;
                        Build[H].RotateS(S,(a+n*c)+"");
                    }
                    if(arg[4]=="reset$") Build[H].ResetS(S,arg[5]);
                }
            }
        }

        return Build;
    }
    private static string[] TxtToSeonArray(string str)
	{
		str = str.Replace("\t", " ");
    	str = str.Replace("\r", " ");
    	str = str.Replace("\n", " ");
    	str = str.Replace("[", " ");
    	str = str.Replace("]", " ");
		str = str.Trim();

		while(str.Contains("  "))
        	str = str.Replace("  ", " ");

    	str = string.Join(',',str.Split('.'));
        return str.Split(' ');
	}
    private static int BuildUlam(int X, int Y, int id) // <0;199>
	{	
        if(id>=50 && id<100) { X++; id-=50; }
        if(id>=100 && id<150) { Y++; id-=100; }
        if(id>=150 && id<200) { X++; Y++; id-=150; }
        if(id>=200) return 0;

		int sX = 2*(id%5);
		int sY = id/5;
		if(sY%2==0) sX++;
		return SC_fun.MakeUlam(10*X + sX, 10*Y + sY);
	}
}


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
    public static bool[] tag_odd = new bool[32];
    public static bool[] tag_even = new bool[32];
    public static bool[] tag_structural = new bool[32];

    public const int max_dict_size = 64;
    public static Dictionary<int, CBiomeInfo> WorldMap = new Dictionary<int, CBiomeInfo>();

    //Initializes seed in generator, returns the actual seed string
    public static string SetSeed(string s_seed)
    {
        if(Parsing.IntC(s_seed)) {
            seed = Parsing.IntU(s_seed);
            return s_seed;
        }
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
            tag_odd[i] = false;
            tag_even[i] = false;
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
            if(SC_data.TagContains(tags,"odd")) tag_odd[i] = true;
            if(SC_data.TagContains(tags,"even")) tag_even[i] = true;
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
	public static int MoveVariant(int size)
	{
		if(size<=80 && size>=71) return 1;
        if(size<=70 && size>=61) return 2;
		if(size<=60 && size>=40) return 3;
        if(size<=39 && size>=30) return 2;
		if(size<=29 && size>=20) return 1;
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
        int[] XY = SC_fun.UlamToXY(ulam);
        if(XY[0] < -2000 || XY[0] >= 2000) return 0;
        if(XY[1] < -2000 || XY[1] >= 2000) return 0;
        

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
        int move_variant = MoveVariant(size);
        int table_size = move_variant*2 + 1;
        int table_square = table_size * table_size;
        if(tag_centred[biome] || move_variant==0) return new Vector3(0f,0f,0f);

        int field_num = Deterministics.Random10e3(ulam+seed) % table_square + 1;
        if(tag_even[biome] && field_num % 2 != 0)
        {
            if(field_num != table_square) field_num++;
            else field_num--;
        }
        if(tag_odd[biome] && field_num % 2 == 0)
        {
            field_num++;
        }
        int[] xy = SC_fun.UlamToXY(field_num);
        return new Vector3(xy[0]*10f,xy[1]*10f,0f);
    }
}

public class SC_worldgen : MonoBehaviour
{
    public int GenerationRange;
    public int StructRange;
    public int FrameMaxCreations;
    public bool AllowDebugInfo;

    public Transform player;
    public Transform respawn;
    public SC_object_holder SC_object_holder;
    public SC_control SC_control;
    public SC_fun SC_fun;

    public Dictionary<string,SC_object_holder> Holders = new Dictionary<string,SC_object_holder>();

    void Start()
    {
        BothGenerationsTry();
    }
    void Update()
    {
        BothGenerationsTry();
        if(AllowDebugInfo) DebugInfo();
    }

    void BothGenerationsTry()
    {
        Vector3 host;
        if(SC_control.living) host = player.position;
        else host = respawn.position;
        GenerateTry(host);
        StructureTry(host);
    }

    bool high_started = false;
    bool started = false;
    int last_x = 0, last_y = 0;
    void GenerateTry(Vector3 host)
    {
        int frame_creations = 0;
        int cx = (int)Mathf.Round(host.x/10f);
        int cy = (int)Mathf.Round(host.y/10f);
        bool just_refresh = (started && last_x==cx && last_y==cy);
        started = true; last_x = cx; last_y = cy;

        int x,y,t,t_lngt=(2*GenerationRange+1)*(2*GenerationRange+1);
        if(!(Mathf.Abs(cx) > 20100 || Mathf.Abs(cy) > 20100))
        for(t=1;t<=t_lngt;t++)
        {
            int[] xy = SC_fun.UlamToXY(t);
            x = cx + xy[0]; y = cy + xy[1];
            if((x+y)%2==0)
            {
                string holder_name = "ast_"+x+"_"+y;
                if(!Holders.ContainsKey(holder_name))
                {
                    if(frame_creations < FrameMaxCreations || !high_started) frame_creations++;
                    else
                    {
                        started=false;
                        continue;
                    }

                    SC_object_holder holder = Instantiate(SC_object_holder,10*new Vector3(x,y,0f),Quaternion.identity);
                    CObjectInfo obj = Universe.GetObject(SC_fun.MakeUlam(x,y));
                    holder.Objects = new CObjectInfo[]{obj};
                    holder.name = holder_name;
                    holder.holder_name = holder_name;
                    holder.Unlock();
                    Holders.Add(holder_name,holder);
                }
                else if(!Holders[holder_name].aborted_worldgen) Holders[holder_name].terminate_in = 300;
            }
        }
        high_started = true;
    }

    bool started2 = false;
    int last_x2 = 0, last_y2 = 0;
    void StructureTry(Vector3 host)
    {
        int cx = (int)Mathf.Round(host.x/100f);
        int cy = (int)Mathf.Round(host.y/100f);
        bool just_refresh = (started2 && last_x2==cx && last_y2==cy);
        started2 = true; last_x2 = cx; last_y2 = cy;

        int x,y;
        if(!(Mathf.Abs(cx) > 20100 || Mathf.Abs(cy) > 20100))
        for(x=cx-StructRange;x<=cx+StructRange;x++)
        for(y=cy-StructRange;y<=cy+StructRange;y++)
        if(x%2==0 && y%2==0)
        {
            string holder_name = "S_"+x+"_"+y;
            if(!Holders.ContainsKey(holder_name))
            {
                SC_object_holder holder = Instantiate(SC_object_holder,100*new Vector3(x,y,0f),Quaternion.identity);
                holder.name = holder_name;
                holder.holder_name = holder_name;
                holder.Objects = Universe.GetSector(holder_name);
                holder.Unlock();
                Holders.Add(holder_name,holder);
            }
            else if(!Holders[holder_name].aborted_worldgen) Holders[holder_name].terminate_in = 300;
        }
    }
    
    void DebugInfo()
    {
        if(Input.GetKeyDown("o"))
        {
            int x = (int)Mathf.Round(player.position.x/10f);
            int y = (int)Mathf.Round(player.position.y/10f);
            int ulam = Generator.SC_fun.MakeUlam(x,y);
            CObjectInfo obj = Universe.GetObject(ulam);
            if(obj!=null) UnityEngine.Debug.Log(
                "Obj: "+obj.obj+"\n"+
                "Ulam: "+ulam+" xy: "+x+";"+y+"\n"+
                "Position: "+obj.position+"\n"+
                "Size: "+obj.size+"\n"+
                "Biome: "+obj.biome+"\n"
            );
            else UnityEngine.Debug.Log(
                "Obj: null\n"+
                "Ulam: "+ulam+" xy: "+x+";"+y+"\n"
            );
        }
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
