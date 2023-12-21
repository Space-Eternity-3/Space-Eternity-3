using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class SC_main_buttons : MonoBehaviour {

	public Canvas[] Screens;
	public RectTransform[] ScreenParents;
	public SC_connection SC_connection;
	public SC_data SC_data;
	public SC_account SC_account;
	public bool fullS;

	public RectTransform NameField;
	public RectTransform Name2parent, Name5parent;
	public RectTransform ParScreen2, ParScreen5;

	public Text info_rel;
	public int start_SAS = 0;

	void Start()
	{
		Time.timeScale = 1f;
		fullS=Screen.fullScreen;
		SAS(-2);

		if(SC_data.TempFile=="-1") start_SAS = 1;
		if(SC_data.TempFile=="-2") start_SAS = 2;
		if(SC_data.has_played=="0") start_SAS = 5;

		foreach(RectTransform rct in ScreenParents) {
			rct.localPosition = new Vector3(0f,10000f,0f);
		}
		ScreenParents[start_SAS].localPosition = new Vector3(0f,0f,0f);
	}
	bool screen_started = false;
	void ScreensStartUpdate()
	{
		SAS(start_SAS);

		foreach(RectTransform rct in ScreenParents) {
			rct.localPosition = new Vector3(0f,0f,0f);
		}
	}
	public void Button5PermanentExit()
	{
		Debug.Log("Permanent Quit");
		Application.Quit();
	}
	void Update()
	{
		if(Input.GetMouseButtonDown(0) && !screen_started) {
			ScreensStartUpdate();
			screen_started = true;
		}

		if(Input.GetKeyDown(KeyCode.Escape)) SAS(0);
		if(Input.GetKeyDown(KeyCode.F11)) fullS=!fullS;
            
        if(Screen.fullScreen&&!fullS)
        {
            Screen.SetResolution(1280,720,true);
            Screen.fullScreen=false;
        }
        if(!Screen.fullScreen&&fullS)
		{
            Screen.SetResolution(1920,1080,true);
            Screen.fullScreen=true;
        }
	}
	public void SAS(int n)
	{
		// 0 - main menu
		// 1 - singleplayer
		// 2 - multiplayer
		// 3 - settings
		// 4 - account
		// 5 - how to play

		// -1 - none
		// -2 - all

		int i,lngt=Screens.Length;
		bool base_false = false; if(n==-2) base_false = true;
		for(i=0;i<lngt;i++) Screens[i].enabled = base_false;
		if(n==0)
		{
            SC_data.RemoveWarning();
			SC_account.RemoveWarning();
		}
		if(n>=0) Screens[n].enabled=true;
		if(n==3)
		{
			//MAIN MENU, CAN BE HERE
			SC_settings[] SC_settings = FindObjectsOfType<SC_settings>();
			foreach(SC_settings ss in SC_settings)
			{
				ss.valueRead();
			}
		}

		//Nick field location changer
		if(n==2) NameField.SetParent(Name2parent,false);
		if(n==4) NameField.SetParent(Name5parent,false);

		//Account warning jumper
		if(n==2) SC_account.warning_field5.SetParent(ParScreen2,false);
		if(n==4) SC_account.warning_field5.SetParent(ParScreen5,false);
	}
}
