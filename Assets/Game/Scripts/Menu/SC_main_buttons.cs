﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class SC_main_buttons : MonoBehaviour {

	public Canvas[] Screens;
	public SC_connection SC_connection;
	public SC_data SC_data;
	public SC_account SC_account;
	public SC_universe_create SC_universe_create;

	public RectTransform NameField;
	public RectTransform Name2parent, Name5parent;
	public RectTransform ParScreen2, ParScreen5;

	public Text info_rel;

	void Start()
	{
		Time.timeScale = 1f;
		foreach(Canvas cnv in Screens)
			cnv.enabled = true;

		SAS(0);
		if(SC_data.TempFile=="-1") SAS(1); //singleplayer return
		if(SC_data.TempFile=="-2") SAS(2); //multiplayer return
		//if(SC_data.has_played=="0") SAS(5); //how to play page
	}
	public void Button5PermanentExit()
	{
		#if UNITY_EDITOR
		Debug.Log("Permanent Quit");
		#else
		Application.Quit();
		#endif
	}
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(SC_universe_create.creating_index==0) SAS(0);
			else SC_universe_create.EndCreating();
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
		int base_false = 1; if(n==-2) base_false = 0;
		for(i=0;i<lngt;i++) Screens[i].targetDisplay = base_false;
		if(n>=0) Screens[n].targetDisplay = 0;
		if(n==0)
		{
			SC_account.RemoveWarning();
		}

		//Nick field location changer
		if(n==2) NameField.SetParent(Name2parent,false);
		if(n==4) NameField.SetParent(Name5parent,false);

		//Account warning jumper
		if(n==2) SC_account.warning_field5.SetParent(ParScreen2,false);
		if(n==4) SC_account.warning_field5.SetParent(ParScreen5,false);
	}
}
