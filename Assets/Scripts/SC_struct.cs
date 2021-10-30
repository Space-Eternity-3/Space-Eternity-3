using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_struct : MonoBehaviour
{
    public int structureID;
    public int ID;
    public float X,Y;
    public SC_fun SC_fun;
    public Transform legs;
    public Transform Communtron1;

    void Start()
    {
        if(transform.position.z<100)
        {
            transform.position+=SC_fun.GetBiomeMove(ID);
        }
    }
    void Update()
    {
        if(transform.position.z<100)
		{
			//Optimalize
			float ssX=Mathf.Round(transform.position.x/180f);
			float ssY=Mathf.Round(transform.position.y/180f);
			float llX=Mathf.Round(legs.position.x/180f);
			float llY=Mathf.Round(legs.position.y/180f);
			float distance=Mathf.Sqrt((ssX-llX)*(ssX-llX)+(ssY-llY)*(ssY-llY));
			if(distance>2f&&Communtron1.position.z==0f)
			{
				SC_fun.GenListRemove(ID,1);
				Destroy(gameObject);
			}
		}
    }
}
