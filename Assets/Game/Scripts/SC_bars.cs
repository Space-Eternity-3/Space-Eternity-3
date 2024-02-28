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

	public Text left_text;
	int left_max = 0;
	int left_value = 0;

	public SC_ui_bar_amount health_ui_bar;
	public SC_ui_bar_amount turbo_ui_bar;
	public SC_ui_bar_amount boss_ui_bar;
	public SC_ui_bar_amount power_ui_bar;
	public SC_ui_bar_amount healthold_ui_bar;
	
	public Image Time;
	public SC_clock_arrow DynamicClockArrow;
	public Text BossHealthText;
	public RectTransform butter;
	public Transform CompassInfo, TimeInfo;

	public SC_boss bos;
	public SC_data SC_data;

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

		if(bos!=null)
		{
			//Boss time clock
			left_value = bos.timer_bar_value;
			left_max = bos.timer_bar_max;
			left_text.text = ConvertToTime((left_value+49)/50);
			if(left_max==0) left_max=1;
			Time.fillAmount = left_value*1f/left_max;
			DynamicClockArrow.LaterUpdate();

			Vector2 mousePosition = Input.mousePosition;
			if(!SC_data.SC_control.SC_fun.AreCoordinatesInsideRect(butter,mousePosition.x,mousePosition.y)) {
				CompassInfo.localPosition = new Vector3(10000f,0f,0f);
				TimeInfo.localPosition = new Vector3(0f,0f,0f);
			} else {
				CompassInfo.localPosition = new Vector3(0f,0f,0f);
				TimeInfo.localPosition = new Vector3(10000f,0f,0f);	
			}

			//Boss health bar
			double_left = true;
			float max_health = bos.dataID[7] / 100f;
			float act_health = bos.dataID[6] / 100f;
			int int_act = (int)act_health;
			if(int_act==0) int_act=1;
			BossHealthText.text = "Boss "+int_act+"/"+(int)max_health;
			float job_value = act_health / max_health;
			if(job_value>=0 && job_value<=1) boss_ui_bar.value = job_value;
			else boss_ui_bar.value = 0;
		}
		else
		{
			CompassInfo.localPosition = new Vector3(0f,0f,0f);
			TimeInfo.localPosition = new Vector3(10000f,0f,0f);
		}

		Vector3 left = pos_center - dx + ndx;
		Vector3 right = pos_center + dx + ndx;
		Vector3 left_up = pos_center - dx + dy;
		Vector3 left_down = pos_center - dx - dy;
		Vector3 right_up = pos_center + dx + dy;
		Vector3 right_down = pos_center + dx - dy;

		if(double_left)
		{
			if(!swap_left) {
				going_H = left_up;
				going_B = left_down;
			} else {
				going_H = left_down;
				going_B = left_up;
			} } else {
				going_H = left;
				going_B = left;
		}
		
		if(double_right)
		{
			if(!swap_right) {
				going_F = right_up;
				going_P = right_down;
			} else {
				going_F = right_down;
				going_P = right_up;
			} } else {
				going_F = right;
				going_P = right;
		}
		
		UI_health.localPosition = going_H;
		UI_boss.localPosition = going_B;
		UI_fuel.localPosition = going_F;
		UI_power.localPosition = going_P;
	}
	public void MuchLaterUpdate()
	{
		health_ui_bar.MuchLaterUpdate();
		turbo_ui_bar.MuchLaterUpdate();
		power_ui_bar.MuchLaterUpdate();
		boss_ui_bar.MuchLaterUpdate();
		healthold_ui_bar.MuchLaterUpdate();
	}
}
