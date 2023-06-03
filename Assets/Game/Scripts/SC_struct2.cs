using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_struct2 : MonoBehaviour
{
    public int X=0,Y=0,ID=1;
    int counter_to_destroy = 200;
    bool mother=true;
    public SC_fun SC_fun;
    public Transform legs;
    public Transform Communtron1;

    void OnDestroy()
    {
        SC_fun.GenListRemove(ID,0);
    }
    void Start()
    {
        if(transform.position.z<100f) mother=false;
    }
    void FixedUpdate()
    {
        if(!mother)
		{	
			//Optimalize
			float ssX=X;
			float ssY=Y;
			float llX=Mathf.Round(legs.position.x/10f);
			float llY=Mathf.Round(legs.position.y/10f);
			float distance=Mathf.Sqrt((ssX-llX)*(ssX-llX)+(ssY-llY)*(ssY-llY));
			if(distance>11f&&Communtron1.position.z==0f)
			{
				if(counter_to_destroy==0)
				{
					SC_fun.GenListRemove(ID,0);
					Destroy(gameObject);
				}
				else counter_to_destroy--;
			}
			else counter_to_destroy = 200;
		}
    }
}
