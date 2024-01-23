using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldData
{
    public static SC_data SC_data;
    public static int a=0; // line
    public static int c=0; // sector id

    //Technical methods
    public static void Load(int bX, int bY) //Loads X;Y data to memory
    {
        string[] uAst = SC_data.GetAsteroid(bX,bY).Split(';');
        c=int.Parse(uAst[0]);
        a=int.Parse(uAst[1]);
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
            else type = CalculateFromString(SC_data.TypeSet[int.Parse(elements[1])*7 + size-4]);

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
                if(gen==-1) gen = CalculateFromString(SC_data.FobGenerate[type]);
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
    private static int CalculateFromString(string chance_string)
    {
        int decider = UnityEngine.Random.Range(0,1000);
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

public class SC_worldgen : MonoBehaviour
{
    // empty class now
}
