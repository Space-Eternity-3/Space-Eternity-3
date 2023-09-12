using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using WebSocketSharp;
using UnityEngine.SceneManagement;
using System;
using System.Text;

public class SC_control : MonoBehaviour {

	public Canvas Screen1;
	public Canvas Screen2;
	public Canvas Screen3;
	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron3;
	public Transform Communtron4;
	public Transform Communtron5;
	public GameObject CommuntronM1;
	public Transform player;
	public Transform player_illusion;
	public Transform camera;
	public Transform explosion;
	public Transform explosionM;
	public Transform explosion2;
	public Transform explosion2M;
	public Transform receiveParticles;
	public Transform respawn2M;
	public Transform InvisiPart;
	public Transform ImmortalParticles;
	public Transform unstablePart;
	public Transform impulseHidden;
	public Transform particlesBossDamage;
	public Transform particlesBossDamageM;
	public Transform particlesBossExplosion;
	public Transform particlesBossExplosionM;
	public Transform TelepParticles;
	public Transform[] particlesEmptyBulb = new Transform[6];
	public Rigidbody playerR;
	public Transform drill3T;
	public Transform respawn_point;
	public Text servername,pingname;
	public Text TextConstYou;
	float mX,mY,mmX,mmY,X,Y,F=0.3f;
	bool big_vel=false;

	public Renderer engine;
	public Material E1;
	public Material E2;
	public Material E3;
	public Material E4;

	bool engineON=false;
	bool engineOFF=false;
	public bool turbo=false;
	bool brake=false;
	public bool drill3B=false;
	public int enMode=0;
	public string conID;
	public string livID="0";
	public string immID="0";
	public string sr_livID="-1";
	public string sr_immID="-1";

	public Image rocket_fuel;
	public Image health;
	public Image healthOld;
	public Image power;
	
	int licznikD=0, licznikC=0, timerH=0;
	public int livTime=0;
	int reg_wait=0;
	int cooldown=0;
	public int imp_cooldown=0;
	int presed=-25;
	int dmLicz=0;
	int saveCo=0;
	public int max_players = 10;
	public bool blockEscapeThisFrame = false;

	public float F_barrier;
	public float IL_barrier;
	public float IM_barrier;

	public float VacuumDrag,Engines;
	public float unit=0.0008f;
	public float health_base;
	public float mushroom_force;

	int localPing=0;
	int returnedPing=0;
	string trping="0,00";
	float truePing;
	public int intPing=-1;
	bool dont=false;
	bool repeted=false;
	bool repetedAF=false;
	public bool gtm1 = false;
	public int bos_num = 0;
	bool damage_sounds_disabled = false;
	public int current_tick = -1;
	public bool absolute_health_sync = true;

	public Color32 HealthNormal;

	public Color32 FuelNormal;
	public Color32 FuelBurning;
	public Color32 FuelBlocked;
	
	public Color32 PowerNormal; 
	public Color32 PowerBurning;
	public Color32 PowerBlocked;

	string worldDIR="";
	int worldID=1;

	public WebSocket ws;
	public int connectionID=0;
	public bool living;
	public float healBalance = 0f;
	public float damageBalance = 0f;
	Vector3 solidPos;
	bool connectWorks=true;
	public string nick;
	string getData="";
	string getInventory="";
	int[] invdata = new int[18];
	Vector3[] betterInvConverted = new Vector3[9];
	
	public SC_fun SC_fun;
	public SC_upgrades SC_upgrades;
	public SC_backpack SC_backpack;
	public SC_data SC_data;
	public SC_sounds SC_sounds;
	public SC_slots SC_slots;
	public SC_artefacts SC_artefacts;
	public SC_invisibler SC_invisibler;
	public SC_bullet SC_bullet;
	public SC_projection SC_projection;
	public SC_lists SC_lists;
	public SC_camera SC_camera;
	public SC_effect SC_effect;
	public SC_seek_data SC_seek_data;
	public SC_bars SC_bars;
	public SC_gameplay_set SC_gameplay_set;
	public SC_inv_mover SC_inv_mover; //left
	public SC_chat SC_chat;

	public List<bool> NUL = new List<bool>();
	public List<Transform> RR = new List<Transform>();
	public List<SC_players> PL = new List<SC_players>();
	public List<Transform> RU = new List<Transform>();
	public List<TextMesh> N = new List<TextMesh>();
	public List<Canvas> NC = new List<Canvas>();
	public List<Text> NCT = new List<Text>();
	public List<Slider> NCH = new List<Slider>();
	public List<Image> NCHOF = new List<Image>();
	public List<int> ramvis = new List<int>();

	public List<int> others1_RPC = new List<int>();
	public List<int> others2_RPC = new List<int>();
	public List<float> health_RPC = new List<float>();
	public List<float> rposX_RPC = new List<float>();
	public List<float> rposY_RPC = new List<float>();
	public List<string> nick_RPC = new List<string>();

	public static Queue cmdList = new Queue();
	int fixup=0;

	public float health_V=1f, turbo_V=0f, power_V=0f;
	public Text health_Text, turbo_Text, power_Text;
	
	public int ArtSource = 0;
	public bool pause = false;
	public bool timeStop = false;
	public int timeInvisiblePulse;
	public bool impulse_reset;
	public bool show_positions;
	
	public int unstable_probability;
	public int unstable_sprobability;
	public float unstable_force;
	public float graviton_force;

	public float at_unstable_regen1;
	public float at_unstable_regen2;
	public float at_unstable_regen3;
	public float unstabling_max_deviation;
	
	bool escaped = false;
	string RPU = "XXX";
	int MTPloadedCounter=0;
	Quaternion lock_rot = new Quaternion(0f,0f,0f,0f);
	
	public bool impulse_enabled;
	public int impulse_time;
	
	public bool public_placed = false;
	public bool f1 = false;

	public bool flag_impulse_start = false;
	public bool flag_invisibility_pulse = false;
	
	float V_to_F(float V,bool turboo)
	{
		if(V==0f) return 0f;
		V=Engines*V;
		if(turboo) V=V*Mathf.Pow(1.08f,SC_upgrades.MTP_levels[1]);
		if(V>=0f) return V*(V+15f)/1000f;
		else
		{
			V=Mathf.Abs(V);
			return -V*(V+15f)/1000f;
		}
	}
	public void GravitonCatch()
	{
		foreach(SC_boss bos in SC_lists.SC_boss) {
			if(bos.InArena("range"))
			{
				Vector3 vect = bos.bossModels.position - player.position;
				vect = new Vector3(vect.x,vect.y,0f);
				playerR.velocity /= 2;
				playerR.velocity += graviton_force * Vector3.Normalize(vect);
				return;
			}
		}
	}
	public bool HealBalanceGood()
	{
		float heal = GetHealthFraction(healBalance);
		return (Mathf.Round(health_V*10000f)/10000f+heal)<1f;
	}
	public float GetHealthFraction(float hp)
	{
		float potHHH = SC_upgrades.MTP_levels[0]+SC_artefacts.GetProtLevelAdd()+float.Parse(SC_data.Gameplay[26]);
		if(potHHH<-50f) potHHH = -50f; if(potHHH>56.397f) potHHH = 56.397f;
		float heal=0.02f*hp/(Mathf.Ceil(50*Mathf.Pow(health_base,potHHH))/50f);
		return heal;
	}
	public void LaterUpdate()
	{
		Screen1.enabled = !f1;
		Screen2.enabled = !f1;
		for(int ji=1;ji<max_players;ji++){
			NC[ji].enabled = !f1 && (PL[ji].ArtSource % 25!=1);
		}
		List<SC_pulse_bar> spbs = SC_lists.SC_pulse_bar;
		foreach(SC_pulse_bar spb in spbs) {
			spb.canvas.enabled = !f1;
		}
		
		if(!timeStop){
		
		//SHOT
		bool wr_tick = (int)Communtron4.position.y!=100 || current_tick!=-1;
		bool wr_comms = Communtron3.position.y==0f && Communtron2.position.x==0f && Communtron3.position.z==0f;
		bool wr_have = SC_slots.InvHaving(24) || SC_slots.InvHaving(39) || SC_slots.InvHaving(48) || SC_slots.InvHaving(64) || SC_slots.InvHaving(65);
		bool wr_cotr = !(Input.GetKey(KeyCode.LeftControl) && (SC_slots.InvHaving(48) || SC_slots.InvHaving(65)));
		bool wr_isok = cooldown==0 && wr_comms && wr_have && !impulse_enabled && !Input.GetMouseButton(0) && wr_cotr;
		bool wr_moustay = Input.GetMouseButton(1) && !Input.GetMouseButtonDown(1);
		bool wr_unstabling = (!SC_artefacts.unstabling);
		
		if(SC_effect.effect!=8 && wr_tick && wr_isok && wr_moustay && !public_placed && livTime>=50 && (intPing!=-1 || (int)Communtron4.position.y!=100) && wr_unstabling)
		{
			int slot, typ = 1;

			//Bullet types
			if(SC_slots.InvHaving(24))
			{
				typ = 1;
				slot = SC_slots.InvChange(24,-1,true,false,true);
				if((int)Communtron4.position.y==100) SendMTP("/InventoryChange "+connectionID+" 24 -1 "+slot);
			}
			else if(SC_slots.InvHaving(39))
			{
				typ = 2;
				slot = SC_slots.InvChange(39,-1,true,false,true);
				if((int)Communtron4.position.y==100) SendMTP("/InventoryChange "+connectionID+" 39 -1 "+slot);
			}
			else if(SC_slots.InvHaving(48))
			{
				typ = 3;
				slot = SC_slots.InvChange(48,-1,true,false,true);
				if((int)Communtron4.position.y==100) SendMTP("/InventoryChange "+connectionID+" 48 -1 "+slot);
			}
			else if(SC_slots.InvHaving(64))
			{
				typ = 14;
				slot = SC_slots.InvChange(64,-1,true,false,true);
				if((int)Communtron4.position.y==100) SendMTP("/InventoryChange "+connectionID+" 64 -1 "+slot);
			}
			else if(SC_slots.InvHaving(65))
			{
				typ = 15;
				slot = SC_slots.InvChange(65,-1,true,false,true);
				if((int)Communtron4.position.y==100) SendMTP("/InventoryChange "+connectionID+" 65 -1 "+slot);
			}

			int coltyp = 0;
			if(typ==1) coltyp = 0;
			if(typ==2) coltyp = 1;
			if(typ==3) coltyp = 4;
			if(typ==14) coltyp = 2;
			if(typ==15) coltyp = 3;
			
			cooldown = (int)float.Parse(SC_data.Gameplay[97+coltyp]);

			float xpo = Input.mousePosition.x-Screen.width/2, ypo=Input.mousePosition.y-Screen.height/2;
			if(typ!=3) playerR.velocity += Skop(float.Parse(SC_data.Gameplay[30]),new Vector3(-xpo,-ypo,0f));

			SC_bullet.Shot(
				transform.position,
				new Vector3(xpo,ypo,0f),
				playerR.velocity*0.02f,
				typ,
				0
			);
			SC_invisibler.invisible = false;
		}

		if(SC_artefacts.unstabling && cooldown==0)
		{
			cooldown = (int)float.Parse(SC_data.Gameplay[97+4]);

			power_V += (at_unstable_regen2)/50f;

			float xpo = Input.mousePosition.x-Screen.width/2, ypo=Input.mousePosition.y-Screen.height/2;
			float angle = Mathf.Atan2(ypo,xpo);
			int dever = UnityEngine.Random.Range(-10,11);
			float dever2 = dever/10f;
			angle += (dever2 * unstabling_max_deviation) * Mathf.PI / 180f;

			xpo = Mathf.Cos(angle);
			ypo = Mathf.Sin(angle);
			
			SC_bullet.next_bullet_virtual = true;
			SC_bullet.Shot(
				transform.position,
				new Vector3(xpo,ypo,0f),
				playerR.velocity*0.02f,
				3,
				0
			);
		}

		if(Input.GetMouseButtonDown(1)&&Communtron3.position.y==0f&&Communtron3.position.z==0f&&Communtron2.position.x==0f)
		{
			//Healing potion
			if(SC_slots.InvHaving(55)) if(AllowingPotion("healing"))
			{
				if(!SC_invisibler.invisible)
				{
					Transform trn11 = Instantiate(particlesEmptyBulb[0],transform.position,new Quaternion(0f,0f,0f,0f));
					trn11.GetComponent<SC_seeking>().enabled = true;
				}
				int slot = SC_slots.InvChange(55,-1,true,false,true);
				if((int)Communtron4.position.y==100) {
					SendMTP("/InventoryChange "+connectionID+" 55 -1 "+slot);
					SendMTP("/Heal "+connectionID+" 1");
					if(!SC_invisibler.invisible) SendMTP("/EmitParticles "+connectionID+" 11 0 0");
					healBalance += float.Parse(SC_data.Gameplay[31]);
				}
				else HealSGP("1");
			}
			else InfoUp("Potion blocked",380);

			//Turbo potion
			if(SC_slots.InvHaving(57)) if(AllowingPotion("turbo"))
			{
				if(!SC_invisibler.invisible)
				{
					Transform trn11 = Instantiate(particlesEmptyBulb[1],transform.position,new Quaternion(0f,0f,0f,0f));
					trn11.GetComponent<SC_seeking>().enabled = true;
				}
				int slot = SC_slots.InvChange(57,-1,true,false,true);
				if((int)Communtron4.position.y==100) {
					SendMTP("/InventoryChange "+connectionID+" 57 -1 "+slot);
					if(!SC_invisibler.invisible) SendMTP("/EmitParticles "+connectionID+" 12 0 0");
				}
				turbo_V = 1f;
			}
			else InfoUp("Potion blocked",380);

			//Power potion
			if(SC_slots.InvHaving(59)) if(AllowingPotion("power"))
			{
				if(!SC_invisibler.invisible)
				{
					Transform trn11 = Instantiate(particlesEmptyBulb[2],transform.position,new Quaternion(0f,0f,0f,0f));
					trn11.GetComponent<SC_seeking>().enabled = true;
				}
				int slot = SC_slots.InvChange(59,-1,true,false,true);
				if((int)Communtron4.position.y==100) {
					SendMTP("/InventoryChange "+connectionID+" 59 -1 "+slot);
					if(!SC_invisibler.invisible) SendMTP("/EmitParticles "+connectionID+" 13 0 0");
				}
				power_V = 1f;
			}
			else InfoUp("Potion blocked",380);

			//Blank potion
			if(SC_slots.InvHaving(61)) if((AllowingPotion("blank")))
			{
				if(!SC_invisibler.invisible)
				{
					Transform trn11 = Instantiate(particlesEmptyBulb[3],transform.position,new Quaternion(0f,0f,0f,0f));
					trn11.GetComponent<SC_seeking>().enabled = true;
				}
				int slot = SC_slots.InvChange(61,-1,true,false,true);
				if((int)Communtron4.position.y==100) {
					SendMTP("/InventoryChange "+connectionID+" 61 -1 "+slot);
					SendMTP("/Heal "+connectionID+" 2");
					if(!SC_invisibler.invisible) SendMTP("/EmitParticles "+connectionID+" 14 0 0");
					healBalance += float.Parse(SC_data.Gameplay[39]);
				}
				else HealSGP("2");
				SC_effect.EffectClean();
			}
			else InfoUp("Potion blocked",380);

			//Killing potion
			if(SC_slots.InvHaving(63)) if(AllowingPotion("killing"))
			{
				if(!SC_invisibler.invisible)
				{
					Transform trn11 = Instantiate(particlesEmptyBulb[4],transform.position,new Quaternion(0f,0f,0f,0f));
					trn11.GetComponent<SC_seeking>().enabled = true;
				}
				int slot = SC_slots.InvChange(63,-1,true,false,true);
				if((int)Communtron4.position.y==100) {
					SendMTP("/InventoryChange "+connectionID+" 63 -1 "+slot);
					if(!SC_invisibler.invisible) SendMTP("/EmitParticles "+connectionID+" 15 0 0");
				}
				DamageFLOAT(float.Parse(SC_data.Gameplay[35]));
				if(SC_artefacts.GetArtefactID()==6) power_V += at_unstable_regen3/50f;
			}
			else InfoUp("Potion blocked",380);

			//Max potion
			if(SC_slots.InvHaving(71)) if(AllowingPotion("max"))
			{
				if(!SC_invisibler.invisible)
				{
					Transform trn11 = Instantiate(particlesEmptyBulb[5],transform.position,new Quaternion(0f,0f,0f,0f));
					trn11.GetComponent<SC_seeking>().enabled = true;
				}
				int slot = SC_slots.InvChange(71,-1,true,false,true);
				if((int)Communtron4.position.y==100) {
					SendMTP("/InventoryChange "+connectionID+" 71 -1 "+slot);
					SendMTP("/Heal "+connectionID+" 3");
					if(!SC_invisibler.invisible) SendMTP("/EmitParticles "+connectionID+" 17 0 0");
					healBalance += 10000f;
				}
				else HealSGP("3");
				SC_effect.EffectClean();
				turbo_V = 1f;
				if(AllowingPotion("power-unlocked")) power_V = 1f;
			}
			else InfoUp("Potion blocked",380);
		}
		
		}
		if(power_V<0f) power_V=0f; if(power_V>1f) power_V=1f;
		
		//bar colors {1,2,3}	
		healthOld.color = HealthNormal;
		
		if(SC_artefacts.GetArtefactID()==2)
		if(!impulse_enabled) power.color=PowerNormal;
		else power.color=PowerBurning;
		
		if(SC_artefacts.GetArtefactID()==3)
		if(SC_invisibler.invisible) power.color=PowerBurning;
		else
		{
			if(power_V<IL_barrier) power.color=PowerBlocked;
			else power.color=PowerNormal;
		}

		if(SC_artefacts.GetArtefactID()==6)
			if(!SC_artefacts.unstabling) power.color=PowerNormal;
			else power.color=PowerBurning;
		
		if(turbo && !impulse_enabled) rocket_fuel.color=FuelBurning;
		else
		{
			if(turbo_V<F_barrier) rocket_fuel.color=FuelBlocked;
			else rocket_fuel.color=FuelNormal;
		}
		
		//cam pos
		camera.position=new Vector3(player.position.x,player.position.y,camera.position.z);


		if(!timeStop){
			
		//rotate player
		
		mmX=Input.mousePosition.x-Screen.width/2;
		mmY=Input.mousePosition.y-Screen.height/2;
		
		if(!impulse_enabled || impulse_reset)
		{
			mX=Input.mousePosition.x-Screen.width/2;
			mY=Input.mousePosition.y-Screen.height/2;
			impulse_reset = false;
		}

		if(mmX==0) mmX=1;
		if(mmY==0) mmY=1;
		
		if(mX==0) mX=1;
		if(mY==0) mY=1;
		
		float pom=Mathf.Atan(mY/mX)*57.296f;
		Quaternion quat_food=new Quaternion(0f,0f,0f,0f);
		if(mX<0) pom=pom+180f;
		
		quat_food.eulerAngles = new Vector3(0f,0f,pom);
		player.rotation = quat_food;
		if(ArtSource % 2 == 0) SC_artefacts.SC_seeking2.GetComponent<Transform>().rotation = quat_food;
		else SC_artefacts.SC_seeking2.GetComponent<Transform>().rotation = new Quaternion(0f,0f,0f,0f);
		
		quat_food.eulerAngles = new Vector3(90f-pom,90f,-90f);
		player_illusion.rotation = quat_food;
		
		SC_artefacts.SC_seeking.Update();
		SC_artefacts.SC_seeking2.Update();
		List<SC_seeking> scs = SC_lists.SC_seeking;
		foreach(SC_seeking sc in scs)
		{
			if(sc.idWord=="inv_part") sc.Update();
		}

		//BRAKE
		if(PressedNotInChat(KeyCode.LeftAlt,"hold")&&!pause)
		{
			brake=true;
			if(!turbo&&!engineON)
			{
				if(Communtron2.position.x==0)
				{F=V_to_F(float.Parse(SC_data.Gameplay[10]),false); enMode=3;}
				else
				{F=V_to_F(float.Parse(SC_data.Gameplay[13]),false); enMode=3;}
				//engine.material=E4;
			}
		}
		else
		{
			brake=false;
			//engine.material=E1;
		}
		//ENGINE
		if((PressedNotInChat(KeyCode.Space,"hold")||tempengine)&&living&&!pause)
		{
			engineON=true;
			if(!turbo)
			{
				if(Communtron2.position.x==0)
				{
					engineOFF=false;
					F=V_to_F(float.Parse(SC_data.Gameplay[9]),false);
					enMode=1;
				}
				else
				{
					engineOFF=true;
					F=V_to_F(float.Parse(SC_data.Gameplay[12]),false);
					enMode=1;
				}
				//engine.material=E2;
			}
		}
		else
		{
			engineOFF=false;
			engineON=false;
			//engine.material=E1;
		}
		//TURBO
		if(((PressedNotInChat(KeyCode.LeftShift,"hold")||tempturbo)&&turbo_V>0f)&&Communtron2.position.x==0&&living&&!pause)
		{
			if(turbo_V>=F_barrier)
			{
				turbo=true;
				F=V_to_F(float.Parse(SC_data.Gameplay[11]),true);
				enMode=2;
			}
		}
		else
		{
			turbo=false;
		}

		//CHECK MATERIAL
		int M;
		if(turbo||impulse_enabled)
		{
			//turbo
			engine.material=E3;
			M=2;
		}
		else if(engineON)
		{
			//normal
			engine.material=E2;
			M=1;
		}
		else if(brake)
		{
			//brake
			engine.material=E4;
			M=3;
		}
		else 
		{
			//off
			engine.material=E1;
			M=0;
		}
		
		//DRILL
		if(!SC_invisibler.invisible)
		{
			if(PressedNotInChat(KeyCode.R,"down")&&!pause)
			{
				if(drill3B) 
				{
					drill3B=false;
					Communtron2.position-=new Vector3(1f,0f,0f);
				}
				else
				{
					drill3B=true;
					Communtron2.position+=new Vector3(1f,0f,0f);
				}
			}
		}
		else
		{
			if(drill3B) 
			{
				drill3B=false;
				Communtron2.position-=new Vector3(1f,0f,0f);
			}
		}
		
		health_Text_update();
		
		}

		if(!living)
		{
			//update force dumb correction
			transform.position=solidPos;
			playerR.velocity=new Vector3(0f,0f,0f);
		}

		SC_projection.MuchLaterUpdate();

		if(Input.GetKeyDown(KeyCode.F1)) f1 = !f1;
		
		//Game pause
		if(Screen3.enabled) f1 = false;
		if(Input.GetKeyUp("escape"))
		{
			escaped = false;
		}
		if(Input.GetKeyDown("escape") && !SC_inv_mover.active && !SC_chat.typing && !blockEscapeThisFrame && !escaped)
			esc_press(true);

		blockEscapeThisFrame = false;

		//Restart lags
		if(truePing>2.5f)
		{
			Debug.LogWarning("Ping over 2.50s");
			MenuReturn();
		}

		SC_bars.MuchLaterUpdate();
	}
	public bool AllowingPotion(string potname)
	{
		bool power_unlocked = (SC_artefacts.GetArtefactID()==2 || SC_artefacts.GetArtefactID()==3 || SC_artefacts.GetArtefactID()==6);
		if(potname=="power-unlocked") return power_unlocked;

		if(impulse_enabled) return false;

		bool hB = HealBalanceGood();
		bool tB = turbo_V<0.95f;
		bool pB = power_V<0.95f && power_unlocked;
		bool eB = SC_effect.effect!=0;

		switch(potname)
		{
			case "healing": return hB;
			case "turbo": return tB;
			case "power": return pB;
			case "blank": return hB || eB;
			case "killing": return true;
			case "max": return hB || tB || pB || eB;
			default: return false;
		}
	}
	void KillMe()
	{
		SC_effect.EffectClean();
		solidPos=transform.position+new Vector3(0f,0f,2500f);
		Communtron1.position+=new Vector3(0f,0f,75f);
		if(!damage_sounds_disabled)
		{
			SC_sounds.PlaySound(transform.position,2,2);
			Instantiate(explosion,transform.position,transform.rotation);
		}
		living=false;

		List<SC_boss> boses = SC_lists.SC_boss;
		foreach(SC_boss bos in boses)
		{
			if(bos.bosnumed)
			{
				if((int)Communtron4.position.y!=100)
					bos.GiveUpSGP();
				else
					bos.GiveUpMTP(true);
			}
		}

		if((int)Communtron4.position.y==100)
		{
			livID=(int.Parse(livID)+1)+"";
			damageBalance = 0;
		}
		Debug.Log("Player died");
		Screen1.targetDisplay=1;
		Screen2.targetDisplay=1;
		Screen3.targetDisplay=0;
				
		SC_invisibler.invisible = false;
		RemoveImpulse();

		InLaterUpdateIfNotLiving();
	}
	void InLaterUpdateIfNotLiving()
	{
		if(living) return;

		int y;
		for(y=0;y<9;y++)
		{
			SC_slots.SlotX[y] = 0;
			SC_slots.SlotY[y] = 0;
		}
		for(y=0;y<21;y++)
		{
			SC_slots.BackpackX[y] = 0;
			SC_slots.BackpackY[y] = 0;
		}
		SC_slots.ResetYAB();
		for(y=0;y<5;y++) SC_upgrades.MTP_levels[y]=0;
		for(y=0;y<5;y++) SC_upgrades.UPG_levels[y]=0;

		transform.position=solidPos;
		playerR.velocity=new Vector3(0f,0f,0f);
	}
	void ImmortalMe()
	{
		SC_effect.EffectClean();
		health_V=1f;
		SC_slots.BackpackY[15]--; //sureMTP
		SC_slots.BackpackYA[15]--;
		SC_slots.BackpackYB[15]--;
		if(SC_slots.BackpackY[15]==0)
		{
			SC_slots.BackpackX[15] = 41;
			SC_slots.BackpackY[15]++;
			SC_slots.BackpackYA[15]++;
			SC_slots.BackpackYB[15]++;
		}
		if((int)Communtron4.position.y==100)
		{
			immID=(int.Parse(immID)+1)+"";
			damageBalance = 0;
		}
		if(!damage_sounds_disabled) Instantiate(ImmortalParticles,transform.position,transform.rotation);
		Debug.Log("Player avoided death");
	}
	public void esc_press(bool bo)
	{
		if(bo) escaped = true;
		pause = !pause;
		Screen3.enabled = pause;
		if((int)Communtron1.position.z == 0)
		{
			if(pause)
			{
				Screen1.targetDisplay = 1;
				Screen2.targetDisplay = 1;
			}
			else
			{
				Screen1.targetDisplay = 0;
				Screen2.targetDisplay = 0;
			}
		}
		if(pause && (int)Communtron4.position.y!=100) {Time.timeScale = 0f; timeStop = true;}
		else {Time.timeScale = 1f; timeStop = false;}
	}
	public void InvisiblityPulseSend()
	{
		if(SC_invisibler.invisible)
			flag_invisibility_pulse = true;
	}
	public void MainSaveData()
	{
		int z;
		for(z=0;z<5;z++)
		{
			SC_data.upgrades[z]=SC_upgrades.MTP_levels[z]+"";
		}
		for(z=0;z<21;z++)
		{
			SC_data.backpack[z,0]=SC_slots.BackpackX[z]+"";
			SC_data.backpack[z,1]=SC_slots.BackpackY[z]+"";
		}
		for(z=0;z<9;z++)
		{
			SC_data.inventory[z,0]=SC_slots.SlotX[z]+"";
			SC_data.inventory[z,1]=SC_slots.SlotY[z]+"";
		}

		if(health_V>0f)
		{
			SC_data.data[0]=(Mathf.Round(transform.position.x*10000f)/10000f)+"";
			SC_data.data[1]=(Mathf.Round(transform.position.y*10000f)/10000f)+"";
			SC_data.data[2]=(Mathf.Round(health_V*10000f)/10000f)+"";
			SC_data.data[3]=(Mathf.Round(turbo_V*10000f)/10000f)+"";
			SC_data.data[4]=(Mathf.Round(respawn_point.position.x*10000f)/10000f)+"";
			SC_data.data[5]=(Mathf.Round(respawn_point.position.y*10000f)/10000f)+"";
			SC_data.data[6]=timerH+"";
			SC_data.data[7]=(Mathf.Round(power_V*10000f)/10000f)+"";
		}
		else
		{
			SC_data.data[0]=(Mathf.Round(respawn_point.position.x*10000f)/10000f)+"";
			SC_data.data[1]=(Mathf.Round(respawn_point.position.y*10000f)/10000f)+"";
			SC_data.data[2]="1";
			SC_data.data[3]="0";
			SC_data.data[4]=(Mathf.Round(respawn_point.position.x*10000f)/10000f)+"";
			SC_data.data[5]=(Mathf.Round(respawn_point.position.y*10000f)/10000f)+"";
			SC_data.data[6]="0";
			SC_data.data[7]="0";
		}
		SC_data.Save("player_data");

		SC_data.UniverseX[worldID-1,0]=SC_camera.TotalTime+"";
		SC_data.Save("universeX");

		SC_data.Save("biomes");

		while(SC_data.ArchivedWorld.Count>0) SC_data.ArchiveSave(0);
		for(z=0;z<16;z++) SC_data.SaveAsteroid(z);
	}
	void OnApplicationQuit()
	{
		dont=true;
		MenuReturn();
	}
	public void MenuReturn()
	{
		if(repeted) return;
		repeted=true;

		damage_sounds_disabled = true;
		SC_effect.OneFrameDamage();

		List<SC_bullet> buls = SC_lists.SC_bullet;
		foreach(SC_bullet bul in buls)
		{
			if(bul.mode!="mother")
			{
				bul.block_graphics = true;
				bul.MakeDestroy(false);
			}
		}

		if(!dont)
		{
			if(Communtron4.position.y==100f) SC_data.TempFile="-2";
			else SC_data.TempFile="-1";
			SC_data.Save("temp");
		}

		Debug.Log("Quit");
		if((int)Communtron4.position.y==100)
		{
			try{
				ws.Close();
			}catch(Exception e){}
		}
		else if((int)Communtron4.position.y!=0) MainSaveData();
		if(!dont) SceneManager.LoadScene("MainMenu");
		repetedAF=true;
	}
	void health_Text_update()
	{
		float potH=SC_upgrades.MTP_levels[0]+SC_artefacts.GetProtLevelAdd()+float.Parse(SC_data.Gameplay[26]);
		if(potH<-50f) potH = -50f; if(potH>56.397f) potH = 56.397f;
		float maxH=50f*Mathf.Pow(health_base,potH) - 1f;
		//health_V=Mathf.Round(health_V*10000f)/10000f;
		float curH=Mathf.Round(health_V*10000f)/10000f*maxH;
		float maxHr=Mathf.Ceil(maxH);
		float curHr=Mathf.Ceil((curH*maxHr)/maxH);
		if(curHr == (curH*maxHr)/maxH) curHr=maxHr+1f;
		if(curHr <= 0) curHr = 1;
		health_Text.text="Health "+curHr+"/"+(maxHr+1f);

		float curFr=Mathf.Floor(turbo_V*50f);
		turbo_Text.text="Turbo "+curFr+"/50";
		
		float curPr=Mathf.Floor(power_V*50f);
		power_Text.text=SC_artefacts.bar3namets[SC_artefacts.GetArtefactID()]+" "+curPr+"/50";
	}
	public float Pitagoras(Vector3 pit)
	{
		return Mathf.Sqrt(pit.x*pit.x+pit.y*pit.y+pit.z*pit.z);
	}
	public void RemoveImpulse()
	{
		impulse_time = 0;
		impulse_enabled = false;
		playerR.velocity = new Vector3(0f,0f,0f);
		//SC_invisibler.invisible_or = false;
	}
	void FixedUpdate()
	{
		if(reg_wait>0) reg_wait--;
		if(cooldown>0) cooldown--;
		if(imp_cooldown>0) imp_cooldown--;
		if(licznikD>0) licznikD--;
		if(dmLicz>0) dmLicz--;
		if(saveCo>0) saveCo--;

		impulse_time--;
		if(impulse_time==1) RemoveImpulse();
		for(int ij=0;ij<max_players;ij++)
			if(ramvis[ij]>0) ramvis[ij]--;
		
		livTime++;

		//Main save data
		if((int)Communtron4.position.y!=100)
		if(saveCo==0)
		{
			saveCo=750; //15 seconds
			MainSaveData();
		}

		//Grow Loaded
		if((int)Communtron4.position.y==100)
		{
			if(MTPloadedCounter==400) { //every 8 seconds, but disappears after 10 seconds
				SendMTP("/GrowLoaded "+connectionID+" "+string.Join(";",SC_fun.GenListsB0)+";");
				MTPloadedCounter = 0;
			}
			MTPloadedCounter++;
		}

		//something engine
		float pX=0f,pY=0f;
		if(engineON||turbo||brake)
		{
			X=mX*F*SC_effect.GetSpeedFMultiplier()/Pitagoras(new Vector3(mX,mY,0f));
			Y=mY*F*SC_effect.GetSpeedFMultiplier()/Pitagoras(new Vector3(mX,mY,0f));
			pX+=X; pY+=Y;

			if(turbo&&turbo_V<=0f)
			{
				reg_wait=10;
				F=V_to_F(float.Parse(SC_data.Gameplay[9]),false);
				turbo=false;
				engine.material=E2;
			}
		}
		else enMode=0;

		//force generate
		float dX=0f,dY=0f,DragSize=float.Parse(SC_data.Gameplay[14]);
		
		dX-=0.001f*VacuumDrag*DragSize*SC_effect.GetVacuumMultiplier()*playerR.velocity.x*Mathf.Abs(playerR.velocity.x);
		dY-=0.001f*VacuumDrag*DragSize*SC_effect.GetVacuumMultiplier()*playerR.velocity.y*Mathf.Abs(playerR.velocity.y);
		dX-=0.015f*VacuumDrag*DragSize*SC_effect.GetVacuumMultiplier()*playerR.velocity.x;
		dY-=0.015f*VacuumDrag*DragSize*SC_effect.GetVacuumMultiplier()*playerR.velocity.y;

		if(Mathf.Abs(dX)>Mathf.Abs(playerR.velocity.x)) dX=playerR.velocity.x;
		if(Mathf.Abs(dY)>Mathf.Abs(playerR.velocity.y)) dY=playerR.velocity.y;
		
		if(!impulse_enabled) playerR.velocity+=new Vector3(dX+pX,dY+pY,0f);
		else
		{
			float FF = SC_artefacts.ImpulseSpeed;
			float Fx = FF*mX/Mathf.Sqrt(mX*mX+mY*mY);
			float Fy = FF*mY/Mathf.Sqrt(mX*mX+mY*mY);
			float Fmul; if(impulse_time-1 >= 5) Fmul = 1;
			else Fmul = (impulse_time-1) * 0.2f;
			playerR.velocity = new Vector3(Fx,Fy,0f) * Fmul * SC_effect.GetSpeedMultiplier();
		}

		//Red health reduce
		SC_bars.healthold_ui_bar.value=health_V;
		SC_bars.turbo_ui_bar.value=turbo_V;
		SC_bars.power_ui_bar.value=power_V;

		if(SC_bars.healthold_ui_bar.value<SC_bars.health_ui_bar.value)
		{
			if(licznikC<0)
			SC_bars.health_ui_bar.value-=0.0625f;
			licznikC--;
		}
		else
		{
			SC_bars.health_ui_bar.value=SC_bars.healthold_ui_bar.value;
			licznikC=10;
		}
		
		//unstability movement
		if(SC_artefacts.GetArtefactID()==6 && livTime>=50 && (intPing!=-1 || (int)Communtron4.position.y!=100))
		{
			if(UnityEngine.Random.Range(0,unstable_probability)==0)
			{
				float alp = UnityEngine.Random.Range(0,360);
				float ux = Mathf.Cos((alp*3.14159f)/180f);
				float uy = Mathf.Sin((alp*3.14159f)/180f);
				
				playerR.velocity += new Vector3(ux*unstable_force,uy*unstable_force,0f);
				Quaternion quat_foo = new Quaternion(0f,0f,0f,0f);
				quat_foo.eulerAngles = new Vector3(0f,0f,alp+90f);
				Transform trn = Instantiate(unstablePart,transform.position,quat_foo);
				trn.GetComponent<SC_seeking>().enabled = true;
				SendMTP("/EmitParticles "+connectionID+" 7 "+alp+" 0");

				power_V += at_unstable_regen1/50f;
			}
			if(UnityEngine.Random.Range(0,unstable_sprobability)==0)
			{
				bool wr_tick = (int)Communtron4.position.y!=100 || current_tick!=-1;

				float alp = UnityEngine.Random.Range(0,360);
				float ux = Mathf.Cos((alp*3.14159f)/180f);
				float uy = Mathf.Sin((alp*3.14159f)/180f);
				
				if(SC_effect.effect!=8 && wr_tick) SC_bullet.Shot(
					transform.position,
					new Vector3(ux,uy,0f),
					playerR.velocity*0.02f,
					3,
					0
				);
				
				Quaternion quat_foo = new Quaternion(0f,0f,0f,0f);
				quat_foo.eulerAngles = new Vector3(0f,0f,alp+90f);
				Transform trn = Instantiate(unstablePart,transform.position,quat_foo);
				trn.GetComponent<SC_seeking>().enabled = true;
				SendMTP("/EmitParticles "+connectionID+" 7 "+alp+" 0");

				power_V += at_unstable_regen2/50f;
			}
		}

		//health regeneration
		if(health_V<1f&&timerH==0 && (int)Communtron4.position.y!=100)
		{
			float potHH = SC_upgrades.MTP_levels[0]+SC_artefacts.GetProtLevelAdd()+float.Parse(SC_data.Gameplay[26]);
			if(potHH<-50f) potHH = -50f; if(potHH>56.397f) potHH = 56.397f;
			float true_add = unit*SC_artefacts.GetProtRegenMultiplier()*float.Parse(SC_data.Gameplay[5])/(Mathf.Ceil(50*Mathf.Pow(health_base,potHH))/50f);
			if(true_add>0) health_V += true_add;
		}
		//timer H
		if(timerH>0)
		{
			if(SC_artefacts.GetArtefactID() != 1) timerH--;
			else timerH-=2;
			if(timerH<0) timerH=0;
		}
		//turbo regeneration
		if(!turbo && !impulse_enabled)
		{
			turbo_V+=unit*float.Parse(SC_data.Gameplay[0]);
		}
		//power regeneration
		int nn = SC_artefacts.GetArtefactID();
		if(
			(nn==2 && !impulse_enabled)||
			(nn==3 && !SC_invisibler.invisible)||
			(nn==5 && false)||
			(nn==6 && !SC_artefacts.unstabling)
			
		) power_V += unit * SC_artefacts.powerRM[nn];
		
		//turbo eat
		if(turbo && !impulse_enabled)
		{
			turbo_V-=unit*float.Parse(SC_data.Gameplay[1]);
		}
		
		//power eat
		int mm = SC_artefacts.GetArtefactID();
		if(
			(mm==2 && false)||
			(mm==3 && SC_invisibler.invisible)||
			(nn==5 && false)||
			(mm==6 && SC_artefacts.unstabling)
			
		) power_V -= unit * SC_artefacts.powerUM[mm];
		
		//invisibility stop
		if(power_V<=0f && SC_invisibler.invisible) SC_invisibler.invisible = false;
		
		//reducing bars
		if(health_V>1f) health_V=1f;
		if(turbo_V>1f) turbo_V=1f;
		if(turbo_V<0f) turbo_V=0f;
		if(power_V>1f) power_V=1f;
		if(power_V<0f) power_V=0f;

		//Cmd list activator
		Queue tsList = Queue.Synchronized(cmdList);
		while(tsList.Count > 0)
			cmdDo(tsList.Dequeue()+"");

		List<SC_bullet> buls = SC_lists.SC_bullet;
		foreach(SC_bullet bul in buls)
		{
			bul.AfterFixedUpdate();
		}

		//RPU converter
		int h;
		if(Communtron4.position.y==100f && RPU!="XXX")
			TranslateRPU();

		//drill fixed update
		if(drill3B&&drill3T.localPosition.y<1.44f)
		{
			drill3T.localPosition+=new Vector3(0f,0.05f,0f);
		}
		if(!drill3B&&drill3T.localPosition.y>0.46f)
		{
			drill3T.localPosition-=new Vector3(0f,0.05f,0f);
		}
		
		if(SC_invisibler.invisible) drill3T.localPosition = new Vector3(drill3T.localPosition.x,0.45f,drill3T.localPosition.z);
		if(drill3T.localPosition.y>=1.44f)
		{
			Communtron1.localScale=new Vector3(Communtron1.localScale.x,Communtron1.localScale.y,2f);
		}
		else
		{
			Communtron1.localScale=new Vector3(Communtron1.localScale.x,Communtron1.localScale.y,1f);
		}

		//Websocket sends
		if((int)Communtron4.position.y==100)
		{
			float trX,trY,rpX,rpY,heB,fuB,poB,mPPx,mPPy;
			string isImpulse = "F"; if(impulse_enabled) isImpulse = "T";
			string isImpulseStart = "F"; if(flag_impulse_start) { isImpulseStart = "T"; flag_impulse_start = false; }
			string isInviPulse = "F"; if(flag_invisibility_pulse) { isInviPulse = "T"; flag_invisibility_pulse = false; }
			trX=Mathf.Round(transform.position.x*10000f)/10000f;
			trY=Mathf.Round(transform.position.y*10000f)/10000f;
			rpX=Mathf.Round(respawn_point.position.x*10000f)/10000f;
			rpY=Mathf.Round(respawn_point.position.y*10000f)/10000f;
			heB=Mathf.Round(health_V*10000f)/10000f;
			fuB=Mathf.Round(turbo_V*10000f)/10000f;
			poB=Mathf.Round(power_V*10000f)/10000f;
			int sendOther=enMode*16+(int)Communtron5.position.x*4+(int)Communtron2.position.x*2+(int)CommuntronM1.transform.position.x*1;
			int compressedEffect = 0;
			if(SC_effect.effect==5) compressedEffect = 1;
			if(SC_effect.effect==6) compressedEffect = 2;
			if(SC_effect.effect==8) compressedEffect = 3;
			int sendOtherParasite=ArtSource + 25*compressedEffect;
			
			if(living){
				/*
				0/0 - posX
				1/1 - posY
				2[] - ()
				3[] - ()
				4[] - rotation
				5[] - others2
				6/4 - respX
				7/5 - respY
				8/2 - () healthBar
				9/3 - fuelBar
				10/6 - () timerH
				11/7 - powerBar
					*/
				SendMTP(
					"/PlayerUpdate "+connectionID+" "+

					trX+";"+trY+";;;"+transform.rotation.eulerAngles.z+";"+
					sendOther+"&"+sendOtherParasite+";"+
					rpX+";"+rpY+";;"+
					fuB+";;"+poB+" "+
					
					localPing+" "+immID+" "+isImpulse + isImpulseStart + isInviPulse
				);
			}
			else SendMTP("/PlayerUpdate "+connectionID+" 1 "+localPing+" "+immID+" FFF");
		}

		localPing++;
		
		SC_projection.AfterFixedUpdate();
		for(int ij=1;ij<max_players;ij++)
			PL[ij].AfterFixedUpdate();
		
		if(!Input.GetMouseButton(1)) public_placed = false;
		gtm1 = false;
	}
	void ConvertBRP(string brp)
	{
		int i,j,lngt=brp.Length;
		for(i=0;i<lngt;i+=2)
		{
			int[] b1 = Char1ToBool6(brp,i);
			int[] b2 = Char1ToBool6(brp,i+1);
			int bul_id = 0; int mn=1;
			for(j=3;j>=0;j--) {
				bul_id+=mn*b2[j];
				mn*=2;
			}
			for(j=5;j>=0;j--) {
				bul_id+=mn*b1[j];
				mn*=2;
			}
			char N = '?';
			if(b2[4]==0 && b2[5]==1) N='L';
			if(b2[4]==1 && b2[5]==0) N='P';
			if(b2[4]==1 && b2[5]==1) N='0';
			SC_seek_data.steerData[bul_id]+=N;
		}
	}
	void TranslateRPU()
	{
		string[] arg = RPU.Split(' ');
		int i,lngt = int.Parse(arg[1]);
		string[] arh = splitChars(arg[2],lngt);
		current_tick = int.Parse(arg[3]);

		for(i=1;i<lngt;i++)
		{
			// i -> Projection ID
			// k -> Place in msg

			int k=i; if(i==connectionID) k=0;
			int atf=0;
			string ari = arh[k];

			if(true)
			{
				if(arh[k]=="0")
				{
					//Player is not joined
					NUL[i] = false;
					PL[i].sourcedPosition = new Vector3(0f,0f,300f); //Position
					PL[i].sourcedRotation = 10000f; //Rotation

					NCT[i].text = "";
					NCH[i].value = 1;
					RU[i].position = new Vector3(0f,0f,300f);
						atf = 0;
					PL[i].ArtSource = atf;
					PL[i].OtherSource = 0;
				}
				else if(arh[k]=="1")
				{
					//Player is joined but not living
					NUL[i] = false;
					PL[i].sourcedPosition = new Vector3(0f,0f,300f); //Position
					PL[i].sourcedRotation = 10000f; //Rotation

					NCT[i].text = nick_RPC[i];
					NCH[i].value = 1;
						float zzz = 1f;
						if(rposX_RPC[i]==0f && rposY_RPC[i]==0f) zzz=300f;
					RU[i].position = new Vector3(rposX_RPC[i],rposY_RPC[i],zzz);
						atf = 0;
					PL[i].ArtSource = atf;
					PL[i].OtherSource = 0;
				}
				else
				{
					//Player is playing normally
					NUL[i] = true;
					PL[i].sourcedPosition = new Vector3(Char4ToFloat(ari,0),Char4ToFloat(ari,4),0f); //Position
					PL[i].sourcedRotation = Char1ToRot(ari,8); //Rotation

					NCT[i].text = nick_RPC[i];
						float liv = health_RPC[i];
						if(liv<0f) liv = 0f; //relict
					NCH[i].value = liv;
						float zzz = 1f;
						if(rposX_RPC[i]==0f && rposY_RPC[i]==0f) zzz=300f;
					RU[i].position = new Vector3(rposX_RPC[i],rposY_RPC[i],zzz);
						atf = others2_RPC[i];
					PL[i].ArtSource = atf;
					PL[i].OtherSource = others1_RPC[i];
				}
					
				//Based on <atf> value
				NC[i].enabled = !f1 && (atf%25!=1);
				NCHOF[i].color = SC_artefacts.Color1N[atf/100];
			}
		}

		fixup--;
		if(fixup<=0)
		{
			intPing = (localPing-returnedPing);
			truePing = intPing/50f;
			trping = retping(truePing);
			pingname.text = "Ping: "+trping+"s";
			fixup = 10;
		}
	}
	public void SendMTP(string msg)
	{
		if(Communtron4.position.y==100)
		{
			if(repetedAF) return;

			msg = msg+" "+livID;

			try {
				ws.Send(msg);
			} catch {
				Debug.LogWarning("Failed sending message: "+msg);
				MenuReturn();
			}
		}
	}
	string[] splitChars(string str, int amount)
	{
		string[] effect = new string[amount];
		int i,n=0,lngt=str.Length;
		for(i=0;i<lngt;i++)
		{
			if(str[i]=='!') effect[n] = "0";
			else if(str[i]=='\"') effect[n] = "1";
			else {
				effect[n] = str[i]+""+str[i+1]+""+str[i+2]+""+str[i+3]+""+str[i+4]+""+str[i+5]+""+str[i+6]+""+str[i+7]+""+str[i+8];
				i+=8;
			}
			n++;
		}
		return effect;
	}
	string retping(float pig)
	{
		int inpg=(int)(pig*100);
		if(inpg%100==0) return pig+",00";
		if(inpg%10==0) return pig+"0";
		return pig+"";
	}
	public void DamageFLOAT(float dmgFLOAT) {if(dmgFLOAT>0f) Damage(dmgFLOAT);}
	public void Damage(float dmg)
	{
		if(!living) return;
		if(livTime<50 || impulse_enabled || !((livID==sr_livID && immID==sr_immID) || (int)Communtron4.position.y!=100)) return;
		float potHHH = SC_upgrades.MTP_levels[0]+SC_artefacts.GetProtLevelAdd()+float.Parse(SC_data.Gameplay[26]);
		if(potHHH<-50f) potHHH = -50f; if(potHHH>56.397f) potHHH = 56.397f;
		dmg=0.02f*dmg/(Mathf.Ceil(50*Mathf.Pow(health_base,potHHH))/50f);
		CookedDamage(dmg);

		//KILL BY CLIENT
		string info = "d";
		if(health_V<=0f && living)
		{
			if(SC_artefacts.GetArtefactID() != 4) info = "K";
			else info = "I";
		}
		
		if((int)Communtron4.position.y==100) {
			damageBalance -= dmg;
			SendMTP("/ClientDamage "+connectionID+" "+dmg+" "+immID+" "+info);
		}

		if(info=="K") KillMe();
		if(info=="I") ImmortalMe();
	}
	public void CookedDamage(float dmg)
	{
		health_V-=dmg;
		if(Mathf.Round(health_V*10000f) == 0f) health_V = 0f;
		timerH=(int)(50f*float.Parse(SC_data.Gameplay[4]));
		if(!damage_sounds_disabled) Instantiate(explosion2,transform.position,transform.rotation);
	}
	public void HealSGP(string arg2)
	{
		string heal_size = "0";
		if(arg2=="1") heal_size = SC_data.Gameplay[31];
		if(arg2=="2") heal_size = SC_data.Gameplay[39];
		if(arg2=="3") heal_size = "10000";

		float potHHH = SC_upgrades.MTP_levels[0]+SC_artefacts.GetProtLevelAdd()+float.Parse(SC_data.Gameplay[26]);
		if(potHHH<-50f) potHHH = -50f; if(potHHH>56.397f) potHHH = 56.397f;
		float heal=0.02f*float.Parse(heal_size)/(Mathf.Ceil(50*Mathf.Pow(health_base,potHHH))/50f);

		if(heal<0) heal=0;
		health_V += heal;
		if(health_V>1f) health_V=1f;
	}
	public Vector3 Skop(float F, Vector3 vec3)
	{
		return SC_fun.Skop(vec3,F);
	}
	bool AllowCollisionDamage()
	{
		SC_adecodron[] SC_adecodron2 = FindObjectsOfType<SC_adecodron>();
		foreach(SC_adecodron adc in SC_adecodron2)
		{
			if(adc.cooldown!=0) return false;
		}
		return true;
	}
	void OnCollisionEnter(Collision collision)
    {
		float CME=float.Parse(SC_data.Gameplay[6]); 

		if(collision.gameObject.name!="mini_crown" && collision.gameObject.name!="aBossScaled0")
		{
			if(dmLicz<=0)
       		if(collision.impulse.magnitude>CME&&collision.relativeVelocity.magnitude>CME)
			{
				dmLicz=20;
				float head_ache = head_ache=collision.impulse.magnitude-CME + 3f;
				float hai=float.Parse(SC_data.Gameplay[7])*head_ache*1.2f;
				if(AllowCollisionDamage()) DamageFLOAT(hai);
			}
		}
    }
	public bool IsAnyBossFight(string rngvs)
	{
		foreach(SC_boss bos in SC_lists.SC_boss) {
			if(bos.InArena(rngvs)) return true;
		}
		return false;
	}
	float max_framal_damage = 0f;
	void OnTriggerStay(Collider collision)
	{
		string neme = collision.gameObject.name;
		if(neme=="damager2"||
		neme=="star_collider_big"||
		neme=="star_collider"||
		neme=="lava_wind"||
		(neme=="S-fire" && IsAnyBossFight("range"))||
		(neme=="damager3" && SC_artefacts.GetArtefactID()!=6))
		{
			if(licznikD==0)
			{
				float dmgg = 0f;
				if(neme=="damager3") 				dmgg = float.Parse(SC_data.Gameplay[28]); //unstable matter
				else if(neme=="star_collider") 		dmgg = float.Parse(SC_data.Gameplay[34]); //fire bullet
				else if(neme=="star_collider_big") 	dmgg = SC_data.GplGet("star_collider_damage"); //stars
				else if(neme=="S-fire") 			dmgg = SC_data.GplGet("boss_starandus_geyzer_damage") * float.Parse(SC_data.Gameplay[32]); //S-fire
				else if(neme=="lava_wind") 			dmgg = SC_data.GplGet("lava_geyzer_damage"); //lava geyzer
				else 								dmgg = float.Parse(SC_data.Gameplay[8]); //spikes

				if(neme=="star_collider") 			SC_effect.SetEffect(5,(int)SC_data.GplGet("cyclic_fire_time"));
				else if(neme=="star_collider_big") 	SC_effect.SetEffect(5,(int)SC_data.GplGet("cyclic_star_time"));
				else if(neme=="S-fire") 			SC_effect.SetEffect(5,(int)SC_data.GplGet("cyclic_starandus_geyzer_time"));
				
				if(dmgg > max_framal_damage) max_framal_damage = dmgg;
			}
		}
	}
	void LateUpdate()
	{
		if(max_framal_damage!=0f) {
			DamageFLOAT(max_framal_damage);
			max_framal_damage = 0f;
			licznikD=25;
		}
	}
	public void InfoUp(string info, int tim)
	{
		SC_slots.inv_full_text2.text = info;
		SC_slots.comm_time2 = tim;
	}
	string info_space_add(string s)
	{
		int i, lngt = s.Length;
		string effect = "";

		for(i=0;i<lngt;i++) if(s[i]!='`') effect += s[i];
		else effect += ' ';
		
		return effect;
	}
	void Ws_OnMessage(object sender, MessageEventArgs e)
    {
		Queue tsList = Queue.Synchronized(cmdList);
		tsList.Enqueue(e.Data);
    }
	void cmdDo(string cmdThis)
	{
		if(cmdThis==null || cmdThis=="")
		{
			Debug.LogWarning("Null command detected: "+(cmdThis==null));
			return;
		}
		string[] arg = cmdThis.Split(' ');

		//Fast commands

		if((arg[0])[0]=='P')
		{
			//Short ping return message
			string pgg = arg[0],pggn="";
			int i,lngt=pgg.Length;
			for(i=1;i<lngt;i++) pggn+=pgg[i];
			int new_returnedPing = int.Parse(pggn);
			if(new_returnedPing < returnedPing && new_returnedPing!=0) Debug.LogError("Error: Wrong message order confirmed: "+(new_returnedPing-returnedPing)+" : "+new_returnedPing+" : "+returnedPing);
			returnedPing = new_returnedPing;
			
			return;
		}
		if(arg[0]=="R")
		{
			//Medium health regenerate message
			float fValue = float.Parse(arg[1]);
			string fImmID = arg[2];
			string fLivID = arg[3];
			float fValueAbsolute = float.Parse(arg[4]);
			//Debug.Log(fValueAbsolute+"|"+arg[4]+" >---< "+fValue+"|"+arg[1]);

			if(immID==fImmID && livID==fLivID) {
				if(!absolute_health_sync) health_V += fValue;
				else health_V = fValueAbsolute + damageBalance;
			}

			return;
		}
		if(arg[0]=="I")
		{
			//Medium livID & immID update
			sr_livID = arg[2];
			sr_immID = arg[1];

			return;
		}
		if(arg[0]=="/RetHeal" && arg[1]==connectionID+"") {
			if(arg[2]=="1") healBalance -= float.Parse(SC_data.Gameplay[31]);
			if(arg[2]=="2") healBalance -= float.Parse(SC_data.Gameplay[39]);
			if(arg[2]=="3") healBalance -= 10000;
		}

		int msl=arg.Length;
		int blo=0;
		if(arg[msl-2]!=conID&&arg[msl-2]!="X") blo=1;
		if(arg[msl-1]!=livID&&arg[msl-1]!="X") blo=2;
		if(blo>0) return;

		if(arg[0]=="/RPU" || arg[0]==".RPU")
		{
			//RPU big variable
			RPU=cmdThis;
			if(arg[0][0]=='/') show_positions = true;
			else show_positions = false;
			return;
		}

		//Old commands

		if(arg[0]=="/RPC")
		{
			//RPC small variables
			int size = int.Parse(arg[1]);
			string str = arg[2];

			int i,N=0,I,lngt = str.Length;
			for(i=0;i<lngt;i++)
			{
				if(N==connectionID) I=0;
				else if(N==0) I=connectionID;
				else I=N;

				if(str[i]=='!') {
					//Next mode
					N++;
				}
				else if(str[i]=='\"') {
					//Jump over X mode
					i++;
					int in1 = RASCII_toInt(str[i+0]);
					int in2 = RASCII_toInt(str[i+1]);
					N += in1*124 + in2;
					i++;
				}
				else
				{
					//Header mode
					int[] array_what = Char1ToBool6(str,i+0);
					if(array_what[0]==1) //Others
					{
						i++;
						others1_RPC[I] = RASCII_toInt(str[i+0]);
						int in1 = RASCII_toInt(str[i+1]);
						int in2 = RASCII_toInt(str[i+2]);
						others2_RPC[I] = in1*124 + in2;
						i+=2;
					}
					if(array_what[1]==1) //Health
					{
						i++;
						health_RPC[I] = Char1ToHealth(str,i+0);
					}
					if(array_what[2]==1) //Resp pos
					{
						i++;
						rposX_RPC[I] = Char4ToFloat(str,i+0);
						rposY_RPC[I] = Char4ToFloat(str,i+4);
						i+=7;
					}
					if(array_what[3]==1) //Nick
					{
						i++;
						string new_nick = "";
						while(str[i]!=':') {
							if(str[i]=='|') new_nick += " ";
							else new_nick += str[i];
							i++;
						}
						if(I==0) nick_RPC[I] = "You";
						else if(new_nick!="0") nick_RPC[I] = new_nick;
						else nick_RPC[I] = "";
					}
					N++;
				}
			}

			return;
		}
		if(arg[0]=="/BRP")
		{
			ConvertBRP(arg[1]);
		}
		if(arg[0]=="/RetDamageUsing")
		{
			int effectID = int.Parse(arg[1]);
			SC_effect.SetEffect(effectID,SC_fun.bullet_effector[effectID]);
			if(effectID==16) GravitonCatch();
		}
		if(arg[0]=="/RetServerDamage")
		{
			//RetServerDamage 1[playerID] 2[dmg] 3[immID] 4[livID] 5[info]
			if(arg[1]==connectionID+"")
			{
				string cis="";
				if(livID==arg[4]) cis+="T"; else cis+="F";
				if(immID==arg[3]) cis+="T"; else cis+="F";

				if(cis=="TT")
					CookedDamage(float.Parse(arg[2]));

				if(arg[5]=="K") //server kill
      			{
			        switch(cis)
			        {
			        	case "TT": KillMe(); break;
			        	case "TF": MenuReturn(); return;
			        	case "FT": break;
			        	case "FF": MenuReturn(); return;
			        }
			      }
			      if(arg[5]=="I") //server immortal
			      {
        			switch(cis)
        			{
          				case "TT": ImmortalMe(); break;
          				case "TF": break;
          				case "FT": MenuReturn(); return;
          				case "FF": MenuReturn(); return;
        			}
      			}
			}
		}
		if(arg[0]=="/RetDamageBalance")
		{
			//RetDamageBalance 1[damage] 2[livID] 3[immID]
			if(livID==arg[2] && immID==arg[3])
				damageBalance += float.Parse(arg[1]);
		}
		if(arg[0]=="/RetUpgrade")
		{
			if(arg[1]==connectionID+"") SC_upgrades.MTP_levels[int.Parse(arg[2])]++;
		}
		if(arg[0]=="/RetNewBulletSend")
		{
			SC_bullet bul1 = SC_bullet.ShotProjection(
				new Vector3(float.Parse(arg[3]),float.Parse(arg[4]),0f),
				new Vector3(float.Parse(arg[5]),float.Parse(arg[6]),0f),
				int.Parse(arg[2]),
				"server",
				int.Parse(arg[7]),
				arg[1]
			);
			bul1.InstantMove(int.Parse(arg[8]));

			if(connectionID+""==arg[1])
			{
				List<SC_bullet> buls = SC_lists.SC_bullet;
				foreach(SC_bullet bul in buls)
				{
					if(bul.ID+""==arg[7] && bul.mode=="projection")
						bul.delta_age = -bul.age - (4/2);
				}
			}

			/*if(connectionID+""!=arg[1]){
				SC_bullet bul2 = SC_bullet.ShotProjection(
					new Vector3(float.Parse(arg[3]),float.Parse(arg[4]),0f),
					new Vector3(float.Parse(arg[5]),float.Parse(arg[6]),0f),
					int.Parse(arg[2]),
					"present",
					int.Parse(arg[7])
				);
				bul2.InstantMove(intPing);
			}*/

			if(connectionID+""!=arg[1]){
				SC_bullet bul3 = SC_bullet.ShotProjection(
					new Vector3(float.Parse(arg[3]),float.Parse(arg[4]),0f),
					new Vector3(float.Parse(arg[5]),float.Parse(arg[6]),0f),
					int.Parse(arg[2]),
					"projection",
					int.Parse(arg[7]),
					arg[1]
				);
				bul3.InstantMove(int.Parse(arg[8]));
				//Future bullets make no sense, change my mind
				//bul3.delta_age = (4/2);
			}
		}
		if(arg[0]=="/RetNewBulletDestroy")
		{
			List<SC_bullet> buls = SC_lists.SC_bullet;
			foreach(SC_bullet bul in buls)
			{
				if(bul.mode!="mother" && bul.ID+""==arg[2])
				{
					bul.max_age = int.Parse(arg[3]);
					bul.destroy_mode = arg[4];
					if(arg[4]=="false") bul.block_graphics = true;
					bul.CheckAge();
				}
			}
		}
		if(arg[0]=="/RetInvisibilityPulse")
		{
			if(connectionID+"" != arg[1])
			{
				int cid = int.Parse(arg[1]);
				if(arg[2]!="wait")
				{
					ramvis[cid] = timeInvisiblePulse;
				}
				else
				{
					if(ramvis[cid]==0) ramvis[cid] = timeInvisiblePulse + 3;
					else if(ramvis[cid]<=timeInvisiblePulse) ramvis[cid] = timeInvisiblePulse;
				}
			}
		}
		if(arg[0]=="/RetSeekData")
		{
			int seek_id = int.Parse(arg[1]);
			int bul_id = int.Parse(arg[2]);
			SC_seek_data.bulletID[seek_id] = bul_id;
			SC_seek_data.steerData[seek_id] = "0000";
		}
		if(arg[0]=="/RetEmitParticles" && arg[1]!=connectionID+"")
		{
			try {
			Vector3 particlePos;
			Quaternion quat_foo = new Quaternion(0f,0f,0f,0f);
			int put = int.Parse(arg[2]);
			int pid = int.Parse(arg[1]);
			if(pid==0) pid = connectionID;
			
			if((put>=6 && put<=8) || (put>=11 && put<=15) || put==17)
			{
				particlePos = PL[pid].GetComponent<Transform>().position;
				if(put==7)
				{
					quat_foo.eulerAngles = new Vector3(0f,0f,90f+float.Parse(arg[3]));
				}
			}
			else particlePos = new Vector3(float.Parse(arg[3]),float.Parse(arg[4]),0f);
		
			switch(put)
			{
				case 1:
					SC_sounds.PlaySound(particlePos,2,2);
					Instantiate(explosionM,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 2:
					Instantiate(explosion2M,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 3: //respawn set
					Instantiate(respawn2M,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 4:
					Instantiate(receiveParticles,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 5: //immortality used
					Instantiate(ImmortalParticles,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 6:
					Instantiate(InvisiPart,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 7: //unstable small pulse
					Transform trn7 = Instantiate(unstablePart,particlePos,quat_foo);
					trn7.GetComponent<SC_seeking>().seek = PL[pid].GetComponent<Transform>();
					trn7.GetComponent<SC_seeking>().enabled = true;
					break;
				case 8: //impulse (hidden)
					Transform trn8 = Instantiate(impulseHidden,particlePos,quat_foo);
					trn8.GetComponent<SC_seeking>().seek = PL[pid].GetComponent<Transform>();
					trn8.GetComponent<SC_seeking>().enabled = true;
					break;
				case 9: //boss damage
					Instantiate(particlesBossDamageM,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 10: //boss explosion
					Instantiate(particlesBossExplosionM,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 16: //telep (hidden)
					Instantiate(TelepParticles,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				default:
					if((put>=11&&put<=15)||put==17)
					{
						int put2 = put;
						if(put==17) put2=16;
						Transform trn11 = Instantiate(particlesEmptyBulb[put2-11],particlePos,new Quaternion(0f,0f,0f,0f));
						trn11.GetComponent<SC_seeking>().seek = PL[pid].GetComponent<Transform>();
						trn11.GetComponent<SC_seeking>().enabled = true;
						break;
					}
					else throw(new Exception());
					break;
			}

			}catch(Exception) {
				Debug.LogWarning("Player with ID "+arg[1]+" sends wrong particle packets. It might be a cheater, but also an error...");
			}
		}
		if(arg[0]=="/RetInventory")
		{
			//RetInventory [connectionID] [item] [deltaCount] [slot]
			if(arg[1]==connectionID+"") SC_slots.InvCorrectionMTP(int.Parse(arg[2]),int.Parse(arg[3]),int.Parse(arg[4]),int.Parse(arg[5]));
		}

		//Other scripts
		if(arg[0]=="/RetInfoClient")
		{
			if(arg[2]==connectionID+"") return;
			InfoUp(info_space_add(arg[1]),500);
		}
		if(arg[0]=="/RetChatMessage")
		{
			if(arg[1]==connectionID+"") return;
			SC_chat.AddMessage(arg[1],new StringBuilder(arg[2]).Replace("\t"," ").ToString());
		}
		if(arg[0]=="/RetAsteroidData")
		{
			//REBUILD IT
			List<SC_asteroid> SCT_asteroid = SC_lists.SC_asteroid;
			foreach(SC_asteroid aul in SCT_asteroid)
			{
				aul.onMSG(cmdThis);
			}
		}
		if(arg[0]=="/RSD"||
		arg[0]=="/RetGiveUpTeleport")
		{
			if(arg[3]=="1024")
			{
				//REBUILD IT
				List<SC_boss> SCT_boss = SC_lists.SC_boss;
				foreach(SC_boss aul in SCT_boss)
				{
					aul.onMSG(cmdThis);
				}
			}
		}
		if(arg[0]=="/RetFobsTurn"||
		arg[0]=="/RetFobsPing"||
		arg[0]=="/RetGeyzerTurn"||
		arg[0]=="/RetGrowNow")
		{
			//Traditional fobs directing
			List<SC_fobs> SCT_fobs = SC_lists.SC_fobs;
			foreach(SC_fobs ful in SCT_fobs)
			{
				if(ful.onMSG(cmdThis)) break;
			}
		}
		if(arg[0]=="/RetFobsChange"||
		arg[0]=="/RetFobsDataChange"||
		arg[0]=="/RetFobsDataCorrection")
		{
			//Fast fobs directing
			List<SC_asteroid> SCT_asteroid = SC_lists.SC_asteroid;
			foreach(SC_asteroid aul in SCT_asteroid)
			{
				bool break_now = false;
				if(aul.ID+""==arg[1])
				foreach(Transform trn in aul.GetComponent<Transform>())
				{
					SC_fobs ful = trn.GetComponent<SC_fobs>();
					if(ful!=null) if(ful.onMSG(cmdThis)) {
						break_now = true;
						break;
					}
				}
				if(break_now) break;
			}
		}
	}
	void Ws_OnOpen(object sender, System.EventArgs e)
    {
		SendMTP("/ImJoined "+connectionID+" "+conID);
    }
	void Ws_OnClose(object sender, System.EventArgs e)
    {
		Debug.Log("Connection E-close");
	}
	void Ws_OnError(object sender, System.EventArgs e)
    {
		Debug.Log("Connection E-error");
	}
	void MTP_InventoryLoad()
	{
		int i;
		int[] invdata = new int[18];

		for(i=0;i<18;i++) invdata[i]=int.Parse(getInventory.Split(';')[i]);
		for(i=0;i<9;i++)
		{
			SC_slots.SlotX[i] = invdata[i*2];
			SC_slots.SlotY[i] = invdata[i*2+1];
		}
	}
	public void LoadSomeGameplayData()
	{
		SC_gameplay_set.GameplayAwakeSet();
	}
	public void AfterAwake()
	{
		int j;
		for(j=0;j<9;j++)
			betterInvConverted[j]=new Vector3(0f,0f,0f);
		
		Screen1.targetDisplay=0;
		Screen2.targetDisplay=0;
		Screen3.enabled = false;

		worldID=(int)Communtron4.position.y;
		worldDIR=SC_data.savesDIR+"UniverseData"+worldID+"/";

		if((int)Communtron4.position.y==100)
		{
			connectionID=int.Parse(SC_data.TempFileConID[0]);
			CommuntronM1.name=SC_data.TempFileConID[1];
			nick=SC_data.TempFileConID[2];
			getData=SC_data.TempFileConID[3];
			getInventory=SC_data.TempFileConID[4];
			max_players=int.Parse(SC_data.TempFileConID[5]);
			SC_upgrades.MTP_loadUpgrades(SC_data.TempFileConID[6]);
			SC_backpack.MTP_loadBackpack(SC_data.TempFileConID[7]);
			SC_data.DatapackMultiplayerLoad(SC_data.TempFileConID[9]);
			conID=SC_data.TempFileConID[10];
			MTP_InventoryLoad();
			TextConstYou.text = nick;

			string[] rw = SC_data.TempFileConID[8].Split('&');
			if(rw.Length>=2)
			{
				string[] tabb = rw[1].Split('?');
				if(tabb.Length>=16000)
				{
					int ri;
					for(ri=0;ri<16000;ri++) SC_data.biome_memories[ri] = tabb[ri];
				}
			}
		}

		Engines*=float.Parse(SC_data.Gameplay[15]);
		SC_artefacts.LoadDataArt();
		
		float limi = 2 * unit * float.Parse(SC_data.Gameplay[0]);
		if(limi > 0.01f) limi = 0.01f;
		F_barrier += limi;
		//IM_barrier -= unit * SC_artefacts.powerRM[2];
		//IL_barrier -= unit * SC_artefacts.powerRM[3];
		
		SC_fun.BTPT();

		int i;
		float tX=0f,tY=0f,tH=0f,tF=0f,tP=0f,tVx=0f,tVy=0f;
		//Global variables starts more important
		
		if((int)Communtron4.position.y==100)
		{
			try{
				ws = new WebSocket(CommuntronM1.name);
			}
			catch(Exception e)
			{
				Debug.LogWarning("Can't join to server (Can't set Websocket)");
				MenuReturn();
				return;
			}
        	
			ws.OnMessage += Ws_OnMessage;
			ws.OnOpen += Ws_OnOpen;
			ws.OnClose += Ws_OnClose;
			ws.OnError += Ws_OnError;
        	
			ws.Connect();
			
			if(getData!="0")
			{
				string[] gtd = getData.Split(';');

				tX=float.Parse(gtd[0]);
				tY=float.Parse(gtd[1]);
				tH=float.Parse(gtd[8]);
				tF=float.Parse(gtd[9]);
				tVx=float.Parse(gtd[6]);
				tVy=float.Parse(gtd[7]);
				timerH=int.Parse(gtd[10]);
				tP=float.Parse(gtd[11]);

				transform.position=new Vector3(tX,tY,0f);
				health_V=tH;
				turbo_V=tF;
				power_V=tP;
				respawn_point.position=new Vector3(tVx,tVy,1f);
			}
		}
		else
		{
			if(SC_data.data[0]!="")
			{
				tX=float.Parse(SC_data.data[0]);
				tY=float.Parse(SC_data.data[1]);
				tH=float.Parse(SC_data.data[2]);
				tF=float.Parse(SC_data.data[3]);
				tVx=float.Parse(SC_data.data[4]);
				tVy=float.Parse(SC_data.data[5]);
				timerH=int.Parse(SC_data.data[6]);
				tP=float.Parse(SC_data.data[7]);

				transform.position=new Vector3(tX,tY,0f);
				health_V=tH;
				turbo_V=tF;
				power_V=tP;
				respawn_point.position=new Vector3(tVx,tVy,1f);
			}

			for(i=0;i<21;i++)
            {
                SC_slots.BackpackX[i]=int.Parse(SC_data.backpack[i,0]);
                SC_slots.BackpackY[i]=int.Parse(SC_data.backpack[i,1]);
            }
		}

		servername.text=CommuntronM1.name;

		SC_bars.healthold_ui_bar.value=health_V;
		SC_bars.health_ui_bar.value=SC_bars.healthold_ui_bar.value;
		SC_bars.turbo_ui_bar.value=turbo_V;
		SC_bars.power_ui_bar.value=power_V;

		SC_slots.ResetYAB();
		Debug.Log("Joined");
	}
	void Start()
	{
		int i;
		for(i=2;i<max_players;i++)
		{
			NUL.Add(false);
			RR.Add(null);
			PL.Add(null);
			RU.Add(null);
			N.Add(null);
			NC.Add(null);
			NCT.Add(null);
			NCH.Add(null);
			NCHOF.Add(null);
			ramvis.Add(0);

			others1_RPC.Add(0);
			others2_RPC.Add(0);
			health_RPC.Add(1);
			rposX_RPC.Add(0);
			rposY_RPC.Add(0);
			nick_RPC.Add("Player");
		}

		for(i=2;i<max_players;i++)
		{
			//Clone player projection
			RR[i] = Instantiate(RR[1],RR[1].position,RR[1].rotation);
			RR[i].name = "Player" + i;
			RR[i].SetParent(RR[1].parent);

			foreach(Transform child in RR[i].GetComponent<Transform>())
			{
				if(child.name == "PlayerMT1") {
					child.name = "PlayerMT" + i;
					PL[i] = child.GetComponent<SC_players>();
					PL[i].IDP = i;
				}
				if(child.name == "MTP_resp1") {
					child.name = "MTP_resp" + i;
					RU[i] = child;
				}
				if(child.name == "TestNick1") {
					child.name = "TestNick" + i;
					N[i] = child.GetComponent<TextMesh>();
				}
			}
		}

		for(i=1;i<max_players;i++)
			PL[i].B_Awake();

		for(i=1;i<max_players;i++)
		{
			NC[i] = N[i].GetComponent<Transform>().GetChild(0).GetComponent<Canvas>();
			foreach(Transform child in NC[i].GetComponent<Transform>())
			{
				if(child.name == "Nickname")
					NCT[i] = child.GetComponent<Text>();

				if(child.name == "HPBar")
					NCH[i] = child.GetComponent<Slider>();
			}
			foreach(Transform child in NCH[i].GetComponent<Transform>())
			{
				if(child.name == "OverFill")
					foreach(Transform child2 in child)
					{
						if(child2.name=="Fill")
							NCHOF[i] = child2.GetComponent<Image>();
					}
			}
		}
		for(i=1;i<max_players;i++)
			PL[i].B_Start();
	}
	int RASCII_toInt(char c)
	{
		int ret;
		//string cc = c+"";
		//int intt = System.Text.Encoding.ASCII.GetBytes(cc)[0];
		int intt = (int)c;
		if(intt>=1 && intt<=31) ret = intt-1;
		else ret = intt-4;
		return ret;
	}
	float Char4ToFloat(string dat, int st)
	{
		bool minus = false;

		int[] bs = new int[4];
		bs[0] = 1906624;
		bs[1] = 15376;
		bs[2] = 124;
		bs[3] = 1;

		int[] get = new int[4];
		get[0] = RASCII_toInt(dat[st+0]);
		get[1] = RASCII_toInt(dat[st+1]);
		get[2] = RASCII_toInt(dat[st+2]);
		get[3] = RASCII_toInt(dat[st+3]);

		if(get[0]>=62)
		{
			minus = true;
			get[0]-=62;
		}

		float ret = (get[0]*bs[0] + get[1]*bs[1] + get[2]*bs[2] + get[3]*bs[3]) / 124f;
		if(minus) ret = -ret;
		return ret;
	}
	public int Char3ToInt(string dat, int st)
	{
		bool minus = false;

		int[] bs = new int[3];
		bs[0] = 15376;
		bs[1] = 124;
		bs[2] = 1;

		int[] get = new int[4];
		get[0] = RASCII_toInt(dat[st+0]);
		get[1] = RASCII_toInt(dat[st+1]);
		get[2] = RASCII_toInt(dat[st+2]);

		if(get[0]>=62)
		{
			minus = true;
			get[0]-=62;
		}

		int ret = (get[0]*bs[0] + get[1]*bs[1] + get[2]*bs[2]);
		if(minus) ret = -ret;
		return ret;
	}
	float Char1ToRot(string dat, int st)
	{
		return (RASCII_toInt(dat[st+0])*360f)/124f;
	}
	int[] Char1ToBool6(string dat, int st)
	{
		int[] ret = new int[6];
		int in1 = RASCII_toInt(dat[st+0]);
		int i,cc=32;
		for(i=0;i<6;i++)
		{
			if(in1-cc >= 0)
			{
				ret[i] = 1;
				in1 -= cc;
			}
			else ret[i] = 0;

			cc /= 2;
		}
		return ret;
	}
	float Char1ToHealth(string dat, int st)
	{
		return RASCII_toInt(dat[st+0])/123f;
	}
	public bool PressedNotInChat(KeyCode keycode, string mode)
	{
		if(mode=="down") return Input.GetKeyDown(keycode) && !SC_chat.typing;
		if(mode=="hold") return Input.GetKey(keycode) && !SC_chat.typing;
		if(mode=="up") return Input.GetKeyUp(keycode) && !SC_chat.typing;
		return false;
	}


	bool tempengine = false;
	bool tempturbo = false;
	void OnGUI()
    {
        if(GUI.Button(new Rect(10, -10010, 100, 40), "Engine")) tempengine = !tempengine;
		if(GUI.Button(new Rect(120, -10010, 100, 40), "Turbo")) tempturbo = !tempturbo;
    }
}
