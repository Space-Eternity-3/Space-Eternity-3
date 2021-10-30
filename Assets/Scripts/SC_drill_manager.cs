using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_drill_manager : MonoBehaviour {

	public Transform Communtron1;

	void OnTriggerEnter(Collider collision)
	{
		if(collision.name=="Drill3"){
			Communtron1.localScale+=new Vector3(0f,1f,0f);
		}
	}
	void OnTriggerExit(Collider collision)
	{
		if(collision.name=="Drill3"){
			Communtron1.localScale-=new Vector3(0f,1f,0f);
		}
	}
}
