using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SC_main_buttons : MonoBehaviour {

	public Canvas[] Screens;
	public SC_connection SC_connection;
	public SC_data SC_data;
	public bool fullS;

	public void Start()
	{
		Time.timeScale = 1f;
		fullS=Screen.fullScreen;
		SAS(0);
		if(SC_data.TempFile=="-1") SAS(1);
		if(SC_data.TempFile=="-2") SAS(2);
	}
	public void Button5PermanentExit()
	{
		Debug.Log("Permanent Quit");
		Application.Quit();
	}
	public void Update()
	{
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
		int i,lngt=Screens.Length;
		for(i=0;i<lngt;i++) Screens[i].enabled=false;
		if(n==0)
		{
            SC_data.warning_text4.text="";
            SC_data.warning_field4.localPosition=new Vector3(10000f,0f,0f);
		}
		if(n!=-1) Screens[n].enabled=true;
		if(n==2) SC_connection.mtpDataLoad();
		if(n==3)
		{
			SC_settings[] SC_settings = FindObjectsOfType<SC_settings>();
			foreach(SC_settings ss in SC_settings)
			{
				ss.valueRead();
			}
		}
	}
}
