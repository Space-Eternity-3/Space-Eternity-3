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

public class CSmoothMemory
{
    private int ms;
    private Vector3[] position;
    private float[] rotation;
    private SC_players SC_players;
    public bool used;

    public CSmoothMemory(int mss, SC_players plrs)
    {
        ms = mss;
        position = new Vector3[ms];
        rotation = new float[ms];
        SC_players = plrs;
        ResetData();
    }
    public void ResetData()
    {
        int i;
        for(i=0;i<ms;i++)
        {
            position[i] = new Vector3(0f,0f,10000f);
            rotation[i] = 10000f;
        }
        used = false;
    }
    public void PushArray(Vector3 npos, float nrot)
    {
        int i;
        for(i=ms-1;i>0;i--)
        {
            position[i] = position[i-1];
            rotation[i] = rotation[i-1];
        }
        position[0] = npos;
        rotation[0] = nrot;
        
        used = true;
    }
    public Vector3 AveragePosition()
    {
        int i,m=ms;
        Vector3 sum=new Vector3(0f,0f,0f);
        for(i=0;i<ms;i++){
            if(position[i].z<100f)
                sum+=position[i];
            else
                m--;
        }
        if(m!=0) return sum/m;
        else return new Vector3(0f,0f,2000f);
    }
    public float AverageRotation()
    {
        int i,m=0;
        float currentAngle = 0f;

        for(i=0;i<ms;i++)
        {
            if(rotation[i]!=10000f)
            {
                currentAngle = SC_players.rotAvg(m,currentAngle,rotation[i]);
                m++;
            }
        }

        return currentAngle;
    }
}

public class SC_boss : MonoBehaviour
{
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
    CSmoothMemory SmoothMemory;

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
    public bool timer_bar_enabled = false;
    public int int_health = 140000;
    public int int_health_max = 220000;

    public SC_data SC_data;
    public SC_control SC_control;
    public SC_structure SC_structure;
    public SC_bars SC_bars;
    public SC_behaviour SC_behaviour;
    
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
            SC_control.transform.position = NextToRandomGate();
            SC_control.RemoveImpulse();
        }
    }
    public void StartFromStructure()
    {try{
        SC_control.SC_lists.AddTo_SC_boss(this);
        SmoothMemory = new CSmoothMemory(4,SC_control.PL[1]);
        world = new CInfo(SC_control.SC_lists,SC_control.player);
        deltapos = new CDeltaPos(transform.position.x,transform.position.y);
        identifier = -UnityEngine.Random.Range(1,1000000000);
        shooters = world.GetShootersList(type,this);
        foreach(CShooter shooter in shooters)
            CreateShooterProjection(shooter.radius,shooter.angle,shooter.bullet_type);

        mother = false;
        multiplayer = ((int)Communtron4.position.y==100);
        solidPosition = transform.position;
        bossModels.localPosition = new Vector3(0f,0f,-4000f*type);

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
            string[] uAst = SC_data.GetAsteroid(bX,bY).Split(';');
			int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]),i;
				
            dataID[0]=scrID;
			if(SC_data.World[a,0,c]!=scrID+"")
			{
				//Not exists
				for(i=1;i<=60;i++){
                    dataID[i] = 0;
                }
				SC_data.World[a,0,c]=dataID[0]+"";
				for(i=1;i<=60;i++){
					SC_data.World[a,i,c]=dataID[i]+"";
				}
			}
			else
			{
				//Exists
				for(i=1;i<=60;i++){
                    try{
					    dataID[i] = int.Parse(SC_data.World[a,i,c]);
                    }catch(Exception){
                        dataID[i] = 0;
                    }
				}
			}
			StateUpdate();
        }
        else //If multiplayer
        {
            //Boss initialization server side
            SC_control.SendMTP(
                "/ScrData "+
                SC_control.connectionID+" "+
                bID+" "+
                scrID+" "+
                type+" "+
                solidPosition.x+" "+
                solidPosition.y
            );

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
        if(SC_structure.actual_state=="default")
        {
            //If not initialized, jump true by default
            int i,lngt=SC_structure.st_structs.Length;
            for(i=0;i<lngt;i++)
                if(SC_structure.st_structs[i]!=null)
                    if(SC_structure.st_structs[i].GetComponent<SC_seon_remote>()!=null)
                        SC_structure.st_structs[i].GetComponent<SC_seon_remote>().jump = true;
        }
        SC_structure.actual_state = GetState(dataID[1],dataID[2]);
        
        float fcr = SC_control.Pitagoras(
            (solidPosition-new Vector3(0f,0f,solidPosition.z))-(SC_control.transform.position-new Vector3(0f,0f,SC_control.transform.position.z))
        );
        bool in_arena_vision = (fcr<=80f);

        if(memory2!=dataID[2] && in_arena_vision)
        {
            if(memory2==1 && dataID[2]==2) SC_control.InfoUp("Battle started!",500);
            if(memory2==2 && dataID[2]==3) SC_control.InfoUp("Boss wins!",500);
            if(memory2==2 && dataID[2]==4) SC_control.InfoUp("Boss defeated!",500);
        }
        memory2 = dataID[2];
    }
    void SetBarLength(int current, int max)
    {
        if(max==0) max=40000;
        CanvBar.GetComponent<RectTransform>().sizeDelta = new Vector2(120f*max/smallest_boss_health,26f); /* 120 - 190 */
        CanvBar.GetComponent<Slider>().value = (current*1f/max);
    }
    public void DamageSGP(float dmg)
    {
        if(dataID[2]!=2) return;
        float actualHealth = dataID[6]/100f;
        actualHealth -= dmg;
        dataID[6] = (int)(actualHealth*100);
        Instantiate(SC_control.particlesBossDamage,Boss.position-new Vector3(0f,0f,Boss.position.z),Quaternion.identity);
    }
    void SaveSGP()
	{
		string[] uAst = SC_data.GetAsteroid(bX,bY).Split(';');
        int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]),i;
		for(i=0;i<=60;i++) SC_data.World[a,i,c]=dataID[i]+"";
	}
    void CreateShooterProjection(float rad, float angle_rad, int typ)
    {
        Transform gob = Instantiate(ShooterProjections[0],shooterCenter.position,Quaternion.identity);
        gob.parent = shooterCenter;
        gob.localPosition = new Vector3(rad,0f,0f);
        shooterCenter.eulerAngles = new Vector3(0f,0f,angle_rad*180f/3.14159f);
        gob.parent = shooterCenterOver;
        shooterCenter.rotation = Quaternion.identity;

    }
    void FixedUpdate()
    {
        if(transform.GetComponent<SC_seon_remote>()==null)
            FixedUpdateT();

        if(transform.GetComponent<SC_seon_remote>()!=null)
            if(transform.GetComponent<SC_seon_remote>().SC_structure==null)
                FixedUpdateT();
    }
    public void FixedUpdateT()
    {
        if(mother) return;

        //Checking player relative position to arena
        float fcr = SC_control.Pitagoras(
            (solidPosition-new Vector3(0f,0f,solidPosition.z))-(SC_control.transform.position-new Vector3(0f,0f,SC_control.transform.position.z))
        );
        bool in_arena_range = (fcr<=37f);
        bool in_arena_vision = (fcr<=80f);
        string iar="F"; if(in_arena_range) iar="T";

        //Force give up counter
        if(!multiplayer && !in_arena_range && dataID[2]==2) force_give_up_counter++;
        else force_give_up_counter = 0;
        if(force_give_up_counter>=50) GiveUpSGP();

        //Multiplayer boss refresher
        if(multiplayer && SC_control.livTime%5==0)
            SC_control.SendMTP("/ScrRefresh "+SC_control.connectionID+" "+bID+" "+iar);
        
        //Singleplayer boss mechanics update
        if(!multiplayer) BossUpdateMechanics();

        //Position & rotation update
        if(multiplayer && dataID[2]!=2 && SmoothMemory.used) SmoothMemory.ResetData();
        solidPosition = new Vector3(solidPosition.x,solidPosition.y,transform.position.z);
        transform.position = solidPosition;
        if(multiplayer && dataID[2]==2)
        {
            SmoothMemory.PushArray(solidPosition + new Vector3(
                ScrdToFloat(dataID[8]),
                ScrdToFloat(dataID[9]),
                0f
            ),ScrdToFloat(dataID[10]));

            Boss.position = SmoothMemory.AveragePosition();
            Boss.eulerAngles = new Vector3(0f,0f,SmoothMemory.AverageRotation());
        }
        else
        {
            Boss.position = (solidPosition + new Vector3(
                ScrdToFloat(dataID[8]),
                ScrdToFloat(dataID[9]),
                0f
            ));
            Boss.eulerAngles = new Vector3(0f,0f,ScrdToFloat(dataID[10]));
        }

        //Give up allow checker
        if((in_arena_range && (dataID[2]==2)) && !bosnumed) {
            SC_control.bos_num++;
            bosnumed = true;
        }
        if(!(in_arena_range && (dataID[2]==2)) && bosnumed) {
            SC_control.bos_num--;
            bosnumed = false;
        }

        //Time bar controller
        string ass = SC_structure.actual_state;
        if(in_arena_vision && (ass=="B1"||ass=="B2"||ass=="B3")) {
            timer_bar_value = dataID[4];
            timer_bar_max = dataID[5];
            timer_bar_enabled = true;
            SC_bars.bos = this;
        }
        else {
            timer_bar_enabled = false;
            SC_bars.bos = null;
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
    string romeNumber(int num)
    {
        if(num==1) return "I";
        if(num==2) return "II";
        if(num==3) return "III";
        return "IV";
    }
    int getTimeSize(int n)
    {
        return 9000;
    }
    int getBossHealth(int n, int typ)
    {
        return 100000;
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
    public void GiveUpSGP()
    {
        if(dataID[2]!=2) return;
        dataID[4] = 1;
    }
    public void GiveUpMTP(bool killed)
    {
        if(dataID[2]!=2) return;
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
