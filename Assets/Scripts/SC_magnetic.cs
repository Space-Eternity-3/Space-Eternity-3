using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_magnetic : MonoBehaviour {

	public Transform player;
	public Rigidbody playerR;
	public float CritRange;
	public float MaxRange;
	public float Force;
	public bool sameForce;

	void FixedUpdate()
	{
		if(transform.position.z>100f) return;

		float range;
		float dX=player.position.x-transform.position.x;
		float dY=player.position.y-transform.position.y;
		range=Mathf.Sqrt(dX*dX+dY*dY);

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
