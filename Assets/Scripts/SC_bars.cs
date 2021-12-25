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
	
	public void LateUpdate()
	{
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
