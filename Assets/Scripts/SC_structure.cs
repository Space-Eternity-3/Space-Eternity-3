using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_structure : MonoBehaviour
{
	public SC_fun SC_fun;
	public Transform legs;
	public Transform Communtron1;
    public int X=0,Y=0,ID=1;
	bool mother = true;
	
	void Start()
	{
		if(transform.position.z<=100f) mother = false;
		transform.position += SC_fun.GetBiomeMove(ID);
	}
	void Update()
	{
		if(!mother)
		{
			//Optimalize
			float ssX=X;
			float ssY=Y;
			float llX=Mathf.Round(legs.position.x/100f);
			float llY=Mathf.Round(legs.position.y/100f);
			float distance=Mathf.Sqrt((ssX-llX)*(ssX-llX)+(ssY-llY)*(ssY-llY));
			if(distance>2f&&Communtron1.position.z==0f)
			{
				SC_fun.GenListRemove(ID,1);
				Destroy(gameObject);
			}
		}
	}
}
