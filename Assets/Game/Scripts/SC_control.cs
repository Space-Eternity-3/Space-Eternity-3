using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using WebSocketSharp;
using UnityEngine.SceneManagement;
using System;

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
	public Transform particlesEmptyBulb;
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
	int presed=-25;
	int dmLicz=0;
	int saveCo=0;
	public int max_players = 10;

	public float F_barrier;
	public float IL_barrier;
	public float IM_barrier;
	public bool invBlockExit;

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

	public Color32 HealthNormal;

	public Color32 FuelNormal;
	public Color32 FuelBurning;
	public Color32 FuelBlocked;
	
	public Color32 PowerNormal; 
	public Color32 PowerBurning;
	public Color32 PowerBlocked;

	public Transform darkner;
	Vector3 darknerV;

	string worldDIR="";
	int worldID=1;

	public WebSocket ws;
	public int connectionID=0;
	public bool living;
	public float healBalance = 0f;
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

	public Queue cmdList = new Queue();
	int fixup=0;

	public float health_V=1f, turbo_V=0f, power_V=0f;
	public Text health_Text, turbo_Text, power_Text;
	
	public int ArtSource = 0;
	public bool pause = false;
	public bool timeStop = false;
	public int timeInvisiblePulse;
	public bool impulse_reset;
	
	public int unstable_probability;
	public int unstable_sprobability;
	public float unstable_force;
	
	bool escaped = false;
	string RPU = "XXX";
	int MTPloadedCounter=5;
	Quaternion lock_rot = new Quaternion(0f,0f,0f,0f);
	Vector3 memPosition = new Vector3(0f,0f,0f);
	
	public bool impulse_enabled;
	public int impulse_time;
	
	public bool public_placed = false;
	public bool f1 = false;

	public string invCurrent()
	{
		return "2";
	}
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
	public void LaterUpdate()
	{
		if(Communtron3.position.z<0) Communtron3.position=new Vector3(Communtron3.position.x,Communtron3.position.y,0f);
		
		Screen1.enabled = !f1;
		Screen2.enabled = !f1;
		for(int ji=1;ji<max_players;ji++){
			NC[ji].enabled = !f1 && (PL[ji].ArtSource % 100!=1);
		}
		List<SC_pulse_bar> spbs = SC_lists.SC_pulse_bar;
		foreach(SC_pulse_bar spb in spbs) {
			spb.canvas.enabled = !f1;
		}
		
		if(!timeStop){
		
		//SHOT
		bool wr_comms = Communtron3.position.y==0f && Communtron2.position.x==0f && Communtron3.position.z==0f;
		bool wr_have = SC_slots.InvHaving(24) || SC_slots.InvHaving(39) || SC_slots.InvHaving(48);
		bool wr_cotr = (!Input.GetKey(KeyCode.LeftControl) || !SC_slots.InvHaving(48));
		bool wr_isok = cooldown==0 && wr_comms && wr_have && !impulse_enabled && !Input.GetMouseButton(0) && wr_cotr;
		bool wr_moustay = Input.GetMouseButton(1) && !Input.GetMouseButtonDown(1);
		
		if(wr_isok && wr_moustay && !public_placed && livTime>=50 && (intPing!=-1 || (int)Communtron4.position.y!=100))
		{
			cooldown=7;
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

			float xpo = Input.mousePosition.x-Screen.width/2, ypo=Input.mousePosition.y-Screen.height/2;
			if(typ!=3) playerR.velocity += Skop(float.Parse(SC_data.Gameplay[30]),new Vector3(-xpo,-ypo,0f));

			SC_bullet.Shot(
				transform.position,
				new Vector3(xpo,ypo,0f),
				playerR.velocity*0.02f,
				typ
			);
			SC_invisibler.invisible = false;
		}

		if(Input.GetMouseButtonDown(1)&&Communtron3.position.y==0f&&Communtron3.position.z==0f&&Communtron2.position.x==0f&&SC_slots.InvHaving(55))
		if((Mathf.Round(health_V*10000f)/10000f+healBalance)<1f && !impulse_enabled)
		{
			if(!SC_invisibler.invisible)
			{
				Transform trn11 = Instantiate(particlesEmptyBulb,transform.position,new Quaternion(0f,0f,0f,0f));
				trn11.GetComponent<SC_seeking>().enabled = true;
			}
			int slot = SC_slots.InvChange(55,-1,true,false,true);
			if((int)Communtron4.position.y==100) {
				SendMTP("/InventoryChange "+connectionID+" 55 -1 "+slot);
				SendMTP("/Heal "+connectionID+" 1");
				if(!SC_invisibler.invisible) SendMTP("/EmitParticles "+connectionID+" 11 0 0");
				healBalance += float.Parse(SC_data.Gameplay[31]);
			}
			else HealSGP();
		}
		else InfoUp("Potion blocked",380);
		
		}
		
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
		if(Input.GetKey(KeyCode.LeftAlt)&&!pause)
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
		if((Input.GetKey(KeyCode.Space))&&living&&!pause)
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
		if((Input.GetKey(KeyCode.LeftShift)&&turbo_V>0f)&&Communtron2.position.x==0&&living&&!pause)
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
			if(Input.GetKeyDown(KeyCode.R)&&!pause)
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

		SC_projection.MuchLaterUpdate();

		if(Input.GetKeyDown(KeyCode.F1)) f1 = !f1;
		
		//Game pause
		if(Screen3.enabled) f1 = false;
		if(Input.GetKeyUp("escape"))
		{
			escaped = false;
		}
		if(Input.GetKey("escape")&&!invBlockExit&&!escaped)
			esc_press(true);

		//Restart lags
		if(truePing>2.5f)
		{
			Debug.LogWarning("Ping over 2.50s");
			MenuReturn();
		}
	}
	void KillMe()
	{
		solidPos=transform.position+new Vector3(0f,0f,2500f);
		Communtron1.position+=new Vector3(0f,0f,75f);
		SC_sounds.PlaySound(transform.position,2,2);
		Instantiate(explosion,transform.position,transform.rotation);
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
		}
		Debug.Log("Player died");
		Screen1.targetDisplay=1;
		Screen2.targetDisplay=1;
		Screen3.targetDisplay=0;
				
		SC_invisibler.invisible = false;
		RemoveImpulse();
	}
	void ImmortalMe()
	{
		health_V=1f;
		SC_slots.BackpackY[15]--; //sureMTP
		SC_slots.BackpackYA[15]--;
		SC_slots.BackpackYB[15]--;
		if((int)Communtron4.position.y==100)
		{
			immID=(int.Parse(immID)+1)+"";
		}
		Instantiate(ImmortalParticles,transform.position,transform.rotation);
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
	public void InvisiblityPulseSend(string str)
	{
		if(SC_invisibler.invisible)
		SendMTP("/InvisibilityPulse "+connectionID+" "+str);
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

		List<SC_bullet> buls = SC_lists.SC_bullet;
		foreach(SC_bullet bul in buls)
		{
			if(bul.mode!="mother")
			bul.MakeDestroy(false);
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
		if(cooldown>0)
		{
			if(!SC_slots.InvHaving(48)) cooldown--;
			else if(livTime%2==0) cooldown--;
		}
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
			if(MTPloadedCounter==5) SendMTP("/GrowLoaded "+string.Join(";",SC_fun.GenListsB0)+"; "+connectionID);
			MTPloadedCounter--;
			if(MTPloadedCounter==0) MTPloadedCounter=5;
		}

		//something engine
		float pX=0f,pY=0f;
		if(engineON||turbo||brake)
		{
			X=mX*F/Pitagoras(new Vector3(mX,mY,0f));
			Y=mY*F/Pitagoras(new Vector3(mX,mY,0f));
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
		
		dX-=0.001f*VacuumDrag*DragSize*playerR.velocity.x*Mathf.Abs(playerR.velocity.x);
		dY-=0.001f*VacuumDrag*DragSize*playerR.velocity.y*Mathf.Abs(playerR.velocity.y);
		dX-=0.015f*VacuumDrag*DragSize*playerR.velocity.x;
		dY-=0.015f*VacuumDrag*DragSize*playerR.velocity.y;

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
			playerR.velocity = new Vector3(Fx,Fy,0f) * Fmul;
		}

		//Red health reduce
		healthOld.fillAmount=(health_V*0.8f)+0.1f;
		rocket_fuel.fillAmount=(turbo_V*0.8f)+0.1f;
		power.fillAmount=(power_V*0.8f)+0.1f;

		if(healthOld.fillAmount<health.fillAmount)
		{
			if(licznikC<0)
			health.fillAmount-=0.05f;
			licznikC--;
		}
		else
		{
			health.fillAmount=healthOld.fillAmount;
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
			}
			if(UnityEngine.Random.Range(0,unstable_sprobability)==0)
			{
				float alp = UnityEngine.Random.Range(0,360);
				float ux = Mathf.Cos((alp*3.14159f)/180f);
				float uy = Mathf.Sin((alp*3.14159f)/180f);
				
				SC_bullet.Shot(
					transform.position,
					new Vector3(ux,uy,0f),
					playerR.velocity*0.02f,
					3
				);
				
				//playerR.velocity += new Vector3(ux*unstable_force,uy*unstable_force,0f);
				Quaternion quat_foo = new Quaternion(0f,0f,0f,0f);
				quat_foo.eulerAngles = new Vector3(0f,0f,alp+90f);
				Transform trn = Instantiate(unstablePart,transform.position,quat_foo);
				trn.GetComponent<SC_seeking>().enabled = true;
				SendMTP("/EmitParticles "+connectionID+" 7 "+alp+" 0");
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
			(nn==6 && false)
			
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
			(mm==6 && false)
			
		) power_V -= unit * SC_artefacts.powerUM[mm];
		
		//invisibility stop
		if(power_V<=0f && SC_invisibler.invisible) SC_invisibler.invisible = false;
		
		//reducing bars
		if(health_V>1f) health_V=1f;
		if(turbo_V>1f) turbo_V=1f;
		if(turbo_V<0f) turbo_V=0f;
		if(power_V>1f) power_V=1f;
		if(power_V<0f) power_V=0f;

		if(rocket_fuel.fillAmount<0.1f) rocket_fuel.fillAmount=0.1f;
		if(rocket_fuel.fillAmount>0.9f) rocket_fuel.fillAmount=0.9f;
		if(healthOld.fillAmount<0.12f) healthOld.fillAmount=0.12f;
		if(healthOld.fillAmount>0.9f) healthOld.fillAmount=0.9f;
		if(power.fillAmount<0.1f) power.fillAmount=0.1f;
		if(power.fillAmount>0.9f) power.fillAmount=0.9f;

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
		if(drill3T.localPosition.y>=1.44f&&Mathf.Sqrt(playerR.velocity.x*playerR.velocity.x+playerR.velocity.y*playerR.velocity.y)<1000f)
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
			float trX,trY,rgX,rgY,rpX,rpY,heB,fuB,poB,mPPx,mPPy;
			string isImpulse = "F";
			if(impulse_enabled) isImpulse = "T";
			trX=Mathf.Round(transform.position.x*10000f)/10000f;
			trY=Mathf.Round(transform.position.y*10000f)/10000f;
			rgX=Mathf.Round(playerR.velocity.x*10000f)/10000f;
			rgY=Mathf.Round(playerR.velocity.y*10000f)/10000f;
			rpX=Mathf.Round(respawn_point.position.x*10000f)/10000f;
			rpY=Mathf.Round(respawn_point.position.y*10000f)/10000f;
			heB=Mathf.Round(health_V*10000f)/10000f;
			fuB=Mathf.Round(turbo_V*10000f)/10000f;
			poB=Mathf.Round(power_V*10000f)/10000f;
			mPPx=Mathf.Round(memPosition.x*10000f)/10000f;
			mPPy=Mathf.Round(memPosition.y*10000f)/10000f;
			int sendOther=enMode*16+(int)Communtron5.position.x*4+(int)Communtron2.position.x*2+(int)CommuntronM1.transform.position.x*1;
			int sendOtherParasite=ArtSource;
			
			if(living){
				/*
				0/0 - posX
				1/1 - posY
				2[] - velX
				3[] - velY
				4[] - rotation
				5[] - others2
				6/4 - respX
				7/5 - respY
				8x/2 - healthBar
				9/3 - fuelBar
				10x/6 - timerH
				11/7 - powerBar
					*/
				SendMTP(
					"/PlayerUpdate "+connectionID+" "+
					trX+";"+trY+";"+rgX+";"+rgY+";"+
					transform.rotation.eulerAngles.z+";"+sendOther+"&"+sendOtherParasite+";"+
					rpX+";"+rpY+";;"+fuB+";;"+poB+" "+localPing+" 250 "+livID+" "+immID+" "+isImpulse
					//---optional---
					+" "+mPPx+" "+mPPy
				);
			}
			else SendMTP("/PlayerUpdate "+connectionID+" 1 "+localPing+" 250 "+livID+" "+immID+" F");
		}

		memPosition = new Vector3(transform.position.x,transform.position.y,0f);

		localPing++;
		
		SC_projection.AfterFixedUpdate();
		for(int ij=1;ij<max_players;ij++)
			PL[ij].AfterFixedUpdate();
		
		if(!Input.GetMouseButton(1)) public_placed = false;
		gtm1 = false;
	}
	void TranslateRPU()
	{
		string[] arg = RPU.Split(' ');
		int i,lngt = int.Parse(arg[1]);
		string[] arh = splitChars(arg[2],lngt);

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
				NC[i].enabled = !f1 && (atf%100!=1);
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

			msg = msg+" "+conID+" "+livID;

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
		
		if((int)Communtron4.position.y==100)
			SendMTP("/ClientDamage "+connectionID+" "+dmg+" "+immID+" "+livID+" "+info);

		if(info=="K") KillMe();
		if(info=="I") ImmortalMe();
	}
	public void CookedDamage(float dmg)
	{
		health_V-=dmg;
		if(Mathf.Round(health_V*10000f) == 0f) health_V = 0f;
		timerH=(int)(50f*float.Parse(SC_data.Gameplay[4]));
		Instantiate(explosion2,transform.position,transform.rotation);
	}
	public void HealSGP()
	{
		float potHHH = SC_upgrades.MTP_levels[0]+SC_artefacts.GetProtLevelAdd()+float.Parse(SC_data.Gameplay[26]);
		if(potHHH<-50f) potHHH = -50f; if(potHHH>56.397f) potHHH = 56.397f;
		float heal=0.02f*float.Parse(SC_data.Gameplay[31])/(Mathf.Ceil(50*Mathf.Pow(health_base,potHHH))/50f);

		if(heal<0) heal=0;
		health_V += heal;
		if(health_V>1f) health_V=1f;
	}
	public Vector3 Skop(float F, Vector3 vec3)
	{
		float sqrt = Mathf.Sqrt(vec3.x*vec3.x + vec3.y*vec3.y + vec3.z*vec3.z);
		return vec3*F/sqrt;
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
				DamageFLOAT(hai);
			}
		}
		//else if(Pitagoras(playerR.velocity)<=50f) playerR.velocity += Skop(mushroom_force,playerR.velocity);
    }
	void OnTriggerStay(Collider collision)
	{
		if(collision.gameObject.name=="damager2"||(collision.gameObject.name=="damager3"&&SC_artefacts.GetArtefactID()!=6))
		{
			if(licznikD==0) licznikD=5;
			else if(licznikD<=5)
			{
				string neme = collision.gameObject.name;
				licznikD=25;
				float dmgg = 0f;
				if(neme=="damager2") dmgg = float.Parse(SC_data.Gameplay[8]); //spikes
				if(neme=="damager3") dmgg = float.Parse(SC_data.Gameplay[28]); //unstable matter
				if(dmgg!=0f) DamageFLOAT(dmgg);
			}
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
		string[] arg = e.Data.Split(' ');
		if(
		//Fast commands
		arg[0]=="/RPU"||
		arg[0]=="R"||
		arg[0]=="I"||
		arg[0]=="/RetHeal"||
		(arg[0])[0]=='P'||

		//Old commands	
		arg[0]=="/RetUpgrade"||
		arg[0]=="/RetFobsChange"||
		arg[0]=="/RetFobsDataChange"||
		arg[0]=="/RetFobsDataCorrection"||
		arg[0]=="/RetFobsTurn"||
		arg[0]=="/RetAsteroidData"||
		arg[0]=="/RSD"||
		arg[0]=="/RetGiveUpTeleport"||
		arg[0]=="/RetFobsPing"||
		arg[0]=="/RetGeyzerTurn"||
		arg[0]=="/RetInventory"||
		arg[0]=="/RetGrowNow"||
		arg[0]=="/RetServerDamage"||
		arg[0]=="/RetNewBulletSend"||
		arg[0]=="/RetNewBulletDestroy"||
		arg[0]=="/RetInfoClient"||
		arg[0]=="/RetInvisibilityPulse"||
		arg[0]=="/RPC"||
		(arg[0]=="/RetEmitParticles" && arg[1]!=connectionID+""))
		{
			cmdList.Enqueue(e.Data);
		}
    }
	void cmdDo(string cmdThis)
	{
		string[] arg = cmdThis.Split(' ');

		//Fast commands

		if((arg[0])[0]=='P')
		{
			//Short ping return message
			string pgg = arg[0],pggn="";
			int i,lngt=pgg.Length;
			for(i=1;i<lngt;i++) pggn+=pgg[i];
			returnedPing = int.Parse(pggn);
			
			return;
		}
		if(arg[0]=="R")
		{
			//Medium health regenerate message
			float fValue = float.Parse(arg[1]);
			string fImmID = arg[2];
			string fLivID = arg[3];

			if(immID==fImmID && livID==fLivID)
				health_V += fValue;

			return;
		}
		if(arg[0]=="I")
		{
			//Medium livID & immID update
			sr_livID = arg[2];
			sr_immID = arg[1];

			return;
		}
		if(arg[0]=="/RetHeal" && arg[1]==connectionID+"" && arg[2]=="1") {
			healBalance -= float.Parse(SC_data.Gameplay[31]);
		}

		int msl=arg.Length;
		int blo=0;
		if(arg[msl-2]!=conID&&arg[msl-2]!="X") blo=1;
		if(arg[msl-1]!=livID&&arg[msl-1]!="X") blo=2;
		if(blo>0) return;

		if(arg[0]=="/RPU")
		{
			//RPU big variable
			RPU=cmdThis;
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

			if(connectionID+""==arg[1])
			{
				List<SC_bullet> buls = SC_lists.SC_bullet;
				foreach(SC_bullet bul in buls)
				{
					if(bul.ID+""==arg[7] && bul.mode=="projection")
						bul.delta_age = -bul.age - (SC_fun.smooth_size/2);
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
				bul3.delta_age = (SC_fun.smooth_size/2);
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
		if(arg[0]=="/RetEmitParticles")
		{
			Vector3 particlePos;
			Quaternion quat_foo = new Quaternion(0f,0f,0f,0f);
			int put = int.Parse(arg[2]);
			int pid = int.Parse(arg[1]);
			if(pid==0) pid = connectionID;
			
			if((put>=6 && put<=8) || put==11)
			{
				particlePos = PL[pid].GetComponent<Transform>().position;
				if(put==7||false)
				{
					quat_foo.eulerAngles = new Vector3(0f,0f,90f+float.Parse(arg[3]));
				}
			}
			else particlePos = new Vector3(float.Parse(arg[3]),float.Parse(arg[4]),0f);
		
			switch(put)
			{
				case 1: //explosion
					SC_sounds.PlaySound(particlePos,2,2);
					Instantiate(explosionM,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 2:
					Instantiate(explosion2M,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 3:
					Instantiate(respawn2M,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 4:
					Instantiate(receiveParticles,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 5:
					Instantiate(ImmortalParticles,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 6:
					Instantiate(InvisiPart,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 7:
					Transform trn7 = Instantiate(unstablePart,particlePos,quat_foo);
					trn7.GetComponent<SC_seeking>().seek = PL[pid].GetComponent<Transform>();
					trn7.GetComponent<SC_seeking>().enabled = true;
					break;
				case 8: //impulse hidden
					Transform trn8 = Instantiate(impulseHidden,particlePos,quat_foo);
					trn8.GetComponent<SC_seeking>().seek = PL[pid].GetComponent<Transform>();
					trn8.GetComponent<SC_seeking>().enabled = true;
					break;
				case 9:
					Instantiate(particlesBossDamageM,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 10:
					Instantiate(particlesBossExplosionM,particlePos,new Quaternion(0f,0f,0f,0f));
					break;
				case 11:
					Transform trn11 = Instantiate(particlesEmptyBulb,particlePos,new Quaternion(0f,0f,0f,0f));
					trn11.GetComponent<SC_seeking>().seek = PL[pid].GetComponent<Transform>();
					trn11.GetComponent<SC_seeking>().enabled = true;
					break;
				default:
					Debug.LogWarning("Unknown particles ID: "+put);
					break;
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
		if(arg[0]=="/RetFobsChange"||
		arg[0]=="/RetFobsDataChange"||
		arg[0]=="/RetFobsDataCorrection"||
		arg[0]=="/RetFobsTurn"||
		arg[0]=="/RetFobsPing"||
		arg[0]=="/RetGeyzerTurn"||
		arg[0]=="/RetGrowNow")
		{
			//REBUILD IT
			List<SC_fobs> SCT_fobs = SC_lists.SC_fobs;
			foreach(SC_fobs ful in SCT_fobs)
			{
				if(ful.onMSG(cmdThis)) break;
			}
		}
	}
	void Ws_OnOpen(object sender, System.EventArgs e)
    {
		SendMTP("/ImJoined "+connectionID+" "+immID+" "+livID);
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
		darknerV=darkner.localPosition;

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
        	
			try{
				ws.Connect();
			}
			catch(Exception e)
			{
				Debug.LogWarning("Can't join to server (Can't connect)");
				MenuReturn();
				return;
			}
			
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

		memPosition = transform.position;

		servername.text=CommuntronM1.name;

		healthOld.fillAmount=(health_V*0.8f)+0.1f;
		health.fillAmount=healthOld.fillAmount;
		rocket_fuel.fillAmount=(turbo_V*0.8f)+0.1f;
		power.fillAmount=(power_V*0.8f)+0.1f;

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
		string cc = c+"";
		int intt = System.Text.Encoding.ASCII.GetBytes(cc)[0];
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

		float[] get = new float[4];
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
}
