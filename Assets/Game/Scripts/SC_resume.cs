using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_resume : MonoBehaviour
{
    public SC_control SC_control;
	public Transform normal_menu;
	public Transform giveup_menu;
	
	public void Resume()
	{
		SC_control.esc_press(false);
	}
	public void Quit()
	{
		SC_control.MenuReturn();
	}
	public void GiveUp()
	{
		SC_control.esc_press(false);
		List<SC_boss> boses = SC_control.SC_lists.SC_boss;
		foreach(SC_boss bos in boses)
		{
			if(bos.bosnumed)
			{
				if(!bos.multiplayer)
					bos.GiveUpSGP();
				else
					bos.GiveUpMTP();
			}
		}
	}

	void Start()
	{
		Update();
	}
	void Update()
	{
		if(SC_control.bos_num > 0) {
			normal_menu.localPosition = new Vector3(10000f,0f,0f);
			giveup_menu.localPosition = new Vector3(0f,0f,0f);
		}
		else {
			normal_menu.localPosition = new Vector3(0f,0f,0f);
			giveup_menu.localPosition = new Vector3(10000f,0f,0f);
		}
	}
}
