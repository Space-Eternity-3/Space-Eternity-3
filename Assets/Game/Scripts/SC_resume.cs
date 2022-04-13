using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_resume : MonoBehaviour
{
    public SC_control SC_control;
	
	public void Resume()
	{
		SC_control.esc_press(false);
	}
	public void Quit()
	{
		SC_control.MenuReturn();
	}
}
