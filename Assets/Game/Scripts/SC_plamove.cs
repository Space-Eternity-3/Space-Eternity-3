using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_plamove : MonoBehaviour
{
    public Transform[] outerfaces = new Transform[10];
	public Transform[] interfaces = new Transform[10];
	
	Vector3[] positions = new Vector3[10];
	
	void Start()
	{
		int i;
		for(i=1;i<10;i++)
		{
			positions[i] = outerfaces[i].localPosition;
		}
		positions[0] = new Vector3(10000f,0f,0f);
	}
	void LateUpdate()
	{
		int i,p,n=1;
		
		for(i=1;i<10;i++)
		{
			if(interfaces[i].localPosition.x!=10000f) {p=n; n++;}
			else p=0;
			outerfaces[i].localPosition = positions[p];
		}
	}
}
