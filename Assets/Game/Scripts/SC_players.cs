using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_players : MonoBehaviour
{
    //Constants
    public float dragS, dragM;
    public Material normal,active,turbo,brake;
    public Material M1,M2,M3,M4;
    public SC_snd_start SC_snd_start;
    public SC_fun SC_fun;

    //Relative constants
    public Renderer engine,nose,drillRen;
    public Transform drillPar;
    public Transform drill3T;
    public SC_invisibler SC_invisibler;

    //Less relative constants
    public Transform TestNick;

    //Roots
	public Transform atZ, atS;      //Z - root parents (static)
	public Transform atZZ, atSS;    //S - root artefact effects (clonable)
    public Transform CanvPS;        //PS - root player mini canvas (clonable)
	
    //Auto sets
	Transform Aeffs, Beefs;
    Transform CanvP;
    public int IDP, IDP_phys;

    //Gameplay variables
    public int OtherSource;
	public int ArtSource;
    public Vector3 sourcedPosition = new Vector3(0f,0f,0f);
    public Quaternion sourcedRotation = Quaternion.identity;
    Vector3[] memSourced=new Vector3[20];

    void Awake()
    {	
        int i;
        for(i=0;i<20;i++){
            memSourced[i]=new Vector3(0f,0f,300f);
        }
		
		Aeffs = Instantiate(atS,atS.position,atS.rotation);
		Aeffs.parent = atZ; Aeffs.name = "atS" + IDP;
		Aeffs.GetComponent<SC_seeking>().seek = transform;
		
		Beefs = Instantiate(atSS,atSS.position,atSS.rotation);
		Beefs.parent = atZZ; Beefs.name = "atSS" + IDP;
		Beefs.GetComponent<SC_seeking>().seek = transform;

        CanvP = Instantiate(CanvPS,new Vector3(0f,0f,0f),Quaternion.identity);
        CanvP.SetParent(TestNick,false); CanvP.name = "CanvP" + IDP;
		
		IDP_phys = IDP;
    }
	void Start()
	{
		if(IDP==SC_fun.SC_control.connectionID) IDP=0;
	}
    void ArrayPusher(Vector3 new_push)
    {
        int i;
        for(i=19;i>0;i--){
            memSourced[i]=memSourced[i-1];
        }
        memSourced[0]=new_push;
    }
    Vector3 ArrayAvarge(int n)
    {
        int i,m=n;
        Vector3 sum=new Vector3(0f,0f,0f);
        for(i=0;i<n;i++){
            if(memSourced[i].z<100f)
                sum+=memSourced[i];
            else
                m--;
        }
        if(m!=0) return sum/m;
        else return new Vector3(0f,0f,0f);
    }
    public void AfterFixedUpdate()
    {
		ArrayPusher(sourcedPosition);
        Vector3 avar=ArrayAvarge(SC_fun.smooth_size);
        if(SC_fun.SC_control.NUL[IDP_phys]) transform.position=new Vector3(avar.x,avar.y,0f);
		else transform.position=new Vector3(0f,0f,10000f+IDP*5f);
        transform.rotation=sourcedRotation;

		int guitar=ArtSource;
        int bas=OtherSource;
		
		int A=guitar/100;
		int B=guitar%100;
		
		SC_seeking obj = Aeffs.GetComponent<SC_seeking>();
		SC_seeking obj2 = Beefs.GetComponent<SC_seeking>();
		
		obj2.offset = new Vector3(0f,0f,-450f*B);
		if(B % 2 == 0) Beefs.rotation = transform.rotation;
		else Beefs.rotation = new Quaternion(0f,0f,0f,0f);
		
		if(B==1)
		{
			if(SC_fun.SC_control.ramvis[IDP]>0 && SC_fun.SC_control.ramvis[IDP]<=SC_fun.SC_control.timeInvisiblePulse)
			{
				obj.offset = new Vector3(0f,0f,450f);
				SC_invisibler.visible = true;
			}
			else
			{
				obj.offset = new Vector3(0f,0f,0f);
				SC_invisibler.visible = false;
			}
			Aeffs.rotation = transform.rotation;
			SC_invisibler.invisible = true;
		}
		else
		{
			SC_fun.SC_control.ramvis[IDP]=0;
			obj.offset = new Vector3(0f,0f,-450f*A);
			Aeffs.rotation = new Quaternion(0f,0f,0f,0f);
			SC_invisibler.invisible = false;
		}
		
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
        if(D==1&&drill3T.localPosition.y<1.44f){
	        drill3T.localPosition+=new Vector3(0f,0.05f,0f);
		}
    	if(D==0&&drill3T.localPosition.y>0.46f){
		   	drill3T.localPosition-=new Vector3(0f,0.05f,0f);
	    }
		
		if(SC_invisibler.invisible) drill3T.localPosition = new Vector3(drill3T.localPosition.x,0.45f,drill3T.localPosition.z);
		
		SC_invisibler.LaterUpdate();
		obj.Update();
		obj2.Update();
    }
}
