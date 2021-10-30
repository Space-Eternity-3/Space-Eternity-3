using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_struct2 : MonoBehaviour
{
    int ID=1;
    public SC_fun SC_fun;
    public Transform legs;
    public Transform Communtron1;

    void Start()
    {
        if(transform.position.z<100)
        {
            ID=SC_fun.CheckID((int)(transform.position.x/10f),(int)(transform.position.y/10f));
        }
    }
    void Update()
    {
        if(transform.position.z<100)
		{	
			//Optimalize
			float ssX=Mathf.Round(transform.position.x/10f);
			float ssY=Mathf.Round(transform.position.y/10f);
			float llX=Mathf.Round(legs.position.x/10f);
			float llY=Mathf.Round(legs.position.y/10f);
			float distance=Mathf.Sqrt((ssX-llX)*(ssX-llX)+(ssY-llY)*(ssY-llY));
			if(distance>10f&&Communtron1.position.z==0f)
			{
				SC_fun.GenListRemove(ID,0);
				Destroy(gameObject);
			}
		}
    }
}
