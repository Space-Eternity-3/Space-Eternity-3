using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_nose : MonoBehaviour {

	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron3;
	public Transform Communtron5;
	public Material D1;
	public Material D2;
	public Material D3;
	public Material D4;
	public Renderer Drill;
	public Renderer Drill3;
	//public Light light;
	//public Transform rFuelParticles;
	int lR=255,lG=255,lB=255;

	void FixedUpdate()
	{
		int mat=1;
		bool dest=(Communtron1.position.x>0f);
		bool dri=(Communtron1.localScale==new Vector3(2f,2f,2f));
		bool dron=(Communtron2.position.x>0f);
		if(dron) mat=4;
		if((Input.GetMouseButton(0)||Input.GetMouseButton(1))&&Communtron3.position.y==0f)
		{
			mat=3;
			if(dron) mat=4;
			if(dri) mat=2;
		}
		else
		{
			if(dron)
			{
				mat=4;
			}
			if(dest&&!dron&&Communtron3.position.y==0f)
			{
				mat=3;
			}
			else
			{
				if(dri) mat=2;
			}
		}
		switch(mat)
		{
			case 1:
			{
				Drill.material=D1;
				Drill3.material=D1;
				break;
			}
			case 2:
			{
				Drill.material=D2;
				Drill3.material=D2;
				break;
			}
			case 3:
			{
				Drill.material=D3;
				Drill3.material=D3;
				break;
			}
			case 4:
			{
				Drill.material=D4;
				Drill3.material=D4;
				break;
			}
		}
		Communtron5.position=new Vector3(mat-1,Communtron5.position.y,Communtron5.position.z);
		//light.color=new Color32((byte)lR,(byte)lG,(byte)lB,255);
		if(mat==2)
		{
			if(lR>220) lR-=1;
			if(lG>220) lG-=1;
			if(lB>220) lB-=1;
			//rFuelParticles.localPosition=new Vector3(0f,1.9f,0f);
		}
		else
		{
			if(lR<255) lR+=1;
			if(lG<255) lG+=1;
			if(lB<255) lB+=1;
			//rFuelParticles.localPosition=new Vector3(1000f,1.9f,0f);
		}
	}
}
