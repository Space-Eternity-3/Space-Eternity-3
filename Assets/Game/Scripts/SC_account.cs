using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using System;
using System.Diagnostics;

public class SC_account : MonoBehaviour
{
    public RectTransform PreLoginPlane;
    public RectTransform AfterLoginPlane;
    public RectTransform PassInputName1;
    public RectTransform PassInputLog1;
    public RectTransform PassInputLog2;

    public InputField IF_a1;
    public InputField IF_n1;
    public InputField IF_p1;
    public InputField IF_p2;

    public Button Register, Login, Logout, More;
    public Button Account;
    public Text regText;
    public Canvas Screen5;

    Vector3 nickPrePos = new Vector3(0f,0f,0f);
    Vector3 passPrePos = new Vector3(0f,0f,0f);

    public Text warning_text5; public Transform warning_field5;

    public bool use_local_authorization = false;
    public string authorizationServer = "ws://localhost:27683";
    public string accountPage = "se3.page";
    public string tutorialPage = "se3.page";
    public string trailerPage = "se3.page";

    public bool logged = false;
    public bool confirming = false;
    public bool tried_login = false;
    public int waiting_for = 0;

    WebSocket ws;
    public bool connected_to_authorizer = false;
    public bool delayed_login = false;
    public bool make_wrong_ask = false;

    public List<string> delayed_messages = new List<string>();

    public SC_main_buttons SC_main_buttons;
    public SC_data SC_data;

    void Awake()
    {
        nickPrePos = PassInputName1.localPosition;
        passPrePos = PassInputLog1.localPosition;
    }
    void Start()
    {
        if(use_local_authorization)
            authorizationServer = "ws://localhost:27684";

        AllInputLoad();
        ws = new WebSocket(authorizationServer);
        ws.OnMessage += Ws_OnMessage;
        ws.OnOpen += Ws_OnOpen;
        ws.OnClose += Ws_OnClose;
        ws.ConnectAsync();
    }
    bool passwordGood(int f)
    {
        if(f==1) return IF_p1.text.Length > 6;
        if(f==2) return IF_p2.text.Length > 6;
        return false;
    }
    void Update()
    {
        if(make_wrong_ask) {
            AskServer("Wrong ask");
            waiting_for=0;
        }

        Account.interactable = connected_to_authorizer;

        if(connected_to_authorizer && delayed_login)
        {
            delayed_login = false;
            _Login();
        }

        while(delayed_messages.Count > 0)
        {
            doMsg(delayed_messages[0]);
            delayed_messages.RemoveAt(0);
        }

        //Login buttons set
        if(!logged)
        {
            PreLoginPlane.localPosition = new Vector3(0f,0f,0f);
            AfterLoginPlane.localPosition = new Vector3(10000f,0f,0f);
        }
        else
        {
            PreLoginPlane.localPosition = new Vector3(10000f,0f,0f);
            AfterLoginPlane.localPosition = new Vector3(0f,0f,0f);
        }
        if(IF_p1.text != IF_p2.text && confirming) regText.text = "Cancel";
        else regText.text = "Register";

        //Interactabling
        IF_p1.interactable = !logged && (waiting_for==0);
        IF_p2.interactable = (waiting_for==0);
        Login.enabled = !confirming && (waiting_for==0);
        Register.enabled = (waiting_for==0);
        Logout.enabled = (waiting_for==0);
        More.enabled = (waiting_for==0);

        //Interaction shape
        if(!logged)
        {
            PassInputName1.localPosition = nickPrePos;

            if(confirming) //Confirming password for registration
            {
                PassInputLog1.localPosition = new Vector3(10000f,0f,0f);
                PassInputLog2.localPosition = passPrePos;
            }
            else //Unlogged
            {
                PassInputLog1.localPosition = passPrePos;
                PassInputLog2.localPosition = new Vector3(10000f,0f,0f);
            }
        }
        else //Logged
        {
            PassInputName1.localPosition = nickPrePos;
            PassInputLog1.localPosition = passPrePos;
            PassInputLog2.localPosition = new Vector3(10000f,0f,0f);
        }
    }
    public void _Register()
    {
        RemoveWarning();
        if(!confirming)
        {
            if(passwordGood(1))
            {
                IF_p2.text = "";
                confirming = true;
            }
            else SetWarning("Password too short");
        }
        else
        {
            if(IF_p1.text == IF_p2.text)
                AskServer("/Register " + IF_n1.text + " " + IF_p1.text);
            else confirming = false;
        }
    }
    public void _Login()
    {
        RemoveWarning();
        AskServer("/Login " + IF_n1.text + " " + IF_p1.text);
    }
    public void _Logout()
    {
        logged = false;
        confirming = false;
    }
    public void _More()
    {
        SendToPage(accountPage);
    }
    public void _CloseConditional()
    {
        if(confirming)
            confirming = false;
    }
    void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        delayed_messages.Add(e.Data);
    }
    void doMsg(string eData)
    {
        string[] arg = eData.Split(" ");
        int msg_code = int.Parse(arg[1]);
        if(arg[0]=="/RetLogin")
        {
            tried_login = true;
            if(msg_code==1)
            {
                UnityEngine.Debug.Log("Logged in!");
                logged = true;
                confirming = false;
            }
            else
            {
                logged = false;
                confirming = false;
            }
        }

        waiting_for--;
        if(msg_code!=1 && msg_code!=7)
        {
            switch(msg_code)
            {
                case 2: SetWarning("Wrong nick format. Nick should not contain any special characters except for _ and -"); break;
                case 3: SetWarning("User with this nickname already exists."); break;
                case 4: SetWarning("Wrong nickname."); break;
                case 5: SetWarning("Wrong password."); break;
                case 8: SetWarning("Wrong password lenght (must be 7-32 characters)."); break; //Will never be executed from here because of client side filter
                case 15: SetWarning("Registration limit from this IP reached. Please, wait one minute."); break;
                default: SetWarning("Unknown error."); break;
            }
        }
    }
    void Ws_OnOpen(object sender, System.EventArgs e)
    {
        UnityEngine.Debug.Log("Connected to authorization server.");
        connected_to_authorizer = true;
    }
    void Ws_OnClose(object sender, System.EventArgs e)
    {
        if(connected_to_authorizer) {
            UnityEngine.Debug.Log("Connection to the authorization server closed.");
            _Logout();
            connected_to_authorizer = false;
            make_wrong_ask = true;
        }
        else UnityEngine.Debug.Log("Failied connecting to the authorization server. Running in offline mode.");
    }
    void OnDestroy()
    {
        try{
            ws.CloseAsync();
        }catch(Exception){}
        AllInputSave();
    }
    bool AskServer(string s)
    {
        waiting_for++;
        try {
            ws.Send(s);
        }
        catch(Exception) {
            Account.interactable = false;
            connected_to_authorizer = false;
            if(Screen5.targetDisplay==0) SC_main_buttons.SAS(0);
            return false;
        }
        return true;
    }
    public void RemoveWarning()
    {
        warning_field5.localPosition=new Vector3(10000f,0f,0f);
    }
    public void SetWarning(string e)
    {
        warning_text5.text="Error: "+e;
        warning_field5.localPosition=new Vector3(0f,-186f,0f);
    }
    public void SetWarningRaw(string e)
    {
        warning_text5.text=e;
        warning_field5.localPosition=new Vector3(0f,-186f,0f);
    }
    public void AllInputLoad()
    {
        IF_n1.text = SC_data.MultiplayerInput[0];
        IF_a1.text = SC_data.MultiplayerInput[1];
        IF_p1.text = SC_data.MultiplayerInput[2];
        if(SC_data.MultiplayerInput[3]==IF_n1.text && IF_n1.text!="") delayed_login = true;
    }
    public void AllInputSave()
    {
        SC_data.MultiplayerInput[0] = IF_n1.text; //nickname
        SC_data.MultiplayerInput[1] = IF_a1.text; //address
        SC_data.MultiplayerInput[2] = IF_p1.text; //password
        if(tried_login) {
            if(logged) SC_data.MultiplayerInput[3] = IF_n1.text; //auto login
            else SC_data.MultiplayerInput[3] = "..........................";
        }
        SC_data.Save("settings");
    }
    public void GoToTutorial()
    {
        SendToPage(tutorialPage);
    }
    public void GoToTrailer()
    {
        SendToPage(trailerPage);
    }
    public void DiscardHowToPlay()
    {
        SC_data.has_played = "1";
    }
    public void SendToPage(string s)
    {
        UnityEngine.Debug.Log("Sending to "+s);
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = s,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
