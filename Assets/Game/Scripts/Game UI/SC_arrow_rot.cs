using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_arrow_rot : MonoBehaviour {

	public Transform respawn_point; //dynamic variable

	public Transform player;
	public Transform coords;
	public Transform compass;
	public SC_data SC_data;
	public float deltaPos;
	public bool this_main;
	public bool silentior;
	public SC_arrow_rot[] compasses = new SC_arrow_rot[8];
	
	bool E=false;

	Vector3 sPosM;
	Vector3 sPosR;
	Vector3 sPosNN;

	void Awake()
	{
		sPosM=compass.localPosition;
		sPosR=coords.localPosition-new Vector3(0f,deltaPos,0f);
		sPosNN=transform.localPosition;
	}
	void Start()
	{
		if(this_main) if(SC_data.compass_mode=="0") ChangePosVisible();
	}
	void LateUpdate()
	{
		if(this_main) B_Update();
	}
	public void B_Update()
	{
		float dX=player.position.x-respawn_point.position.x;
		float dY=player.position.y-respawn_point.position.y;
		float dZ=respawn_point.position.z;
		
		if(dX==0f) dX=0.0001f;
		float alpha=Mathf.Atan(dY/dX)*(180f/3.14159f)-90f;
		bool other_player_arrow = (!this_main && !silentior);
		if(dX>=0f) alpha+=180f;

		if(Mathf.Sqrt(dX*dX+dY*dY)<1f || (other_player_arrow&&(dZ>=100f || dZ<=-100f)) || (other_player_arrow&&respawn_point.GetComponent<SC_invisibler>().invisible) || (!this_main&&!SC_data.SC_control.show_positions))
			transform.localPosition=new Vector3(1000f,0f,0f);
		else
			transform.localPosition=sPosNN;
		transform.eulerAngles=new Vector3(0f,0f,alpha);
	}
	public void ChangePosVisible()
	{
			if(this_main) for(int i=0;i<8;i++) compasses[i].ChangePosVisible();
			
			if(!E)
			{
				coords.localPosition=sPosR;
				compass.localPosition=sPosM+new Vector3(0f,deltaPos,0f);
				if(this_main) SC_data.compass_mode="0";
				E=true;
			}
			else
			{
				compass.localPosition=sPosM;
				coords.localPosition=sPosR+new Vector3(0f,deltaPos,0f);
				if(this_main) SC_data.compass_mode="1";
				E=false;
			}
			if(this_main) SC_data.Save("settings");
	}
}
