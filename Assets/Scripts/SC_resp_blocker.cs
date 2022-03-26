using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_resp_blocker : MonoBehaviour
{
	public Transform Communtron3;
	public Transform player;
	public float radius;
	public bool blocking;
	public SC_control SC_control;
	
	void Update()
	{
		Vector3 pomv = transform.position-player.position;
		pomv -= new Vector3(0f,0f,pomv.z);
		if(SC_control.Pitagoras(pomv) < radius) blocking = true;
		else blocking = false;
	}
	public bool IsAllowing()
	{
		SC_resp_blocker[] rbs = FindObjectsOfType<SC_resp_blocker>();
		foreach(SC_resp_blocker rb in rbs)
		{
			if(rb.blocking) return false;
		}
		return true;
	}
}
