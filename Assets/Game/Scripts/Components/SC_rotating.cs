using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_rotating : MonoBehaviour
{
	public Vector3 nat_v3;
	public bool opposite_allow;
	public bool opposite;
	public bool enable_anti_rotor;
	public int frame_size;
	public SC_control SC_control;
	
	Vector3 vec3 = new Vector3(0f,0f,0f);

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
		if(frame_size!=0)
			if(SC_control.livTime%frame_size!=0) return;

		if(!enable_anti_rotor) vec3 = transform.localRotation.eulerAngles;
		if(opposite) vec3 += nat_v3;
		else vec3 -= nat_v3;
		Quaternion qua4 = new Quaternion(0f,0f,0f,0f);
		qua4.eulerAngles = vec3;
		if(!enable_anti_rotor) transform.localRotation = qua4;
		else transform.rotation = qua4;
	}
}
