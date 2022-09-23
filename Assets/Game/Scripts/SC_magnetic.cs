using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_magnetic : MonoBehaviour {

	public Transform player;
	public Rigidbody playerR;
	public bool player_magnetic;
	public float CritRange;
	public float MaxRange;
	public float Force;
	public bool sameForce;
	int fixeds = 0;

	SC_players SC_players;
	public SC_control SC_control;

	void Start()
	{
		SC_players = gameObject.GetComponent<SC_players>();
	}
	bool isImpulsing()
	{
		int IDP = SC_players.IDP_phys;
		if(SC_control.others2_RPC[IDP]%100==2) return true;
		else return false;
	}
	void FixedUpdate()
	{
		fixeds++;
	}
	void Update()
	{
		//Magnetic double check
		Vector3 tpz = transform.position;
		if(tpz.z>100f || tpz.z<-100f) return;
		Vector3 ppz = player.position;

		float range;
		float dX=ppz.x-tpz.x;
		float dY=ppz.y-tpz.y;
		range=Mathf.Sqrt(dX*dX+dY*dY);

		if(range>=MaxRange) return;
		if(range==0 || (player_magnetic && isImpulsing())) return;

		float nX,nY;
		if(!sameForce)
		{
			float F;
			if(range>CritRange) F=Force*(MaxRange-range);
			else F=Force*(MaxRange-CritRange);
			nX=dX*F/range;
			nY=dY*F/range;
		}
		else
		{
			nX=dX*Force/range;
			nY=dY*Force/range;
		}

		playerR.velocity += fixeds*new Vector3(nX,nY,0f);
	}
	void LateUpdate()
	{
		fixeds = 0;
	}
}
