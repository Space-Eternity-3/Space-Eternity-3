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
	public int ImpulseTime;
	public float ImpulseSpeed;
	
	public SC_slots SC_slots;
	public SC_bars SC_bars;
	public SC_data SC_data;
	public SC_control SC_control;
	public SC_seeking SC_seeking;
	public SC_seeking SC_seeking2;
	public SC_invisibler SC_invisibler;

	public bool unstabling;
	
	public void LoadDataArt()
	{
		ProtLevelAdd = Parsing.FloatE(SC_data.Gameplay[16]);
		ProtRegenMultiplier = Parsing.FloatE(SC_data.Gameplay[17]);
		
		powerRM[2] = Parsing.FloatE(SC_data.Gameplay[18]);
		ImpulseTime = (int)(Parsing.FloatE(SC_data.Gameplay[19])*50+2);
		if(ImpulseTime < 2) ImpulseTime = 2;
		ImpulseSpeed = Parsing.FloatE(SC_data.Gameplay[20]);
		
		powerRM[3] = Parsing.FloatE(SC_data.Gameplay[21]);
		powerUM[3] = Parsing.FloatE(SC_data.Gameplay[22]);

		powerRM[6] = Parsing.FloatE(SC_data.Gameplay[107]);
		
		SC_control.unstable_probability = (int)(Parsing.FloatE(SC_data.Gameplay[23])*50+1);
		if(SC_control.unstable_probability < 1) SC_control.unstable_probability = 1;
		SC_control.unstable_sprobability = (int)(Parsing.FloatE(SC_data.Gameplay[24])*50+1);
		if(SC_control.unstable_sprobability < 1) SC_control.unstable_sprobability = 1;
		SC_control.unstable_force = Parsing.FloatE(SC_data.Gameplay[25]);

		SC_control.LoadSomeGameplayData();
	}
	public bool IsArtefact(int n)
	{
		int i;
		if(n==41) return true;
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
	public bool IsUnchargedImmortality()
	{
		if(SC_slots.BackpackX[15]==41 && SC_slots.BackpackY[15]>0) return true;
		return false;
	}
	public int GetArtSource(int n)
	{
		int eff;
		if(!SC_invisibler.invisible)
		{
			eff = n*100;
			if(SC_control.impulse_enabled) eff+=2;
			else if(unstabling) eff+=3;
			else {} //something
			return eff;
		}
		else return 1;
		
		/* (%100-ID) %2:[1-static 0-rotate]
		
		0 - default
		1 - invisible (modified)
		2 - impulse
		3 - unstabling
		
		*/
	}
	public float GetProtLevelAdd()
	{
		if(GetArtefactID()!=1) return 0f;
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
		bool un = IsUnchargedImmortality();
		
		namet.text = namets[n];
		description.text = descriptions[n];
		SC_bars.double_right = bar3[n];

		if(un) {
			namet.text = namets[7];
			description.text = descriptions[7];
		}
		
		if(SC_control.SC_shield.green_time==0) SC_control.HealthNormal = Color1N[n];
		else SC_control.HealthNormal = SC_control.ShieldBarColor;
		
		SC_control.FuelNormal = Color2N[n];
		SC_control.FuelBurning = Color2B[n];
		SC_control.FuelBlocked = Color2L[n];
		
		SC_control.PowerNormal = Color3N[n];
		SC_control.PowerBurning = Color3B[n];
		SC_control.PowerBlocked = Color3L[n];

		bool key_A = SC_control.PressedNotInChat(KeyCode.A,"down");
		
		if(n==3 && key_A && !SC_control.pause && SC_control.livTime!=0)
		{
			if(SC_invisibler.invisible) SC_invisibler.invisible = false;
			else if(SC_control.power_V >= SC_control.IL_barrier)
			{
				SC_invisibler.invisible = true;
				Transform trn = Instantiate(SC_control.InvisiPart,SC_control.transform.position,new Quaternion(0f,0f,0f,0f));
				trn.GetComponent<SC_seeking>().enabled = true;
				if((int)SC_control.Communtron4.position.y == 100)
				{
					SC_control.SendMTP("/EmitParticles "+SC_control.connectionID+" 6 0 0");
				}
			}
		}
		if(n!=3) SC_invisibler.invisible = false;
		
		if(n==2 && key_A && SC_control.imp_cooldown==0 && !SC_control.pause && !SC_control.drill3B)
		{
			if(SC_control.power_V >= SC_control.IM_barrier && SC_control.SC_shield.green_time==0)
			{
				SC_control.imp_cooldown = (int)SC_data.GplGet("impulse_cooldown");

				SC_control.impulse_enabled = true;
				SC_control.impulse_time = ImpulseTime;
				SC_control.power_V -= SC_control.IM_barrier;
				SC_control.impulse_reset = true;
				SC_control.SetVirtualShield(ImpulseTime,"orange");

				//NOT UPDATE, CAN BE HERE
				SC_colboss[] cbss = FindObjectsOfType<SC_colboss>();
				foreach(SC_colboss cbs in cbss) {
					cbs.impulse_used = false;
				}
				
				Transform trn = Instantiate(SC_control.impulseHidden,SC_control.transform.position,new Quaternion(0f,0f,0f,0f));
				trn.GetComponent<SC_seeking>().enabled = true;
				if((int)SC_control.Communtron4.position.y == 100)
				{
					SC_control.flag_impulse_start = true;
					SC_control.SendMTP("/EmitParticles "+SC_control.connectionID+" 8 0 0");
				}
			}
		}

		if(n==6 && !SC_control.pause)
		{
			if(key_A && SC_control.power_V>=0.2f && SC_control.SC_shield.green_time==0 && SC_control.SC_effect.effect!=8)
				unstabling = true;
		}
		if(SC_control.power_V==0f || n!=6) unstabling = false;
		
		if(SC_invisibler.invisible) SC_seeking.offset = new Vector3(0f,0f,450f);
		else SC_seeking.offset = new Vector3(0f,0f,-450f*n); //normal projection
		
		SC_control.ArtSource = GetArtSource(n);
		SC_seeking2.offset = new Vector3(0f,0f,-450f*(SC_control.ArtSource % 100));
		
		SC_bars.LateUpdate();
		SC_control.LaterUpdate();
		SC_invisibler.LaterUpdate();
	}
}
