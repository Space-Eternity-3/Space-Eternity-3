using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SC_boss : MonoBehaviour
{
    public Transform TestBossNick;
    public Transform Boss;
    public Transform CanvPS;
    public Transform Communtron4;
    Transform CanvP,CanvNick,CanvBar;
    public int type;
    public int smallest_boss_health = 140000;

    public string[] BossNames = new string[7];
    public Color32[] arenaColors = new Color32[7];
    const string scrID = "1024";

    bool mother = true;
    public bool multiplayer = false;
    Vector3 solidPosition = new Vector3(0f,0f,0f);

    public int bX=0,bY=0,bID=1,sID=1;

    string memory2 = "-1";
    public bool bosnumed = false;
    public string[] dataID = new string[21];

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
            string[] dataIDs = arg[2].Split(';');
            for(i=0;i<=20;i++)
            {
                try{
                    dataID[i] = int.Parse(dataIDs[i])+"";
                }catch(Exception){
                    dataID[i] = "0";
                }
            }

            StateUpdate();
        }
    }
    public void StartFromStructure()
    {
        mother = false;
        multiplayer = ((int)Communtron4.position.y==100);
        solidPosition = transform.position;

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
			if(SC_data.World[a,0,c]!=scrID)
			{
				//Not exists
				for(i=0;i<20;i++){
                    dataID[i+1] = "0";
                }
				SC_data.World[a,0,c]=dataID[0]+"";
				for(i=0;i<20;i++){
					SC_data.World[a,i+1,c]=dataID[i+1]+"";
				}
				if(uAst[2]=="T") SC_data.SaveAsteroid(c);
			}
			else
			{
				//Exists
				for(i=0;i<20;i++){
					dataID[i+1] = SC_data.World[a,i+1,c];
                    if(dataID[i+1]=="") dataID[i+1] = "0";
				}
			}
			StateUpdate();
        }
        else //If multiplayer
        {
            SC_control.SendMTP(
                "/ScrData "+
                SC_control.connectionID+" "+
                bID+" "+
                scrID+" "+
                type+" "+
                solidPosition.x+" "+
                solidPosition.y
            );
        }

        FixedUpdate();
        SC_bars.LateUpdate();
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
        SC_structure.actual_state = GetState(int.Parse(dataID[1]),int.Parse(dataID[2]));
        
        float fcr = SC_control.Pitagoras(
            (solidPosition-new Vector3(0f,0f,solidPosition.z))-(SC_control.transform.position-new Vector3(0f,0f,SC_control.transform.position.z))
        );
        bool in_arena_vision = (fcr<=80f);

        if(memory2!=dataID[2] && in_arena_vision)
        {
            if(memory2=="1" && dataID[2]=="2") SC_control.InfoUp("Battle started!",500);
            if(memory2=="2" && dataID[2]=="3") SC_control.InfoUp("Boss wins!",500);
            if(memory2=="2" && dataID[2]=="4") SC_control.InfoUp("Boss defeated!",500);
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
        if(dataID[2]!="2") return;
        float actualHealth = int.Parse(dataID[6])/100f;
        actualHealth -= dmg;
        dataID[6] = (int)(actualHealth*100)+"";
        Instantiate(SC_control.particlesBossDamage,Boss.position-new Vector3(0f,0f,Boss.position.z),Quaternion.identity);
    }
    void SaveSGP()
	{
		string[] uAst = SC_data.GetAsteroid(bX,bY).Split(';');
        int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]),i;
		for(i=0;i<=20;i++) SC_data.World[a,i,c]=dataID[i];
		if(uAst[2]=="T") SC_data.SaveAsteroid(c);
	}
    void FixedUpdate()
    {
        if(mother) return;

        float fcr = SC_control.Pitagoras(
            (solidPosition-new Vector3(0f,0f,solidPosition.z))-(SC_control.transform.position-new Vector3(0f,0f,SC_control.transform.position.z))
        );
        bool in_arena_range = (fcr<=35f);
        bool in_arena_vision = (fcr<=80f);
        string iar="F"; if(in_arena_range) iar="T";

        if(!multiplayer && !in_arena_range)
            GiveUpSGP();

        solidPosition = new Vector3(solidPosition.x,solidPosition.y,transform.position.z);
        transform.position = solidPosition;
        Boss.position = solidPosition + new Vector3(
            ScrdToFloat(dataID[8]),
            ScrdToFloat(dataID[9]),
            0f
        );
        Boss.eulerAngles = new Vector3(0f,0f,ScrdToFloat(dataID[10]));

        if((in_arena_range && (dataID[2]=="2")) && !bosnumed)
        {
            SC_control.bos_num++;
            bosnumed = true;
        }
        if(!(in_arena_range && (dataID[2]=="2")) && bosnumed)
        {
            SC_control.bos_num--;
            bosnumed = false;
        }

        if(SC_control.livTime%5==0) //Precisely 10 times per second
        {
            if(multiplayer) SC_control.SendMTP("/ScrRefresh "+SC_control.connectionID+" "+bID+" "+iar);
            if(!multiplayer) Fixed10Broken();
        }

        string ass = SC_structure.actual_state;
        if(in_arena_vision && (ass=="B1"||ass=="B2"||ass=="B3"))
        {
            timer_bar_value = int.Parse(dataID[4]);
            timer_bar_max = int.Parse(dataID[5]);
            timer_bar_enabled = true;
        }
        else timer_bar_enabled = false;

        CanvNick.GetComponent<Text>().text = BossNames[type] + " " + romeNumber(int.Parse(dataID[1])+1);
        SetBarLength(int.Parse(dataID[6]),int.Parse(dataID[7]));
    }
    void Fixed10Broken()
    {
        var sta = dataID[2];
        dataID[3] = (int.Parse(dataID[3])+1)+"";
        if(sta=="2") dataID[4] = (int.Parse(dataID[4])-1)+"";
        if((sta=="1" || sta=="3" || sta=="4") && int.Parse(dataID[3])>=10)
        {
            if(sta=="1") dataID[2] = "2";
            else if(sta=="3" || sta=="4") dataID[2] = "0";
            resetScr();
        }
        else if(sta=="2" && int.Parse(dataID[4])<=0)
        {
            dataID[2] = "3";
            resetScr();
        }
        else if(sta=="2" && int.Parse(dataID[6])<=0)
        {
            dataID[2] = "4";
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
    string getTimeSize(string n)
    {
        if(n=="0") return "1800";
        if(n=="1") return "2100";
        if(n=="2") return "2400";
        return "600";
    }
    string getBossHealth(string n, int typ)
    {
        if(typ==0)
        {
            if(n=="0") return "160000";
            if(n=="1") return "180000";
            if(n=="2") return "200000";
        }
        return "40000";
    }
    public void resetScr()
    {
        string mem6 = dataID[6];
        string mem8 = dataID[8];
        string mem9 = dataID[9];
        string mem10 = dataID[10];

        int i;
        for(i=3;i<=20;i++) dataID[i] = "0";
        string sta = dataID[2];

        if(sta=="0")
        {
            
        }
        else if(sta=="1")
        {
            dataID[5] = getTimeSize(dataID[1]); //Max time
            dataID[4] = dataID[5]; //Time left
            dataID[7] = getBossHealth(dataID[1],type); //Max health
            dataID[6] = dataID[7]; //Boss health
        }
        else if(sta=="2")
        {
            dataID[5] = getTimeSize(dataID[1]); //Max time
            dataID[4] = dataID[5]; //Time left
            dataID[7] = getBossHealth(dataID[1],type); //Max health
            dataID[6] = dataID[7]; //Boss health
        }
        else if(sta=="3")
        {
            dataID[7] = getBossHealth(dataID[1],type); //Max health
            dataID[6] = mem6; //Boss health
            dataID[8] = mem8; dataID[9] = mem9; dataID[10] = mem10; //Position & Rotation
        }
        else if(sta=="4")
        {
            dataID[1] = (int.Parse(dataID[1])+1)+"";
            Instantiate(SC_control.particlesBossExplosion,Boss.position-new Vector3(0f,0f,Boss.position.z),Quaternion.identity);
        }

        StateUpdate();
    }
    public void GiveUpSGP()
    {
        if(dataID[2]!="2") return;

        dataID[4] = "1";
        Fixed10Broken();
    }
    public void GiveUpMTP(bool killed)
    {
        if(dataID[2]!="2") return;

        if(fightingPlayers()<=1) //Only you on arena
        {
            SC_control.SendMTP("/ScrSabotage "+SC_control.connectionID+" "+bID);
        }
        else if(!killed) //More players on arena
        {
            SC_control.transform.position = NextToRandomGate();
            SC_control.playerR.velocity = new Vector3(0f,0f,0f);
        }
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
    int fightingPlayers()
    {
        int i,amount = 0;
        float fcr = SC_control.Pitagoras(
            (solidPosition-new Vector3(0f,0f,solidPosition.z))-(SC_control.transform.position-new Vector3(0f,0f,SC_control.transform.position.z))
        ); bool in_arena_range = (fcr<=35f);
        if(in_arena_range) amount++;

        if(multiplayer)
        for(i=1;i<SC_control.max_players;i++)
        {
            if(SC_control.NUL[i])
            {
                Vector3 sps = SC_control.PL[i].sourcedPosition;
                
                fcr = SC_control.Pitagoras(
                    (solidPosition-new Vector3(0f,0f,solidPosition.z))-(sps-new Vector3(0f,0f,sps.z))
                ); in_arena_range = (fcr<=35f);

                if(in_arena_range) amount++;
            }
        }
        return amount;
    }
    string FloatToScrd(float src) {
        return ((int)(src*100000))+"";
    }
    float ScrdToFloat(string src) {
        return int.Parse(src)/100000f;
    }
    void OnDestroy()
    {
        if(bosnumed) SC_control.bos_num--;
    }
}
