using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_drill : MonoBehaviour
{
    public bool Mining=false;
    int counter=-1;
    public int type=-1;
	public bool freeze = false;

	int down, up;

    public Transform rHydrogenParticles;

    public Transform Communtron1;
    public Transform Communtron3;
    public Transform Communtron4;
    public Transform CommuntronM1;

    public SC_slots SC_slots;
    public SC_control SC_control;
    public SC_upgrades SC_upgrades;
    public SC_data SC_data;
    public SC_asteroid SC_asteroid;

    void OnTriggerEnter(Collider collision)
	{
		if(collision.gameObject.name=="Drill3")
		{
			Communtron1.localScale+=new Vector3(0f,1f,0f);
			Mining=true;
		}
	}
	void OnTriggerExit(Collider collision)
	{
		if(collision.gameObject.name=="Drill3")
		{
			Communtron1.localScale-=new Vector3(0f,1f,0f);
			Mining=false;
		}
	}
	int GetTimeDrill()
	{
		float matpow = Mathf.Pow(SC_asteroid.upg3hugity,SC_upgrades.MTP_levels[2]+float.Parse(SC_data.Gameplay[2]));
		down = (int)(SC_asteroid.upg3down/matpow);
		up = (int)(SC_asteroid.upg3up/matpow);
		return UnityEngine.Random.Range(down,up+1);
	}
	bool make_drill_start = true;
	bool make_drill_end = false;
    void FixedUpdate()
    {
		counter++;
        if(!Input.GetMouseButton(0)) counter=1;
		if(counter==1) counter=-GetTimeDrill();

		if(Communtron1.localScale==new Vector3(2f,2f,2f)&&Mining&&Communtron1.position.z==0f&&(Communtron3.position.y==0f||CommuntronM1.position.x==1f)&&Input.GetMouseButton(0))
		{
			if(make_drill_start)
			{
				SC_control.ActualDrillGroup++;
				if((int)Communtron4.position.y == 100)
					SC_control.SendMTP("/DrillAsk "+SC_control.connectionID+" "+type+" "+SC_control.ActualDrillGroup);
				
				make_drill_start = false;
				make_drill_end = true;
			}
			SC_control.ActualDrillType=type;

			rHydrogenParticles.localPosition=new Vector3(0f,1.9f,0f);
			CommuntronM1.position=new Vector3(1f,0f,0f);

			if((int)Communtron4.position.y != 100)
			{
				int mined=0;
				if(counter==0) mined=SC_asteroid.SetLoot(type);			
				if(mined>0 && SC_slots.InvHaveB(mined,1,true,true,true,0))
					SC_slots.InvChange(mined,1,true,true,true);
			}
		}
		else if(make_drill_end)
		{
			SC_control.ActualDrillType=-1;

			make_drill_end = false;
			make_drill_start = true;
		}
		if((!(Communtron1.localScale==new Vector3(2f,2f,2f) && Input.GetMouseButton(0)))&&Communtron1.position.z==0f)
		{
			rHydrogenParticles.localPosition=new Vector3(0f,1.9f,-1000f);
			CommuntronM1.position=new Vector3(0f,0f,0f);
		}
    }
}
