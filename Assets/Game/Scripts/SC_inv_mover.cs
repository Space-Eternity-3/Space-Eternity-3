using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SC_inv_mover : MonoBehaviour {

	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron4;
	public SC_control SC_control;
	public SC_backpack SC_backpack;

	public bool active=false;
	float closed;
	float opened;

	public float step_size=17f;
	public float A;
	public bool backwards;
	public bool updown;
	public bool tab;
	public bool inv;

	void Update()
	{
		//Inventory extender
		if(inv && !SC_control.pause)
		{
			if(!active && Input.GetKeyDown(KeyCode.E) && !SC_control.SC_chat.typing && Communtron1.position.z==0f)
				active=true;
			
			else if(active && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Communtron1.position.z!=0f))
			{
				active = false;
				SC_control.blockEscapeThisFrame = true;
			}
		}

		//Tab extender
		if(tab && (int)Communtron4.position.y==100)
		{
			if(Input.GetKey(KeyCode.Tab)) active=true;
			else active=false;
		}
	}
	void FixedUpdate()
	{
		if(!updown)
		{
			if(active)
			{
				if((int)transform.localPosition.x>(int)opened) transform.localPosition-=new Vector3(step_size,0f,0f);
				else if((int)transform.localPosition.x<(int)opened) transform.localPosition+=new Vector3(step_size,0f,0f);
			}
			else
			{
				if((int)transform.localPosition.x>(int)closed) transform.localPosition-=new Vector3(step_size,0f,0f);
				else if((int)transform.localPosition.x<(int)closed) transform.localPosition+=new Vector3(step_size,0f,0f);
			}
		}

		if(updown)
		{
			if(active)
			{
				if((int)transform.localPosition.y>(int)opened) transform.localPosition-=new Vector3(0f,step_size,0f);
				else if((int)transform.localPosition.y<(int)opened) transform.localPosition+=new Vector3(0f,step_size,0f);
			}
			else
			{
				if((int)transform.localPosition.y>(int)closed) transform.localPosition-=new Vector3(0f,step_size,0f);
				else if((int)transform.localPosition.y<(int)closed) transform.localPosition+=new Vector3(0f,step_size,0f);
			}
		}
	}
	void Start()
	{
		if(!updown) closed=transform.localPosition.x;
		else closed=transform.localPosition.y;
		if(!backwards) opened=closed+(A*step_size);
		else opened=closed-(A*step_size);
	}
}
