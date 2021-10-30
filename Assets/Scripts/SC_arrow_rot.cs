using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_arrow_rot : MonoBehaviour {

	public Transform respawn_point;
	public Transform player;
	public Transform coords;
	public Transform compass;
	public float deltaPos;
	bool E=false;

	Vector3 sPosM;
	Vector3 sPosR;
	Vector3 sPosNN;

	void Start()
	{
		sPosM=compass.localPosition;
		sPosR=coords.localPosition-new Vector3(0f,deltaPos,0f);
		sPosNN=transform.localPosition;
	}
	void Update()
	{
		float dX=player.position.x-respawn_point.position.x;
		float dY=player.position.y-respawn_point.position.y;
		if(dX==0f) dX=0.0001f;
		float alpha=Mathf.Atan(dY/dX)*(180f/3.14159f)-90f;
		if(dX>=0f) alpha+=180f;
		if(Mathf.Sqrt(dX*dX+dY*dY)<1f)
		{
			transform.localPosition=new Vector3(1000f,0f,0f);
		}
		else
		{
			transform.localPosition=sPosNN;
		}
		transform.eulerAngles=new Vector3(0f,0f,alpha);

		if(Input.GetKeyDown(KeyCode.F))
		{
			//ChangePosVisible();
		}
	}
	public void ChangePosVisible()
	{
			if(!E)
			{
				coords.localPosition=sPosR;
				compass.localPosition=sPosM+new Vector3(0f,deltaPos,0f);
				E=true;
			}
			else
			{
				compass.localPosition=sPosM;
				coords.localPosition=sPosR+new Vector3(0f,deltaPos,0f);
				E=false;
			}
	}
}
