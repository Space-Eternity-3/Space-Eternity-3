using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_bullet : MonoBehaviour
{
    public Vector3 st_vect;
    public int type;
    public string mode = "mother";  //[mother, present, projection, server]
    public int gun_owner = 0; // <0: boss =0 player sgp/mtp ! only for controllers
    public int ID = 0;
    public bool dev_bullets_show = false;

    public float bullet_speed;
    public string[] SafeNames = new string[0];

    public Renderer bulletRE;
    public Transform Communtron4;

    public Transform Bullet3Effect;

    public int[] StartSounds = new int[16];
    public Material[] BulletMaterials = new Material[16];
    public Transform[] BulletEffects = new Transform[16];
    public int[] LoopSounds = new int[16];
    public int[] EndSounds = new int[16];
    public Transform[] BulletEnd = new Transform[16];

    public SC_fun SC_fun;
    public SC_sounds SC_sounds;
    public SC_snd_loop SC_snd_loop;
    public SC_control SC_control;
    public SC_seek_data SC_seek_data;

    public int age = 0;
    public int max_age = 100;
    public int delta_age = 0;
    public int float_age = 0;
    public int start_tick = -1000;
    public int seekPointer = -1;
    public bool turn_used = false;
    public bool controller = false;
    public string destroy_mode = "";
    bool multiplayer = false;
    bool destroyed = false;
    public bool boss_damaged = false;
    public bool block_graphics = false;
    int looper = 0;
    int loopSndID = -1;

    public string projectionOwner="";

    //pre-defined values
    public float normal_damage = 0;
    public bool is_unstable = false;

    public SC_bullet Shot(Vector3 position, Vector3 vector, Vector3 delta, int typ, int gun_own)
    {
        SC_bullet gob = Instantiate(gameObject, position, Quaternion.identity).GetComponent<SC_bullet>();
        SC_control.SC_lists.AddTo_SC_bullet(gob);
        gob.type = typ;
        gob.mode = "present";
        gob.gun_owner = gun_own;
        gob.controller = true;
        gob.st_vect = SC_fun.Skop(vector, bullet_speed) + delta;
        gob.ID = UnityEngine.Random.Range(0,1000000000);

        if(!multiplayer && (gob.type==9 || gob.type==10))
            gob.seekPointer = SC_seek_data.createSeekPointer(gob.ID);

        if(typ==3) gob.is_unstable = true;
        else gob.is_unstable = false;
        if(gun_own==0)
        {
            //Player pre-define
            if(typ==1) gob.normal_damage = float.Parse(SC_fun.SC_data.Gameplay[3]);
            if(typ==2) gob.normal_damage = float.Parse(SC_fun.SC_data.Gameplay[27]);
            if(typ==3) gob.normal_damage = float.Parse(SC_fun.SC_data.Gameplay[28]);
            /*if(!gob.is_unstable)*/ gob.normal_damage *= Mathf.Pow(1.08f,SC_control.SC_upgrades.MTP_levels[3]);
        }
        else
        {
            //Boss pre-define
            gob.normal_damage = SC_fun.boss_damages[typ] * float.Parse(SC_fun.SC_data.Gameplay[32]);
        }

        SC_bullet bul = ShotProjection(
            position,
            gob.st_vect,
            typ,
            "projection",
            gob.ID,
            SC_control.connectionID+""
        );
        bul.delta_age = -1000; //just very low

        if(multiplayer)
            SC_control.SendMTP(
                "/NewBulletSend "+
                SC_control.connectionID+" "+
                typ+" "+
                position.x+" "+position.y+" "+
                gob.st_vect.x+" "+gob.st_vect.y+" "+
                gob.ID+" "+gob.normal_damage
            );
        
        return gob;
    }
    public SC_bullet ShotProjection(Vector3 position, Vector3 vector, int typ, string mod, int idd, string pown)
    {
        SC_bullet gob = Instantiate(gameObject, position, Quaternion.identity).GetComponent<SC_bullet>();
        SC_control.SC_lists.AddTo_SC_bullet(gob);
        gob.type = typ;
        gob.mode = mod;
        gob.controller = false;
        gob.st_vect = vector;
        gob.ID = idd;
        gob.projectionOwner = pown;

        return gob;
    }
    public void CheckAge()
    {
        if (age >= max_age)
        {
            MakeDestroy(mode=="projection");
            return;
        }
    }
    public void InstantMove(int n)
    {
        int i;
        for(i=0;i<n;i++)
        {
            //Singleplayer seek add
            if(!multiplayer && controller && seekPointer!=-1)
            {
                //A few frames before action, so you can put adding here safely
                if(SC_control.transform.position.z<100f && (!SC_control.GetComponent<SC_invisibler>().invisible || SC_control.SC_effect.effect!=0))
                {
                    Vector3 dif_v = transform.position - SC_control.transform.position;
                    float angle = SC_fun.AngleBetweenVectorAndOX(st_vect.x,st_vect.y) - SC_fun.AngleBetweenVectorAndOX(dif_v.x,dif_v.y);
                    while(angle<-180f) angle+=360f;
                    while(angle>180f) angle-=360f;
                    if(angle>0) SC_seek_data.steerData[seekPointer]+="L";
                    else SC_seek_data.steerData[seekPointer]+="P";
                }
                else SC_seek_data.steerData[seekPointer]+="0";
            }

            //Seek update
            char c = '0';
            if(seekPointer!=-1)
                c = SC_seek_data.tryGet(seekPointer,age);
            if(c!='0')
            {
                float[] get = new float[2];
                if(c=='L') get = SC_fun.RotateVector(st_vect.x,st_vect.y,SC_fun.seek_default_angle);
                if(c=='P') get = SC_fun.RotateVector(st_vect.x,st_vect.y,-SC_fun.seek_default_angle);
                st_vect = new Vector3(get[0],get[1],0f);
                BulletRotate();
            }

            //Age change
            age++;
            CheckAge();
            transform.position += st_vect;
        }
    }
    public void MakeDestroy(bool graphics)
    {
        if(destroyed) return;
        destroyed = true;

        if(loopSndID!=-1) SC_snd_loop.RemoveFromLoop(loopSndID);

        if(controller && multiplayer)
            SC_control.SendMTP(
                "/NewBulletRemove "+
                SC_control.connectionID+" "+
                ID+" "+
                age
            );
        if(controller)
        {
            List<SC_bullet> buls = SC_control.SC_lists.SC_bullet;
            foreach(SC_bullet bul in buls)
            {
                if(bul.ID==ID && bul.mode!="mother" && !bul.controller)
                {
                    if(block_graphics) bul.destroy_mode = "false";
                    bul.max_age = age;
                    bul.CheckAge();
                }
            }
        }
        Destroy(gameObject);

        if(!graphics) return;
        if(destroy_mode=="false" && !SC_fun.force_destroy_effect[type]) return;

        if(EndSounds[type]!=-1) SC_sounds.PlaySound(transform.position, 2, EndSounds[type]); // 1 1 15
        if(BulletEnd[type]!=null) Instantiate(BulletEnd[type], transform.position, new Quaternion(0f, 0f, 0f, 0f));
    }
    bool techstarted = false;
    void TechStart()
    {
        if(techstarted) return;
        techstarted = true;

        //start tick set
        start_tick = SC_control.current_tick - age;

        //seek check
        if(seekPointer==-1 && (type==9 || type==10))
            seekPointer = SC_seek_data.getSeekPointer(ID);
        if(seekPointer!=-1) max_age = 250;
    }
    void BulletRotate()
    {
        if(type==10) return;
        if(st_vect.x==0f) transform.eulerAngles = new Vector3(0f,0f,90f);
        else transform.eulerAngles = new Vector3(0f,0f,180f*Mathf.Atan(st_vect.y/st_vect.x)/Mathf.PI);
        if(st_vect.x<0 || (st_vect.x==0 && st_vect.y<0)) transform.eulerAngles += new Vector3(0f,0f,180f);
    }
    void Start()
    {
        multiplayer = (int)Communtron4.position.y==100;
        if(mode=="mother") return;

        TechStart();

        //bullet scaling
        float r2 = SC_fun.other_bullets_colliders[type];
        float R2 = 0.32f + r2;
        float u2 = r2 / R2;
        transform.localScale = 2*new Vector3(R2,R2,R2*5f/8f);
        transform.GetComponent<SphereCollider>().radius = u2/2;

        BulletRotate();

        if(mode=="projection")
        {
            if(StartSounds[type]!=-1) SC_sounds.PlaySound(transform.position,2,StartSounds[type]); //1 1 17
            if(BulletMaterials[type]!=null) bulletRE.material = BulletMaterials[type];
            else bulletRE.enabled = false;
            if(BulletEffects[type]!=null)
            {
                Transform trn = Instantiate(BulletEffects[type], transform.position, transform.rotation);
                trn.parent = transform;
            }
            if(LoopSounds[type]!=-1) loopSndID = SC_snd_loop.AddToLoop(LoopSounds[type],transform.position); // x x 2
        }
        else if(dev_bullets_show) bulletRE.material = BulletMaterials[0];
        else bulletRE.enabled = false;
    }
    public void AfterFixedUpdate()
    {
        if(mode=="mother") return;

        TechStart();

        //Server lag, stop bullet
        if(seekPointer==-1 && SC_control.current_tick + 3 < start_tick + age && start_tick>=0) return;
        if(seekPointer!=-1 && SC_seek_data.steerData[seekPointer].Length<=age) return;

        //Auto-steering bullets can't go into future (any bullets)
        //if(seekPointer!=-1 && delta_age>0) delta_age=0;

        if(loopSndID!=-1) SC_snd_loop.sound_pos[loopSndID] = transform.position;

        looper++;
        if(looper>=3) looper=0; //Start: 1-2-0-1-2-0...

        if(mode=="present" || mode=="server") InstantMove(1);
        if(mode=="projection")
        {
            if(delta_age == 0 || !multiplayer)
            {
                transform.position -= float_age*(st_vect/3);
                float_age = 0;
                InstantMove(1);
            }
            else if(delta_age > 0)
            {
                InstantMove(2);
                delta_age--;
            }
            else if(delta_age < 0)
            {
                transform.position += 2*(st_vect/3);
                float_age+=2;
                delta_age++;
            }
            while(float_age >= 3)
            {
                transform.position -= st_vect;
                float_age-=3;
                delta_age--;
                InstantMove(1);
            }
        }
    }
    void OnTriggerEnter(Collider collision)
    {
        string neme = collision.gameObject.name;
        string nume = collision.gameObject.transform.root.gameObject.name;

        bool aBossScaled0 = false; //Boss collider blocker
        if(neme=="aBossScaled0") {
            SC_boss bos = collision.gameObject.GetComponent<SC_colboss>().SC_boss;
            aBossScaled0 = (multiplayer || bos.dataID[2]!=2);
        }
        
        if(gun_owner==0)
        {
            if(
                neme[0] != 'S' &&
                nume[0] != 'P' &&
                !aBossScaled0 &&
                Array.IndexOf(SafeNames, neme) == -1 &&
                controller
            )
            MakeDestroy(false);
        }
        else
        {
            if(
                neme=="Seon_special_collider" &&
                controller
            )
            MakeDestroy(false);
        }
    }
    void OnDestroy()
    {
        SC_control.SC_lists.RemoveFrom_SC_bullet(this);
    }
}
