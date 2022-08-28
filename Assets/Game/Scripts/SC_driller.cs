using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_driller : MonoBehaviour
{
    public Transform Communtron4;
    public Transform Drill, Particles;
    public Renderer noseR, drillR;
    public Material passiveDrill, activeDrill;
    
    public SC_Fob21 SC_Fob21;
    public SC_asteroid SC_asteroid;
    public SC_data SC_data;
    public SC_fobs SC_fobs;

    public float hiddenPos;
    public float colorPos;
    public float activePos;
    public float speed;

    bool mother = true;
    bool active = false;
    bool in_asteroid = false;
    bool drilling = false;

    int drillTime=10000;

    void Start()
    {
        if(transform.position.z < 100f) mother = false;
        drillTime = GetTimeDrill();
    }
    void Update()
    {
        if(mother) return;

        if(SC_Fob21.pub_count<5f) active = true;
        else active = false;

        if(in_asteroid)
        {
            noseR.material = activeDrill;
            drillR.material = activeDrill;
        }
        else
        {
            noseR.material = passiveDrill;
            drillR.material = passiveDrill;
        }
        if(drilling)
        {
            Particles.localPosition = new Vector3(0f,0f,0f);
        }
        else
        {
            Particles.position = new Vector3(0f,0f,10000f);
        }
    }
    int GetTimeDrill() {return UnityEngine.Random.Range(180,420);}
    int GetMined()
    {
        string[] uAst = SC_data.GetAsteroid(SC_fobs.X,SC_fobs.Y).Split(';');
        int c=int.Parse(uAst[0]), a=int.Parse(uAst[1]);
        int get_type = int.Parse(SC_data.World[a,0,c]);
        if(get_type<0) get_type = 0;
        else get_type = get_type % 16;
        int ret = SC_asteroid.SetLoot(get_type,true);
        return ret;
    }
    void FixedUpdate()
    {
        if(!mother)
        {
            if(active && Drill.localPosition.y > activePos) Drill.localPosition -= new Vector3(0f,speed,0f);
            if(!active && Drill.localPosition.y < hiddenPos) Drill.localPosition += new Vector3(0f,speed,0f);
        
            if(Drill.localPosition.y <= colorPos) in_asteroid = true;
            else in_asteroid = false;

            if(active && Drill.localPosition.y <= activePos) drilling = true;
            else drilling = false;

            if((int)Communtron4.position.y==100) return;
            if(drilling && drillTime==999999) drillTime = GetTimeDrill();
            if(drilling && drillTime<=0)
            {
                drillTime = 1000000;
                int mined = GetMined();
                if(mined!=0) SC_Fob21.Fob2Drilled(mined);
            }
            if(!drilling) drillTime = 1000000;
            drillTime--;
        }
    }
}
