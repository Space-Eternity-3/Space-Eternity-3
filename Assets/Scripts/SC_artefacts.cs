using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_artefacts : MonoBehaviour
{
    public int[] objIDs = new int[7];
	public bool[] bar3 = new bool[7];
	
	public string[] bar3namets = new string[7];
	public string[] namets = new string[7];
	public string[] descriptions = new string[7];
	public string[] actions = new string[7];
	
	public Color32[] Color1N = new Color32[7];
	
	public Color32[] Color2N = new Color32[7];
	public Color32[] Color2B = new Color32[7];
	public Color32[] Color2L = new Color32[7];
	
	public Color32[] Color3N = new Color32[7];
	public Color32[] Color3B = new Color32[7];
	
	public Text namet, description;
	public Text bar3namet;
	
	public SC_slots SC_slots;
	public SC_bars SC_bars;
	public SC_control SC_control;
	
	public bool IsArtefact(int n)
	{
		int i;
		for(i=1;i<7;i++)
			if(objIDs[i]==n) return true;
		return false;
	}
	void Start()
	{
		LateUpdate();
	}
	void LateUpdate()
	{
		int i;
		
		//VISIBILITY and PSYCHICAL (Y==YA) in this case
		for(i=1;i<=6;i++)
		{
			if(SC_slots.BackpackX[15]==objIDs[i] && SC_slots.BackpackY[15]>0) break;
		}
		if(i==7) i=0;
		
		namet.text = namets[i];
		description.text = descriptions[i];
		SC_bars.double_right = bar3[i];
		
		SC_control.HealthNormal = Color1N[i];
		
		SC_control.FuelNormal = Color2N[i];
		SC_control.FuelBurning = Color2B[i];
		SC_control.FuelBlocked = Color2L[i];
		
		bar3namet.text = bar3namets[i] + " 0/50";
		SC_control.PowerNormal = Color3N[i];
		SC_control.PowerBurning = Color3B[i];
		
		SC_bars.LateUpdate();
		SC_control.LaterUpdate();
	}
}
