using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class SC_connection : MonoBehaviour
{
    public SC_data SC_data;
    public SC_account SC_account;
    public Button BT_connect,BT_disconnect,BT_join;
    public InputField IF_nickname,IF_adress;
    public Text TX_connect;
    public Canvas Screen3, Screen5;
    public Text TX_disconnect;

    Vector3 POS_connect,POS_disconnect,POS_join;
    Vector3 POS_hidden;

    int connectionID=-1;
    int BTc=0,BTd=2,BTj=2;
    public int connectionState=0;
    int connect_in_update=0;

    public List<string> UpdatableVersions = new List<string>();
    
    //0 - Disconnected
    //1 - Connecting
    //2 - Allowing
    //3 - Connected
    //4 - Joining

    WebSocket ws;
    bool justLPH;
    int freety=0;
    int waiter=0;
    int counter=0;
    string conID="0";

    //int memcon=0;

    string getData="";
    string getInventory="";
    string maxPlayers="";
    string transfer_datapack="0";
    string getUpgrades="";
    string getBackpack="";
    string getSeed="";

    void Awake()
    {
        SC_data.CollectAwakeUniversal();
        if(SC_data.crashed) return;
    }
    string adressConvert(string ador)
    {
        int lngt=ador.Length;
        if(!(lngt>4&&ador[0]=='w'&&ador[1]=='s'&&((ador[2]==':'&&ador[3]=='/'&&ador[4]=='/')||(lngt>5&&ador[2]=='s'&&ador[3]==':'&&ador[4]=='/'&&ador[5]=='/'))))
            ador = "wss://"+ador;

        lngt = ador.Length;
        if(ador[lngt-1]==':') ador += "27683";

        return ador;
    }
    void SendMTP(string msg)
    {
        msg=msg+" "+conID+" 0";	

        try
        {
            ws.Send(msg);
        }
        catch(Exception e2)
        {
            Debug.LogWarning("Failied sending message: "+msg);
            V_Stop();
        }
    }
    void Start()
    {
        POS_connect=BT_connect.GetComponent<Transform>().localPosition;
        POS_disconnect=BT_disconnect.GetComponent<Transform>().localPosition;
        POS_join=BT_join.GetComponent<Transform>().localPosition;
        POS_hidden=new Vector3(10000f,0f,0f);
        Update();
    }
    void Update()
    {
        //0 - Active
        //1 - Disabled
        //2 - Hidden

        if(connectionState==0) {BTc=0; BTd=1; BTj=2;}
        if(connectionState==1) {BTc=1; BTd=0; BTj=2;}
        if(connectionState==2) {BTc=1; BTd=0; BTj=2;}
        if(connectionState==3) {BTc=2; BTd=0; BTj=0;}
        if(connectionState==4) {BTc=2; BTd=0; BTj=1;}

        if(connectionState<=2) TX_disconnect.text = "Stop";
        else TX_disconnect.text = "Disconnect";

        if(BTc==0)
        {
            TX_connect.text="Connect";
            BT_connect.interactable=true;
            IF_adress.interactable=true;
        }
        else
        {
            TX_connect.text="Connecting";
            BT_connect.interactable=false;
            IF_adress.interactable=false;
        }
        IF_nickname.interactable = BTc==0 && !(SC_account.logged || SC_account.confirming) && (SC_account.waiting_for==0);

        if(BTc==2) BT_connect.GetComponent<Transform>().localPosition=POS_hidden;
        else BT_connect.GetComponent<Transform>().localPosition=POS_connect;

        if(BTd==0) BT_disconnect.interactable=true;
        else BT_disconnect.interactable=false;
        if(BTd==2) BT_disconnect.GetComponent<Transform>().localPosition=POS_hidden;
        else BT_disconnect.GetComponent<Transform>().localPosition=POS_disconnect;

        if(BTj==0) BT_join.interactable=true;
        else {BT_join.interactable=false; freety=0;}
        if(BTj==2) BT_join.GetComponent<Transform>().localPosition=POS_hidden;
        else BT_join.GetComponent<Transform>().localPosition=POS_join;

        if(connect_in_update==1)
        {
            connect_in_update=0;
            True_connect();
        }
        if(connect_in_update>1) connect_in_update--;
    }
    void FixedUpdate()
    {
        if(waiter>0) waiter--;
        if(waiter==0&&connectionState==2)
            ws.Close();

        if(connectionState==3) SendMTP("/ImConnected "+connectionID+" 250");
    }
    void Ws_OnClose(object sender, System.EventArgs e)
    {
        if(connectionState<4)
        {
            connectionState=0;
            Debug.Log("Connection E-closed");
        }
    }
    void Ws_OnError(object sender, System.EventArgs e)
    {
        Debug.Log("Connection E-error");
        connectionState=0;
    }
    void Ws_OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("Connection E-open");
        waiter=200;
        connectionState=2;
        SendMTP("/AllowConnection "+IF_nickname.text+" "+SC_data.clientRedVersion+" "+conID);
    }
    void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        int msl=e.Data.Split(' ').Length;
		if(e.Data.Split(' ')[msl-2]!=conID&&e.Data.Split(' ')[msl-2]!="X") return;

        if(e.Data.Split(' ')[0]=="/RetAllowConnection")
        {
            if(e.Data.Split(' ')[1]!="-1")
            {
                connectionState=3;
                connectionID=int.Parse(e.Data.Split(' ')[1]);
                getData=e.Data.Split(' ')[2];
                getInventory=e.Data.Split(' ')[3];
                maxPlayers=e.Data.Split(' ')[4];
                transfer_datapack=e.Data.Split(' ')[5];
                getUpgrades=e.Data.Split(' ')[6];
                getBackpack=e.Data.Split(' ')[7];
                getSeed=e.Data.Split(' ')[8];
                Debug.Log("Connected "+e.Data.Split(' ')[1]);
            }
            else
            {
                Debug.Log("Connection dennied");
                waiter = 0;
            }
        }
    }
    public void V_Connect()
    {
        if(connectionState!=0) return;
        connectionState=1;
        connect_in_update=2;
    }
    public void V_Stop()
    {
        if(ws!=null) ws.CloseAsync();
        connectionState=0;
    }
    void True_connect()
    {
        conID = UnityEngine.Random.Range(1,999999999)+"";

        string url = adressConvert(IF_adress.text);
        try
        {
            ws = new WebSocket(url);
        }
        catch(Exception e)
        {
            Debug.Log("Wrong adress: "+e.ToString());
            connectionState=0;
            return;
        }

        ws.OnOpen += Ws_OnOpen;
        ws.OnMessage += Ws_OnMessage;
        ws.OnClose += Ws_OnClose;
        ws.OnError += Ws_OnError;
        
        ws.ConnectAsync();
    }
    public void V_ConPlay()
    {
        if(connectionState!=3) return;
        connectionState=4;

        SendMTP("/ImConnected "+connectionID+" 500 JOINING");
        ws.Close();

        SC_data.TempFile="100";
		SC_data.TempFileConID[0]=connectionID+"";
        SC_data.TempFileConID[1]=adressConvert(IF_adress.text);
        SC_data.TempFileConID[2]=IF_nickname.text;
        SC_data.TempFileConID[3]=getData;
        SC_data.TempFileConID[4]=getInventory;
        SC_data.TempFileConID[5]=maxPlayers;
        SC_data.TempFileConID[6]=getUpgrades;
        SC_data.TempFileConID[7]=getBackpack;
        SC_data.TempFileConID[8]=getSeed+"";
        SC_data.TempFileConID[9]=transfer_datapack;
        SC_data.TempFileConID[10]=conID;
        SC_data.Save("temp");

        SceneManager.LoadScene("SampleScene");
    }
}
