using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

public class SC_fun : MonoBehaviour
{
    public Transform Communtron1;
	public GameObject[] GenPlaceT = new GameObject[128];
	public GameObject[] GenPlaceM = new GameObject[18];
    public Material[] M_auto = new Material[128];
    public Texture[] Item = new Texture[128];
	public string[] ItemNames = new string[128];
    public Texture Item20u, Item55u, Item57u, Item59u, Item61u, Item63u, Item71u, Item79u;
	public Material[] texture = new Material[16];
	public Material[] textureStone;
	public Material[] textureDark;
	public Material[] textureCopper;
	public Material textureERROR;
	public Material baseFob21Material;
	
	public bool ExperimentalItemInfo;
    public float volume;
	public float seek_default_angle;
	public float camera_add;
	public bool arms_did = false;
	public float difficulty = 1f;
	public bool keep_inventory = false;
	public bool respawn_allow = true; //Only for Update() use!
	public bool respawn_allow_reinit = true;
	public bool dev_bosbul = false;
	public bool jump_ping_simulator = false;

	public float[] boss_damages = new float[16];
	public float[] boss_damages_cyclic = new float[16];
	public float[] other_bullets_colliders = new float[16];
	public bool[] force_destroy_effect = new bool[16];
	public bool[] bullet_air_consistence = new bool[16];
	public int[] bullet_effector = new int[16];

    public bool[] pushed_markers = new bool[9];
	
    public Transform[] structures;
	public Transform biomeCAN;
    public SC_control SC_control;
    public SC_data SC_data;
    public SC_long_strings SC_long_strings;

	//Veteran methods
	public Vector3 Skop(Vector3 V, float F)
	{
		float pita = SC_control.Pitagoras(V);
		if(pita!=0) return V*F/SC_control.Pitagoras(V);
		else return V*F;
	}
	public static float[] RotateVector(float x, float y, float a)
	{
	    float angle = a * (float)Math.PI / 180;

    	float newX = x * (float)Math.Cos(angle) - y * (float)Math.Sin(angle);
    	float newY = x * (float)Math.Sin(angle) + y * (float)Math.Cos(angle);

    	return new float[] { newX, newY };
	}
	public static float AngleBetweenVectorAndOX(float dx, float dy)
	{
    	float angle = (float)Math.Atan2(dy, dx) * (180 / (float)Math.PI);
    	if(angle < 0) angle += 360;
    	return angle;
	}
	public bool AreCoordinatesInsideRect(RectTransform trn, float x, float y, float dx, float dy)
    {
        Vector3[] corners = new Vector3[4];
        trn.GetWorldCorners(corners);

        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

        return (x >= minX-dx && x <= maxX+dx && y >= minY-dy && y <= maxY+dy);
    }
	public float FluentFraction(float f)
    {
        return f*f*((-2)*f+3);
    }
	public Material Fob21Material(int n)
	{
		if(M_auto[0]==null) return baseFob21Material;
		else return M_auto[n];
	}
	public float reduceAngle(float angle)
    {
        while(angle<0) angle+=360f;
        while(angle>=360) angle-=360f;
        return angle;
    }
    public float rotAvg(int weight1, float angle1, float angle2)
    {
        float sr;
        angle1 = reduceAngle(angle1);
        angle2 = reduceAngle(angle2);

        sr = (weight1*angle1 + angle2)/(weight1 + 1);
        if(Mathf.Abs(angle2-angle1) <= 180f) return sr;

        angle1 += 180f; angle1 = reduceAngle(angle1);
        angle2 += 180f; angle2 = reduceAngle(angle2);

        sr = (weight1*angle1 + angle2)/(weight1 + 1) - 180f;
        return reduceAngle(sr);
    }

	public static float CalculateAverageAngle(float angle1, float angle2)
    {
		//Written by ChatGPT

        // Konwersja kątów na radiany
        float angle1Rad = DegreesToRadians(angle1);
        float angle2Rad = DegreesToRadians(angle2);
        
        // Obliczenie współrzędnych x i y dla każdego kąta
        float x1 = (float)Math.Cos(angle1Rad);
        float y1 = (float)Math.Sin(angle1Rad);
        float x2 = (float)Math.Cos(angle2Rad);
        float y2 = (float)Math.Sin(angle2Rad);
        
        // Średnia współrzędnych
        float xAvg = (x1 + x2) / 2;
        float yAvg = (y1 + y2) / 2;
        
        // Obliczenie kąta wynikowego z współrzędnych średnich
        float avgAngleRad = (float)Math.Atan2(yAvg, xAvg);
        
        // Konwersja kąta wynikowego z radianów na stopnie
        float avgAngleDeg = RadiansToDegrees(avgAngleRad);
        
        // Upewnienie się, że wynikowy kąt jest w zakresie [0, 360) stopni
        if (avgAngleDeg < 0)
        {
            avgAngleDeg += 360;
        }
        
        return avgAngleDeg;
    }
    
    private static float DegreesToRadians(float degrees)
    {
        return degrees * (float)Math.PI / 180f;
    }
    
    private static float RadiansToDegrees(float radians)
    {
        return radians * 180f / (float)Math.PI;
    }

	//Ulam <---> X,Y methods
    public int MakeUlam(int X,int Y)
    {
        int ID=0,P;
        if(Mathf.Abs(X)>Mathf.Abs(Y)) P=Mathf.Abs(X);
		else P=Mathf.Abs(Y);

		ID=4*(P*P-P)+1;
		X=X+P+1;
		Y=Y+P+1;

		if(X==(2*P+1)&&Y!=1) ID=ID+Y-1;
		else if(Y==(2*P+1)) ID=ID+4*P+1-X;
		else if(X==1) ID=ID+6*P+1-Y;
		else if(Y==1) ID=ID+6*P+X-1;

        return ID;
    }
	public int[] UlamToXY(int ulam)
	{
		int sqrt = IntSqrt(ulam);
		if(sqrt%2==0) sqrt--;

		int x = sqrt/2 + 1;
		int y = -sqrt/2 - 1;
		int pot = sqrt * sqrt;
		int delta = ulam - pot;
		int cwr = delta / (sqrt+1);
		int dlt = delta % (sqrt+1);
		
		if(cwr==0 && dlt==0) return new int[]{x-1,y+1};
		if(cwr>0) y+=(sqrt+1);
		if(cwr>1) x-=(sqrt+1);
		if(cwr>2) y-=(sqrt+1);

		if(cwr==0) y+=dlt;
		if(cwr==1) x-=dlt;
		if(cwr==2) y-=dlt;
		if(cwr==3) x+=dlt;

		return new int[]{x,y};
	}
	public int IntSqrt(int n)
	{
		int a=0, b=n;
		if(b>46340) b=46340; //overflow protection
		while(a<=b)
		{
			int piv = (a+b)/2;
			int sqpiv = piv*piv;
			if(sqpiv > n)
			{
				b = piv - 1;
				continue;
			}
			else if(sqpiv < n)
			{
				a = piv + 1;
				continue;
			}
			else return piv;
		}
		return b;
	}

	//BiomeMemories methods
	public void BiomeMemoriesUpdate()
	{
		string[] old_data = new string[16000];
		int i,j;
		for(i=0;i<16000;i++)
		{
			old_data[i] = SC_data.biome_memories[i];
			SC_data.biome_memories[i] = "";
		}
		for(i=0;i<16000;i++)
		{
			int lngt = old_data[i].Length;
			for(j=1;j<lngt;j+=2) //only odd
			{
				int ulam = i*1000 + j;
				int biome = CharToNum31(old_data[i][j]);
				if(biome!=-1) InsertBiome(ulam,biome);
			}
			SC_data.biome_memories_state[i] = 3;
		}
	}
	public int FindBiome(int ulam)
	{
		int[] LnId = UlamToLnId(ulam);
		int ln = LnId[0];
		int id = LnId[1];
		SC_data.MemorySureMake(ln);

		if(SC_data.biome_memories[ln].Length <= id) return -1;
		int cand = CharToNum31(SC_data.biome_memories[ln][id]);
		if(cand < 0 || cand > 31) return -1;
		else return cand;
	}
	public void InsertBiome(int ulam, int biome)
	{
		if((int)SC_control.Communtron4.position.y==100) return;

		int[] LnId = UlamToLnId(ulam);
		int ln = LnId[0];
		int id = LnId[1];

		while(SC_data.biome_memories[ln].Length <= id)
			SC_data.biome_memories[ln] += '-';
			
		int i;
		StringBuilder ret = new StringBuilder();
		for(i=0;i<id;i++) ret.Append(SC_data.biome_memories[ln][i]);
		ret.Append(Num31ToChar(biome));
		for(i=id+1;i<SC_data.biome_memories[ln].Length;i++) ret.Append(SC_data.biome_memories[ln][i]);
		SC_data.biome_memories[ln] = ret.ToString();

		SC_data.biome_memories_state[ln] = 3;
	}
	public int[] UlamToLnId(int ulam)
	{
		int[] XY = UlamToXY(ulam);
		XY[0] += 2000;
		XY[1] += 2000; //0-3999
		int X = XY[0] / 32;
		int Y = XY[1] / 32;
		int x = XY[0] % 32;
		int y = XY[1] % 32;
		int ln = 125*X + Y;
		int id = 32*x + y;
		return new int[]{ln,id};
	}
	public char Num31ToChar(int num)
	{
		if(num < 10) return (char)(48+num);
		return (char)(55+num);
	}
	public int CharToNum31(char ch)
	{
		if(ch=='-') return -1;
		int num = (int)ch;
		if(num < 65) return num-48;
		else return num-55;
	}

	//Unity methods
	void Start()
	{
		int i;
		for(i=0;i<Item.Length;i++)
		{
			M_auto[i] = new Material(baseFob21Material);
            M_auto[i].mainTexture = Item[i];
		}
	}
}
