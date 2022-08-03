using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Globalization;
using SFB;

public class SC_data : MonoBehaviour
{
    string settingsDIR="./Settings/";
    string savesDIR="../../saves/";
    string datapacksDIR="./Datapacks/";
    string unityDataDIR="./TechnicalData/";

    string gameDIR="./";
    string worldDIR,asteroidDIR;
    int worldID=0;
    bool worlded=false;
    bool multiplayer;
    public bool menu;
    public bool crashed;
    string bin;
    public string example=""; //Default datapack
    public string clientVersion="Beta 1.6",clientRedVersion="Beta_1_6";
    public bool DEV_mode;
	public float global_volume;
	int gpl_number = 30;
    
	bool lockData = false;
	bool remember = false;

    public int[] imgID = new int[16384];
	public int[] imgCO = new int[16384];

    string CustomDataPath="DEFAULT";
    string PreData="";

    public Transform Communtron4;
    public SC_control SC_control;
    public SC_main_buttons SC_main_buttons;
    public Text datapack_name;
    public Text warning_text4; public Transform warning_field4;

    FileStream fr,fw;
    StreamReader sr;
    StreamWriter sw;
    BinaryFormatter bf;

    //Awake Universal
    public string[] MultiplayerInput = new string[2];
    public string TempFile="0";
    public string camera_zoom="-27,5";
    public string volume="0,5";
	public string music="0,5";
	public string compass_mode="0";
    public string[] TempFileConID = new string[11];
    public string[,] UniverseX = new string[8,3];

    //Awake World
    public string seed;
    public string[,] backpack = new string[21,2];
    public string[] upgrades = new string[5];
    public string[] data = new string[8];
    public string[,] inventory = new string[9,2];

    //Asteroids
    public int asteroidCounter=0;
    public string[] WorldSector = new string[16];
    public string[,,] World = new string[100,61,16];

    //Datapacks
    public string craftings;
    public string craftMaxPage;
	public string biomeChances;
    public string[] DrillLoot = new string[16];
    public string[] FobGenerate = new string[16];
	public string[] BiomeTags = new string[32];
    public string[] TypeSet = new string[224];
    public string[] Gameplay = new string[32];
    public string[] ModifiedDrops = new string[128];
    //---------------------------------------------
    public string[] variable = new string[16384];
    public string[] value = new string[16384];
    //---------------------------------------------
    public string[] translateFob = new string[128];
    public string[] translateAsteroid = new string[16];
    //---------------------------------------------
    public string version="";
    public string dataSource="";
    string dataSourceStorage="";

    void DirQ(string path)
    {
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);
    }
    void OpenWrite(string file)
    {
        CloseWrite();
        fw = new FileStream(file, FileMode.Create, FileAccess.Write, 0, 4096, FileOptions.WriteThrough);
        sw = new StreamWriter(fw);
    }
    void CloseWrite()
    {
        try{
            sw.Close();
            fw.Close();
        }catch(Exception){}
    }
    void OpenRead(string file)
    {
        CloseRead();
        fr = new FileStream(file, FileMode.Open);
        sr = new StreamReader(fr);
    }
    void CloseRead()
    {
        try{
            sr.Close();
            fr.Close();
        }catch(Exception){}
    }
    void SaveCrash(string nam)
    {
        CloseWrite();
        crashed=true;
        UnityEngine.Debug.LogError(nam+" can not be saved.");
        Application.Quit();
    }
    void ResetAwakeUniversal()
    {
        int i,j;
        TempFile="0"; volume="0,5"; camera_zoom="-27,5";
		music="0,5"; compass_mode="0";
        seed="";
        for(i=0;i<11;i++) TempFileConID[i]="";
        for(i=0;i<2;i++) MultiplayerInput[i]="";
        for(i=0;i<8;i++) for(j=0;j<3;j++) UniverseX[i,j]="";
    }
    void ResetAwakeWorld()
    {
        int i,j;
        for(i=0;i<21;i++) for(j=0;j<2;j++) backpack[i,j]="0";
        for(i=0;i<9;i++) for(j=0;j<2;j++) inventory[i,j]="0";
        for(i=0;i<5;i++) upgrades[i]="0";
        for(i=0;i<8;i++) data[i]="";
        dataSource=example;
        dataSourceStorage=example;
    }
    void PreAwake()
    {
		//Culture set to comma (India converter)
		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pl-PL");
	
        string file="";
        try{
            
            file=GetFile("defaultdata");
            OpenRead(file);
            example = "";
            while(!sr.EndOfStream) example += sr.ReadLine();
            CloseRead();

        }catch(Exception)
        {
            UnityEngine.Debug.LogError("Default data doesn't exists");
            Application.Quit();
            throw;
        }
		
		int i,j,k;
		PreData=example;
        ResetAwakeUniversal();
        ResetAwakeWorld();
        for(i=0;i<100;i++) for(j=0;j<61;j++) for(k=0;k<16;k++) World[i,j,k]="";
		
        if(menu)
        {
            dataSource=example;
            DatapackTranslate();
        }
    }
    public void WorldID_Interpretate(int n)
    {
        worldDIR=savesDIR+"Universe"+n+"/";
        asteroidDIR=worldDIR+"Asteroids/";
        worldID=n;
        worlded=true;

        if(!Directory.Exists(worldDIR))
        {
            TempFile="-1";
			Save("temp");
			SceneManager.LoadScene("MainMenu");
        }
    }
    string GetPath(string D)
    {
        switch(D)
        {
            case "Temp": return settingsDIR;
            case "UniverseInfo": return savesDIR;
            case "Settings": return settingsDIR;
            case "Seed": return worldDIR;
            case "PlayerData": return worldDIR;
            case "generated": return asteroidDIR;
            case "defaultdata": return unityDataDIR;
            default: return "./ERROR/";
        }
    }
    string GetFile(string D)
    {
        string P=GetPath(D);
        if(D=="generated") return P+"Generated";
        if(D=="Temp") return P+D+".se3";
        if(D=="defaultdata") return P+D+".jse3";
        return P+D+".se3";
    }
    public void CollectAwakeUniversal()
    {
        PreAwake();
        string path="",file="",prePath;
        int i;

        try{
        path=GetPath("Temp");
        file=GetFile("Temp");
        if(Directory.Exists(path)) if(File.Exists(file))
        {
            OpenRead(file);

            TempFile=sr.ReadLine();
            for(i=0;i<11;i++) TempFileConID[i]=sr.ReadLine();

            CloseRead();
            File.Delete(file);
        }
        path=GetPath("Settings");
        file=GetFile("Settings");
        if(Directory.Exists(path)) if(File.Exists(file))
        {
            OpenRead(file);

            volume=float.Parse(sr.ReadLine())+"";
            camera_zoom=float.Parse(sr.ReadLine())+"";
            for(i=0;i<2;i++) MultiplayerInput[i]=sr.ReadLine();
            if(menu)
            {
                PreData=example;
                dataSource=sr.ReadLine();
            }
            else dataSourceStorage=sr.ReadLine();
			music=float.Parse(sr.ReadLine())+"";
			compass_mode=int.Parse(sr.ReadLine())+"";

            CloseRead();
            if(menu) DatapackTranslate();
        }
        prePath=GetPath("UniverseInfo")+"Universe";
        for(i=1;i<=8;i++)
        {
            path=prePath+i+"/";
            file=path+"UniverseInfo.se3";

            if(Directory.Exists(path))
            {
                if(File.Exists(file))
                {
                    OpenRead(file);
                    
                    UniverseX[i-1,0]=int.Parse(sr.ReadLine())+"";
                    UniverseX[i-1,1]=sr.ReadLine();
                    UniverseX[i-1,2]=sr.ReadLine();

                    CloseRead();
                }
                else
                {
                    UniverseX[i-1,0]="0";
                    UniverseX[i-1,1]="DEFAULT~"+example;
                    if(DEV_mode) UniverseX[i-1,2]="DEV";
                    else UniverseX[i-1,2]=clientVersion;
                }
            }
        }

        }catch(Exception)
        {
            CloseRead();
            UnityEngine.Debug.LogWarning("File "+file+" is broken. Deleting it...");

            try{
                if(File.Exists(file)) File.Delete(file);
            }catch(Exception)
            {
                UnityEngine.Debug.LogError("Broken file can't be deleted.");
                Application.Quit();
                throw;
            }

            ResetAwakeUniversal();
            CollectAwakeUniversal();
            return;
        }
    }
    public void CollectAwakeWorld()
    {
        string path="",file="";
        int i,j;

        try{

        path=GetPath("Seed");
        file=GetFile("Seed");
        if(Directory.Exists(path)) if(File.Exists(file))
        {
            OpenRead(file);
            
            seed=int.Parse(sr.ReadLine())+"";
            
            CloseRead();
        }

        path=GetPath("PlayerData");
        file=GetFile("PlayerData");
        if(Directory.Exists(path)) if(File.Exists(file))
        {
            OpenRead(file);

            string[] sourceTab = new string[4];
            for(i=0;i<4;i++) sourceTab[i]=sr.ReadLine();

            string[] ST_D = sourceTab[0].Split(';');
            string[] ST_I = sourceTab[1].Split(';');
            string[] ST_B = sourceTab[2].Split(';');
            string[] ST_U = sourceTab[3].Split(';');

            for(i=0;i<6;i++) data[i]=float.Parse(ST_D[i])+""; data[6]=int.Parse(ST_D[6])+""; data[7]=float.Parse(ST_D[7])+"";
            for(i=0;i<9;i++) for(j=0;j<2;j++) inventory[i,j]=int.Parse(ST_I[i*2+j])+"";
            for(i=0;i<21;i++) for(j=0;j<2;j++) backpack[i,j]=int.Parse(ST_B[i*2+j])+"";
            for(i=0;i<5;i++) upgrades[i]=int.Parse(ST_U[i])+"";

            CloseRead();
        }

        }catch(Exception)
        {
            CloseRead();
            UnityEngine.Debug.LogWarning("File "+file+" is broken. Deleting it...");

            try{
                if(File.Exists(file)) File.Delete(file);
            }catch(Exception)
            {
                UnityEngine.Debug.LogError("Broken file can't be deleted.");
                Application.Quit();
                throw;
            }

            ResetAwakeWorld();
            CollectAwakeWorld();
            return;
        }

        string[] pod_fal=(UniverseX[worldID-1,1]+"~").Split('~');
        if(pod_fal[0]!="DEFAULT")
        {
            string effer=pod_fal[1];
            for(i=2;i<pod_fal.Length;i++) effer+="~"+pod_fal[i];
            DatapackMultiplayerLoad(effer);
            return;
        }
		else
		{
			dataSource = example;
			PreData = example;
			remember = true;
			DatapackTranslate();
		}
    }
    public void CollectUpdateDatapack()
    {
        string file="";
        int i;
        DirQ(datapacksDIR);

        int lngt=CustomDataPath.Length;
        string str=CustomDataPath;
        if(str[lngt-5]=='.'&&str[lngt-4]=='j'&&str[lngt-3]=='s'&&str[lngt-2]=='e'&&str[lngt-1]=='3') try{
            
            file=CustomDataPath;
            OpenRead(file);
            dataSource="";
            for(i=0;!sr.EndOfStream;i++) dataSource=dataSource+sr.ReadLine();
            CloseRead();
            DatapackTranslate();

        }catch(Exception)
        {
            CloseRead();
            dataSource=PreData;
            DatapackTranslate();
            return;
        }
        else
        {
            dataSource=PreData;
            DatapackTranslate();
        }
    }
    public void Save(string E)
    {
        if(crashed&&E!="temp") return;

        string file,path,pathPre;
        int i,j;

        DirQ(savesDIR);
        DirQ(settingsDIR);
        if(worlded) DirQ(asteroidDIR);

        if(E=="settings")
        {
            file=GetFile("Settings");

            try{
            OpenWrite(file);
            
            sw.WriteLine(volume);
            sw.WriteLine(camera_zoom);
            sw.WriteLine(MultiplayerInput[0]);
            sw.WriteLine(MultiplayerInput[1]);
            if(!menu) sw.WriteLine(dataSourceStorage);
            else sw.WriteLine(dataSource);
			sw.WriteLine(music);
			sw.WriteLine(compass_mode);

            CloseWrite();
            
            }catch(Exception)
            {
                SaveCrash("Settings");
            }
        }
        if(E=="temp")
        {
            file=GetFile("Temp");

            try{
            OpenWrite(file);
            
            sw.WriteLine(TempFile);
            for(i=0;i<11;i++) sw.WriteLine(TempFileConID[i]);
            
            CloseWrite();

            }catch(Exception)
            {
                SaveCrash("TempFile");
            }
        }
        if(E=="universeX")
        {
            pathPre=GetPath("UniverseInfo")+"Universe";

            for(i=1;i<=8;i++)
            {
                path=pathPre+i+"/";
                file=path+"UniverseInfo.se3";
                if(UniverseX[i-1,0]!="")
                {
                    DirQ(path);

                    try{
                    OpenWrite(file);
                    
                    sw.WriteLine(UniverseX[i-1,0]);
                    sw.WriteLine(UniverseX[i-1,1]);
                    sw.WriteLine(UniverseX[i-1,2]);

                    CloseWrite();

                    }catch(Exception)
                    {
                        SaveCrash("UniverseInfo"+i);
                    }
                }
            }
        }
        if(E=="seed")
        {
            file=GetFile("Seed");

            try{
            OpenWrite(file);
            
            sw.WriteLine(seed);

            CloseWrite();

            }catch(Exception)
            {
                SaveCrash("Seed");
            }
        }
        if(E=="player_data")
        {
            string[] effectTab = new string[4];
            for(i=0;i<4;i++) effectTab[i]="";

            for(i=0;i<8;i++) effectTab[0]=effectTab[0]+data[i]+";";
            for(i=0;i<9;i++) for(j=0;j<2;j++) effectTab[1]=effectTab[1]+inventory[i,j]+";";
            for(i=0;i<21;i++) for(j=0;j<2;j++) effectTab[2]=effectTab[2]+backpack[i,j]+";";
            for(i=0;i<5;i++) effectTab[3]=effectTab[3]+upgrades[i]+";";

            file=GetFile("PlayerData");

            try{
            OpenWrite(file);
            
            for(i=0;i<4;i++) sw.WriteLine(effectTab[i]);

            CloseWrite();

            }catch(Exception)
            {
                SaveCrash("PlayerData");
            }
        }
    }
    public string GetAsteroid(int X,int Y)
    {
        // X - Real position <INF;INF>
        // pX - Cut position <INF;INF>\<-9;-1>
        // gX - Sector position <INF;INF>
        // rX - Reduced position <0;9>

        int pX,pY,gX,gY,rX,rY,rSS;
        if(X<0) pX=X-9; else pX=X;
        if(Y<0) pY=Y-9; else pY=Y;
        gX=pX/10; gY=pY/10;
        if(X<0) rX=-(X%10); else rX=(X%10);
        if(Y<0) rY=-(Y%10); else rY=(Y%10);
        rSS=rX*10+rY;

        int i,j;
        for(i=0;i<16;i++){
            if(WorldSector[i]==gX+";"+gY) return i+";"+rSS+";F";
        }

        SC_control.MainSaveData();

        string path,file,filePre;
        path=GetPath("generated");
        filePre=GetFile("generated");
        file=filePre+"_"+gX+"_"+gY+".se3";

		AsteroidReset(asteroidCounter);
        WorldSector[asteroidCounter]=gX+";"+gY;
        
		if(Directory.Exists(path))
        {
            if(File.Exists(file))
            {
                string[] lines = new string[101];

                try{

                OpenRead(file);

                for(i=0;i<=100;i++)
                {
                    lines[i]=sr.ReadLine();
                }

                CloseRead();

                if(seed==lines[0]) for(i=0;i<100;i++)
                {
                    string[] line = lines[i+1].Split(';');
                    int lngt=line.Length; if(lngt>61) lngt=61;
                    for(j=0;j<lngt;j++) if(line[j]!="") World[i,j,asteroidCounter]=int.Parse(line[j])+"";
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Asteroid file "+file+" has a wrong seed. Generating new...");
                    AsteroidReset(asteroidCounter);
                }
                
                }catch(Exception)
                {
                    CloseRead();
                    UnityEngine.Debug.LogWarning("Asteroid file "+file+" is broken. Generating new...");
					AsteroidReset(asteroidCounter);

                    try{
                        if(File.Exists(file)) File.Delete(file);
                    }catch(Exception)
                    {
                        UnityEngine.Debug.LogError("Broken file can't be deleted.");
                        Application.Quit();
                        throw;
                    }
                    return GetAsteroid(X,Y);
                }
            }
            else AsteroidReset(asteroidCounter);
        }
        else AsteroidReset(asteroidCounter);

        asteroidCounter++;
        if(asteroidCounter==16) asteroidCounter=0;
        return ((asteroidCounter+15)%16)+";"+rSS+";T";
    }
    void AsteroidReset(int A)
    {
        int i,j;
        for(i=0;i<100;i++) for(j=0;j<61;j++) World[i,j,A]="";
    }
    string ReduceAst(string str)
    {
        int i,lngt=str.Length;
        string eff="";
        for(i=lngt-1;i>=0;i--)
        {
            if(str[i]==';') lngt--;
            else break;
        }
        for(i=0;i<lngt;i++) eff+=str[i];
        return eff;
    }
    public void SaveAsteroid(int A)
    {
        if(WorldSector[A]=="") return;

        int i,j;
        string file,filePre;
        DirQ(asteroidDIR);

        filePre=GetFile("generated");
        string[] gPos = WorldSector[A].Split(';');
        int gX=int.Parse(gPos[0]),gY=int.Parse(gPos[1]);
        file=filePre+"_"+gX+"_"+gY+".se3";
        
        try{

        OpenWrite(file);
        
        string locef;
        sw.WriteLine(seed);
        for(i=0;i<100;i++)
        {
            locef=World[i,0,A];
            for(j=1;j<61;j++) locef=locef+";"+World[i,j,A];
            sw.WriteLine(ReduceAst(locef));
        }

        CloseWrite();

        }catch(Exception)
        {
            SaveCrash("Asteroids "+file);
        }
    }
    public void OpenDataDir()
    {
        warning_text4.text="";
        warning_field4.localPosition=new Vector3(10000f,0f,0f);

        var extensions = new []
        {
            new ExtensionFilter("Jse3 files", "jse3", "txt"),
        };
        DirQ(datapacksDIR);
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "./Datapacks/", extensions, true);

        if(paths.Length==0) return;

        CustomDataPath = paths[0];
        PreData=dataSource;
        CollectUpdateDatapack();
        Save("settings");
    }
    public void ResetDatapack()
    {
        PreData=dataSource;
        warning_text4.text="";
        warning_field4.localPosition=new Vector3(10000f,0f,0f);
        dataSource=example;
        DatapackTranslate();
        Save("settings");
    }
    public void RemoveWarning()
    {
        warning_text4.text="";
        warning_field4.localPosition=new Vector3(10000f,0f,0f);
    }
    void DatapackError(string e)
    {
        if(menu)
        {
            warning_text4.text="Can't import datapack: "+e;
            warning_field4.localPosition=new Vector3(0f,-186f,0f);
        }
        UnityEngine.Debug.LogWarning("Can't import datapack: "+e);

        if(dataSource!=PreData)
        {
            dataSource=PreData;
            if(!menu && (worldID>=1 && worldID<=8)) remember = true;
            DatapackTranslate();
        }
        else
        {
            UnityEngine.Debug.LogError("Datapack jse3 critical error: "+e);
            datapack_name.text="ERROR";
            Application.Quit();
        }
    }
    string ConstructPsPath(string[] tab, string var, int n)
    {
        string effect=""; int i;
        for(i=0;i<n;i++) effect=effect+tab[i]+";";
        return effect+var;
    }
    string Translate(string str, int mode)
    {
        if(str=="") return "ERROR";

        int i;
        if(mode==1)
        {
            for(i=0;i<16;i++)
            {
                if(translateAsteroid[i]==str) return i+"";
            }
        }
        else if(mode==2)
        {
            for(i=0;i<128;i++)
            {
                if(translateFob[i]==str) return i+"";
            }
        }

        //If just translated
        try{

            string ipr=int.Parse(str)+"";
            return ipr;

        }catch(Exception){return "ERROR";}
    }
    string TranslateAll(string str, int mode)
    {
        int i,lngt=str.Length,builds=0;
        string effect="",build="",pom;
        bool reading=true;
        char c;

        for(i=0;i<=lngt;i++)
        {
            if(i!=lngt) c=str[i];
            else c=';';

            if(reading)
            {
                if(c!='('&&c!='+'&&c!='-'&&c!=';')
                {
                    build=build+c;
                }
                else
                {
                    pom=Translate(build,mode);
                    build="";
                    if(pom=="ERROR") return "ERROR";
                    effect=effect+pom;
                    builds++;
                    if(c=='(')
                    {
                        effect=effect+";";
                        reading=false;
                    }
                    else effect=effect+";1";
                    if(c=='+'||c=='-') effect=effect+";";
                    if(c=='-') i++;
                }
            }
            else
            {
                if(c!=')'&&c!='+'&&c!='-'&&c!='>'&&c!=';') effect=effect+c;
                if(c=='+'||c=='-') effect=effect+";";
                if(c=='>'||c=='+') reading=true;
            }
            if(c=='-'&&builds==1) effect=effect+"0;0;";
        }
        return effect;
    }
    string PercentRemove(string str)
    {
        int i,lngt=str.Length;
        string effect="";
        for(i=0;i<lngt-1;i++) effect=effect+str[i];
        return effect;
    }
    string AllPercentRemove(string str, bool must_be_1000)
    {
        if(str=="ERROR") return "ERROR";
        
        string[] tab=str.Split(';');
        int i,lngt=tab.Length,lng;
        string pom;
        int totalChance=0,pre;

        for(i=0;i<lngt;i++)
        {
            pom=tab[i];
            lng=pom.Length;
            if(pom[lng-1]=='%')
            {
                lng--;
                pom=PercentRemove(pom);
                pre=totalChance;
                totalChance+=int.Parse((float.Parse(pom)*10f)+"");
                tab[i]=pre+";"+(totalChance-1);
            }
        }

        if(must_be_1000&&totalChance!=1000) return "ERROR";
        if(totalChance>1000) return "ERROR";

        string effect=tab[0];
        for(i=1;i<lngt;i++) effect=effect+";"+tab[i];
        return effect;
    }
    void DatapackTranslate()
    {
        if(lockData)
        {
            UnityEngine.Debug.LogError("Unexpected datapack load try");
            return;
        }

        string raw=""; char c;
        bool comment=false,catch_all=false;
        int i,lngt=dataSource.Length;
		
        for(i=0;i<lngt;i++)
        {
            c=dataSource[i];
            if(c=='<') comment=true;
            if(c=='\''&&!comment) {catch_all=!catch_all; continue;}
            if((c!=' '||catch_all)&&c!='\t'&&!comment) raw=raw+c;
            if(c=='>') comment=false;
        }

        lngt=raw.Length;
        if(lngt==0)
        {
            DatapackError("Empty file");
            return;
        }

        string[] build_path = new string[1024];
        string build="";
        bool varB=false;
        int clam_level=0,varN=0;

        for(i=0;i<lngt;i++)
        {
            c=raw[i];
            if(c=='~')
            {
                DatapackError("Illegal symbol: "+c);
                return;
            }
            if(!varB)
            {
                if(c=='{')
                {
                    build_path[clam_level]=build;
                    clam_level++;
                    build="";
                }
                else if(c=='}')
                {
                    if(clam_level==0&&build=="")
                    {
                        DatapackError("Unexpected symbol '}'");
                        return;
                    }
                    clam_level--;
                    build_path[clam_level]="";
                }
                else if(c==':')
                {
                    if(build=="")
                    {
                        DatapackError("Variable name can't be empty.");
                        return;
                    }
                    variable[varN]=ConstructPsPath(build_path,build,clam_level);
                    build="";
                    varB=true;
                }
                else if(c==';')
                {
                    DatapackError("Unexpected symbol ';'");
                    return;
                }
                else build=build+c;
            }
            else
            {
                if(c=='{'||c=='}'||c==':')
                {
                    DatapackError("Unexpected symbol '"+c+"'");
                    return;
                }
                else if(c==';')
                {
                    value[varN]=build;
                    build="";
                    varN++;
                    varB=false;
                }
                else build=build+c;
            }
        }
        if(clam_level!=0)
        {
            DatapackError("Number of '{' is not equal to number of '}'");
            return;
        }
        if(raw[lngt-1]!=';'&&raw[lngt-1]!='}')
        {
            DatapackError("Unexpected ending");
            return;
        }
        if(varN==0)
        {
            DatapackError("No variables");
            return;
        }

        FinalTranslate(varN);

    }
    void FinalTranslate(int varN)
    {
        //Reset data
        int y;
        craftings="";
        craftMaxPage="";
		biomeChances="";
        for(y=0;y<16;y++) DrillLoot[y]="";
        for(y=0;y<16;y++) FobGenerate[y]="";
		for(y=0;y<32;y++) BiomeTags[y]="";
        for(y=0;y<224;y++) TypeSet[y]="";
        for(y=0;y<32;y++) Gameplay[y]="";
        for(y=0;y<128;y++) ModifiedDrops[y]="";
        for(y=0;y<16;y++) translateAsteroid[y]="";
        for(y=0;y<128;y++) translateFob[y]="";
        datapack_name.text="";
        version="";

        //Final translate
        int i,mID;
		
		//Starting actions
        for(i=0;i<varN;i++)
        {
            string[] psPath = variable[i].Split(';');
            int lgt=psPath.Length;
            
			if(lgt==1)
            {
                if(psPath[0]=="name") datapack_name.text=value[i];
                if(psPath[0]=="version") version=value[i];
            }
            else if(lgt==2)
            {
                if(psPath[0]=="gameplay")
                {
                    try{ //3, 27, 28, 29, 30, 31
                    //copper_bullet_damage <- bullet_level_add
                    //red_bullet_damage
                    //unstable_matter_damage
                    //at_impulse_damage

					//Normal gameplay
                    if(psPath[1]=="turbo_regenerate_multiplier") Gameplay[0]=float.Parse(value[i])+"";
                    if(psPath[1]=="turbo_use_multiplier") Gameplay[1]=float.Parse(value[i])+"";

					if(psPath[1]=="health_level_add") Gameplay[26]=float.Parse(value[i])+"";
                    if(psPath[1]=="drill_level_add") Gameplay[2]=float.Parse(value[i])+"";

                    if(psPath[1]=="health_regenerate_cooldown") Gameplay[4]=float.Parse(value[i])+"";
                    if(psPath[1]=="health_regenerate_multiplier") Gameplay[5]=float.Parse(value[i])+"";
                    if(psPath[1]=="crash_minimum_energy") Gameplay[6]=float.Parse(value[i])+"";
                    if(psPath[1]=="crash_damage_multiplier") Gameplay[7]=float.Parse(value[i])+"";
                    
                    if(psPath[1]=="spike_damage") Gameplay[8]=float.Parse(value[i])+"";
                    if(psPath[1]=="unstable_matter_damage") Gameplay[28]=float.Parse(value[i])+"";
                    if(psPath[1]=="copper_bullet_damage") Gameplay[3]=float.Parse(value[i])+"";
                    if(psPath[1]=="red_bullet_damage") Gameplay[27]=float.Parse(value[i])+"";

                    if(psPath[1]=="player_normal_speed") Gameplay[9]=float.Parse(value[i])+"";
                    if(psPath[1]=="player_brake_speed") Gameplay[10]=float.Parse(value[i])+"";
                    if(psPath[1]=="player_turbo_speed") Gameplay[11]=float.Parse(value[i])+"";
                    if(psPath[1]=="drill_normal_speed") Gameplay[12]=float.Parse(value[i])+"";
                    if(psPath[1]=="drill_brake_speed") Gameplay[13]=float.Parse(value[i])+"";

                    if(psPath[1]=="vacuum_drag_multiplier") Gameplay[14]=float.Parse(value[i])+"";
                    if(psPath[1]=="all_speed_multiplier") Gameplay[15]=float.Parse(value[i])+"";
					
					//Artefact gameplay
					if(psPath[1]=="at_protection_health_level_add") Gameplay[16]=float.Parse(value[i])+"";
					if(psPath[1]=="at_protection_health_regenerate_multiplier") Gameplay[17]=float.Parse(value[i])+"";
					
					if(psPath[1]=="at_impulse_power_regenerate_multiplier") Gameplay[18]=float.Parse(value[i])+"";
					if(psPath[1]=="at_impulse_time") Gameplay[19]=float.Parse(value[i])+"";
					if(psPath[1]=="at_impulse_speed") Gameplay[20]=float.Parse(value[i])+"";
                    if(psPath[1]=="at_impulse_damage") Gameplay[29]=float.Parse(value[i])+"";
					
					if(psPath[1]=="at_illusion_power_regenerate_multiplier") Gameplay[21]=float.Parse(value[i])+"";
					if(psPath[1]=="at_illusion_power_use_multiplier") Gameplay[22]=float.Parse(value[i])+"";
					
					if(psPath[1]=="at_unstable_normal_avarage_time") Gameplay[23]=float.Parse(value[i])+"";
					if(psPath[1]=="at_unstable_special_avarage_time") Gameplay[24]=float.Parse(value[i])+"";
					if(psPath[1]=="at_unstable_force") Gameplay[25]=float.Parse(value[i])+"";

                    }catch(Exception){DatapackError("Error in variable: "+variable[i]); return;}
                }
            }
            else if(lgt==3)
            {
                if(psPath[0]=="game_translate")
                {
                    if(psPath[1]=="Asteroids")
                    {
                        try{

                            mID=int.Parse(psPath[2]);
                            translateAsteroid[mID]=value[i];

                        }catch(Exception){DatapackError("Error in variable: "+variable[i]); return;}
                    }
                    else if(psPath[1]=="Items_and_objects")
                    {
                        try{

                            mID=int.Parse(psPath[2]);
                            translateFob[mID]=value[i];

                        }catch(Exception){DatapackError("Error in variable: "+variable[i]); return;}
                    }
                }
            }
        }

        //Dictionary required actions
        int mID2, crMax=0;
        string[] crafts = new string[3584]; //512*7
        for(i=0;i<3584;i++) crafts[i]="0;0;0;0;0;0";
		int cur1000biome = 0;

        for(i=0;i<varN;i++)
        {
            string raw_name=variable[i];
            string[] psPath = variable[i].Split(';');
            int lgt=psPath.Length;
            
			if(lgt==2)
            {
                if(psPath[0]=="drill_loot"||psPath[0]=="objects_generate"||psPath[0]=="modified_drops")
                {
                    try{

                        if(psPath[0]=="modified_drops") mID=int.Parse(Translate(psPath[1],2));  //Items and objects
                        else mID=int.Parse(Translate(psPath[1],1));                             //Asteroids
                        variable[i]=psPath[0]+";"+mID;

                        value[i]=TranslateAll(value[i],2);
                        value[i]=AllPercentRemove(value[i],false);

                        int vsl=value[i].Split(';').Length;
                        if((vsl%2!=0&&psPath[0]=="modified_drops")||(vsl%3!=0&&psPath[0]!="modified_drops"))
                        {DatapackError("Error in variable: "+raw_name); return;}

                        if(psPath[0]=="drill_loot") DrillLoot[mID]=value[i];
                        if(psPath[0]=="objects_generate") FobGenerate[mID]=value[i];
                        if(psPath[0]=="modified_drops") ModifiedDrops[mID]=value[i];

                    }catch(Exception){DatapackError("Error in variable: "+raw_name); return;}
                }
            }
            else if(lgt==3)
            {
                if(psPath[0]=="craftings")
                {
                    try{

                        mID=int.Parse(psPath[1]);
				        if(psPath[2]=="title_image")
				        {
					        mID2=7;
					        mID=7*(mID-1)+6;
					        value[i]=Translate(value[i],2)+";1;0;0;-1;1";
				        }
				        else
				        {
					        mID2=int.Parse(psPath[2]);
					        mID=7*(mID-1)+mID2-1;
					        value[i]=TranslateAll(value[i],2);
				        }
				        variable[i]=psPath[0]+";"+mID;
                  
				        if(value[i].Split(';')[0]=="ERROR") {DatapackError("Error in variable: "+raw_name); return;}
                        if(value[i].Split(';').Length!=6||!((mID2>=1&&mID2<=5)||(psPath[2]=="title_image"))) {DatapackError("Error in variable: "+raw_name); return;}
                        if(value[i].Split(';')[0]==value[i].Split(';')[2]) {DatapackError("Error in variable: "+raw_name); return;}

                        crafts[mID]=value[i];
                        if(mID>crMax) crMax=mID;

                    }catch(Exception){DatapackError("Error in variable: "+raw_name); return;}
                }
                else if(psPath[0]=="generator_settings")
                {
                    try{
                        mID=int.Parse(psPath[1]);
						if(mID<0||mID>31) throw new Exception("error");
						
						if(psPath[2]=="settings")
						{
							BiomeTags[mID] = value[i].Replace(' ','_');
						}
						else if(psPath[2]!="chance")
						{
							if(psPath[2]=="all_sizes") mID2=-4;
							else mID2=int.Parse(psPath[2])-4;
						
							if((mID2<0||mID2>6)&&mID2!=-4) throw new Exception("error");
							if(mID2!=-4) mID=7*mID+mID2;
							else mID=7*mID;
							variable[i]=psPath[0]+";"+mID;

							value[i]=TranslateAll(value[i],1);
							value[i]=AllPercentRemove(value[i],true);

							if(value[i].Split(';').Length%3!=0) {DatapackError("Error in variable: "+raw_name); return;}

							if(mID2!=-4) TypeSet[mID]=value[i];
							else
							{
								int uu;
								for(uu=0;uu<7;uu++) TypeSet[mID+uu]=value[i];
							}
						}

                    }catch(Exception){DatapackError("Error in variable: "+raw_name); return;}
                }
            }
        }
		
		//Late actions
		for(i=0;i<varN;i++)
        {
			string raw_name=variable[i];
            string[] psPath = variable[i].Split(';');
            int lgt=psPath.Length;
			
			if(lgt==3)
			{
				if(psPath[0]=="generator_settings")
                {
					try{
                        mID=int.Parse(psPath[1]);
						if(mID<0||mID>31) throw new Exception("error");
						
						if(psPath[2]=="chance")
						{
							if(mID==0) throw new Exception("error");
							string efe = mID+";"+cur1000biome+";";
							
							int le = value[i].Length;
							if((value[i])[le-1]=='%') value[i] = PercentRemove(value[i]);
							else throw new Exception("error");
							
							int mno; if(TagContains(BiomeTags[mID],"structural")) mno = 2;
							else mno = 1;
							
							cur1000biome += mno * int.Parse((float.Parse(value[i])*10f)+"");
							efe += (cur1000biome-1)+";";
							biomeChances += efe;
						}
					}
					catch(Exception){DatapackError("Error in variable: "+raw_name); return;}
				}
			}
		}
		
        craftings=crafts[0];
        for(i=1;i<=crMax;i++)
        {
            craftings=craftings+";"+crafts[i];
        }
        craftMaxPage=((crMax/7)+1)+"";
		
		//last biome chance correction
		if(biomeChances!="") biomeChances = PercentRemove(biomeChances);

        //Check if all is good
        for(i=0;i<gpl_number;i++) if(Gameplay[i]=="") {DatapackError("Required variable not found: gameplay_"+i); return;}
		if(cur1000biome>1000) {DatapackError("Total biome chance can't be over 1000p. Current: "+cur1000biome+"p. 1p = 0,1%. Note: structural option doubles 'p' ussage, but the chance is still multiplied by 1."); return;}

        if(version!=clientVersion) {DatapackError("Wrong version or empty version variable."); return;}
        if(datapack_name.text=="DEFAULT"&&dataSource!=example) {DatapackError("Custom datapack name can't be DEFAULT. Change it using a text editor."); return;}
        if(datapack_name.text=="") {DatapackError("Datapack name can't be empty. Change it using a text editor."); return;}
		
		try{
			checkDatapackGoodE();
		}catch(Exception){DatapackError("Unknown error detected."); return;}
		
		//Last
		if(remember)
		{
			UniverseX[worldID-1,1]="DEFAULT~"+GetDatapackSe3();
            Save("universeX"); 
		}
    }
	public bool TagContains(string tags, string tag)
	{
		return (Array.IndexOf(tags.Replace('[','_').Replace(']','_').Replace('_',',').Split(','),tag)>-1);
	}
    bool IntsAll(string str, int div)
    {
        string[] strs = str.Split(';');
        int i=0,lngt = strs.Length;
        int pom;
        
        try{

            if(str!="")
            for(i=0;i<lngt;i++) 
            {
                pom = int.Parse(strs[i]);
            }

        }catch(Exception){return false;}

        if(i%div==0) return true;
        else return false;
    }
	bool In1000(string str, bool must_be_1000)
	{
		string[] strT = str.Split(';');
		int i, lngt = strT.Length;
		if(str=="") lngt = 0;
		
		bool ended = true;
		int actual = -1;
		for(i=1;i<lngt;i++)
		{
			if(ended)
			{
				if(actual+1 != int.Parse(strT[i])) return false;
				actual++; ended = false;
			}
			else
			{
				if(actual > int.Parse(strT[i]) + 1) return false;
				actual = int.Parse(strT[i]); ended = true; i++;
			}
		}
		if(!must_be_1000 && actual<=999) return true;
		if(must_be_1000 && actual==999) return true;
		return false;
	}
	bool GoodItems(string str, bool craft_mode)
	{
		string[] strT = str.Split(';');
		int i, lngt = strT.Length;
		if(str=="") lngt = 0;
		
		for(i=0;i<lngt;i+=2)
		{
			if(int.Parse(strT[i+1]) < 0) return false;
			if(strT[i]=="0" && strT[i+1]!="0") return false;
			if(strT[i+1]=="0" && strT[i]!="0") return false;
		}
		if(craft_mode) for(i=0;i<lngt;i+=6)
		{
			if(strT[i]!="0" && strT[i+1]!="0" && strT[i+2]!="0" && strT[i+3]!="0" && strT[i+4]!="0" && strT[i+5]!="0")
			if(
				strT[i+4] == "0" ||
				strT[i] == "0" ||
				strT[i] == strT[i+2] ||
				strT[i+2] == strT[i+4] ||
				strT[i+4] == strT[i]
			)return false;
		}
		return true;
	}
	bool DrillGoodItem(string str)
	{
		string[] strT = str.Split(';');
		int i, lngt = strT.Length;
		if(str=="") lngt = 0;
	
		for(i=0;i<lngt;i+=3)
		{
			if(strT[i+2]=="0") return false;
		}
		return true;
	}
	//MUST BE INSIDE try{} catch(Exception){}
	void checkDatapackGoodE()
	{
		int i;
		
		//Check int arrays
        if(!IntsAll(craftings,6) || !GoodItems(craftings,true)) throw new Exception("error");
		if(!IntsAll(biomeChances,3) || !In1000(biomeChances,false)) throw new Exception("error");
        for(i=0;i<16;i++)
        {
            if(!IntsAll(DrillLoot[i],3) || !In1000(DrillLoot[i],false) || !DrillGoodItem(DrillLoot[i])) throw new Exception("error");
            if(!IntsAll(FobGenerate[i],3) || !In1000(FobGenerate[i],false)) throw new Exception("error");
        }
        for(i=0;i<224;i++)
        {
            if(TypeSet[i]!="") if(!IntsAll(TypeSet[i],3) || !In1000(TypeSet[i],true)) throw new Exception("error");
        }
        for(i=0;i<128;i++)
        {
            if(!IntsAll(ModifiedDrops[i],2) || !GoodItems(ModifiedDrops[i],false)) throw new Exception("error");
        }
	}
    public void DatapackMultiplayerLoad(string raw)
    {
        int i;
        string[] raws = raw.Split('~');

        try{

			//Load data
            craftings = raws[0];
			biomeChances = raws[8];
            craftMaxPage = int.Parse(raws[1])+"";

            for(i=0;i<16;i++) DrillLoot[i] = raws[2].Split('\'')[i];
            for(i=0;i<16;i++) FobGenerate[i] = raws[3].Split('\'')[i];
            for(i=0;i<224;i++) TypeSet[i] = raws[4].Split('\'')[i];
            for(i=0;i<gpl_number;i++)
            {
                if(false) Gameplay[i] = int.Parse(raws[5].Split('\'')[i])+"";
                else Gameplay[i] = float.Parse(raws[5].Split('\'')[i])+"";
            }
            for(i=0;i<128;i++) ModifiedDrops[i] = raws[6].Split('\'')[i];
			for(i=0;i<32;i++) BiomeTags[i] = raws[7].Replace(' ','_').Split('\'')[i];

			checkDatapackGoodE();
        
        }catch(Exception)
        {
            dataSource = "";
            PreData = example;
            DatapackError("Wrong se3 datapack. Loading DEFAULT...");
            if(Communtron4.position.y==100) SC_control.MenuReturn();
            lockData = true;
            return;
        }
        lockData = true;
    }
	public string GetDatapackSe3()
	{
		string eff=craftings+"~"+craftMaxPage;
		
		eff+="~"+string.Join("\'",DrillLoot);
		eff+="~"+string.Join("\'",FobGenerate);
		eff+="~"+string.Join("\'",TypeSet);
		eff+="~"+string.Join("\'",Gameplay);
		eff+="~"+string.Join("\'",ModifiedDrops);
		eff+="~"+string.Join("\'",BiomeTags);
		
		eff+="~"+biomeChances;
		
		return eff;
	}
}
