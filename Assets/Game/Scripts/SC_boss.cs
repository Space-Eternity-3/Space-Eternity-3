using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_boss : MonoBehaviour
{
    public Transform TestBossNick;
    public Transform CanvPS;
    public Transform Communtron4;
    Transform CanvP,CanvNick;
    public int type;

    public string[] BossNames = new string[7];
    const string scrID = "1024";

    bool mother = true;
    public bool multiplayer = false;

    public int bX=0,bY=0,bID=1,sID=1;

    public string[] dataID = new string[21];
    /*
    p0 -> DataType (1024)
    p1 -> GeneralState (1,2,3,4)
    ---
    p2 -> AdditionalState
    p3 -> cld_2
    (...)
    */

    public int timer_bar_value = 180;
    public bool timer_bar_enabled = false;

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
                dataID[i] = dataIDs[i];

            StateUpdate();
        }
    }
    public void StartFromStructure()
    {
        mother = false;

        multiplayer = ((int)Communtron4.position.y==100);

        CanvP = Instantiate(CanvPS,new Vector3(0f,0f,0f),Quaternion.identity);
        CanvP.SetParent(TestBossNick,false); CanvP.name = "CanvBS";
        CanvP.localScale = new Vector3(1f,1f,1f) * 0.007f;

        foreach(Transform nck in CanvP)
        {
            if(nck.GetComponent<Text>()!=null)
            {
                nck.GetComponent<Text>().text = BossNames[type];
            }
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
                transform.position.x+" "+
                transform.position.y
            );
        }

        FixedUpdate();
        SC_bars.LateUpdate();
    }
    public void StateUpdate()
    {
        SC_structure.actual_state = GetState(int.Parse(dataID[1]),int.Parse(dataID[2]));
        if(!multiplayer) SaveSGP();
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

        if(SC_control.livTime%10==0) SC_control.SendMTP("/ScrRefresh "+SC_control.connectionID+" "+bID);
        
        string ass = SC_structure.actual_state;
        if(
            SC_control.Pitagoras(
            (transform.position-new Vector3(0f,0f,transform.position.z))-(SC_control.transform.position-new Vector3(0f,0f,SC_control.transform.position.z))
            )<=80f &&
            (ass=="B1"||ass=="B2"||ass=="B3"||ass=="a1b1"||ass=="a2b2"||ass=="a3b3")
        ){
            timer_bar_value = 180;
            timer_bar_enabled = true;
        }
        else timer_bar_enabled = false;
    }
}
