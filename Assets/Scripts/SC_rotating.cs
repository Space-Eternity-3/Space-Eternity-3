using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_rotating : MonoBehaviour
{
	public Vector3 nat_v3;
	public bool opposite_allow;
	public bool opposite;
	
	void Start()
	{
		if(UnityEngine.Random.Range(0,2) == 0 && opposite_allow)
			opposite = true;
		else
			opposite = false;
		
		int i = UnityEngine.Random.Range(0,30);
		while(i>0)
		{
			FixedUpdate();
			i--;
		}
	}
    void FixedUpdate()
	{
		Vector3 vec3 = transform.localRotation.eulerAngles;
		if(opposite) vec3 += nat_v3;
		else vec3 -= nat_v3;
		Quaternion qua4 = new Quaternion(0f,0f,0f,0f);
		qua4.eulerAngles = vec3;
		transform.localRotation = qua4;
	}
}
