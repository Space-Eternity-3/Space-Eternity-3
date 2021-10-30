using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_players : MonoBehaviour
{
    public float dragS, dragM;
    public Rigidbody playerR;
    public Transform sourced;
    public Rigidbody sourcedR;
    public Renderer engine,nose,drillRen;
    public Material normal,active,turbo,brake;
    public Material M1,M2,M3,M4;
    public Transform drillPar;
    public Transform drill3T;
    Vector3[] memSourced=new Vector3[20];
    
    public SC_snd_start SC_snd_start;
    public SC_fun SC_fun;

    void Start()
    {
        int i;
        for(i=0;i<20;i++){
            memSourced[i]=sourced.position;
        }
    }
    void ArrayPusher()
    {
        int i;
        for(i=19;i>0;i--){
            memSourced[i]=memSourced[i-1];
        }
        memSourced[0]=sourced.position;
    }
    Vector3 ArrayAvarge(int n)
    {
        int i,m=n;
        Vector3 sum=new Vector3(0f,0f,0f);
        for(i=0;i<n;i++){
            if(memSourced[i].z<100f)
            sum+=memSourced[i];
            else m--;
        }
        if(m!=0) return sum/m;
        else return new Vector3(0f,0f,0f);
    }
    void FixedUpdate()
    {
        Vector3 avar=ArrayAvarge(SC_fun.smooth_size);
        transform.position=new Vector3(avar.x,avar.y,memSourced[0].z);
        transform.rotation=sourced.rotation;
        //playerR.velocity=sourcedR.velocity;
        ArrayPusher();

        //DRAG
        //float X=-dragS*playerR.velocity.x*Mathf.Abs(playerR.velocity.x)-dragM*playerR.velocity.x;
		//float Y=-dragS*playerR.velocity.y*Mathf.Abs(playerR.velocity.y)-dragM*playerR.velocity.y;
		//playerR.velocity=new Vector3(playerR.velocity.x+X,playerR.velocity.y+Y,0f);

        int bas=(int)(Mathf.Round(memSourced[0].z*10000f));
        int M=bas/16;
        if(M>3) M=0;
        if(SC_snd_start.enMode!=M)
        {
            SC_snd_start.enMode=M;
            if(M!=0) SC_snd_start.ManualStartLoop(M-1);
            else SC_snd_start.ManualEndLoop();
        }
        switch(M)
        {
            case 0:
                engine.material=normal;
                break;
            case 1: 
                engine.material=active;
                break;
            case 2:
                engine.material=turbo;
                break;
            case 3:
                engine.material=brake;
                break;
        }
        int P=(bas%16)/4;
        if(P>3) P=3;
        switch(P)
        {
            case 0:
                nose.material=M1;
                drillRen.material=M1;
                break;
            case 1:
                nose.material=M2;
                drillRen.material=M2;
                break;
            case 2:
                nose.material=M3;
                drillRen.material=M3;
                break;
            case 3:
                nose.material=M4;
                drillRen.material=M4;
                break;
        }
        if(bas%2==0) drillPar.localPosition=new Vector3(0f,1.9f,-1000f);
        else drillPar.localPosition=new Vector3(0f,1.9f,0f);

        int D=(bas%4)/2;
        if(D==1&&drill3T.localPosition.y<1.4f){
	        drill3T.localPosition+=new Vector3(0f,0.05f,0f);
		}
    	if(D==0&&drill3T.localPosition.y>0.45f){
		   	drill3T.localPosition-=new Vector3(0f,0.05f,0f);
	    }
    }
}
