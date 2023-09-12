using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_drill : MonoBehaviour
{
	//WARNING! PUBLIC VARIABLES REQUIRED TO SET IN SC_structure

    bool Mining=false;
    int counter=-1;
    public int type=-1;
	public bool freeze = false;

	int down=10, up=20;

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
	void Start()
	{
		down = (int)(SC_asteroid.upg3down/Mathf.Pow(SC_asteroid.upg3hugity,SC_upgrades.MTP_levels[2]+float.Parse(SC_data.Gameplay[2])));
		up = (int)(SC_asteroid.upg3up/Mathf.Pow(SC_asteroid.upg3hugity,SC_upgrades.MTP_levels[2]+float.Parse(SC_data.Gameplay[2])));
	}
	int GetTimeDrill() {
		return UnityEngine.Random.Range(down,up);
	}
    void FixedUpdate()
    {
		counter++;
        if(!Input.GetMouseButton(0)) counter=1;
		if(counter==1) counter=-GetTimeDrill();
		int mined;

		if(Communtron1.localScale==new Vector3(2f,2f,2f)&&Mining&&Communtron1.position.z==0f&&(Communtron3.position.y==0f||CommuntronM1.position.x==1f)&&Input.GetMouseButton(0))
		{
			rHydrogenParticles.localPosition=new Vector3(0f,1.9f,0f);
			CommuntronM1.position=new Vector3(1f,0f,0f);
			mined=0;

			if(counter==0) mined=SC_asteroid.SetLoot(type);			
			if(mined>0 && SC_slots.InvHaveB(mined,1,true,true,true,0))
			{
				int slot = SC_slots.InvChange(mined,1,true,true,true);
				if((int)Communtron4.position.y == 100) SC_control.SendMTP("/InventoryChange "+SC_control.connectionID+" "+mined+" 1 "+slot);
			}
		}
		if((!(Communtron1.localScale==new Vector3(2f,2f,2f) && Input.GetMouseButton(0)))&&Communtron1.position.z==0f)
		{
			rHydrogenParticles.localPosition=new Vector3(0f,1.9f,-1000f);
			CommuntronM1.position=new Vector3(0f,0f,0f);
		}
    }
}
