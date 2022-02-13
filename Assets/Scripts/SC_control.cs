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
	public Rigidbody playerR;
	public Transform drill3T;
	public Transform respawn_point;
	public Transform Copper_bullet;
	public Text servername,pingname;
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

	public float F_barrier;
	public float IL_barrier;
	public float IM_barrier;
	public bool invBlockExit;

	public float VacuumDrag,Engines;
	public float unit=0.0008f;
	public float health_base;

	int localPing=0;
	int returnedPing=0;
	string trping="0,00";
	float truePing;
	bool dont=false;
	bool repeted=false;
	bool repetedAF=false;
	public bool gtm1 = false;

	public Color32 HealthNormal;

	public Color32 FuelNormal;
	public Color32 FuelBurning;
	public Color32 FuelBlocked;
	
	public Color32 PowerNormal; 
	public Color32 PowerBurning;
	public Color32 PowerBlocked;

	public Transform darkner;
	Vector3 darknerV;
	public SC_bullet1 SC_bullet1;

	string worldDIR="";
	int worldID=1;

	public WebSocket ws;
	public int connectionID=0;
	public bool living;
	Vector3 solidPos;
	bool connectWorks=true;
	public string nick;
	string getData="";
	string getInventory="";
	string getPush="";
	int[] invdata = new int[18];
	Vector3[] betterInvConverted = new Vector3[9];
	
	public SC_fun SC_fun;
	public SC_upgrades SC_upgrades;
	public SC_backpack SC_backpack;
	public SC_data SC_data;
	public SC_sounds SC_sounds;
	public SC_snd_start SC_snd_start;
	public SC_slots SC_slots;
	public SC_halloween SC_halloween;
	public SC_artefacts SC_artefacts;
	public SC_invisibler SC_invisibler;

	public Transform[] P = new Transform[10];
	public SC_players[] PL = new SC_players[10];
	public Rigidbody[] R = new Rigidbody[10];
	public TextMesh[] N = new TextMesh[10];
	public Transform[] RU = new Transform[10];

	Vector3[] Pa = new Vector3[10];
	Vector3[] Ra = new Vector3[10];
	Vector3[] RRa = new Vector3[10];
	float[] Rt = new float[10];
	int[] Fa = new int[10];
	int[] At = new int[10];
	string[] Na = new string[10];
	bool[] Nulx = new bool[10];
	
	public bool[] NUL = new bool[10];

	public string[] cmdArray = new string[2048];
	int n=0; int fixup=0;

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
	
	public bool impulse_enabled;
	public int impulse_time;
	
	public int[] ramvis = new int[10];
	public bool public_placed = false;

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
		
		if(!timeStop){
		
		//SHOT
		bool wr_comms = Communtron3.position.y==0f && Communtron2.position.x==0f && Communtron3.position.z==0f;
		bool wr_have = SC_slots.InvHaving(24) || SC_slots.InvHaving(39) || SC_slots.InvHaving(48);
		bool wr_isok = cooldown==0 && wr_comms && wr_have && !impulse_enabled && !Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftControl);
		bool wr_moustay = Input.GetMouseButton(1) && !Input.GetMouseButtonDown(1);
		
		if(wr_isok && wr_moustay && !public_placed)
		{
			cooldown=7;
			int slot;

			//Bullet types
			if(SC_slots.InvHaving(24))
			{
				SC_bullet1.type = 1;
				slot = SC_slots.InvChange(24,-1,true,false,true);
				if((int)Communtron4.position.y==100) SendMTP("/InventoryChange "+connectionID+" 24 -1 "+slot);
			}
			else if(SC_slots.InvHaving(39))
			{
				SC_bullet1.type = 2;
				slot = SC_slots.InvChange(39,-1,true,false,true);
				if((int)Communtron4.position.y==100) SendMTP("/InventoryChange "+connectionID+" 39 -1 "+slot);
			}
			else if(SC_slots.InvHaving(48))
			{
				SC_bullet1.type = 3;
				slot = SC_slots.InvChange(48,-1,true,false,true);
				if((int)Communtron4.position.y==100) SendMTP("/InventoryChange "+connectionID+" 48 -1 "+slot);
			}

			Shot(Input.mousePosition.x-Screen.width/2,Input.mousePosition.y-Screen.height/2);
			SC_invisibler.invisible = false;
		}
		
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
		SC_seeking[] scs = FindObjectsOfType<SC_seeking>();
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
		if(Input.GetKey(KeyCode.Space)&&living&&!pause)
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

		if(SC_snd_start.enMode!=M)
        {
            SC_snd_start.enMode=M;
            if(M!=0) SC_snd_start.ManualStartLoop(M-1);
            else SC_snd_start.ManualEndLoop();
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

		//KILL
		if(health_V<=0f&&living)
		{
			if(SC_artefacts.GetArtefactID() != 4) //Accept death
			{
				solidPos=transform.position+new Vector3(0f,0f,2500f);
				Communtron1.position+=new Vector3(0f,0f,75f);
				SC_sounds.PlaySound(transform.position,2,2);
				//for(int i=0;i<5;i++)
				Instantiate(explosion,transform.position,transform.rotation);
				living=false;
				if((int)Communtron4.position.y==100)
				{
					livID=(int.Parse(livID)+1)+"";
					float truX=Mathf.Round(transform.position.x*10000f)/10000f;
					float truY=Mathf.Round(transform.position.y*10000f)/10000f;
					SendMTP("/EmitParticles "+connectionID+" 1 "+truX+" "+truY);
					SendMTP("/InventoryReset "+connectionID+" "+livID);
				}
				Debug.Log("Player died");
				Screen1.targetDisplay=1;
				Screen2.targetDisplay=1;
				Screen3.targetDisplay=0;
				
				SC_invisibler.invisible = false;
				RemoveImpulse();
			}
			else
			{
				health_V=1f;
				SC_slots.BackpackY[15]--; //sureMTP
				SC_slots.BackpackYA[15]--;
				SC_slots.BackpackYB[15]--;
				if((int)Communtron4.position.y==100)
				{
					SendMTP("/InventoryChange "+connectionID+" 45 -1 24");
					float troX=Mathf.Round(transform.position.x*10000f)/10000f;
					float troY=Mathf.Round(transform.position.y*10000f)/10000f;
					SendMTP("/EmitParticles "+connectionID+" 5 "+troX+" "+troY);
				}
				Instantiate(ImmortalParticles,transform.position,transform.rotation);
				Debug.Log("Player avoided death");
			}
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

		//Game pause
		if(Input.GetKeyUp("escape")) escaped = false;
		if(Input.GetKey("escape")&&!invBlockExit&&!escaped)
		esc_press(true);

		//Restart lags
		if(truePing>2.5f)
		{
			Debug.LogWarning("Ping over 2.50s");
			MenuReturn();
		}
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

		SC_bullet1[] SCT_bullet1 = FindObjectsOfType<SC_bullet1>();
		foreach(SC_bullet1 bul in SCT_bullet1)
		{
			bul.serverExitDestroy();
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
			SendMTP("/ImDisconnected "+connectionID);
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
		float maxH=50f*Mathf.Pow(health_base,potH);
		float curH=50f*health_V*Mathf.Pow(health_base,potH);
		float maxHr=Mathf.Ceil(maxH);
		float curHr=Mathf.Ceil((curH*maxHr)/maxH);
		if(curHr>maxHr) curHr=maxHr;
		health_Text.text="Health "+curHr+"/"+maxHr;

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
	void Shot(float ix, float iy)
	{
		SC_bullet1.mX = ix;
		SC_bullet1.mY = iy;
		SC_bullet1.mode = 0;
		SC_bullet1.velo = playerR.velocity;
		if((int)Communtron4.position.y!=100) SC_bullet1.id=0;
		else
		{
			SC_bullet1.id=UnityEngine.Random.Range(1,999999999);
			SendMTP("/BulletSend "+connectionID+" "+SC_bullet1.type+" 1 "+transform.position.x+" "+transform.position.y+" "+SC_bullet1.id+";"+SC_bullet1.mX+";"+SC_bullet1.mY+" "+playerR.velocity.x+" "+playerR.velocity.y);
		}
		Instantiate(Copper_bullet,transform.position,transform.rotation);
	}
	void FixedUpdate()
	{
		if(timerH>0)
		{
			if(SC_artefacts.GetArtefactID() != 1) timerH--;
			else timerH-=2;
			if(timerH<0) timerH=0;
		}
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
		for(int ij=0;ij<10;ij++)
			if(ramvis[ij]>0) ramvis[ij]--;
		
		livTime++;

		//Main save data
		if((int)Communtron4.position.y!=100)
		if(saveCo==0)
		{
			saveCo=500; //10 seconds
			MainSaveData();
		}

		//Grow Loaded
		if((int)Communtron4.position.y==100)
		{
			if(MTPloadedCounter==5) SendMTP("/GrowLoaded "+SC_fun.GenLists[0]+" "+connectionID);
			MTPloadedCounter--;
			if(MTPloadedCounter==0) MTPloadedCounter=5;
		}
		
		//rigidbody disable
		if(impulse_enabled)
		{
			//playerR.detectCollisions = false;
		}
		else
		{
			//playerR.detectCollisions = true;
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
		if(SC_artefacts.GetArtefactID()==6 && livTime>=50)
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
				
				SC_bullet1.type = 3;
				Shot(-ux,-uy);
				
				//playerR.velocity += new Vector3(ux*unstable_force,uy*unstable_force,0f);
				Quaternion quat_foo = new Quaternion(0f,0f,0f,0f);
				quat_foo.eulerAngles = new Vector3(0f,0f,alp+90f);
				Transform trn = Instantiate(unstablePart,transform.position,quat_foo);
				trn.GetComponent<SC_seeking>().enabled = true;
				SendMTP("/EmitParticles "+connectionID+" 7 "+alp+" 0");
			}
		}

		//health regeneration
		if(health_V>0f&&health_V<1f&&timerH==0)
		{
			float potHH = SC_upgrades.MTP_levels[0]+SC_artefacts.GetProtLevelAdd()+float.Parse(SC_data.Gameplay[26]);
			if(potHH<-50f) potHH = -50f; if(potHH>56.397f) potHH = 56.397f;
			health_V+=unit*SC_artefacts.GetProtRegenMultiplier()*float.Parse(SC_data.Gameplay[5])/(Mathf.Ceil(50*Mathf.Pow(health_base,potHH))/50f);
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

		//Update ??? [/RetPlayerUpdate]
		int h;
		if(Communtron4.position.y==100f&&RPU!="XXX")
		{
			string eData=RPU;
			string[] tabe=eData.Split(' ');
			float PRx=0f;
			int FRx=0, ATx=1;
        	int i,k;
			float pxx,pyy;
			returnedPing=int.Parse(tabe[connectionID+21]);
			for(i=1;i<=9;i++)
			{
				Vector3 Px=new Vector3(0f,0f,1000f+i*5);
				Vector3 Rx=new Vector3(0f,0f,0f);
				Vector3 RRx=new Vector3(0f,0f,1000f);
				FRx=0; ATx=0;
				
				string Nax="";
				
				bool exist=false;
				k=i;
				if(connectionID==i) k=0;
				if(tabe[k+1]!="0")
				{
					if(tabe[k+1]=="1") Nax=tabe[k+11];
					else
					{
						string[] tabe2=tabe[k+1].Split(';');
						pxx=float.Parse(tabe2[0]);
						pyy=float.Parse(tabe2[1]);
						Px=new Vector3(pxx,pyy,0f);
						
						pxx=float.Parse(tabe2[2]);
						pyy=float.Parse(tabe2[3]);
						Rx=new Vector3(pxx,pyy,0f);
						
						PRx=float.Parse(tabe2[4]);
						
						string[] tbo = tabe2[5].Split('&');
						FRx=int.Parse(tbo[0]);
						if(tbo.Length>0) ATx=int.Parse(tbo[1]);
						else ATx=0;
						
						pxx=float.Parse(tabe2[6]);
						pyy=float.Parse(tabe2[7]);
						RRx=new Vector3(pxx,pyy,1f);
						
						if(RRx==new Vector3(0f,0f,1f)) RRx=new Vector3(0f,0f,1000f);
						Nax=tabe[k+11];
					}
					if(Nax=="0") Nax="";
					exist=true;
				}
				Pa[i]=Px; Ra[i]=Rx; if(exist)Rt[i]=PRx; Fa[i]=FRx; At[i]=ATx; Na[i]=Nax; RRa[i]=RRx; Nulx[i]=exist;
			}
			for(h=1;h<=9;h++)
			{
				NUL[h] = Nulx[h];
				P[h].position = Pa[h];
				PL[h].ArtSource = At[h];
				PL[h].OtherSource = Fa[h];
				R[h].velocity = Ra[h];
				P[h].rotation = Quaternion.Euler(0f,0f,Rt[h]);
				N[h].text = Na[h];
				RU[h].position = RRa[h];
			}

			fixup--;
			if(fixup<=0)
			{
				truePing=(localPing-returnedPing)/50f;
				trping=retping(truePing);
				pingname.text="Ping: "+trping+"s";
				fixup=10;
			}
		}
		localPing++;

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
			float trX,trY,rgX,rgY,rpX,rpY,heB,fuB,poB;
			trX=Mathf.Round(transform.position.x*10000f)/10000f;
			trY=Mathf.Round(transform.position.y*10000f)/10000f;
			rgX=Mathf.Round(playerR.velocity.x*10000f)/10000f;
			rgY=Mathf.Round(playerR.velocity.y*10000f)/10000f;
			rpX=Mathf.Round(respawn_point.position.x*10000f)/10000f;
			rpY=Mathf.Round(respawn_point.position.y*10000f)/10000f;
			heB=Mathf.Round(health_V*10000f)/10000f;
			fuB=Mathf.Round(turbo_V*10000f)/10000f;
			poB=Mathf.Round(power_V*10000f)/10000f;
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
				6/2 - respX
				7/3 - respY
				8/4 - healthBar
				9/5 - fuelBar
				10/6 - timerH
				11/7 - powerBar
					*/
				SendMTP(
					"/PlayerUpdate "+connectionID+" "+
					trX+";"+trY+";"+rgX+";"+rgY+";"+
					transform.rotation.eulerAngles.z+";"+sendOther+"&"+sendOtherParasite+";"+
					rpX+";"+rpY+";"+heB+";"+fuB+";"+timerH+";"+poB+" "+localPing+" 250"
				);
			}
			else SendMTP("/PlayerUpdate "+connectionID+" 1 "+localPing+" 250");
		}

		string[] tempCmd = new string[2048];
		int y;
		for(y=0;(n>0&&y<2048);y++,n--)
		{
			tempCmd[y]=cmdArray[n-1];
			cmdArray[n-1]="0";
		}
		for(;y>0;y--)
		{
			cmdDo(tempCmd[y-1]);
		}
		
		for(int ij=1;ij<10;ij++) PL[ij].AfterFixedUpdate();
		
		if(!Input.GetMouseButton(1)) public_placed = false;
		gtm1 = false;
	}
	public void SendMTP(string msg)
	{
		if(Communtron4.position.y==100)
		{
			if(repetedAF) return;

			msg=msg+" "+conID;
			if(true) msg=msg+" X";

			try
			{
				ws.Send(msg);
			}
			catch
			{
				Debug.LogWarning("Failed sending message: "+msg);
				MenuReturn();
			}
		}
	}
	string retping(float pig)
	{
		int inpg=(int)(pig*100);
		if(inpg%100==0) return pig+",00";
		if(inpg%10==0) return pig+"0";
		return pig+"";
	}
	public void DamageINT(int dmgINT) {if(dmgINT!=0) Damage(dmgINT*0.01666667f);}
	public void Damage(float dmg)
	{
		if(livTime<50 || impulse_enabled) return;
		float potHHH = SC_upgrades.MTP_levels[0]+SC_artefacts.GetProtLevelAdd()+float.Parse(SC_data.Gameplay[26]);
		if(potHHH<-50f) potHHH = -50f; if(potHHH>56.397f) potHHH = 56.397f;
		dmg=(1.2f*dmg)/(Mathf.Ceil(50*Mathf.Pow(health_base,potHHH))/50f);
		health_V-=dmg;
		//SC_sounds.PlaySound(transform.position,2,0);
		if(health_V>0f) Instantiate(explosion2,transform.position,transform.rotation);
		timerH=(int)(50f*float.Parse(SC_data.Gameplay[4]));
		
		if((int)Communtron4.position.y==100)
		{
			float truX=Mathf.Round(transform.position.x*10000f)/10000f;
			float truY=Mathf.Round(transform.position.y*10000f)/10000f;
			if(health_V>0f) SendMTP("/EmitParticles "+connectionID+" 2 "+truX+" "+truY);
			InvisiblityPulseSend("wait");
		}
	}
	void OnCollisionEnter(Collision collision)
    {
		float CME=float.Parse(SC_data.Gameplay[6]); 

		if(dmLicz<=0)
       	if(collision.impulse.magnitude>CME&&collision.relativeVelocity.magnitude>CME)
		{
			dmLicz=20;
			float head_ache=collision.impulse.magnitude-CME+1f;
			int hai=(int)Mathf.Ceil(((head_ache+2f)/50f)*float.Parse(SC_data.Gameplay[7])/0.01666667f);
			DamageINT(hai);
		}
    }
	void OnTriggerStay(Collider collision)
	{
		if(collision.gameObject.name=="damager2"||(collision.gameObject.name=="damager3"&&SC_artefacts.GetArtefactID()!=6))
		{
			if(licznikD==0) licznikD=5;
			else if(licznikD<=5)
			{
				licznikD=25;
				DamageINT(int.Parse(SC_data.Gameplay[8]));
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
		int msl=arg.Length;
		int blo=0;
		if(arg[msl-2]!=conID&&arg[msl-2]!="X") blo=1;
		if(arg[msl-1]!=livID&&arg[msl-1]!="X") blo=2;
		if(blo>0) return;

		if(arg[0]=="/RetPlayerUpdate")
		{
			RPU=e.Data;
			returnedPing=int.Parse(arg[connectionID+21]);
			return;
		}
		if(arg[0]=="/RetKickConnection"&&int.Parse(arg[1])==connectionID)
		{
			Debug.Log("Server kicked");
			ws.Close();
		}
		if(arg[0]=="/RetBulletDestroy"||
		arg[0]=="/RetUpgrade"||
		arg[0]=="/RetFobsChange"||
		arg[0]=="/RetFobsDataChange"||
		arg[0]=="/RetFobsDataCorrection"||
		arg[0]=="/RetFobsTurn"||
		arg[0]=="/RetAsteroidData"||
		arg[0]=="/RetFobsPing"||
		arg[0]=="/RetGeyzerTurn"||
		arg[0]=="/RetInventory"||
		arg[0]=="/GrowNow"||
		arg[0]=="/InfoClient"||
		arg[0]=="/RetBulletSend"||
		arg[0]=="/RetInvisibilityPulse"||
		(arg[0]=="/RetEmitParticles" && arg[1]!=connectionID+""))
		{
			cmdArray[n]=e.Data;
			n++;
		}
    }
	void cmdDo(string cmdThis)
	{
		string[] arg = cmdThis.Split(' ');
		if(arg[0]=="/RetBulletSend")
		{
			if(arg[1]!=connectionID+"")
			{
				SC_bullet1.mX=float.Parse(arg[6].Split(';')[1]);
				SC_bullet1.mY=float.Parse(arg[6].Split(';')[2]);
				SC_bullet1.type=int.Parse(arg[2]);
				SC_bullet1.mode=int.Parse(arg[3]);
				SC_bullet1.id=int.Parse(arg[6].Split(';')[0]);
				SC_bullet1.velo=new Vector3(float.Parse(arg[7]),float.Parse(arg[8]),0f);
				Instantiate(Copper_bullet,new Vector3(float.Parse(arg[4]),float.Parse(arg[5]),0f),transform.rotation);
			}
		}
		if(arg[0]=="/RetUpgrade")
		{
			if(arg[1]==connectionID+"") SC_upgrades.MTP_levels[int.Parse(arg[2])]++;
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
			
			if(put>=6 && put<=8)
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
		if(arg[0]=="/RetBulletDestroy")
		{
			SC_bullet1[] SCT_bullet1 = Component.FindObjectsOfType<SC_bullet1>();
			foreach(SC_bullet1 bul in SCT_bullet1)
			{
				bul.cmdDoo(cmdThis);
			}
		}
		if(arg[0]=="/RetAsteroidData")
		{
			SC_asteroid[] SCT_asteroid = Component.FindObjectsOfType<SC_asteroid>();
			foreach(SC_asteroid aul in SCT_asteroid)
			{
				aul.onMSG(cmdThis);
			}
		}
		if(arg[0]=="/InfoClient")
		{
			if(arg[2]==connectionID+"") return;
			InfoUp(info_space_add(arg[1]),500);
		}
		if(arg[0]=="/RetFobsChange"||
		arg[0]=="/RetFobsDataChange"||
		arg[0]=="/RetFobsDataCorrection"||
		arg[0]=="/RetFobsTurn"||
		arg[0]=="/RetFobsPing"||
		arg[0]=="/RetGeyzerTurn"||
		arg[0]=="/GrowNow")
		{
			SC_fobs[] SCT_fobs = Component.FindObjectsOfType<SC_fobs>();
			foreach(SC_fobs ful in SCT_fobs)
			{
				ful.onMSG(cmdThis);
			}
		}
	}
	void Ws_OnOpen(object sender, System.EventArgs e)
    {
		SendMTP("/ImNotKicked "+connectionID);
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
		for(j=0;j<2048;j++)
		{
			cmdArray[j]="0";
			if(j<9) betterInvConverted[j]=new Vector3(0f,0f,0f);
		}
		
		Screen1.targetDisplay=0;
		Screen2.targetDisplay=0;
		Screen3.enabled = false;

		worldID=(int)Communtron4.position.y;
		worldDIR="../../saves/UniverseData"+worldID+"/";
		darknerV=darkner.localPosition;

		if((int)Communtron4.position.y==100)
		{
			connectionID=int.Parse(SC_data.TempFileConID[0]);
			CommuntronM1.name=SC_data.TempFileConID[1];
			nick=SC_data.TempFileConID[2];
			getData=SC_data.TempFileConID[3];
			getInventory=SC_data.TempFileConID[4];
			getPush=SC_data.TempFileConID[5];
			SC_upgrades.MTP_loadUpgrades(SC_data.TempFileConID[6]);
			SC_backpack.MTP_loadBackpack(SC_data.TempFileConID[7]);
			SC_data.DatapackMultiplayerLoad(SC_data.TempFileConID[9]);
			conID=SC_data.TempFileConID[10];
			MTP_InventoryLoad();
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
			try
			{
				ws = new WebSocket(CommuntronM1.name);
			}
			catch(Exception e)
			{
				Debug.LogWarning("Can't join to server E1");
				MenuReturn();
				return;
			}
        	
			ws.OnMessage += Ws_OnMessage;
			ws.OnOpen += Ws_OnOpen;
        	
			try
			{
				ws.Connect();
			}
			catch(Exception e)
			{
				Debug.LogWarning("Can't join to server E2");
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
		else if(SC_data.data[0]!="")
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
		
		servername.text=CommuntronM1.name;

		healthOld.fillAmount=(health_V*0.8f)+0.1f;
		health.fillAmount=healthOld.fillAmount;
		rocket_fuel.fillAmount=(turbo_V*0.8f)+0.1f;
		power.fillAmount=(power_V*0.8f)+0.1f;

		if((int)Communtron4.position.y!=100)
        {
			for(i=0;i<21;i++)
            {
                SC_slots.BackpackX[i]=int.Parse(SC_data.backpack[i,0]);
                SC_slots.BackpackY[i]=int.Parse(SC_data.backpack[i,1]);
            }
        }

		SC_slots.ResetYAB();
		Debug.Log("Joined");
	}
}
