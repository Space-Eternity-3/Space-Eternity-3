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
    public SC_control SC_control;

    public int age = 0;
    public int max_age = 100;
    bool controller = false;
    bool multiplayer = false;

    public SC_bullet Shot(Vector3 position, Vector3 vector, Vector3 delta, int typ)
    {
        SC_bullet gob = Instantiate(gameObject, position, Quaternion.identity).GetComponent<SC_bullet>();
        gob.type = typ;
        gob.mode = "present";
        gob.controller = true;
        gob.st_vect = SC_fun.Skop(vector, bullet_speed * speed[typ]) + delta;
        gob.ID = UnityEngine.Random.Range(0,1000000000);

        ShotProjection(
            position,
            gob.st_vect,
            typ,
            "projection",
            gob.ID
        );

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
    public SC_bullet ShotProjection(Vector3 position, Vector3 vector, int typ, string mod, int idd)
    {
        SC_bullet gob = Instantiate(gameObject, position, Quaternion.identity).GetComponent<SC_bullet>();
        gob.type = typ;
        gob.mode = mod;
        gob.controller = false;
        gob.st_vect = vector;
        gob.ID = idd;

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
        if (mode == "mother")
        {
            multiplayer = (int)Communtron4.position.y==100;
            return;
        }

        if(mode=="projection")
        {
            if (type != 3) SC_sounds.PlaySound(transform.position, 2, 1);
            else SC_sounds.PlaySound(transform.position, 2, 17);

            bulletRE.material = BulletMaterials[type];
            if (type == 3)
            {
                Transform trn = Instantiate(Bullet3Effect, transform.position, Quaternion.identity);
                trn.parent = transform;
            }
        }
        else bulletRE.material = BulletMaterials[0];
    }
    void FixedUpdate()
    {
        if (mode == "mother") return;
        InstantMove(1);
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
            neme == "Asteroid(Clone)" &&
            !controller
        ))
        {
            MakeDestroy(mode=="projection");
        }

    }
}
