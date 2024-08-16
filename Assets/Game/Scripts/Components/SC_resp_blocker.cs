using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_resp_blocker : MonoBehaviour
{
	public Transform player;
	public float radius;
	public SC_control SC_control;
	
	void FixedUpdate()
	{
		//Set respawn breaking sequence
		if(SC_control.SC_fun.respawn_allow_reinit)
		{
			SC_control.SC_fun.respawn_allow_reinit = false;
			SC_control.SC_fun.respawn_allow = SC_control.SC_slots.InvHaving(20);
		}

		//Try to break respawn allow
		if(SC_control.SC_fun.respawn_allow)
		{
			Vector3 plapos = player.position;
			if(SC_control.Pitagoras(plapos - transform.position) < radius)
				SC_control.SC_fun.respawn_allow = false;
		}
	}
	void LateUpdate()
	{
		//Signalize to start calculation
		SC_control.SC_fun.respawn_allow_reinit = true;
	}
}
