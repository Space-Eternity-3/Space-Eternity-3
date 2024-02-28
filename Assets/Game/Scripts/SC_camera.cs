using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class SC_camera : MonoBehaviour {

	public Transform Communtron1;
	public Transform Communtron4;
	public float min;
	public float max;
	public float camZ;
	public Camera cameraC;
	public SC_control SC_control;
	public SC_fun SC_fun;
	public SC_data SC_data;
	public Transform player;
	public Transform respawn;
	public Transform background;

	int sCounter=50;
	public int TotalTime=0;
	bool fullS;

	int worldID=0;

	void save_zoom(float localZ)
	{
		SC_data.camera_zoom=localZ+"";
		SC_data.Save("settings");
	}
	void Update()
	{
		camZ=float.Parse(SC_data.camera_zoom);
		
		//Ctrl + Scroll
		if(Input.GetAxisRaw("Mouse ScrollWheel")<0&&SC_control.PressedNotInChat(KeyCode.LeftControl,"hold")&&Communtron1.position.z==0f&&!SC_control.pause)
 		{
     		if(camZ>min) camZ-=2.5f;
			save_zoom(camZ);
 		}
		if(Input.GetAxisRaw("Mouse ScrollWheel")>0&&SC_control.PressedNotInChat(KeyCode.LeftControl,"hold")&&Communtron1.position.z==0f&&!SC_control.pause)
 		{
     		if(camZ<max) camZ+=2.5f;
			save_zoom(camZ);
 		}

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
	void LateUpdate()
	{
		transform.position=new Vector3(transform.position.x,transform.position.y,camZ+SC_fun.camera_add);
		SC_fun.camera_add = 0;
	}
	void FixedUpdate()
	{
		if(Communtron1.position.z>1)
		{
			Communtron1.position-=new Vector3(0f,0f,1f);
			if(Communtron1.position.z==1)
			{
				Debug.Log("Respawned");
				SC_control.health_V=1f;
				SC_control.turbo_V=0f;
				SC_control.power_V=0f;
				if(!SC_control.pause)
				{
					SC_control.actualTarDisp = 0;
				}
				SC_control.playerR.velocity=new Vector3(0f,0f,0f);
				SC_control.living=true;
				player.position=new Vector3(respawn.position.x,respawn.position.y,0f);
				Communtron1.position-=new Vector3(0f,0f,1f);
			}
		}
		if(sCounter==0)
		{
			TotalTime++;
			sCounter=50;
		}
		sCounter--;
	}
	void Awake()
	{
		SC_data.CollectAwakeUniversal();
		fullS=Screen.fullScreen;

		camZ=float.Parse(SC_data.camera_zoom);
		worldID=int.Parse(SC_data.TempFile);

		//Interpretate worldID
		if(!((worldID>=1&&worldID<=8)||(worldID==100)))
		{
			SC_control.MenuReturn();
			return;
		}
		Communtron4.position+=new Vector3(0f,worldID,0f);
		
		if(worldID!=100)
		{
			SC_data.WorldID_Interpretate(worldID);
			SC_data.CollectAwakeWorld();

			if(SC_data.UniverseX[worldID-1,2]!=SC_data.clientVersion) {
				SC_data.UniverseX[worldID-1,2] = SC_data.clientVersion;
				Debug.Log("Version of Universe"+worldID+" updated to "+SC_data.clientVersion);
			}
		}
		
		if(worldID!=100) {
			SC_data.seed = Generator.SetSeed(SC_data.seed);
			SC_data.Save("seed");
		}
		else Generator.SetSeed(SC_data.TempFileConID[8].Split('&')[0]);
		
		if(worldID!=100)
		{
			TotalTime=int.Parse(SC_data.UniverseX[worldID-1,0]);
		}

		SC_control.AfterAwake();
	}
}
