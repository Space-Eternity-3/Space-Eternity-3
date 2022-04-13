using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_legs : MonoBehaviour {

	public Transform player;
	public Transform respawn;
	public Transform Communtron1;
	public Transform always_on_player;
	public float jumpSize;
	public int legsID;

	void ChildGenerate()
	{
		SC_point_expand[] SC_point_expand=FindObjectsOfType<SC_point_expand>();
		foreach(SC_point_expand pet in SC_point_expand)
		{
			if(pet.mode==legsID) pet.PointGenerate(jumpSize);
		}
	}
	void Start()
	{
		float pX=Mathf.Round(player.position.x/jumpSize)*jumpSize;
		float pY=Mathf.Round(player.position.y/jumpSize)*jumpSize;
		if(transform.position==new Vector3(pX,pY,0f)) ChildGenerate();
		FixedUpdate();
	}
	void FixedUpdate()
	{
		//Instant jump
		float pX=Mathf.Round(player.position.x/jumpSize)*jumpSize;
		float pY=Mathf.Round(player.position.y/jumpSize)*jumpSize;

		float rX=Mathf.Round(respawn.position.x/jumpSize)*jumpSize;
		float rY=Mathf.Round(respawn.position.y/jumpSize)*jumpSize;
		
		if(transform.position!=new Vector3(pX,pY,0f)&&Communtron1.position.z==0f)
		{
			transform.position=new Vector3(pX,pY,0f);
			ChildGenerate();
		}
		if(transform.position!=new Vector3(rX,rY,0f)&&Communtron1.position.z!=0f)
		{
			transform.position=new Vector3(rX,rY,0f);
			ChildGenerate();
		}
	}
	void Update()
	{
		float pX=Mathf.Round(player.position.x/jumpSize)*jumpSize;
		float pY=Mathf.Round(player.position.y/jumpSize)*jumpSize;
		always_on_player.position=new Vector3(pX,pY,0f);
	}
}