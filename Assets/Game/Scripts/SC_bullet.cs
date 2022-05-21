using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_bullet : MonoBehaviour
{
    public Vector3 st_vect;
    public int type;
    public string mode = "mother";  //[mother, present, projection, server]
    public bool priv = true;
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

    int lifetime = 0;

    public void Shot(Vector3 position, Vector3 vector, Vector3 delta, int typ, string mod, bool pri)
    {
        SC_bullet gob = Instantiate(gameObject, position, Quaternion.identity).GetComponent<SC_bullet>();
        gob.type = typ;
        gob.mode = mod;
        gob.priv = pri;
        gob.st_vect = SC_fun.Skop(vector, bullet_speed * speed[typ]) + delta;
        gob.ID = UnityEngine.Random.Range(0,1000000000);
        if((int)Communtron4.position.y==100)
            SC_control.SendMTP("/NewBulletSend "+SC_control.connectionID+" "+typ+" "+position.x+";"+position.y+" "+gob.st_vect.x+";"+gob.st_vect.y+" "+gob.ID);
    }
    public void MakeDestroy()
    {
        if(type==1 || type==2) SC_sounds.PlaySound(transform.position, 2, 1);
        if(type==3) SC_sounds.PlaySound(transform.position, 2, 15);
        
        Instantiate(BulletEnd[type], transform.position, new Quaternion(0f, 0f, 0f, 0f));
        Destroy(gameObject);
    }
    Vector3 GetActualVect(Vector3 V)
    {
        return V;
    }
    void Start()
    {
        if (mode == "mother") return;

        if (type != 3) SC_sounds.PlaySound(transform.position, 2, 1);
        else SC_sounds.PlaySound(transform.position, 2, 17);

        bulletRE.material = BulletMaterials[type];
        if (type == 3)
        {
            Transform trn = Instantiate(Bullet3Effect, transform.position, Quaternion.identity);
            trn.parent = transform;
        }
    }
    void FixedUpdate()
    {
        if (mode == "mother") return;

        transform.position += GetActualVect(st_vect);

        lifetime++;
        if (lifetime >= 100) MakeDestroy();
    }
    void OnTriggerEnter(Collider collision)
    {
        string neme = collision.gameObject.name;
        string nume = collision.gameObject.transform.root.gameObject.name;
        bool physical = priv && mode=="present";
        
        if ((
            neme[0] != 'S' &&
            nume[0] != 'P' &&
            Array.IndexOf(SafeNames, neme) == -1 &&
            physical
        ) || (
            neme == "Asteroid(Clone)" &&
            !physical
        ))
        {
            MakeDestroy();
        }

    }
}
