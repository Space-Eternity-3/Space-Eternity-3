using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SC_fobs : MonoBehaviour
{
    public bool mother=true;
    bool true_mother=true;

    public Transform player, legs;
    public Transform DropParticles;
    public Transform DropParticlesM;
    public Transform receiveParticles;
    public Transform alienKillParticles;

    public Transform WindThis;
    public Transform Communtron1, Communtron2, Communtron3, Communtron4;
    public int DropID, DropCount;
    public bool PickUp;
    public bool ShotTurn, GrowTurn, GeyzerTurn;
    public bool IsEmpty, IsStorage, IsTreasure;
    public bool CandySource;
    public bool WindObject;

    public int ShotID, BulletID, GrowID, GeyzerID, ObjID, ObjID2;
    public int GrowTimeMin, GrowTimeMax;
    public int placeSoundID,breakSoundID;
    public string lootSE3;
    
    int GrowTimeLeft=100;
    public int GeyzerTime=0;
    public int inGeyzer=0;

    public Transform[] MTPplayers = new Transform[10];

    public SC_control SC_control;
    public SC_asteroid SC_asteroid;
    public SC_backpack SC_backpack;
    public SC_fun SC_fun;
    public SC_data SC_data;
    public SC_sounds SC_sounds;
    public SC_push SC_push;
    public SC_slots SC_slots;
    public SC_halloween SC_halloween;
    public SC_snd_loop SC_snd_loop;
    SC_Fob21 SC_Fob21;

    string worldDIR="";
	int worldID=1;
    public int ID,index,X,Y;
    int i, rand;
    bool ReplaceReserved=false;
    bool ReR=false;
    bool com1act=false;
    int id21, count21;
    bool cursed=false;
    bool emptyShow=false; int onu=2;
    bool multiplayer=false;
    string potFob21Name="0;0;";
    public int MTPblocker = 0;
    Transform mainWind;
    int destroyTime = -1;
    int loopSndID = -1;
    int loopSndID2 = -1;
    public int lsid;
    public bool lsb;
    bool started = false;
	bool colied = false;

    public bool GetRespond()
    {
        try{
            transform.position += new Vector3(0f,0f,0f);
        }catch(Exception)
        {
            return false;
        }
        return true;
    }
    string getLoot(string str)
    {
        int[] marray = new int[5];
        int i,j,rander=UnityEngine.Random.Range(0,10000);
        int lngt=str.Split(';').Length/5;
        for(i=0;i<lngt;i++)
        {
            for(j=0;j<5;j++) marray[j]=int.Parse(str.Split(';')[5*i+j]);
            if(rander>=marray[3]&&rander<=marray[4]) return marray[0]+";"+UnityEngine.Random.Range(marray[1],marray[2]+1);
        }
        Debug.Log("Game crashed! Error in loot nbt.");
        return "0;0";
    }
    void MTPsend(int st1, int st2, int ed, int di, int dc, int sl)
    {
        //FobsChange [ConnectionID] [UlamID] [index] [StartID] [Start2ID] [EndID] [Item] [Count] [Slot] [CandyCount]
        int cc=0;
        SC_control.SendMTP("/FobsChange "+SC_control.connectionID+" "+ID+" "+index+" "+st1+" "+st2+" "+ed+" "+di+" "+dc+" "+sl+" "+cc);
    }
    public void onMSG(string eData)
    {
        if(mother) return;
        string[] arg = eData.Split(' ');

        if(arg[0]=="/RetFobsChange")
        if((arg[1]==ID+"") && (arg[2]==index+""))
        {
            int turEnd=int.Parse(arg[3]);
            if(ObjID!=turEnd)
            {
                potFob21Name = arg[4]+";";
                Replace(turEnd,true);
            }
        }
        if(arg[0]=="/RetFobsTurn")
        if((arg[1]!=SC_control.connectionID+"") && (arg[2]==ID+"") && (arg[3]==index+""))
        {
            int turEnd = int.Parse(arg[4]);
            Replace(turEnd,true);
        }
        if(arg[0]=="/RetFobsDataChange")
        if((arg[1]==ID+"") && (arg[2]==index+"") && (arg[5]!=SC_control.connectionID+""))
        {
            int itym=int.Parse(arg[3]);
            int coynt=int.Parse(arg[4]);
            int id21=int.Parse(arg[7]);

            if(ObjID==id21)
            {
                SC_Fob21.item=itym;
                SC_Fob21.count+=coynt;
            }
            else
            {
                coynt = int.Parse(arg[6]);
                potFob21Name=itym+";"+coynt+";";
                Replace(id21,true);
            }
        }
        if(arg[0]=="/RetFobsDataCorrection")
        if((arg[1]==ID+"") && (arg[2]==index+"") && (arg[4]==SC_control.connectionID+""))
        {
            int itym = int.Parse(arg[3].Split(';')[0]);
            int deltaCoynt = int.Parse(arg[3].Split(';')[2]);
            int id21=int.Parse(arg[5]);

            if(ObjID==id21)
            {
                SC_Fob21.count -= deltaCoynt;
                if(SC_Fob21.count == 0) SC_Fob21.item = 0;
                else SC_Fob21.item = itym;
            }
        }
        if(arg[0]=="/GrowNow")
        if((arg[1]==ID+"") && (arg[2]==index+"") && GrowTurn)
        {
            Replace(GrowID,true);
        }
        if(arg[0]=="/RetGeyzerTurn")
        if((arg[1]==ID+"") && (arg[2]==index+"") && GeyzerTurn)
        {
            Replace(GeyzerID,true);
        }
        if(arg[0]=="/RetFobsPing")
        if(arg[1]==(SC_control.connectionID+";"+ID+";"+index))
        {
            MTPblocker--;
        }
    }
    bool HasPhysicalVersion(int b)
    {
        if(b>=1&&b<=19) return true; //20 -> item respawn
        if(b>=21&&b<=22) return true; //23 -> magnetic alien waited //24 -> copper bullet
        if(b>=25&&b<=38) return true; //39 -> red bullet
        if(b>=40&&b<=47) return true; //48 -> unstable bullet
		if(b==48&&Input.GetKey(KeyCode.LeftControl)) return true;
		if(b>=49&&b<=51) return true;
        return false;
    }
    void Replace(int id, bool MTPchange)
    {
        if(ReplaceReserved) return;
        ReplaceReserved=true;

        if(id==0&&gameObject.name!="FOB0_Empty(Clone)")
        {
            if(!IsTreasure) Instantiate(DropParticles,transform.position,transform.rotation);
            else Instantiate(receiveParticles,transform.position,transform.rotation);
            SC_sounds.PlaySound(transform.position,0,breakSoundID);
        }
        else if(GeyzerTurn&&id==GeyzerID) Instantiate(alienKillParticles,transform.position,transform.rotation);
        
        if(!MTPchange)
        {
            string[] uAst = SC_data.GetAsteroid(X,Y).Split(';');
            int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]);
            SC_data.World[a,index+1,c]=id+"";
            SC_data.World[a,21+index*2,c]="";
            SC_data.World[a,22+index*2,c]="";
            if(uAst[2]=="T") SC_data.SaveAsteroid(c);
        }

        //In game replace
        GameObject gobT=SC_asteroid.GenPlaceT[0];
        int tid=id; //tid -> Physical ID

        if((tid<8||tid>11)&&tid!=16&&tid!=30&&tid!=50&&tid!=51)
        {
            if(tid==21||tid==2) SC_asteroid.GenPlaceT[tid].name=potFob21Name;
            try{gobT=Instantiate(SC_asteroid.GenPlaceT[tid],transform.position,transform.rotation);}catch(Exception)
				{
					gobT=Instantiate(SC_asteroid.GenPlaceT[0],transform.position,transform.rotation);
				}
        }
        else
        {
            int tud=0;
			rand = UnityEngine.Random.Range(0,3);
			if(tid>=8&&tid<=11) tud=tid-8;
			if(tid==16) tud=4;
			if(tid==30) tud=5;
			if(tid==50) tud=6;
			if(tid==51) tud=7;
			try{gobT=Instantiate(SC_asteroid.GenPlaceM[tud*3+rand],transform.position,transform.rotation);}catch(Exception)
				{
					gobT=Instantiate(SC_asteroid.GenPlaceT[0],transform.position,transform.rotation);
				}
        }
        gobT.GetComponent<SC_fobs>().MTPblocker = MTPblocker;
        if(ObjID==25&&id==23) gobT.GetComponent<SC_fobs>().GeyzerTime = GeyzerTime;

        if(gobT.name!="FOB0_Empty(Clone)"&&gameObject.name=="FOB0_Empty(Clone)") SC_sounds.PlaySound(transform.position,1,gobT.GetComponent<SC_fobs>().placeSoundID);
        if(ObjID==25&&id==23) SC_sounds.PlaySound(transform.position,2,8);
        if(ObjID==23&&id==25) SC_sounds.PlaySound(transform.position,2,9);
        if((ObjID==13||ObjID==23||ObjID==25||ObjID==27)&&id==40) SC_sounds.PlaySound(transform.position,2,12);
        gobT.transform.parent = gameObject.transform.parent;

        if(!WindObject) Destroy(gameObject);
        else
        {
            mother = true;
            destroyTime = 50;
            transform.position = new Vector3(0f,0f,10000f);
        }
		gobT.GetComponent<SC_fobs>().index = index;
        gobT.GetComponent<SC_fobs>().StartM();
    }
    public void StartM()
    {
        Start();
    }
    void Start()
    {
        if(started) return;
        else started = true;

        if(transform.position.z<100f) {mother=false; true_mother=false;}
        if((int)Communtron4.position.y==100)
        {
            multiplayer=true;
        }
		worldID=(int)Communtron4.position.y;
		worldDIR="../../saves/UniverseData"+worldID+"/WorldData/";

        if(IsEmpty) gameObject.GetComponent<Renderer>().enabled=false;
		
		if(IsStorage)
		{
			SC_Fob21 = gameObject.GetComponent<SC_Fob21>();
			SC_Fob21.X=X;
			SC_Fob21.Y=Y;
			SC_Fob21.ID=ID;
			SC_Fob21.uID=index;
			SC_Fob21.StartF();
		}

		if(!mother)
		{
			X=transform.parent.GetComponent<SC_asteroid>().X;
			Y=transform.parent.GetComponent<SC_asteroid>().Y;
			ID=transform.parent.GetComponent<SC_asteroid>().ID;
			//index=(int)Mathf.Round(transform.position.z*100000f)-1;
		}

        if(WindObject&&!mother)
        {
            mainWind=Instantiate(WindThis,transform.position,transform.rotation);
            mainWind.GetComponent<SC_terminate>().SC_fobs = gameObject.GetComponent<SC_fobs>();
            mainWind.GetComponent<SC_terminate>().partgey = true;
        }
        if(lsb && !mother)
        {
            loopSndID = SC_snd_loop.AddToLoop(lsid,transform.position);
        }

        if(SC_data.ModifiedDrops.Length>ObjID)
        if(SC_data.ModifiedDrops[ObjID]!="")
        {
            string[] drp = SC_data.ModifiedDrops[ObjID].Split(';');
            DropID=int.Parse(drp[0]);
            DropCount=int.Parse(drp[1]);
        }

        if(GrowTurn&&!mother)
        {
            if(!multiplayer)
            {
                string[] uAst = SC_data.GetAsteroid(X,Y).Split(';');
                int c=int.Parse(uAst[0]), a=int.Parse(uAst[1]);

                if(!((SC_data.World[a,0,c]=="6" && (ObjID==5||ObjID==6)) || ObjID==25))
                {
                    GrowTimeLeft=99999999;
                }
                else
                {
                    if(SC_data.World[a,21+index*2,c]!="")
                    {
                        GrowTimeLeft=int.Parse(SC_data.World[a,21+index*2,c]);
                    }
                    else
                    {
                        GrowTimeLeft=UnityEngine.Random.Range(GrowTimeMin,GrowTimeMax+1);
                        SC_data.World[a,21+index*2,c]=GrowTimeLeft+"";
                        if(uAst[2]=="T") SC_data.SaveAsteroid(c);
                    }
                }
            }
            else GrowTimeLeft=99999999;
        }
    }
    void Update()
    {
        if(onu>0) onu--;
        if(!mother)
        {
            //Storage data get
            if(IsStorage&&!mother)
            {
                id21=SC_Fob21.pub_item;
                count21=SC_Fob21.pub_count;
            }
        }
        if(Communtron4.position.x!=0f&&Input.GetMouseButtonDown(0)) cursed=true;
        if(cursed&&Input.GetMouseButtonUp(0)) cursed=false;
    }
	public void AfterTriggerEnter(Collider collision)
	{
		if((collision.gameObject.name=="Bullet1(Clone)")&&!collision.gameObject.GetComponent<SC_bullet1>().turn_used&&ShotTurn&&!mother)
        {
            SC_bullet1 bull = collision.gameObject.GetComponent<SC_bullet1>();
			bull.turn_used = true; bull.MakeDestroy("TurnDestroy");
            if(bull.mode==0 && (bull.type==BulletID || BulletID==0))
            {
                if(multiplayer)
                {
                    MTPblocker++;
                    SC_control.SendMTP("/FobsTurn "+SC_control.connectionID+" "+ID+" "+index+" "+ObjID+" "+ObjID2+" "+ShotID);
                    SC_control.SendMTP("/FobsPing "+SC_control.connectionID+";"+ID+";"+index);
                }
                Replace(ShotID,multiplayer);
            }
        }
	}
    void OnTriggerEnter(Collider collision)
    {
		if(IsEmpty) return;
		AfterTriggerEnter(collision);
    }
    void OnDestroy()
    {
        if(lsb && !true_mother)
        {
            try{
                SC_snd_loop.RemoveFromLoop(loopSndID);
            }catch(Exception e) {}
        }

        try{
            if(com1act)
			{
				if(IsEmpty) Communtron1.position+=new Vector3(1f,0f,0f);
				else Communtron1.position-=new Vector3(1f,0f,0f);
			}
        }catch(Exception e) {}
    }
    void FixedUpdate()
    {
        if(destroyTime>0) destroyTime--;
        if(destroyTime==0) Destroy(gameObject);

        if(lsb && !true_mother)
        {
            SC_snd_loop.sound_pos[loopSndID] = transform.position;
        }
        if(inGeyzer>0&&GeyzerTurn&&!mother)
        {
            GeyzerTime++;
            if(GeyzerTime>=140)
            {
                if(!multiplayer)
                {
                    Replace(GeyzerID,multiplayer);
                }
                else
                {
                    SC_control.SendMTP("/GeyzerTurnTry "+SC_control.connectionID+" "+ID+" "+index);
                }
            }
        }
        if(GrowTurn&&!mother&&!multiplayer)
        {
            if(GrowTimeLeft==0) Replace(GrowID,multiplayer);
            GrowTimeLeft--;

            if(!multiplayer)
            {
                string[] uAst = SC_data.GetAsteroid(X,Y).Split(';');
                int c=int.Parse(uAst[0]),a=int.Parse(uAst[1]);
                SC_data.World[a,21+index*2,c]=GrowTimeLeft+"";
                if(uAst[2]=="T") SC_data.SaveAsteroid(c);
            }
        }
        if(IsEmpty) gameObject.GetComponent<Renderer>().enabled=false;
    }
    bool topDistance(float minDis)
    {
        float oX = transform.position.x;
        float oY = transform.position.y;
        float pX,pY,dist;
        int i;
        for(i=0;i<10;i++)
        {
            if(MTPplayers[i].position.z<100f)
            {
                pX=MTPplayers[i].position.x;
                pY=MTPplayers[i].position.y;
                dist=Mathf.Sqrt((oX-pX)*(oX-pX)+(oY-pY)*(oY-pY));
                if(dist<minDis) return false;
            } 
        }
        return true;
    }
    float safeDistance(int id)
    {
        if(id==0) return 100000f; //ERROR
        if(id==2||id==21||id==29) return 2f; //BIGGER OBJECT
        return 2f; //DEFAULT
    }
    void OnMouseOver()
    {
        int slot;
        float oX=transform.position.x, oY=transform.position.y;
		float pX=player.position.x, pY=player.position.y;
		float distance=Mathf.Sqrt((oX-pX)*(oX-pX)+(oY-pY)*(oY-pY));

		if(SC_control.impulse_enabled) return;

        if(IsEmpty&&Communtron3.position.y==0f&&distance<15f&&topDistance(safeDistance(SC_slots.SelectedItem()))&&!Input.GetMouseButton(0)&&Communtron2.position.x==0f&&
        HasPhysicalVersion(SC_slots.SelectedItem())&&MTPblocker<=0)
        {
            if(!Input.GetMouseButton(1)&&emptyShow)
            gameObject.GetComponent<Renderer>().enabled=true;

            if(Input.GetMouseButtonDown(1)&&!ReR)
            {
                ReR=true;
                int hId=SC_slots.SelectedItem();
                slot = SC_slots.InvChange(hId,-1,true,false,false);
                
                if(multiplayer)
                {
                    MTPblocker++;
                    MTPsend(ObjID,ObjID2,hId,hId,-1,slot);
                    SC_control.SendMTP("/FobsPing "+SC_control.connectionID+";"+ID+";"+index);
					SC_control.InvisiblityPulseSend("none");
                }
				SC_control.public_placed = true;
                Replace(hId,multiplayer);
            }
        }
        if(IsStorage&&distance<15f&&Communtron1.position.z==0f&&MTPblocker<=0)
        {
            if(Input.GetMouseButtonDown(0)&&!ReplaceReserved&&
            Communtron2.position.x==0f&&Communtron3.position.y==0f&&SC_Fob21.pub_count==0)
            {
                if(SC_slots.InvHaveB(DropID,1,true,true,true,1))
                {
                    slot = SC_slots.InvChange(DropID,DropCount,true,true,false);

                    if(multiplayer)
                    {
                        MTPblocker++;
                        MTPsend(ObjID,ObjID2,0,DropID,DropCount,slot);
                        SC_control.SendMTP("/FobsPing "+SC_control.connectionID+";"+ID+";"+index);
						SC_control.InvisiblityPulseSend("none");
                    }
                    Replace(0,multiplayer);
                }
            }
        }
        if(PickUp&&distance<15f&&Communtron1.position.z==0f&&!SC_backpack.destroyLock&&SC_push.clicked_on==0)
        {
            if(MTPblocker<=0){
            if(Input.GetMouseButton(0)&&
            Communtron2.position.x==0f&&Communtron3.position.y==0f)
            {
                if(SC_slots.InvHaveB(DropID,1,true,true,true,1))
                {
                    slot = SC_slots.InvChange(DropID,DropCount,true,true,false);

                    if(multiplayer)
                    {
                        MTPblocker++;
                        MTPsend(ObjID,ObjID2,0,DropID,DropCount,slot);
                        SC_control.SendMTP("/FobsPing "+SC_control.connectionID+";"+ID+";"+index);
						SC_control.InvisiblityPulseSend("none");
                    }
                    Replace(0,multiplayer);
                }
            }
            }
        }
        if(IsTreasure&&distance<15f&&Communtron1.position.z==0f)
        {
            if(MTPblocker<=0){
            if(Input.GetMouseButtonDown(0)&&
            Communtron2.position.x==0f&&Communtron3.position.y==0f)
            {
                if(SC_slots.InvHaveB(-1,1,true,true,true,1))
                {
                    string localDrop=getLoot(lootSE3);
                    int ldI=int.Parse(localDrop.Split(';')[0]);
                    int ldC=int.Parse(localDrop.Split(';')[1]);
                    
                    slot = SC_slots.InvChange(ldI,ldC,true,true,false);

                    if(multiplayer)
                    {
                        MTPblocker++;
                        MTPsend(ObjID,ObjID2,0,ldI,ldC,slot);
                        SC_control.SendMTP("/FobsPing "+SC_control.connectionID+";"+ID+";"+index);
						SC_control.InvisiblityPulseSend("none");
                    }
                    Replace(0,multiplayer);
                }
            }
            }
        }
    }
    void OnMouseEnter()
    {
        if(onu==0) emptyShow=true;
        if(IsStorage)
        {
            Communtron4.position+=new Vector3(1f,0f,0f);
        }
        if(PickUp||IsStorage||IsTreasure)
        {
            com1act=true;
            Communtron1.position+=new Vector3(1f,0f,0f);
        }
		if(IsEmpty)
		{
			com1act=true;
            Communtron1.position-=new Vector3(1f,0f,0f);
		}
    }
    void OnMouseExit()
    {
        if(IsStorage)
        {
            Communtron4.position-=new Vector3(1f,0f,0f);
        }
        if(PickUp||IsStorage||IsTreasure)
        {
            com1act=false;
            Communtron1.position-=new Vector3(1f,0f,0f);
        }
		if(IsEmpty)
		{
			com1act=false;
            Communtron1.position+=new Vector3(1f,0f,0f);
		}
    }
}
