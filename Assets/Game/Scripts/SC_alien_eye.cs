using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_alien_eye : MonoBehaviour {

	Transform eye;
	public Transform[] eyes;
	int rand, Mn=0, counter, eyeslength;
	float Fn;
	public int min_time;
	public int max_time;

	void Start()
	{
		eye=gameObject.GetComponent<Transform>();
		eyeslength = eyes.Length;
	}
	void FixedUpdate()
	{
		int i;
		if(counter==0) counter=Random.Range(min_time,max_time);
		if(counter>0) counter--;
		if(counter==0)
		{
			rand=Random.Range(0,2);
			if(rand==0&&Mn==-1) Mn=0; else
			if(rand==1&&Mn==-1) Mn=1; else
			if(rand==0&&Mn==0) Mn=-1; else
			if(rand==1&&Mn==0) Mn=1; else
			if(rand==0&&Mn==1) Mn=-1; else
			if(rand==1&&Mn==1) Mn=0;
			Fn=Mn*0.2f;
			eye.localPosition=new Vector3(Fn,0.18f,0f);
			for(i=0;i<eyeslength;i++)
				eyes[i].localPosition=new Vector3(Fn,0.18f,0f);
		}
	}
}
