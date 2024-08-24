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
	bool living=true;

	public Color32 col_sel,col_desel;

	public Transform[] Slot = new Transform[10];
	Vector3 Slot_pom;

	void Update()
	{
		if(!living)
		{
			if(Communtron1.position.z==0) living=true;
			else return;
		}
		transform.localPosition=new Vector3(50f*(selected-5),0f,0f);
		
		if(SC_push.clicked_on==0 && !SC_control.SC_chat.typing && !SC_control.pause)
		{
			//Scroll selection
			if(Input.GetAxisRaw("Mouse ScrollWheel")<0 && !Input.GetKey(KeyCode.LeftControl))
				selected++;
				
			if(Input.GetAxisRaw("Mouse ScrollWheel")>0 && !Input.GetKey(KeyCode.LeftControl))
				selected--;

			//Number selection
			for(int i=1;i<=9;i++)
				if(SC_control.PressedNotInChat((KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + i),"down"))
					selected = i;
		}

		if(selected>9) selected=1;
		if(selected<1) selected=9;

		Communtron1.position=new Vector3(Communtron1.position.x,selected,Communtron1.position.z);
	}
	public void set_selected(int seth)
	{
		selected=seth;
	}
}
