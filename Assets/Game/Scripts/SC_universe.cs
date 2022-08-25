using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public class SC_universe : MonoBehaviour
{
    public Transform BT_create;
    public Transform BT_play;
    public Transform BT_confirm;
    public Text info;
    public Text playT;
    public Button playBu;
    public Button createBu;
    public Transform Communtron1;
    public SC_data SC_data;

    bool crB=true;
    bool plB=false;
    bool coB=false;
    bool before_moment=false;
    public int WorldID;
    string namte;

    string timeF="0";
    string scoreF="0";
    string versionF="NA";

    Vector3 cr,pl,co;
    Vector3 disab=new Vector3(10000f,0f,0f);

    void P_escaped()
    {
        if(coB)
        {
            coB=false;
            plB=true;
        }
        if(before_moment)
        {
            plB=false;
            crB=true;
            coB=false;
            Communtron1.position=new Vector3(0f,Communtron1.position.y,Communtron1.position.z);
        }
    }
    string TimeConvert(int sec)
    {
        int min,hours;
        string minS, secS;
        hours=sec/3600;
        sec-=hours*3600;
        min=sec/60;
        sec-=min*60;
        if(min<10) minS="0"+min; else minS=min+"";
        if(sec<10) secS="0"+sec; else secS=sec+"";
        if(hours>0) return hours+":"+minS+":"+secS;
        else return minS+":"+secS;
    }
    void Start()
    {
        playBu.enabled=false;

        cr=BT_create.localPosition;
        pl=BT_play.localPosition;
        co=BT_confirm.localPosition;

	    if(Directory.Exists(SC_data.savesDIR+"Universe"+WorldID+"/"))
    	{
            timeF=SC_data.UniverseX[WorldID-1,0];
            if(timeF=="") timeF="0";
	    	scoreF=SC_data.UniverseX[WorldID-1,1];
            scoreF=scoreF.Split('~')[0];
            versionF=SC_data.UniverseX[WorldID-1,2];
            crB=false; plB=true;
		}
        info.text="Time: "+TimeConvert(int.Parse(timeF))+"\nVersion: "+versionF;
        if(scoreF!="DEFAULT"&&scoreF!="0") info.text=info.text+"\n"+scoreF;
        if(!(SC_data.DEV_mode||versionF=="NA"||(versionF=="DEV"&&SC_data.DEV_mode)||(versionF==SC_data.clientVersion)))
        {
            playT.text="Incompatible";
            playT.fontSize=38;
        }
        else playBu.enabled=true;
    }
    void Update()
    {
        if(crB) BT_create.localPosition=cr; else BT_create.localPosition=disab;
        if(plB) BT_play.localPosition=pl; else BT_play.localPosition=disab;
        if(coB) BT_confirm.localPosition=co; else BT_confirm.localPosition=disab;
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            P_escaped();
        }
        if(Communtron1.position.x==0f) createBu.enabled=true;
        else createBu.enabled=false;
    }
    public void V_Create()
    {
        crB=false;
        plB=true;
        info.text="New Universe"+GetDatapackName(SC_data.datapack_name.text,false);
        //if(SC_data.datapack_name.text!="DEFAULT") info.text=info.text+"\n"+SC_data.datapack_name.text;
        before_moment=true;
        playBu.enabled=true;
        playT.text="Play";
        playT.fontSize=50;
        Communtron1.position=new Vector3(1f,Communtron1.position.y,Communtron1.position.z);
    }
    public void V_Delete()
    {
        if(before_moment)
        {
            Communtron1.position=new Vector3(0f,Communtron1.position.y,Communtron1.position.z);
            V_Confirm();
        }
        else
        {
            coB=true;
            plB=false;
        }
    }
    string GetDatapackName(string str,bool raw)
    {
        if(raw)
        {
            if(str=="DEFAULT") return "DEFAULT";
            else return "Custom Data";
        }
        else
        {
            if(str=="DEFAULT") return "";
            else return "\nCustom Data";
        }
    }
    public void V_Play()
    {
		SC_data.TempFile=WorldID+"";
        SC_data.Save("temp");

        string dir=SC_data.savesDIR+"Universe"+WorldID+"/";
        if(!Directory.Exists(dir))
        {
		    SC_data.UniverseX[WorldID-1,0]="0";
			string dtpn = GetDatapackName(SC_data.datapack_name.text,true);
            SC_data.UniverseX[WorldID-1,1] = dtpn+"~"+SC_data.GetDatapackSe3(); //SC_data.dataSource;
            if(SC_data.DEV_mode) SC_data.UniverseX[WorldID-1,2]="DEV";
            else SC_data.UniverseX[WorldID-1,2]=SC_data.clientVersion;
            SC_data.Save("universeX");
        }
        SceneManager.LoadScene("SampleScene");
    }
    public void V_Cancel()
    {
        P_escaped();
    }
    public void V_Confirm()
    {
        coB=false;
        plB=false;
        crB=true;

        SC_data.UniverseX[WorldID-1,0]="";
        SC_data.UniverseX[WorldID-1,1]="";
        SC_data.UniverseX[WorldID-1,2]="";

        string drs=SC_data.savesDIR+"Universe"+WorldID+"/";
        if(Directory.Exists(drs))
        Directory.Delete(drs,true);
    }
}
