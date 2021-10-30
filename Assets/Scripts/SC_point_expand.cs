using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_point_expand : MonoBehaviour {

	public Transform asteroid;
	public SC_fun SC_fun;

	public void PointGenerate()
	{
		int X=(int)Mathf.Round(transform.position.x/10f);
		int Y=(int)Mathf.Round(transform.position.y/10f);
		int ID=SC_fun.CheckID(X,Y);
		if(X<=50000&&X>=-50000&&Y<=50000&&Y>=-50000&&SC_fun.AsteroidCheck(ID))
		{
			SC_fun.GenListAdd(ID,0);
			char it=SC_fun.AsteroidChar(ID);
			if(it=='V') {Instantiate(asteroid, transform.position, Quaternion.identity); return;}
		}
	}
}
