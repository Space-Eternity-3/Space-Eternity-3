using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_selected : MonoBehaviour {

	public Transform Communtron1;
	public Transform Communtron4;
	public SC_control SC_control;
	public SC_push SC_push;
	public Image sel;
	public int selected = 1;
	bool cont;
	bool living=true;

	public Color32 col_sel,col_desel;

	public Transform[] Slot = new Transform[10];
	Vector3 Slot_pom;

	void Update()
	{
		int i;
		if(!living)
		{
			if(Communtron1.position.z==0) living=true;
			else return;
		}
		transform.localPosition=new Vector3(50f*(selected-5),0f,0f);
		
		if(SC_push.clicked_on==0)
		{
			if(Input.GetAxisRaw("Mouse ScrollWheel")<0&&!Input.GetKey(KeyCode.LeftControl)&&!SC_control.SC_chat.typing&&!SC_control.pause)
 			{
				cont=true;
     			if(cont) selected++;
 			}
			if(Input.GetAxisRaw("Mouse ScrollWheel")>0&&!Input.GetKey(KeyCode.LeftControl)&&!SC_control.SC_chat.typing&&!SC_control.pause)
 			{
				cont=true;
     			if(cont) selected--;
 			}
		}

		if(selected>9) selected=1;
		if(selected<1) selected=9;

		Communtron1.position=new Vector3(Communtron1.position.x,selected,Communtron1.position.z);

		//if(Input.GetMouseButton(0)) sel.color=col_sel;
		//else sel.color=col_desel;
	}
	public void set_selected(int seth)
	{
		selected=seth;
	}
}
