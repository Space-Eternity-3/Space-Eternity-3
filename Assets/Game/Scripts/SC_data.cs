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
    public string settingsDIR="./Settings/";
    public string savesDIR="../../saves/";
    public string datapacksDIR="./Datapacks/";
    public string gameDIR="./";
    public TextAsset SourcePackJse3;

    public string worldDIR,asteroidDIR;
    int worldID=0;
    bool worlded=false;
    bool multiplayer;
    public bool menu;
    public bool crashed;
    public string example=""; //Default datapack
    public string clientVersion, clientRedVersion;
    public bool DEV_mode;
	public float global_volume;
	int gpl_number = 112;
    
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
    string writingStorage="";

    //Awake Universal
    public string[] MultiplayerInput = new string[4];
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
    public string[] biome_memories = new string[16000];

    //Asteroids
    public int asteroidCounter=0;
    public string[] WorldSector = new string[17]; //16th reserved for archived saving
    public string[,,] World = new string[100,61,17];

    public List<string> ArchivedWorldSector = new List<string>();
    public List<string[,]> ArchivedWorld = new List<string[,]>();

    //Datapacks
    public string craftings;
    public string craftMaxPage;
	public string biomeChances;
    public string[] DrillLoot = new string[16];
    public string[] FobGenerate = new string[64];
	public string[] BiomeTags = new string[32];
    public string[] CustomStructures = new string[32];
    public string[] TypeSet = new string[224];
    public string[] Gameplay = new string[256];
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

        string[] gpl_info = new string[]{
            
            //[0] - variable name
            //[1] - variable filter (*)all or (+)positive

            "turbo_regenerate_multiplier:+", //0
            "turbo_use_multiplier:+", //1
            "drill_level_add:*", //2
            "copper_bullet_damage:+", //3
            "health_regenerate_cooldown:+", //4
            "health_regenerate_multiplier:+", //5
            "crash_minimum_energy:+", //6
            "crash_damage_multiplier:+", //7
            "spike_damage:+", //8
            "player_normal_speed:*", //9
            "player_brake_speed:*", //10
            "player_turbo_speed:*", //11
            "drill_normal_speed:*", //12
            "drill_brake_speed:*", //13
            "vacuum_drag_multiplier:*", //14
            "all_speed_multiplier:*", //15
            "at_protection_health_level_add:*", //16
            "at_protection_health_regenerate_multiplier:+", //17
            "at_impulse_power_regenerate_multiplier:+", //18
            "at_impulse_time:+", //19
            "at_impulse_speed:*", //20
            "at_illusion_power_regenerate_multiplier:+", //21
            "at_illusion_power_use_multiplier:+", //22
            "at_unstable_normal_avarage_time:+", //23
            "at_unstable_special_avarage_time:+", //24
            "at_unstable_force:*", //25
            "health_level_add:*", //26
            "red_bullet_damage:+", //27
            "unstable_matter_damage:+", //28
            "at_impulse_damage:+", //29
            "bullet_owner_push:*", //30
            "healing_potion_hp:+", //31
            "boss_damage_multiplier:+", //32
            "coal_bullet_damage:+", //33
            "fire_bullet_damage:+", //34
            "killing_potion_hp:+", //35
            "cyclic_damage_multiplier:+", //36
            "boss_unstable_effectivity:+", //37
            "boss_fire_effectivity:+", //38
            "blank_potion_hp:+", //39
            "stone_geyzer_force_multiplier:*", //40
            "magnetic_geyzer_force_multiplier:*", //41
            "amethyst_grow_time_min:+", //42
            "amethyst_grow_time_max:+", //43
            "magnetic_alien_grow_time:+", //44
            "copper_bullet_speed:*", //45
            "red_bullet_speed:*", //46
            "unstable_bullet_speed:*", //47
            "fire_bullet_speed:*", //48
            "coal_bullet_speed:*", //49
            "boss_bullet_speed:+", //50
            "boss_seeker_speed:+", //51
            "cyclic_fire_damage:+", //52
            "cyclic_poison_damage:+", //53
            "cyclic_remote_damage:+", //54
            "cyclic_fire_time:+", //55
            "cyclic_spike_time:+", //56
            "cyclic_spikeball_time:+", //57
            "cyclic_stickybulb_time:+", //58
            "star_collider_damage:+", //59
            "boss_starandus_geyzer_damage:+", //60
            "boss_adecodron_sphere_damage:+", //61
            "boss_octogone_sphere_damage:+", //62
            "boss_bullet_electron_damage:+", //63
            "boss_bullet_fire_damage:+", //64
            "boss_bullet_spike_damage:+", //65
            "boss_bullet_brainwave_damage:+", //66
            "boss_bullet_rocket_damage:+", //67
            "boss_bullet_spikeball_damage:+", //68
            "boss_bullet_copper_damage:+", //69
            "boss_bullet_red_damage:+", //70
            "boss_bullet_unstable_damage:+", //71
            "boss_bullet_graviton_damage:+", //72
            "boss_bullet_neutronium_damage:+", //73
            "boss_battle_time:+", //74
            "boss_hp_protector_1:+", //75
            "boss_hp_protector_2:+", //76
            "boss_hp_protector_3:+", //77
            "boss_hp_adecodron_1:+", //78
            "boss_hp_adecodron_2:+", //79
            "boss_hp_adecodron_3:+", //80
            "boss_hp_octogone_1:+", //81
            "boss_hp_octogone_2:+", //82
            "boss_hp_octogone_3:+", //83
            "boss_hp_starandus_1:+", //84
            "boss_hp_starandus_2:+", //85
            "boss_hp_starandus_3:+", //86
            "boss_hp_degenerator_1:+", //87
            "boss_hp_degenerator_2:+", //88
            "boss_hp_degenerator_3:+", //89
            "cyclic_star_time:+", //90
            "cyclic_starandus_geyzer_time:+", //91
            "copper_bullet_defrange:+", //92
            "red_bullet_defrange:+", //93
            "coal_bullet_defrange:+", //94
            "fire_bullet_defrange:+", //95
            "unstable_bullet_defrange:+", //96
            "copper_bullet_cooldown:+", //97
            "red_bullet_cooldown:+", //98
            "coal_bullet_cooldown:+", //99
            "fire_bullet_cooldown:+", //100
            "unstable_bullet_cooldown:+", //101
            "impulse_cooldown:+", //102
            "lava_geyzer_force_multiplier:*", //103
            "lava_geyzer_damage:+", //104
            "treasure_loot:s", //105
            "dark_treasure_loot:s", //106
            "at_unstable_power_regenerate_multiplier:+", //107
	        "at_unstable_power_normal_eat:+", //108
	        "at_unstable_power_special_eat:+", //109
	        "at_unstable_power_killpot_give:+", //110
	        "at_unstable_max_unstabling_deviation:+", //111
        };
        public int VarNumber(string str,int gnome)
        {
            int i;
            for(i=0;i<gnome;i++)
                if(gpl_info[i].Split(':')[0]==str) return i;
            return -1;
        }
        public string FilterValue(int n,string value)
        {
            string[] spl = gpl_info[n].Split(':');
            if(spl[1]!="s") {
                float parsed = float.Parse(value);
                if(parsed<0 && spl[1]=="+") parsed*=-1;
                return parsed+"";
            }
            else {
                string ret="", str=value+"";
                int i,lngt=str.Length;
                for(i=0;i<lngt;i++)
                    if(new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "-" }.Contains(str[i].ToString())) ret+=str[i];
                return ret;
            }
        }
        public float GplGet(string str)
        {
            return float.Parse(Gameplay[VarNumber(str,gpl_number)]);
        }

    public void ArchiveAdd(int from)
    {
        //Use directly before fragment overwritting
        int i,j,to=ArchivedWorldSector.Count;
        ArchivedWorldSector.Add(WorldSector[from]);
        ArchivedWorld.Add(new string[100,61]);
        for(i=0;i<100;i++)
            for(j=0;j<61;j++) {
                ArchivedWorld[to][i,j] = World[i,j,from];
            }
    }
    public void ArchiveTake(int from, int to)
    {
        int i,j;
        WorldSector[to] = ArchivedWorldSector[from];
        for(i=0;i<100;i++)
            for(j=0;j<61;j++) {
                World[i,j,to] = ArchivedWorld[from][i,j];
            }
        ArchivedWorldSector.RemoveAt(from);
        ArchivedWorld.RemoveAt(from);
    }
    public int GetArchiveIndex(string worldSector)
    {
        int i,lngt=ArchivedWorldSector.Count;
        for(i=0;i<lngt;i++)
            if(ArchivedWorldSector[i]==worldSector)
                return i;
        return -1;
    }
    public void ArchiveSave(int ind)
    {
        ArchiveTake(ind,16);
        SaveAsteroid(16);
    }
    void DirQ(string path)
    {
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);
    }
    void OpenWrite(string file)
    {
        CloseWrite();
        fw = new FileStream(file, FileMode.Create, FileAccess.Write, 0, 4096, FileOptions.WriteThrough);
        sw = new StreamWriter(fw);
        writingStorage = "";
    }
    void SaveLineCrLf(string str)
    {
        writingStorage += str+"\r\n";
    }
    void CloseWrite()
    {
        try{
            sw.Write(writingStorage);
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
        for(i=0;i<4;i++) MultiplayerInput[i]="";
        for(i=0;i<8;i++) for(j=0;j<3;j++) UniverseX[i,j]="";
    }
    void ResetAwakeWorld()
    {
        int i,j;
        for(i=0;i<21;i++) for(j=0;j<2;j++) backpack[i,j]="0";
        for(i=0;i<9;i++) for(j=0;j<2;j++) inventory[i,j]="0";
        for(i=0;i<5;i++) upgrades[i]="0";
        for(i=0;i<8;i++) data[i]="";
        for(i=0;i<16000;i++) biome_memories[i]="";
        dataSource=example;
        dataSourceStorage=example;
    }
    void Start()
    {
        //Raycast auto repair
        Image[] imgs1 = FindObjectsOfType<Image>();
		foreach(Image img in imgs1) {
			if(
                img.GetComponent<Button>()==null &&
                img.GetComponent<SC_instant_button>()==null &&
                img.GetComponent<InputField>()==null &&
                img.gameObject.name!="Handle" &&
                img.GetComponent<SC_raycast_dont_delete>()==null
            ) img.raycastTarget = false;
		}
		RawImage[] imgs2 = FindObjectsOfType<RawImage>();
		foreach(RawImage img in imgs2) {
			if(
                img.GetComponent<Button>()==null &&
                img.GetComponent<SC_instant_button>()==null &&
                img.GetComponent<InputField>()==null &&
                img.GetComponent<SC_raycast_dont_delete>()==null
            ) img.raycastTarget = false;
		}
		Text[] imgs3 = FindObjectsOfType<Text>();
		foreach(Text img in imgs3) {
			if(
                img.GetComponent<Button>()==null &&
                img.GetComponent<SC_instant_button>()==null &&
                img.GetComponent<InputField>()==null &&
                img.GetComponent<SC_raycast_dont_delete>()==null
            ) img.raycastTarget = false;
		}
    }
    void PreAwake()
    {
		//Culture set to comma (India converter)
		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pl-PL");

        if(Application.platform==RuntimePlatform.Android || Application.platform==RuntimePlatform.IPhonePlayer)
        {
            gameDIR = Application.persistentDataPath+"/"+clientRedVersion+"/game/"; // "./";
            settingsDIR = Application.persistentDataPath+"/"+clientRedVersion+"/settings/"; //"./Settings/";
            savesDIR = Application.persistentDataPath+"/saves/"; // "../../saves/";
        }

        example = SourcePackJse3.text.Replace('\r',' ').Replace('\n',' ');
		
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
            case "Biomes": return worldDIR;
            case "PlayerData": return worldDIR;
            case "generated": return asteroidDIR;
            default: return "./ERROR/";
        }
    }
    string GetFile(string D)
    {
        string P=GetPath(D);
        if(D=="generated") return P+"Generated";
        if(D=="Temp") return P+D+".se3";
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
            for(i=0;i<4;i++) MultiplayerInput[i]=sr.ReadLine();
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
                    UniverseX[i-1,2]=clientVersion;
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

        path=GetPath("Biomes");
        file=GetFile("Biomes");
        if(Directory.Exists(path)) if(File.Exists(file))
        {
            OpenRead(file);
            
            for(i=0;i<16000;i++) biome_memories[i]=sr.ReadLine();
            
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
            dataSource=sr.ReadToEnd().Replace('\r',' ').Replace('\n',' ');
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
            
            SaveLineCrLf(volume);
            SaveLineCrLf(camera_zoom);
            SaveLineCrLf(MultiplayerInput[0]);
            SaveLineCrLf(MultiplayerInput[1]);
            SaveLineCrLf(MultiplayerInput[2]);
            SaveLineCrLf(MultiplayerInput[3]);
            if(!menu) SaveLineCrLf(dataSourceStorage);
            else SaveLineCrLf(dataSource);
			SaveLineCrLf(music);
			SaveLineCrLf(compass_mode);

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
            
            SaveLineCrLf(TempFile);
            for(i=0;i<11;i++) SaveLineCrLf(TempFileConID[i]);
            
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
                    
                    SaveLineCrLf(UniverseX[i-1,0]);
                    SaveLineCrLf(UniverseX[i-1,1]);
                    SaveLineCrLf(UniverseX[i-1,2]);

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
            
            SaveLineCrLf(seed);

            CloseWrite();

            }catch(Exception)
            {
                SaveCrash("Seed");
            }
        }
        if(E=="biomes")
        {
            file=GetFile("Biomes");

            try{
            OpenWrite(file);
            
            writingStorage = string.Join("\r\n",biome_memories) + "\r\n";

            CloseWrite();

            }catch(Exception)
            {
                SaveCrash("Biomes");
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
            
            for(i=0;i<4;i++) SaveLineCrLf(effectTab[i]);

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

        //StoreAsteroid(asteroidCounter);
        //SC_control.MainSaveData();

        if(WorldSector[asteroidCounter]!="")
        ArchiveAdd(asteroidCounter);

        int archind = GetArchiveIndex(gX+";"+gY);
        if(archind!=-1)
        {
            //World sector still in archive
            ArchiveTake(archind,asteroidCounter);
        }
        else {

        //Search files for a world sector
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

        }

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
        SaveLineCrLf(seed);
        for(i=0;i<100;i++)
        {
            locef=World[i,0,A];
            for(j=1;j<61;j++) locef=locef+";"+World[i,j,A];
            SaveLineCrLf(ReduceAst(locef));
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

        if(Application.platform==RuntimePlatform.Android || Application.platform==RuntimePlatform.IPhonePlayer)
        {
            return;
        }

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
                if(translateAsteroid[i]+"A"==str) return (16+i)+"";
                if(translateAsteroid[i]+"B"==str) return (32+i)+"";
                if(translateAsteroid[i]+"C"==str) return (48+i)+"";
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
            if((catch_all || (c!=' ' && c!='\r' && c!='\n' && c!='\t')) && !comment)
            {
                if(c=='\r' || c=='\n') raw = raw + " ";
                else raw = raw + c;
            }
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
        for(y=0;y<64;y++) FobGenerate[y]="";
		for(y=0;y<32;y++) BiomeTags[y]="";
        for(y=0;y<224;y++) TypeSet[y]="";
        for(y=0;y<256;y++) Gameplay[y]="";
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
                    try{

                    //Gameplay variable set
                    int gp_num = VarNumber(psPath[1],gpl_number);
                    if(gp_num!=-1) {
                        Gameplay[gp_num] = FilterValue(gp_num,value[i]);
                    }

                    }catch(Exception){DatapackError("Error in variable: "+variable[i]); return;}
                }
                else if(psPath[0]=="custom_structures")
                {
                    try{

                    mID=int.Parse(psPath[1]);
                    CustomStructures[mID]=value[i].Replace('^',' ');

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
		}catch(Exception exc){DatapackError("Unknown error detected"); return;}
		
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
				strT[i] == strT[i+2] //||
				//strT[i+2] == strT[i+4] ||
				//strT[i+4] == strT[i]
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
        }
        for(i=0;i<64;i++)
        {
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
            for(i=0;i<64;i++) FobGenerate[i] = raws[3].Split('\'')[i];
            for(i=0;i<224;i++) TypeSet[i] = raws[4].Split('\'')[i];
            for(i=0;i<gpl_number;i++)
            {
                if(i==105||i==106) Gameplay[i] = raws[5].Split('\'')[i];
                else Gameplay[i] = float.Parse(raws[5].Split('\'')[i])+"";
            }
            for(i=0;i<128;i++) ModifiedDrops[i] = raws[6].Split('\'')[i];
			for(i=0;i<32;i++) BiomeTags[i] = raws[7].Replace(' ','_').Split('\'')[i];
            for(i=0;i<32;i++) CustomStructures[i] = raws[9].Replace('^',' ').Split('\'')[i];

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
		eff+="~"+string.Join("\'",BiomeTags).Replace(' ','_');
		eff+="~"+biomeChances;
        eff+="~"+string.Join("\'",CustomStructures).Replace(' ','^');
		
		return eff;
	}
}
