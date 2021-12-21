using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_artefacts : MonoBehaviour
{	
    public int[] objIDs = new int[6];
	public string default_namet;
	public string[] namets = new string[6];
	public string default_description;
	public string[] descriptions = new string[6];
	public string[] actions = new string[6];
	
	public Text namet, description;
	
	public SC_slots SC_slots;
	
	void Update()
	{
		int i;
		for(i=0;i<6;i++)
		{
			//ONLY VISIBILITY (Y not YA)
			if(SC_slots.BackpackX[15]==objIDs[i] && SC_slots.BackpackY[15]>0)
			{
				namet.text = namets[i];
				description.text = descriptions[i];
				i=99;
			}
		}
		if(i!=100)
		{
			namet.text = default_namet;
			description.text = default_description;
		}
	}
}
