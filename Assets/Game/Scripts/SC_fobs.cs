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
    public bool IsEmpty, IsStorage, IsTreasure, IsResistant;
    public bool WindObject;
    public bool StaticStorage;

    public int ResistanceSize, ResistanceCounter;
    public Vector3 ResistanceParticleVector;
    public Transform ResistanceParticles;

    public int ShotID, BulletID, GrowID, GeyzerID, ObjID, ObjID2;
    public int GrowTimeMin, GrowTimeMax;
    public int placeSoundID,breakSoundID;
    public string lootSE3;
    
    int GrowTimeLeft=100;
    public int GeyzerTime=0;
    public int inGeyzer=0;

    public SC_control SC_control;
    public SC_asteroid SC_asteroid;
    public SC_backpack SC_backpack;
    public SC_fun SC_fun;
    public SC_data SC_data;
    public SC_sounds SC_sounds;
    public SC_push SC_push;
    public SC_slots SC_slots;
    public SC_snd_loop SC_snd_loop;
    SC_Fob21 SC_Fob21;

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
    bool resisting = false;

    bool localRendererMem = true; //Local temp variable
    bool localRendererMem2 = true; //Memory of renderer state

    Renderer localRenderer;
    SC_asteroid asst;

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
        try {
            int[] marray = new int[5];
            int i,j,rander=UnityEngine.Random.Range(0,10000);
            string[] arra = str.Split('-');
            int lngt=arra.Length/5;
            for(i=0;i<lngt;i++)
            {
                for(j=0;j<5;j++) marray[j]=int.Parse(arra[5*i+j]);
                if(rander>=marray[3]&&rander<=marray[4]) return marray[0]+";"+UnityEngine.Random.Range(marray[1],marray[2]+1);
            }
        } catch (Exception) { return "8;1"; }
        return "8;1";
    }
    void SendPlace(int fob,int slot)
    {
        SC_control.SendMTP("/FobPlace "+SC_control.connectionID+" "+ID+" "+index+" "+fob+" "+slot);
    }
    void SendBreak(int fob,int slot)
    {
        SC_control.SendMTP("/FobBreak "+SC_control.connectionID+" "+ID+" "+index+" "+fob+" "+slot);
    }
    public bool onMSG(string eData)
    {
        if(mother) return false;
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
            return true;
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
            return true;
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
            return true;
        }
        if(arg[0]=="/RetFobsTurn")
        if((arg[2]==ID+"") && (arg[3]==index+"") && MTPblocker<=0)
        {
            int turEnd = int.Parse(arg[4]);
            Replace(turEnd,true);
            return true;
        }
        if(arg[0]=="/RetGrowNow")
        if((arg[1]==ID+"") && (arg[2]==index+"") && GrowTurn)
        {
            Replace(GrowID,true);
            return true;
        }
        if(arg[0]=="/RetGeyzerTurn")
        if((arg[1]==ID+"") && (arg[2]==index+"") && GeyzerTurn)
        {
            Replace(GeyzerID,true);
            return true;
        }
        if(arg[0]=="/RetFobsPing")
        if(arg[1]==(SC_control.connectionID+";"+ID+";"+index))
        {
            MTPblocker--;
            return true;
        }
        return false;
    }
    bool CanBePlaced(int b)
    {
        if(b==48 || b==64 || b==65) return Input.GetKey(KeyCode.LeftControl);
        else return FobInteractable(b);
    }
    bool FobInteractable(int b)
    {
        if(b>=1 && b<=19) return true;
        if(b>=21 && b<=23) return true;
        if(b>=25 && b<=38) return true;
        if(b>=40 && b<=47) return true;
		if(b==48) return true;
		if(b==51) return true;
        if(b>=54 && b<=62 && b%2==0) return true;
        if(b>=64 && b<=70) return true;
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
            WorldData.Load(X,Y);
            WorldData.UpdateFob(index+1,id);
        }

        //In game replace
        GameObject gobT=SC_fun.GenPlaceT[0];
        int tid=id; //tid -> Physical ID

        if((tid<8||tid>11)&&tid!=16&&tid!=30&&tid!=50&&tid!=51&&tid!=66)
        {
            if(tid==21||tid==2) SC_fun.GenPlaceT[tid].name=potFob21Name;
            
			try{
				gobT=Instantiate(SC_fun.GenPlaceT[tid],transform.position,transform.rotation);
			}catch(Exception)
			{
				gobT=Instantiate(SC_fun.GenPlaceT[72],transform.position,transform.rotation);
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
			if(tid==51)
			{
				tud=7;
				rand = UnityEngine.Random.Range(0,2);
			}
            if(tid==66) tud=8;
			
			gobT=Instantiate(SC_fun.GenPlaceM[tud*3+rand],transform.position,transform.rotation);
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
        gobT.transform.localScale = new Vector3(
            gobT.transform.localScale.x * gobT.transform.parent.parent.localScale.x,
            gobT.transform.localScale.y * gobT.transform.parent.parent.localScale.y,
            gobT.transform.localScale.z * gobT.transform.parent.parent.localScale.z
        );
        index = -1;
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

        localRenderer = gameObject.GetComponent<Renderer>();
        asst = transform.parent.GetComponent<SC_asteroid>();

        if(transform.position.z<100f) {mother=false; true_mother=false;}

        if((int)Communtron4.position.y==100)
        {
            multiplayer=true;
        }

        if(IsEmpty)
        {
            localRendererMem = false;
            localRendererMem2 = false;
            localRenderer.enabled=false;
        }
		
        if(!mother)
		{
			X=transform.parent.GetComponent<SC_asteroid>().X;
			Y=transform.parent.GetComponent<SC_asteroid>().Y;
			ID=transform.parent.GetComponent<SC_asteroid>().ID;
		}

		if(IsStorage)
		{
			SC_Fob21 = gameObject.GetComponent<SC_Fob21>();
			SC_Fob21.X=X;
			SC_Fob21.Y=Y;
			SC_Fob21.ID=ID;
			SC_Fob21.uID=index;
			SC_Fob21.StartF();
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

        if(GrowTurn && !mother)
        {
            if(!multiplayer)
            {
                WorldData.Load(X,Y);

                if((WorldData.GetType()%16==6 && (ObjID==5||ObjID==6)) || ObjID==25)
                {
                    int nbt1 = WorldData.GetNbt(index+1,0);
                    if(nbt1==0)
                    {
                        GrowTimeLeft = UnityEngine.Random.Range(GrowTimeMin,GrowTimeMax+1);
                        WorldData.UpdateNbt(index+1,0,GrowTimeLeft);
                    }
                    else GrowTimeLeft = nbt1;
                }
                else GrowTimeLeft=99999999;
            }
            else GrowTimeLeft=99999999;
        }
    }
    void Update()
    {
        if(IsEmpty)
        {
            if(localRendererMem2 != localRendererMem) {
                localRendererMem2 = localRendererMem;
                localRenderer.enabled = localRendererMem;
            }
            localRendererMem=false;
        }

        if(WindObject && !mother)
            mainWind.position = transform.position;

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
        if(mother) return;
        SC_bullet bul = collision.gameObject.GetComponent<SC_bullet>();
		if(
            collision.gameObject.name=="Bullet(Clone)" &&
            !bul.turn_used &&
            bul.controller &&
            bul.gun_owner==0 &&
            bul.mode=="present" &&
            ShotTurn
        ){
			bul.turn_used = true; bul.MakeDestroy(false);
            if((bul.type==BulletID || BulletID==0) && (ShotID!=48 || !bul.virtuall))
            {
                if(multiplayer)
                    SC_control.SendMTP("/ShotTurn "+SC_control.connectionID+" "+ID+" "+index+" "+ObjID+" "+bul.ID);
                else
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
            SC_snd_loop.sound_pos[loopSndID] = transform.position;

        if(mother) return;

        //Geyzer turn
        if(inGeyzer>0 && GeyzerTurn)
        {
            GeyzerTime++;
            if(GeyzerTime>=140)
            {
                if(!multiplayer) { Replace(GeyzerID,multiplayer); return; }
                else SC_control.SendMTP("/GeyzerTurnTry "+SC_control.connectionID+" "+ID+" "+index+" "+GeyzerID);
            }
        }

        //Grow turn
        if(GrowTurn && !multiplayer)
        {
            GrowTimeLeft--;
            if(GrowTimeLeft==0) { Replace(GrowID,multiplayer); return; }

            WorldData.Load(X,Y);
            WorldData.UpdateNbt(index+1,0,GrowTimeLeft);
        }
    }
    bool topDistance(float minDis)
    {
        float oX = transform.position.x;
        float oY = transform.position.y;
        float pX,pY,dist;
        int i;
        for(i=1;i<SC_control.max_players;i++)
        {
            if(SC_control.PL[i].GetComponent<Transform>().position.z<100f)
            {
                pX=SC_control.PL[i].GetComponent<Transform>().position.x;
                pY=SC_control.PL[i].GetComponent<Transform>().position.y;
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
    public bool IsTransitionBlocked()
    {
        return (asst.temporary_blocker);
    }
    void OnMouseOver()
    {
        if(mother) return;
		if(IsTransitionBlocked()) return;

        int slot;
        float oX=transform.position.x, oY=transform.position.y;
		float pX=player.position.x, pY=player.position.y;
		float distance=Mathf.Sqrt((oX-pX)*(oX-pX)+(oY-pY)*(oY-pY));

		if(SC_control.impulse_enabled) return;

        float sdst = safeDistance(SC_slots.SelectedItem());

        if(IsEmpty&&Communtron3.position.y==0f&&distance<15f&&distance>=sdst&&topDistance(sdst)&&!Input.GetMouseButton(0)&&Communtron2.position.x==0f&&
        CanBePlaced(SC_slots.SelectedItem())&&MTPblocker<=0)
        {
            if(!Input.GetMouseButton(1)&&emptyShow)
                localRendererMem=true;

            if(Input.GetMouseButtonDown(1)&&!ReR)
            {
                ReR=true;
                int hId=SC_slots.SelectedItem();
                slot = SC_slots.InvChange(hId,-1,true,false,false);
                
                if(multiplayer)
                {
                    MTPblocker++;
                    SendPlace(hId,slot); //Fob place
                }
				SC_control.public_placed = true;
                Replace(hId,multiplayer);
            }
        }
        if(PickUp&&distance<15f&&Communtron1.position.z==0f&&!SC_backpack.destroyLock&&SC_push.clicked_on==0&&FobInteractable(ObjID)&&MTPblocker<=0)
        {
            if(Input.GetMouseButton(0)&&
            Communtron2.position.x==0f&&Communtron3.position.y==0f)
            {
                if(SC_slots.InvHaveB(DropID,1,true,true,true,1))
                {
                    slot = SC_slots.InvChange(DropID,DropCount,true,true,false);

                    if(multiplayer)
                    {
                        MTPblocker++;
                        SendBreak(ObjID,slot); //Fob break
                    }
                    Replace(0,multiplayer);
                }
            }
        }
        if(IsStorage&&!StaticStorage&&distance<15f&&Communtron1.position.z==0f&&FobInteractable(ObjID)&&MTPblocker<=0)
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
                        SendBreak(ObjID,slot); //Storage break
                    }
                    Replace(0,multiplayer);
                }
            }
        }
        if(IsTreasure&&distance<15f&&Communtron1.position.z==0f&&FobInteractable(ObjID)&&MTPblocker<=0)
        {
            if(Input.GetMouseButtonDown(0)&&
            Communtron2.position.x==0f&&Communtron3.position.y==0f)
            {
                if(SC_slots.InvHaveB(-1,1,true,true,true,1))
                {
                    string localDrop;
                    if((int)Communtron4.position.y!=100)
                    {
                        localDrop = getLoot(lootSE3);
                    }
                    else
                    {
                        if(ObjID==37) //normal treasure
                        {
                            if(SC_control.TreasureAllowed.Count>0)
                            {
                                localDrop = SC_control.TreasureAllowed[0];
                                SC_control.TreasureAllowed.RemoveAt(0);
                            }
                            else localDrop = "X";
                        }
                        else //dark treasure
                        {
                            if(SC_control.DarkTreasureAllowed.Count>0)
                            {
                                localDrop = SC_control.DarkTreasureAllowed[0];
                                SC_control.DarkTreasureAllowed.RemoveAt(0);
                            }
                            else localDrop = "X";
                        }
                    }
                    if(localDrop!="X")
                    {
                        int ldI=int.Parse(localDrop.Split(';')[0]);
                        int ldC=int.Parse(localDrop.Split(';')[1]);
                    
                        slot = SC_slots.InvChange(ldI,ldC,true,true,false);

                        if(multiplayer)
                        {
                            MTPblocker++;
                            SendBreak(ObjID,slot); //Treasure break
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
        resisting = false;
    }
}
