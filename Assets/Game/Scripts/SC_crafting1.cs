using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_crafting1 : MonoBehaviour {

	public Transform Communtron2;
	public Transform Communtron4;
	public Button button;
	public Text crName;
	public bool right;
	public int MaxPage=1;
	public int MaxPageB=1;

	public SC_backpack SC_backpack;
	public SC_data SC_data;

	string datapackDIR="./Datapacks/";
	
	public void Clicked()
	{
		if(right) Communtron2.position+=new Vector3(0f,0f,1f);
		else Communtron2.position-=new Vector3(0f,0f,1f);
	}
	void Update()
	{
		if(right)
		{
			if(Communtron2.position.z>=MaxPage-1) button.interactable=false;
			else button.interactable=true;
		}
		if(!right)
		{
			if(Communtron2.position.z<=0) button.interactable=false;
			else button.interactable=true;
		}
		if(right) crName.text="Crafting "+(int)Mathf.Round(Communtron2.position.z+1)+"/"+MaxPage;
	}
	void Start()
	{
		MaxPageB=int.Parse(SC_data.craftMaxPage);
		MaxPage=(int)Mathf.Ceil(float.Parse(SC_data.craftMaxPage)/6);
	}
}
