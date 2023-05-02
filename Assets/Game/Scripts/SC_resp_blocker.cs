using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_resp_blocker : MonoBehaviour
{
	public Transform player;
	public float radius;
	public bool blocking;
	public SC_control SC_control;
	
	void Start()
	{
		SC_control.SC_lists.AddTo_SC_resp_blocker(this);
	}
	void OnDestroy()
	{
		SC_control.SC_lists.RemoveFrom_SC_resp_blocker(this);
	}
	public bool IsAllowing()
	{
		Vector3 plapos = player.position;
		foreach(SC_resp_blocker rpb in SC_control.SC_lists.SC_resp_blocker)
		{
			if(SC_control.Pitagoras(plapos - rpb.transform.position) < rpb.radius)
				return false;
		}
		return true;
	}
}
