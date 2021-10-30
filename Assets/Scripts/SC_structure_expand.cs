using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_structure_expand : MonoBehaviour
{
	public SC_fun SC_fun;

    public void StructureGenerate()
    {
        int X=(int)Mathf.Round(transform.position.x/180f);
		int Y=(int)Mathf.Round(transform.position.y/180f);

        int Xctrl=(int)Mathf.Round(transform.position.x/10f);
		int Yctrl=(int)Mathf.Round(transform.position.y/10f);

		int ID=SC_fun.CheckID(X,Y);
		if(Xctrl<=50000&&Xctrl>=-50000&&Yctrl<=50000&&Yctrl>=-50000&&SC_fun.StructureCheck(ID))
		{
			SC_fun.GenListAdd(ID,1);
			Transform trn = Instantiate(SC_fun.structures[0], transform.position, Quaternion.identity);
            SC_struct SC_struct = trn.GetComponent<SC_struct>();
            SC_struct.ID=ID;
            SC_struct.X=X;
            SC_struct.Y=Y;
		}
    }
}
