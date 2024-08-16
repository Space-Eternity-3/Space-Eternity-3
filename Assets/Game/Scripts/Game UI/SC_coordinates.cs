using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_coordinates : MonoBehaviour {

	public Transform player; //dynamic variable
	public SC_invisibler SC_invisibler; //dynamic variable
	public Text nik; //dynamic variable

	public Text coordinates;
	public bool mtp_coords;
	public Text nickname;
	public Transform infoMTP;
	public SC_control SC_control;

	void LateUpdate()
	{
		if(!mtp_coords) B_Update();
	}
	public void B_Update()
	{
		float fX=(Mathf.Round(player.position.x*10f)/10f);
		float fY=(Mathf.Round(player.position.y*10f)/10f);
		coordinates.text="X "+(int)fX+"\n"+"Y "+(int)fY;

		if(mtp_coords)
		{
			string niks = nik.text;
			nickname.text = niks;
			if(player.position.z>100f || SC_invisibler.invisible || !SC_control.show_positions) coordinates.text="...";
			if(niks==""||niks=="0") infoMTP.localPosition=new Vector3(10000f,0f,0f);
			else infoMTP.localPosition=new Vector3(0f,0f,0f);
		}
	}
}
