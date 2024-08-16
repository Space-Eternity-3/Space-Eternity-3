using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Threading;

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
    bool block_retransmission;

    public List<string> UpdatableVersions = new List<string>();
    
    //0 - Disconnected
    //1 - Connecting
    //2 - Allowing
    //3 - Connected
    //4 - Joining

    WebSocket ws;
    bool justLPH;
    int waiter=0;
    string conID="0";

    //int memcon=0;

    string getData="";
    string getInventory="";
    string maxPlayers="";
    string transfer_datapack="0";
    string getUpgrades="";
    string getBackpack="";
    string getSeed="";

    string gotAddress = "?";
    string delayedWarning = "";
    string ConnectionUrl = "";

    void Awake()
    {
        SC_data.CollectAwakeUniversal();
    }
    string adressConvert(string ador)
    {
        int lngt=ador.Length;
        if(!ador.StartsWith("ws://") && !ador.StartsWith("wss://") && !ador.StartsWith("se3://"))
            ador = "se3://"+ador;

        lngt = ador.Length;
        if(ador[lngt-1]==':') ador += "27683";

        return ador;
    }
    string trueAddressGet(string ador)
    {
        ador = adressConvert(ador);
        if(ador.StartsWith("se3://")) return adressConvert(adressDownload(ador.Substring(6)));
        else return ador;
    }
    string adressDownload(string serverName)
    {
        string e = "se3://ERROR "; //not logged in
        string e1 = "se3://ERROR_1 "; //connection error
        string e2 = "se3://ERROR_2 "; //no such server
        string e3 = "se3://ERROR_3 "; //exception error
        string ask = "/Authorize "+SC_account.IF_n1.text+" "+SC_account.IF_p1.text+" "+serverName+" "+conID;
        string response = null;

        try {

        if(SC_account.connected_to_authorizer && SC_account.logged)
        {
            using(WebSocket _ws = new WebSocket(SC_account.authorizationServer))
            {
                _ws.OnMessage += (sender, e) => {
                    response = e.Data;
                };

                _ws.Connect();
                _ws.Send(ask);

                DateTime startTime = DateTime.Now;
                while(response == null)
                {
                    TimeSpan elapsedTime = DateTime.Now - startTime;
                    if(elapsedTime.TotalSeconds >= 3) {
                        return e1;
                    }
                }

                Thread.Sleep(100); //sleep to wait for server authorize confirmation

                string[] arg = response.Split(' ');
                if(arg[0]=="/RetAuthorize")
                {
                    if(arg[1]=="6")
                        return e2;
                    if(arg[1]=="7")
                        return arg[2];
                    return e1;
                }
                else return e1;
            }
        }
        else return e;

        } catch(Exception) {
            return e3;
        }
    }
    void SendMTP(string msg)
    {
        msg=msg+" 0";

        try
        {
            ws.Send(msg);
        }
        catch(Exception)
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
        if(delayedWarning!="") {
            SC_account.SetWarningRaw("Connection dennied: " + delayedWarning);
            delayedWarning = "";
        }

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
        else {BT_join.interactable=false;}
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
    }
    string getDenyInfo(string code)
    {
        if(code=="-1") return "Incompatible version.";
        if(code=="-2") return "Wrong nick format. Nick should not contain any special characters except for _ and -";
        if(code=="-3") return "This player is already on a server.";
        if(code=="-4") return "The server is full.";
        if(code=="-5") return "You are banned or not on a whitelist.";
        if(code=="-6") return "You were kicked for idling in menu for too long. Try reconnecting.";
        if(code=="-7") return "Failied verifing your SE3 account. Try connecting through address: '" + gotAddress + "'.";
        return "Unknown description, maybe introduced in a newer version.";
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
        block_retransmission = false;
        waiter=200;
        connectionState=2;
        SendMTP("/AllowConnection "+IF_nickname.text+" "+SC_data.clientRedVersion+" "+conID);
    }
    void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        if(e.Data.Split(' ')[0]=="/RetAllowConnection")
        {
            if(e.Data.Split(' ')[1][0]!='-')
            {
                connectionState=3;
                connectionID=Parsing.IntE(e.Data.Split(' ')[1]);
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
                //Retransmission for /AllowConnection but only in authorization mode
                if(SC_account.logged && e.Data.Split(' ')[1]=="-7" && !block_retransmission) {
                    block_retransmission = true;
                    Debug.Log("Retransmission for /AllowConnection... but this time waiting 1500ms");
                    Thread.Sleep(1500);
                    SendMTP("/AllowConnection "+IF_nickname.text+" "+SC_data.clientRedVersion+" "+conID);
                    return;
                }

                if(e.Data.Split(' ')[1]=="-7") gotAddress = e.Data.Split(' ')[2];
                else gotAddress = "?";
                delayedWarning = getDenyInfo(e.Data.Split(' ')[1]);
                waiter = 0;
            }
        }
    }
    public void V_Connect()
    {
        SC_account.RemoveWarning();
        if(connectionState!=0) return;
        if(IF_adress.text.Split(' ').Length > 1) {
            SC_account.SetWarningRaw("Aborted: Server address can't contain space characters.");
            return;
        }
        connectionState=1;
        connect_in_update=2;
    }
    public void V_Stop()
    {
        if(ws!=null) ws.CloseAsync();
        connectionState=0;
    }
    int GenerateRandomNumber(int minValue, int maxValue)
    {
        //Written by ChatGPT, it's a cryptography-safe function (as he said)
        if (minValue >= maxValue)
            throw new ArgumentException("minValue musi byc mniejszy niz maxValue.");

        long range = (long)maxValue - minValue + 1;
        if (range <= 0 || range > int.MaxValue)
            throw new ArgumentException("Zakres jest nieprawidlowy.");

        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] randomNumber = new byte[4]; // 4 bajty = 32 bity
            rng.GetBytes(randomNumber);

            long randomValue = BitConverter.ToUInt32(randomNumber, 0);
            return (int)(minValue + (randomValue % range));
        }
    }
    void True_connect()
    {
        if(IF_adress.text=="") {
            V_Stop();
            SC_account.SetWarningRaw("Aborted: Server address field can't be empty.");
            return;
        }
        if(IF_nickname.text=="") {
            V_Stop();
            SC_account.SetWarningRaw("Aborted: Nickname field can't be empty.");
            return;
        }

        conID = GenerateRandomNumber(1,999999999)+"";

        string raw_url = IF_adress.text;
        ConnectionUrl = trueAddressGet(raw_url);
        if(ConnectionUrl.StartsWith("se3://ERROR")) {
            V_Stop();
            if(ConnectionUrl=="se3://ERROR ") SC_account.SetWarningRaw("Aborted: Servers with such addresses require SE3 account to verify users. Register or login from the main menu.");
            if(ConnectionUrl=="se3://ERROR_1 ") SC_account.SetWarningRaw("Aborted: Error downloading data from the authorization server.");
            if(ConnectionUrl=="se3://ERROR_2 ") SC_account.SetWarningRaw("Aborted: Couldn't find the server with such name. Is your address correct?");
            if(ConnectionUrl=="se3://ERROR_3 ") SC_account.SetWarningRaw("Aborted: Unknown error downloading the server address.");
            return;
        }

        try {
            ws = new WebSocket(ConnectionUrl);
        }
        catch(Exception) {
            SC_account.SetWarningRaw("Aborted: Wrong server address typed or received from authorization server.");
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

        SendMTP("/ImJoining "+connectionID);
        ws.Close();

        SC_data.TempFile="100";
		SC_data.TempFileConID[0]=connectionID+"";
        SC_data.TempFileConID[1]=ConnectionUrl;
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
