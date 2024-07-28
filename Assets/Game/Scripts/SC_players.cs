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
    public SC_shield SC_shield;
    public Transform FixedPlayer;

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
    public Vector3 positionBefore = new Vector3(0f,0f,300f);
    public Vector3 positionBeforeB = new Vector3(0f,0f,300f);
    public Vector3 positionBeforeC = new Vector3(0f,0f,300f);
    bool sleeping = false;

    public void B_Awake()
    {
        SC_control.SC_lists.AddTo_SC_players(this);

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
    public Vector3 SpeculateVelocity()
    {
        if(FixedPlayer.position.z > 100f || positionBefore.z > 100f || positionBeforeB.z > 100f)
            return new Vector3(0f,0f,0f);
        else
            return (FixedPlayer.position - positionBeforeB) / 2f;
    }
    public void AfterFixedUpdate()
    {
        bool actual = SC_fun.SC_control.NUL[IDP_phys];
        bool inrange = true;
        positionBeforeC = positionBeforeB;
        positionBeforeB = positionBefore;
        positionBefore = FixedPlayer.position;
        if(actual)
        {
            if(positionBefore.z > 100f && sourcedPosition.z < 100f)
                transform.GetComponent<SC_player_follower>().teleporting = true;

            if(positionBefore != sourcedPosition && transform.GetComponent<SC_player_follower>().teleporting_unsynced)
                transform.GetComponent<SC_player_follower>().teleporting_unsynced_catalizator = true;

            sleeping = false;
            if(inrange)
            {
                FixedPlayer.position = sourcedPosition;
                FixedPlayer.eulerAngles = new Vector3(0f,0f,sourcedRotation);
            }
            else
            {
                FixedPlayer.position = sourcedPosition;

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
                FixedPlayer.position = new Vector3(0f,0f,10000f+IDP*5f);

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
