using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_arm : MonoBehaviour
{
    public float angular_limit;
    public float frame_step;
    public bool arm_end;
    public float vel_min, vel_max;

    public List<SC_arm> FriendList = new List<SC_arm>();

    public float counter = 0;
    public float vel = 0;

    void VarSet(string var,float set)
    {
        if(var=="vel") {
            vel = set;
            foreach(SC_arm arm in FriendList) {
                arm.vel = set;
            }
        }
        if(var=="counter") {
            counter = set;
            foreach(SC_arm arm in FriendList) {
                arm.counter = set;
            }
        }
    }
    void Start()
    {
        if(arm_end) {
            VarSet("counter",UnityEngine.Random.Range(0,10000));
            VarSet("vel",UnityEngine.Random.Range(vel_min,vel_max+1));
        }
    }
    void FixedUpdate()
    {
        if(!arm_end) return;

        int rnd = UnityEngine.Random.Range(0,3);
        switch(rnd) {
            case 1: vel++; break;
            case 2: vel--; break;
        }
        if(vel<vel_min) vel=vel_min;
        if(vel>vel_max) vel=vel_max;

        VarSet("vel",vel);
    }
    void Update()
    {
        counter += vel * Time.deltaTime * 50f;
        float ang = angular_limit*Mathf.Sin(counter*frame_step);
        if(!arm_end) transform.localEulerAngles = new Vector3(0f,0f,ang);
        else transform.localEulerAngles = new Vector3(-90f-ang,90f,-90f);
    }
}
