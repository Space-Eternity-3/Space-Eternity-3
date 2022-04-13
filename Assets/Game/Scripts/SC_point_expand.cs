using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_point_expand : MonoBehaviour {

	public Transform asteroid;
	public SC_fun SC_fun;
	public int mode;

	public void PointGenerate(float sector_size)
	{
		int X=(int)Mathf.Round(transform.position.x/10f);
		int Y=(int)Mathf.Round(transform.position.y/10f);
		
		int tX=(int)Mathf.Round(transform.position.x/sector_size);
		int tY=(int)Mathf.Round(transform.position.y/sector_size);
		
		int ID=SC_fun.CheckID(tX,tY);
		if(X<=50000&&X>=-50000&&Y<=50000&&Y>=-50000)
		{
			if(mode==0)
			{
				if(SC_fun.AsteroidCheck(ID))
				{
					SC_fun.GenListAdd(ID,mode);
					SC_asteroid sca = Instantiate(asteroid, transform.position, Quaternion.identity).GetComponent<SC_asteroid>();
					sca.X = tX; sca.Y = tY; sca.ID = ID;
				}
			}
			if(mode==1)
			{
				int stch = SC_fun.StructureCheck(ID);
				if(stch!=0)
				{
					SC_fun.GenListAdd(ID,mode);
					SC_structure stc = Instantiate(SC_fun.structures[stch], transform.position, Quaternion.identity).GetComponent<SC_structure>();
					stc.X = tX; stc.Y = tY; stc.ID = ID;
				}
			}
		}
	}
}
