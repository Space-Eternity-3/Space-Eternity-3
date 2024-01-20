using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_resume : MonoBehaviour
{
    public SC_control SC_control;
	public Button GiveUpButton;
	
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
		GiveUpButton.interactable = (SC_control.bos_num > 0);
	}
}
