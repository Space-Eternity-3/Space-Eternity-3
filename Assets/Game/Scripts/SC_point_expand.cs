using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_point_expand : MonoBehaviour {

	public Transform asteroid, struct2empty;
	public SC_fun SC_fun;
	public int mode;

	public void PointGenerate(float sector_size)
	{
		int X=(int)Mathf.Round(transform.position.x/10f);
		int Y=(int)Mathf.Round(transform.position.y/10f);
		
		int tX=(int)Mathf.Round(transform.position.x/sector_size);
		int tY=(int)Mathf.Round(transform.position.y/sector_size);
		
		int ID=SC_fun.CheckID(tX,tY);
		if(X<=20000&&X>=-20000&&Y<=20000&&Y>=-20000)
		{
			if(mode==0)
			{
				if(!SC_fun.GenListContains(ID,0) && ID%2!=0)
				{
					SC_fun.GenListAdd(ID,mode);
					if(SC_fun.AsteroidCheck(ID))
					{
						SC_asteroid sca = Instantiate(asteroid, transform.position, Quaternion.identity).GetComponent<SC_asteroid>();
						sca.X = tX; sca.Y = tY; sca.ID = ID;
					}
					else
					{
						SC_struct2 sca = Instantiate(struct2empty, transform.position, Quaternion.identity).GetComponent<SC_struct2>();
						sca.X = tX; sca.Y = tY; sca.ID = ID;
					}
				}
			}
			if(mode==1)
			{
				int stch = SC_fun.StructureCheck(ID);
				if(stch!=0)
				{
					SC_fun.GenListAdd(ID,mode);
					SC_structure stc;
					
					//All structures are 'custom' from Beta 2.2
					stc = Instantiate(SC_fun.structures[0], transform.position, Quaternion.identity).GetComponent<SC_structure>();
					stc.SeonField = SC_fun.SC_data.CustomStructures[-stch];

					stc.X = tX; stc.Y = tY; stc.ID = ID;
				}
			}
		}
	}
	void Awake() {
		SC_fun.SC_control.SC_lists.AddTo_SC_point_expand(this);
	}
	void Destroy() {
		SC_fun.SC_control.SC_lists.RemoveFrom_SC_point_expand(this);
	}
}
