using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CDeltaPos
{
    public CDeltaPos(float xx, float yy)
    {
        x=xx;
        y=yy;
    }
    public float x,y;
}

public class SC_boss : MonoBehaviour
{
    public bool disable_force_give_up;

    public Transform TestBossNick;
    public Transform Boss;
    public Transform CanvPS;
    public Transform Communtron4;
    Transform CanvP,CanvNick,CanvBar;
    public int type;
    public int smallest_boss_health;

    public string[] BossNames = new string[7];
    public Color32[] arenaColors = new Color32[7];
    const int scrID = 1024;

    public bool mother = true;
    public bool multiplayer = false;
    public int force_give_up_counter = 0;
    Vector3 solidPosition = new Vector3(0f,0f,0f);
    public Transform bossModels;
    public Transform shooterCenter, shooterCenterOver;
    public Transform[] ShooterProjections = new Transform[32];

    public int bX=0,bY=0,bID=1,sID=1;

    int memory2 = -1;
    public int identifier;
    public bool bosnumed = false;
    public int[] dataID = new int[61];

    /*
    K0 -> DataType (1024)
    K1 -> GeneralState
    ---
    K2 -> AdditionalState
    K3 -> TempTimer
    K4 -> BattleTime
    K5 -> MaxBattleTime
    K6 -> IntHealth
    K7 -> MaxIntHealth
    K8 -> delta position X (converted)
    K9 -> delta position Y (converted)
    K10 -> rotation (converted)
    (...)
    */

    public int timer_bar_value = 0;
    public int timer_bar_max = 180;
    public int int_health = 140000;
    public int int_health_max = 220000;

    public SC_data SC_data;
    public SC_control SC_control;
    public SC_object_holder SC_object_holder;
    public SC_bars SC_bars;
    public SC_fun SC_fun;
    public SC_behaviour SC_behaviour;
    public SC_player_follower SC_player_follower; // Main player
    
    public CInfo world;
    public CDeltaPos deltapos;
    public List<CShooter> shooters;

    string GetState(int general, int additional)
    {
        string combo = general+"_"+additional;
        
        if(combo=="0_0") return "A1";
        if(combo=="0_1") return "a1b1";
        if(combo=="0_2") return "B1";
        if(combo=="0_3") return "b1a1";
        if(combo=="1_4") return "b1a2";

        if(combo=="1_0") return "A2";
        if(combo=="1_1") return "a2b2";
        if(combo=="1_2") return "B2";
        if(combo=="1_3") return "b2a2";
        if(combo=="2_4") return "b2a3";

        if(combo=="2_0") return "A3";
        if(combo=="2_1") return "a3b3";
        if(combo=="2_2") return "B3";
        if(combo=="2_3") return "b3a3";
        if(combo=="3_4") return "b3r";

        if(combo=="3_0") return "R";

        if(general==0) return "A1";
        if(general==1) return "A2";
        if(general==2) return "A3";
        if(general==3) return "R";

        return "default";
    }
    public void onMSG(string cmdThis)
    {
        if(mother) return;

        string[] arg = cmdThis.Split(' ');
        if(arg[0]=="/RSD" && arg[1]==bID+"")
        {
            int i;
            string dataIDs = arg[2];
            for(i=0;i<=60;i++)
                dataID[i] = SC_control.Char3ToInt(dataIDs,3*i);

            StateUpdate();
        }
        if(arg[0]=="/RetGiveUpTeleport" && arg[1]==SC_control.connectionID+"" && arg[2]==bID+"")
        {
            SC_control.already_teleported = true;
            SC_player_follower.teleporting = true;
            SC_control.transform.position = NextToRandomGate();
            SC_control.RemoveImpulse();
            SC_control.SC_effect.EffectClean();
        }
    }
    public void StartFromStructure()
    {try{
        SC_control.SC_lists.AddTo_SC_boss(this);
        world = new CInfo(SC_control.SC_lists,SC_control.player);
        deltapos = new CDeltaPos(transform.position.x,transform.position.y);
        identifier = -UnityEngine.Random.Range(1,1000000000);
        shooters = world.GetShootersList(type,this);
        foreach(CShooter shooter in shooters)
            CreateShooterProjection(shooter.radius,shooter.angle,shooter.projection_id,shooter);

        mother = false;
        multiplayer = ((int)Communtron4.position.y==100);
        solidPosition = transform.position;
        bossModels.localPosition = new Vector3(0f,0f,-8000f*type);

        CanvP = Instantiate(CanvPS,new Vector3(0f,0f,0f),Quaternion.identity);
        CanvP.SetParent(TestBossNick,false); CanvP.name = "CanvBS";
        CanvP.localScale = new Vector3(1f,1f,1f) * 0.007f;

        foreach(Transform nck in CanvP)
        {
            if(nck.name=="Nickname")
                CanvNick = nck;

            if(nck.name=="HPBar")
                CanvBar = nck;
        }

        if(!multiplayer) //If singleplayer
        {
            int i;
            WorldData.Load(bX,bY);
            if(WorldData.GetType()!=1024) {
                WorldData.DataGenerate("BOSS");
            }
            for(i=0;i<=60;i++) {
                dataID[i] = WorldData.GetData(i);
            }
			StateUpdate();
        }
        else //If multiplayer
        {
            //Boss initialization server side
            SC_control.SendMTP("/WorldData "+SC_control.connectionID+" "+bID+" "+SC_object_holder.holder_name);

            //Starting refresh
            SC_control.SendMTP("/ScrRefresh "+SC_control.connectionID+" "+bID+" F");
        }

        FixedUpdateT();
        
    }catch(Exception e){
        Debug.LogError(e);
        throw;
    }
    }
    void StateUpdate()
    {
        SC_object_holder.actual_state = GetState(dataID[1],dataID[2]);

        if(memory2!=dataID[2] && InArena("vision"))
        {
            if(memory2==1 && dataID[2]==2) SC_control.InfoUp("Battle started!",500);
            if(memory2==2 && dataID[2]==3) SC_control.InfoUp("Boss wins!",500);
            if(memory2==2 && dataID[2]==4) SC_control.InfoUp("Boss defeated!",500);
            if(memory2==2 && dataID[2]!=2 && InArena("range")) SC_control.SC_effect.EffectClean();
        }
        memory2 = dataID[2];
    }
    void SetBarLength(int current, int max)
    {
        int max_visible = max; if(max==0) max = 1;
        if(max<=20000) max_visible=20000;
        if(max>=140000) max_visible=140000;
        CanvBar.GetComponent<RectTransform>().sizeDelta = new Vector2(120f*max_visible/smallest_boss_health,26f);
        float bar_value = (current*1f/max);
        if(bar_value < 0f) bar_value = 0f;
        if(bar_value > 1f) bar_value = 1f;
        CanvBar.GetComponent<Slider>().value = bar_value;
    }
    public void DamageSGP(float dmg)
    {
        if(dataID[2]!=2 || (type==1 && dataID[18]==2)) return;
        float actualHealth = dataID[6]/100f;
        actualHealth -= dmg;
        dataID[6] = (int)(actualHealth*100);
        Instantiate(SC_control.particlesBossDamage,Boss.position-new Vector3(0f,0f,Boss.position.z),Quaternion.identity);
    }
    void SaveSGP()
	{
        WorldData.Load(bX,bY);
        for(int i=1;i<=60;i++) WorldData.UpdateData(i,dataID[i]);
	}
    void CreateShooterProjection(float rad, float angle_rad, int typ, CShooter shooter)
    {
        if(typ==0) return;
        Transform gob = Instantiate(ShooterProjections[typ],shooterCenter.position,Quaternion.identity);
        gob.parent = shooterCenter;
        gob.localPosition = new Vector3(rad,0f,0f);
        shooterCenter.eulerAngles = new Vector3(0f,0f,angle_rad*180f/3.14159f);
        gob.parent = shooterCenterOver;
        shooterCenter.rotation = Quaternion.identity;
        if(gob.GetComponent<SC_shooter>()!=null)
            gob.GetComponent<SC_shooter>().DeclareAssignment(this,shooter.actives,shooter.one_time_id);
    }
    void FixedUpdate()
    {
        if(transform.GetComponent<SC_seon_remote>()==null)
            FixedUpdateT();
    }
    public void AddForceToBoss(float x, float y)
    {
        float d = ScrdToFloat(dataID[13]);
        float ang = ScrdToFloat(dataID[14]);
        x += Mathf.Cos(ang) * d;
        y += Mathf.Sin(ang) * d;
        dataID[13] = FloatToScrd(Mathf.Sqrt(x*x + y*y));
        dataID[14] = FloatToScrd(Mathf.Atan2(y,x));
    }
    public Vector3 GetVelocity()
    {
        float d = ScrdToFloat(dataID[13]);
        float ang = ScrdToFloat(dataID[14]);
        float x = Mathf.Cos(ang) * d;
        float y = Mathf.Sin(ang) * d;
        return new Vector3(x,y,0f);
    }
    public float GetTransitionFraction(float distance)
    {
        float ret = 0f;
        if(dataID2_client==2) ret = 1f;
        if(dataID2_client==1) ret = Mathf.Min(dataID3_client,40)/40f;
        if(dataID2_client==3 || dataID2_client==4) ret = Mathf.Max(40-dataID3_client,0)/40f;
        
        if(distance < 40f) distance = 40f;
        if(distance > 80f) distance = 80f;
        float X = 1 - (distance-40f)/40f;
        return SC_fun.FluentFraction(X*ret);
    }
    public string TransitionToEffect(string s)
    {
        string result = "";
        for (int i = 2; i < s.Length; i++)
        {
            if(char.IsLower(s[i])) result += char.ToUpper(s[i]);
            else result += s[i];
        }
        return result;
    }
    public string GetReducedState()
    {
        string normal_state = SC_object_holder.actual_state;
        if(normal_state=="default") return "default";
        if(normal_state.Length<=2) return normal_state;
        else return TransitionToEffect(normal_state);
    }

    void Update()
    {
        bool in_arena_vision = InArena("vision");
        bool in_arena_range = InArena("range");
        float fcr = GetArenaFcr();
        if(in_arena_vision) SC_control.SC_fun.camera_add = -12.5f * GetTransitionFraction(fcr);
    }

    //Strange data, do not touch, no one understands, even creator
    int dataID3_client;
    int dataID3_before_state;
    int dataID2_client;
    public int dataID17_client;
    int dataID17_before_state;
    public int dataID18_client;
    public int dataID19_client;
    public int dataID20_client;
    public int dataID21_client;
    bool dataID3_bool = false;

    public void FixedUpdateT()
    {
        if(mother) return;

        //dataID3_client (only for smooth camera movement)
        if(!dataID3_bool)
        {
            dataID3_bool = true;
            dataID3_client = dataID[3];
            dataID3_before_state = dataID[2];
            dataID2_client = dataID[2];
            dataID17_client = dataID[17];
            dataID17_before_state = dataID[18];
        }
        if(dataID[2]!=dataID3_before_state)
        {
            dataID3_before_state = dataID[2];
            dataID3_client = 0;
            dataID2_client = dataID[2];
        }
        else dataID3_client++;
        if(dataID[18]!=dataID17_before_state)
        {
            dataID17_before_state = dataID[18];
            dataID17_client = dataID[19];
        }
        else dataID17_client--;
        dataID18_client = dataID[18];
        dataID19_client = dataID[19];
        dataID20_client = dataID[20];
        dataID21_client = dataID[21];

        //Checking player relative position to arena
        string iar="F"; if(InArena("range")) iar="T";

        //Force give up counter
        if(!disable_force_give_up) {
            if(!multiplayer && !InArena("range") && dataID[2]==2) force_give_up_counter++;
            else force_give_up_counter = 0;
            if(force_give_up_counter>=1000) GiveUpSGP();
        }

        //Multiplayer boss refresher
        if(multiplayer && SC_control.livTime%5==0)
            SC_control.SendMTP("/ScrRefresh "+SC_control.connectionID+" "+bID+" "+iar);
        
        //Singleplayer boss mechanics update
        if(!multiplayer) BossUpdateMechanics();

        //Position & rotation update
        solidPosition = new Vector3(solidPosition.x,solidPosition.y,transform.position.z);
        transform.position = solidPosition;
        
        Boss.position = (solidPosition + new Vector3(
            ScrdToFloat(dataID[8]),
            ScrdToFloat(dataID[9]),
            0f
        ));
        Boss.eulerAngles = new Vector3(0f,0f,ScrdToFloat(dataID[10]));

        //Give up allow checker
        if((InArena("range") && (dataID[2]==2)) && !bosnumed) {
            SC_control.bos_num++;
            bosnumed = true;
        }
        if(!(InArena("range") && (dataID[2]==2)) && bosnumed) {
            SC_control.bos_num--;
            bosnumed = false;
        }

        //Time bar controller
        string ass = SC_object_holder.actual_state;
        if(InArena("vision") && (ass=="B1"||ass=="B2"||ass=="B3")) {
            timer_bar_value = dataID[4];
            timer_bar_max = dataID[5];
            SC_bars.bos = this;
        }
        else {
            if(SC_bars.bos==this) SC_bars.bos = null;
        }
        SC_bars.LateUpdate();

        //Boss name and health bar controller
        if(dataID[2]!=4) CanvNick.GetComponent<Text>().text = BossNames[type] + " " + romeNumber(dataID[1]+1);
        else CanvNick.GetComponent<Text>().text = BossNames[type] + " " + romeNumber(dataID[1]);
        SetBarLength(dataID[6],dataID[7]);
    }
    void BossUpdateMechanics()
    {
        int sta = dataID[2];
        dataID[3]++;
        if(sta==2)
        {
            dataID[4]--;
            world.UpdatePlayers(deltapos);
            if(type==1 && dataID[18]==2) dataID[24] = 0;
            if(dataID[24]>0) dataID[24]--;
            if(dataID[24]%50==0 && dataID[24]!=0) {
                DamageSGP(Parsing.FloatE(SC_data.Gameplay[38]) * Mathf.Pow(1.08f,dataID[25]));
            }
            SC_behaviour._FixedUpdate();
        }
        if((sta==1 || sta==3 || sta==4) && dataID[3]>=50)
        {
            if(sta==1) dataID[2] = 2;
            else if(sta==3 || sta==4) dataID[2] = 0;
            resetScr();
        }
        else if(sta==2 && dataID[4]<=0)
        {
            dataID[2] = 3;
            resetScr();
        }
        else if(sta==2 && dataID[6]<=0)
        {
            dataID[2] = 4;
            resetScr();
        }
        SaveSGP();
    }
    public string romeNumber(int num)
    {
        if(num==1) return "I";
        if(num==2) return "II";
        if(num==3) return "III";
        return "IV";
    }
    int getTimeSize(int n)
    {
        return (int)(SC_data.GplGet("boss_battle_time") * 50);
    }
    int getBossHealth(int n, int typ)
    {
        int ret = 10000;
        if(typ==1) {
            if(n==0) ret = (int)(SC_data.GplGet("boss_hp_protector_1") * 100);
            if(n==1) ret = (int)(SC_data.GplGet("boss_hp_protector_2") * 100);
            if(n==2) ret = (int)(SC_data.GplGet("boss_hp_protector_3") * 100);
        }
        if(typ==2) {
            if(n==0) ret = (int)(SC_data.GplGet("boss_hp_adecodron_1") * 100);
            if(n==1) ret = (int)(SC_data.GplGet("boss_hp_adecodron_2") * 100);
            if(n==2) ret = (int)(SC_data.GplGet("boss_hp_adecodron_3") * 100);
        }
        if(typ==3) {
            if(n==0) ret = (int)(SC_data.GplGet("boss_hp_octogone_1") * 100);
            if(n==1) ret = (int)(SC_data.GplGet("boss_hp_octogone_2") * 100);
            if(n==2) ret = (int)(SC_data.GplGet("boss_hp_octogone_3") * 100);
        }
        if(typ==4) {
            if(n==0) ret = (int)(SC_data.GplGet("boss_hp_starandus_1") * 100);
            if(n==1) ret = (int)(SC_data.GplGet("boss_hp_starandus_2") * 100);
            if(n==2) ret = (int)(SC_data.GplGet("boss_hp_starandus_3") * 100);
        }
        if(typ==6) {
            if(n==0) ret = (int)(SC_data.GplGet("boss_hp_degenerator_1") * 100);
            if(n==1) ret = (int)(SC_data.GplGet("boss_hp_degenerator_2") * 100);
            if(n==2) ret = (int)(SC_data.GplGet("boss_hp_degenerator_3") * 100);
        }
        if(ret<=0) return 1;
        else return ret;
    }
    public void resetScr()
    {
        int mem6 = dataID[6];
        int mem7 = dataID[7];
        int mem8 = dataID[8];
        int mem9 = dataID[9];
        int mem10 = dataID[10];

        int i;
        int sta = dataID[2];
        if(sta==3||sta==4) SC_behaviour._End();
        for(i=3;i<=60;i++) dataID[i] = 0;

        if(sta==0)
        {
            
        }
        else if(sta==1)
        {
            dataID[5] = getTimeSize(dataID[1]); //Max time
            dataID[4] = dataID[5]; //Time left
            dataID[7] = getBossHealth(dataID[1],type); //Max health
            dataID[6] = dataID[7]; //Boss health
        }
        else if(sta==2)
        {
            dataID[5] = getTimeSize(dataID[1]); //Max time
            dataID[4] = dataID[5]; //Time left
            dataID[7] = mem7; //Max health
            dataID[6] = dataID[7]; //Boss health
            SC_behaviour._Start();
        }
        else if(sta==3)
        {
            dataID[7] = mem7; //Max health
            dataID[6] = mem6; //Boss health
            dataID[8] = mem8; dataID[9] = mem9; dataID[10] = mem10; //Position & Rotation
        }
        else if(sta==4)
        {
            dataID[7] = mem7; //Max health
            dataID[8] = mem8; dataID[9] = mem9; dataID[10] = mem10; //Position & Rotation

            dataID[1]++;
            Vector3 poss = Boss.position - new Vector3(0f,0f,Boss.position.z);
            Instantiate(SC_control.particlesBossExplosion,poss,Quaternion.identity);
        }

        StateUpdate();
    }
    public bool InArena(string rngvs)
    {
        float fcr = GetArenaFcr();
        bool in_arena_range = (fcr<=37f);
        bool in_arena_vision = (fcr<=80f);

        if(rngvs=="range") return in_arena_range;
        if(rngvs=="vision") return in_arena_vision;
        return false;
    }
    public float GetArenaFcr()
    {
        if(mother) return 999999f;

        return SC_control.Pitagoras(
            (solidPosition-new Vector3(0f,0f,solidPosition.z))-(SC_control.transform.position-new Vector3(0f,0f,SC_control.transform.position.z))
        );
    }
    public void GiveUpSGP()
    {
        if(dataID[2]!=2) return;
        dataID[4] = 1;
    }
    public void GiveUpMTP()
    {
        SC_control.SendMTP("/GiveUpTry "+SC_control.connectionID+" "+bID);
    }
    Vector3 NextToRandomGate()
    {
        Vector3 posit = solidPosition - new Vector3(0f,0f,solidPosition.z);
        int rand = UnityEngine.Random.Range(0,4);
        if(rand==0) posit += new Vector3(41.5f,0f,0f);
        if(rand==1) posit += new Vector3(-41.5f,0f,0f);
        if(rand==2) posit += new Vector3(0f,41.5f,0f);
        if(rand==3) posit += new Vector3(0f,-41.5f,0f);
        return posit;
    }
    public void CreateTelepPulse(float lx, float ly) {
        Instantiate(SC_control.TelepParticles,bossModels.position-new Vector3(0f,0f,bossModels.position.z),Quaternion.identity);
    }
    public int FloatToScrd(float src) {
        return (int)(src*124);
    }
    public float ScrdToFloat(int src) {
        return src/124f;
    }
    public float[] RotatePoint(float[] crd, float rot, bool convert_to_radians)
    {
        float x = crd[0];
        float y = crd[1];
        float alpha;
        if(convert_to_radians) alpha = rot * 3.14159f / 180f;
        else alpha = rot;
        float[] ret = new float[2];
        ret[0] = ( x*Mathf.Cos(alpha) + y*Mathf.Cos(alpha + 3.14159f / 2f) );
        ret[1] = ( x*Mathf.Sin(alpha) + y*Mathf.Sin(alpha + 3.14159f / 2f) );
        return ret;
    }
    float Pow2(float f) {
        return f*f;
    }
    public float[] GetBounceCoords(float x1,float y1,float x2,float y2,float bounce_radius)
    {
        if(x1==0f && x2==0f && y1==0f && y2==0f) return new float[4]{0f,0f,0f,0f};

        float xa,ya,xb,yb,xc=0f,yc=0f,cnt=0f;
        float x = x2-x1;
        float y = y2-y1;
        if(x==0f) {
            xa = x1;
            ya = Mathf.Sqrt(Pow2(bounce_radius) - Pow2(xa));
            xb = x1;
            yb = -Mathf.Sqrt(Pow2(bounce_radius) - Pow2(xb));

            if(Mathf.Sign(y2-ya)==Mathf.Sign(ya-y1)) {xc=xa; yc=ya; cnt++;}
            if(Mathf.Sign(y2-yb)==Mathf.Sign(yb-y1)) {xc=xb; yc=yb; cnt++;}
        }
        else {
            float a = y/x;
            float b = y1 - a*x1;
            xa = (-a*b + Mathf.Sqrt(Pow2(a)*Pow2(b)-(Pow2(b)-Pow2(bounce_radius))*(Pow2(a)+1)))/(Pow2(a)+1);
            ya = a*xa+b;
            xb = (-a*b - Mathf.Sqrt(Pow2(a)*Pow2(b)-(Pow2(b)-Pow2(bounce_radius))*(Pow2(a)+1)))/(Pow2(a)+1);
            yb = a*xb+b;

            if(Mathf.Sign(x2-xa)==Mathf.Sign(xa-x1)) {xc=xa; yc=ya; cnt++;}
            if(Mathf.Sign(x2-xb)==Mathf.Sign(xb-x1)) {xc=xb; yc=yb; cnt++;}
        }

        if(cnt==0f || Pow2(x2)+Pow2(y2)<=Pow2(bounce_radius)) return new float[4]{x2,y2,0f,0f};

        float alpha = Mathf.Atan2(y1-yc,x1-xc);
        float beta = Mathf.Atan2(-yc,-xc);
        float gamma = 2*beta - alpha;
        float c2_d = Mathf.Sqrt(Pow2(x2-xc)+Pow2(y2-yc));
        float[] get = RotatePoint(new float[2]{c2_d,0f},gamma,false);
        return new float[4]{get[0]+xc,get[1]+yc,gamma,1};
    }
    public void BeforeDestroy()
    {
        if(multiplayer)
            SC_control.SendMTP("/ScrForget "+SC_control.connectionID+" "+bID);
    }
    void OnDestroy()
    {
        SC_control.SC_lists.RemoveFrom_SC_boss(this);
        if(bosnumed) SC_control.bos_num--;
        if(SC_bars.bos==this) SC_bars.bos = null;
    }
}
