using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_bars : MonoBehaviour
{
    public Transform UI_health;
	public Transform UI_fuel;
	public Transform UI_boss;
	public Transform UI_power;
	
	Vector3 going_H = new Vector3(0f,0f,0f);
	Vector3 going_F = new Vector3(0f,0f,0f);
	Vector3 going_B = new Vector3(0f,0f,0f);
	Vector3 going_P = new Vector3(0f,0f,0f);
	
	public Vector3 pos_center;
	public Vector3 dx;
	public Vector3 dy;
	public Vector3 ndx;
	
	public bool double_left;
	public bool double_right;
	public bool swap_left;
	public bool swap_right;

	public Image left_slider;
	public Text left_text;
	int left_max = 0;
	int left_value = 0;
	
	string ConvertToTime(int seconds)
	{
		int minutes = seconds/60;
		seconds = seconds % 60;
		if(seconds<10)
		{
			return minutes+":0"+seconds;
		}
		else
		{
			return minutes+":"+seconds;
		}
	}
	public void LateUpdate()
	{
		double_left = false;
		SC_boss[] boses = FindObjectsOfType<SC_boss>();
		foreach(SC_boss bos in boses)
		{
			if(bos.timer_bar_enabled)
			{
				double_left = true;
				left_value = bos.timer_bar_value;
				left_max = bos.timer_bar_max;
				left_text.text = "Time " + ConvertToTime(left_value);
				if(left_max==0) left_max=1;
				left_slider.fillAmount = 0.116f + (left_value*0.784f/left_max);
				if(left_value==0) left_slider.fillAmount = 0f;
				break;
			}
		}

		if(double_left)
		{
			if(!swap_left)
			{
				going_H = pos_center - dx + dy;
				going_B = pos_center - dx - dy;
			}
			else
			{
				going_H = pos_center - dx - dy;
				going_B = pos_center - dx + dy;
			}
		}
		else
		{
			going_H = pos_center - dx + ndx;
			going_B = pos_center - dx + ndx;
		}
		
		if(double_right)
		{
			if(!swap_right)
			{
				going_F = pos_center + dx + dy;
				going_P = pos_center + dx - dy;
			}
			else
			{
				going_F = pos_center + dx - dy;
				going_P = pos_center + dx + dy;
			}
		}
		else
		{
			going_F = pos_center + dx + ndx;
			going_P = pos_center + dx + ndx;
		}
		
		//TEMP
		UI_health.localPosition = going_H;
		UI_boss.localPosition = going_B;
		UI_fuel.localPosition = going_F;
		UI_power.localPosition = going_P;
	}
}
