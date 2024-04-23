using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_players : MonoBehaviour
{
    //Constants
    public Material normal,active,turbo,brake;
    public Material M1,M2,M3,M4;
    public SC_fun SC_fun;
    public SC_control SC_control;

    //Relative constants
    public Renderer engine,nose,drillRen;
    public Transform drillPar;
    public Transform drill3T;
    public SC_invisibler SC_invisibler;

    //Less relative constants
    public Transform TestNick;

    //Roots
	public Transform atZ, atZZ;      //Z - root parents (static)
	public Transform atS, atSS;    //S - root artefact effects (clonable)
    public Transform eZ, eS;        //S - root effect objects (clonable)
    public Transform CanvPS;        //PS - root player mini canvas (clonable)
	
    //Auto sets
	Transform Aeffs, Beefs, Ceffs;
    Transform CanvP;
    public int IDP, IDP_phys;
    SC_seeking obj, obj2, obj3;
    SC_bar_game SC_bar_game;

    //Gameplay variables
    public int OtherSource;
	public int ArtSource;
    public Vector3 sourcedPosition = new Vector3(0f,0f,300f);
    public float sourcedRotation = 10000f;
    Vector3[] memSourced=new Vector3[20];
    float[] rotSourced=new float[20];
    bool sleeping = false;

    void ResetArray()
    {
        int i;
        for(i=0;i<20;i++){
            memSourced[i]=new Vector3(0f,0f,300f);
            rotSourced[i]=10000f;
        }
    }
    public void B_Awake()
    {
        SC_control.SC_lists.AddTo_SC_players(this);
        ResetArray();

		Aeffs = Instantiate(atS,atS.position,atS.rotation);
		Aeffs.parent = atZ; Aeffs.name = "atS" + IDP;
		Aeffs.GetComponent<SC_seeking>().seek = transform;
		
		Beefs = Instantiate(atSS,atSS.position,atSS.rotation);
		Beefs.parent = atZZ; Beefs.name = "atSS" + IDP;
		Beefs.GetComponent<SC_seeking>().seek = transform;

        Ceffs = Instantiate(eS,eS.position,eS.rotation);
		Ceffs.parent = eZ; Ceffs.name = "eS" + IDP;
		Ceffs.GetComponent<SC_seeking>().seek = transform;

        CanvP = Instantiate(CanvPS,new Vector3(0f,0f,0f),Quaternion.identity);
        CanvP.SetParent(TestNick,false); CanvP.name = "CanvP" + IDP;
        CanvP.localPosition = new Vector3(0f,-50f,0f);

        foreach(Transform trn in CanvP) {
            if(trn.name=="HPBar") SC_bar_game = trn.GetComponent<SC_bar_game>();
        }
		
		IDP_phys = IDP;
    }
    void OnDestroy()
    {
        SC_control.SC_lists.RemoveFrom_SC_players(this);
    }
	public void B_Start()
	{
		if(IDP==SC_fun.SC_control.connectionID) IDP=0;

        obj = Aeffs.GetComponent<SC_seeking>();
		obj2 = Beefs.GetComponent<SC_seeking>();
        obj3 = Ceffs.GetComponent<SC_seeking>();
	}
    void ArrayPusher(Vector3 new_push, float new_rot)
    {
        int i;
        for(i=3;i>0;i--){
            memSourced[i]=memSourced[i-1];
            rotSourced[i]=rotSourced[i-1];
        }
        memSourced[0]=new_push;
        rotSourced[0]=new_rot;
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
    float reduceAngle(float angle)
    {
        while(angle<0) angle+=360f;
        while(angle>=360) angle-=360f;
        return angle;
    }
    public float rotAvg(int weight1, float angle1, float angle2)
    {
        float sr;
        angle1 = reduceAngle(angle1);
        angle2 = reduceAngle(angle2);

        sr = (weight1*angle1 + angle2)/(weight1 + 1);
        if(Mathf.Abs(angle2-angle1) <= 180f) return sr;

        angle1 += 180f; angle1 = reduceAngle(angle1);
        angle2 += 180f; angle2 = reduceAngle(angle2);

        sr = (weight1*angle1 + angle2)/(weight1 + 1) - 180f;
        return reduceAngle(sr);
    }
    float ArrayAvarge_rot(int n)
    {
        int i,m=0;
        float currentAngle = 0f;

        for(i=0;i<n;i++)
        {
            if(rotSourced[i]!=10000f)
            {
                currentAngle = rotAvg(m,currentAngle,rotSourced[i]);
                m++;
            }
        }

        return currentAngle;
    }
    public void AfterFixedUpdate()
    {
        bool actual = SC_fun.SC_control.NUL[IDP_phys];
        bool inrange = true;
        if(actual)
        {
            sleeping = false;
            if(inrange)
            {
                ArrayPusher(sourcedPosition, sourcedRotation);
                Vector3 avar=ArrayAvarge(4);
                float avar_rot=ArrayAvarge_rot(4);

                transform.position = new Vector3(avar.x,avar.y,0f);
                transform.eulerAngles = new Vector3(0f,0f,avar_rot);
            }
            else
            {
                transform.position = sourcedPosition;
                obj.offset = new Vector3(0f,0f,0f);
                obj2.offset = new Vector3(0f,0f,0f);
                obj3.offset = new Vector3(0f,0f,0f);

                if(ArtSource%25==1) {
			        SC_invisibler.visible = (SC_fun.SC_control.ramvis[IDP]>0 && SC_fun.SC_control.ramvis[IDP]<=SC_fun.SC_control.timeInvisiblePulse);
			        SC_invisibler.invisible = true;
		        }
		        else {
			        SC_fun.SC_control.ramvis[IDP]=0;
			        SC_invisibler.invisible = false;
		        }
                SC_invisibler.LaterUpdate();
                return;
            }
        }
		else
        {
            if(!sleeping)
            {
                ResetArray();
                transform.position = new Vector3(0f,0f,10000f+IDP*5f);

                SC_fun.SC_control.ramvis[IDP]=0;
			    obj.offset = new Vector3(0f,0f,0f);
                obj2.offset = new Vector3(0f,0f,0f);
                obj3.offset = new Vector3(0f,0f,0f);
			    SC_invisibler.invisible = false;

                drill3T.localPosition = new Vector3(drill3T.localPosition.x,0.45f,drill3T.localPosition.z);
                sleeping = true;
            }
            return;
        }

		int guitar=ArtSource;
        int bas=OtherSource;
		
		int A=guitar/100;
		int B=guitar%100;
        int C=B/25; B=B%25; //effect parasite division
		
		obj2.offset = new Vector3(0f,0f,-450f*B);
		if(B % 2 == 0) Beefs.rotation = transform.rotation;
		else Beefs.rotation = new Quaternion(0f,0f,0f,0f);

        if(C==1) C=5;
        if(C==2) C=6;
        if(C==3) C=8;

        obj3.offset = new Vector3(0f,0f,-450f*C);
		
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
		
        SC_bar_game.AfterFixedUpdate();
        SC_invisibler.LaterUpdate();
    }
}
