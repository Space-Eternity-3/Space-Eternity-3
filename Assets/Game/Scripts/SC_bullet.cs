using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_bullet : MonoBehaviour
{
    public Vector3 st_vect;
    public int type;
    public string mode = "mother";  //[mother, present, projection, server]
    public int ID = 0;
    public bool dev_bullets_show = false;

    public float bullet_speed;
    public float[] speed = new float[5];
    public string[] SafeNames = new string[0];

    public Renderer bulletRE;
    public Transform Bullet3Effect;
    public Transform[] BulletEnd = new Transform[5];
    public Transform Communtron4;
    public Material[] BulletMaterials = new Material[5];

    public SC_fun SC_fun;
    public SC_sounds SC_sounds;
    public SC_snd_loop SC_snd_loop;
    public SC_control SC_control;

    public int age = 0;
    public int max_age = 100;
    public int delta_age = 0;
    public int float_age = 0;
    bool controller = false;
    bool multiplayer = false;
    bool destroyed = false;
    int looper = 0;
    int loopSndID = -1;

    public string projectionOwner="";

    public SC_bullet Shot(Vector3 position, Vector3 vector, Vector3 delta, int typ)
    {
        SC_bullet gob = Instantiate(gameObject, position, Quaternion.identity).GetComponent<SC_bullet>();
        gob.type = typ;
        gob.mode = "present";
        gob.controller = true;
        gob.st_vect = SC_fun.Skop(vector, bullet_speed * speed[typ]) + delta;
        gob.ID = UnityEngine.Random.Range(0,1000000000);

        SC_bullet bul = ShotProjection(
            position,
            gob.st_vect,
            typ,
            "projection",
            gob.ID,
            SC_control.connectionID+""
        );
        bul.delta_age = -1000; //undefined very low

        if(multiplayer)
            SC_control.SendMTP(
                "/NewBulletSend "+
                SC_control.connectionID+" "+
                typ+" "+
                position.x+" "+position.y+" "+
                gob.st_vect.x+" "+gob.st_vect.y+" "+
                gob.ID
            );
        
        return gob;
    }
    public SC_bullet ShotProjection(Vector3 position, Vector3 vector, int typ, string mod, int idd, string pown)
    {
        SC_bullet gob = Instantiate(gameObject, position, Quaternion.identity).GetComponent<SC_bullet>();
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
            SC_bullet[] buls = FindObjectsOfType<SC_bullet>();
            foreach(SC_bullet bul in buls)
            {
                if(bul.ID==ID && bul.mode!="mother" && !bul.controller)
                {
                    bul.max_age = age;
                    bul.CheckAge();
                }
            }
        }
        Destroy(gameObject);

        if(!graphics) return;
        if(type==1 || type==2) SC_sounds.PlaySound(transform.position, 2, 1);
        if(type==3) SC_sounds.PlaySound(transform.position, 2, 15);

        Instantiate(BulletEnd[type], transform.position, new Quaternion(0f, 0f, 0f, 0f));
    }
    void Start()
    {
        multiplayer = (int)Communtron4.position.y==100;
        if (mode == "mother") return;

        if(mode=="projection")
        {
            if (type != 3) SC_sounds.PlaySound(transform.position, 2, 1);
            else SC_sounds.PlaySound(transform.position, 2, 17);

            bulletRE.material = BulletMaterials[type];
            if (type == 3)
            {
                Transform trn = Instantiate(Bullet3Effect, transform.position, Quaternion.identity);
                trn.parent = transform;
                loopSndID = SC_snd_loop.AddToLoop(2,transform.position);
            }
        }
        else if(dev_bullets_show) bulletRE.material = BulletMaterials[0];
        else bulletRE.enabled = false;
    }
    public void AfterFixedUpdate()
    {
        if(mode=="mother") return;

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
        
        if ((
            neme[0] != 'S' &&
            nume[0] != 'P' &&
            Array.IndexOf(SafeNames, neme) == -1 &&
            controller
        ) || (
            (neme == "Asteroid(Clone)" || neme == "StWall(Clone)") &&
            !controller
        ))
        {
            MakeDestroy(mode=="projection");
        }

    }
}
