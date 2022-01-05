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
	
	public float[] powerRM = new float[7];
	public float[] powerUM = new float[7];
	
	public Color32[] Color1N = new Color32[7];
	
	public Color32[] Color2N = new Color32[7];
	public Color32[] Color2B = new Color32[7];
	public Color32[] Color2L = new Color32[7];
	
	public Color32[] Color3N = new Color32[7];
	public Color32[] Color3B = new Color32[7];
	public Color32[] Color3L = new Color32[7];
	
	public Text namet, description;
	
	public float ProtLevelAdd;
	public float ProtRegenMultiplier;
	
	public SC_slots SC_slots;
	public SC_bars SC_bars;
	public SC_control SC_control;
	public SC_seeking SC_seeking;
	public SC_invisibler SC_invisibler;
	
	public bool IsArtefact(int n)
	{
		int i;
		for(i=1;i<7;i++)
			if(objIDs[i]==n) return true;
		return false;
	}
	public int GetArtefactID()
	{
		int i;
		//VISIBILITY and PSYCHICAL (Y==YA) in this case
		for(i=1;i<=6;i++)
		{
			if(SC_slots.BackpackX[15]==objIDs[i] && SC_slots.BackpackY[15]>0) break;
		}
		if(i==7) i=0;
		return i;
	}
	public int GetArtSource(int n)
	{
		if(!SC_invisibler.invisible) return n*100;
		else return 1;
		
		/* (%100)
		
		1 - invisible
		
		*/
	}
	public float GetProtLevelAdd()
	{
		if(GetArtefactID()!=1 || SC_invisibler.invisible) return 0f;
		else return ProtLevelAdd;
	}
	public float GetProtRegenMultiplier()
	{
		if(GetArtefactID()!=1) return 1f;
		else return ProtRegenMultiplier;
	}
	void Start()
	{
		LateUpdate();
	}
	void LateUpdate()
	{
		int n = GetArtefactID();
		
		namet.text = namets[n];
		description.text = descriptions[n];
		SC_bars.double_right = bar3[n];
		
		SC_control.HealthNormal = Color1N[n];
		
		SC_control.FuelNormal = Color2N[n];
		SC_control.FuelBurning = Color2B[n];
		SC_control.FuelBlocked = Color2L[n];
		
		SC_control.PowerNormal = Color3N[n];
		SC_control.PowerBurning = Color3B[n];
		SC_control.PowerBlocked = Color3L[n];
		
		if(n==3 && Input.GetKeyDown(KeyCode.A) && !SC_control.pause)
		{
			if(SC_invisibler.invisible) SC_invisibler.invisible = false;
			else if(SC_control.power_V >= SC_control.F_barrier) SC_invisibler.invisible = true;
		}
		if(n!=3) SC_invisibler.invisible = false;
		
		if(!SC_invisibler.invisible) SC_seeking.offset = new Vector3(0f,0f,-450f*n); //another direction (projection)
		else SC_seeking.offset = new Vector3(0f,0f,450f);
		SC_control.ArtSource = GetArtSource(n);
		
		SC_bars.LateUpdate();
		SC_control.LaterUpdate();
		SC_invisibler.LaterUpdate();
	}
}
