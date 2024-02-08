using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_legs : MonoBehaviour {

	public Transform player;
	public Transform respawn;
	public Transform Communtron1;
	public Transform SpaceDust;
	public float jumpSize;

	void Start()
	{
		FixedUpdate();
	}
	void FixedUpdate()
	{
		float pX=Mathf.Round(player.position.x/jumpSize)*jumpSize;
		float pY=Mathf.Round(player.position.y/jumpSize)*jumpSize;

		float rX=Mathf.Round(respawn.position.x/jumpSize)*jumpSize;
		float rY=Mathf.Round(respawn.position.y/jumpSize)*jumpSize;
		
		if(transform.position!=new Vector3(pX,pY,0f)&&Communtron1.position.z==0f) transform.position=new Vector3(pX,pY,0f);
		if(transform.position!=new Vector3(rX,rY,0f)&&Communtron1.position.z!=0f) transform.position=new Vector3(rX,rY,0f);
	}
	void Update()
	{
		float pX=Mathf.Round(player.position.x/jumpSize)*jumpSize;
		float pY=Mathf.Round(player.position.y/jumpSize)*jumpSize;
		SpaceDust.position=new Vector3(pX,pY,0f);
	}
}
