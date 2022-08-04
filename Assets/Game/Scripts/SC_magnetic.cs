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
		if(transform.position.z>100f) return;

		float range;
		float dX=player.position.x-transform.position.x;
		float dY=player.position.y-transform.position.y;
		range=Mathf.Sqrt(dX*dX+dY*dY);
		if(range==0 || (player_magnetic && isImpulsing())) return;

		if(range<MaxRange&&!sameForce)
		{
			float F;
			if(range>CritRange) F=Force*(MaxRange-range);
			else F=Force*(MaxRange-CritRange);
			float nX=dX*F/range;
			float nY=dY*F/range;
			playerR.velocity+=new Vector3(nX,nY,0f);
		}
		if(range<MaxRange&&sameForce)
		{
			float nX=dX*Force/range;
			float nY=dY*Force/range;
			playerR.velocity+=new Vector3(nX,nY,0f);
		}
	}
}
