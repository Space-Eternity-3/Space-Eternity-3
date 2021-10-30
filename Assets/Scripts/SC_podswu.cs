using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_podswu : MonoBehaviour
{
    public Renderer[] parts;
    public Material norm,high;
    public Transform Communtron3;
    public Transform player;
    public bool Cmt3z_plus;

    bool active=false;

    bool InDistance(float dist)
	{
		float dX=player.position.x-transform.position.x;
		float dY=player.position.y-transform.position.y;
		if(Mathf.Sqrt(dX*dX+dY*dY)<dist) return true;
		else return false;
	}
    void OnMouseOver()
    {
        int i,lngt=parts.Length;
        if(!active)
        {
            if(InDistance(15f))
            {
                for(i=0;i<lngt;i++) parts[i].material=high;
                if(Cmt3z_plus) Communtron3.position+=new Vector3(0f,0f,1f);
                active=true;
            }
        }
        if(active)
        {
            if(!InDistance(15f))
            {
                for(i=0;i<lngt;i++) parts[i].material=norm;
                if(Cmt3z_plus) Communtron3.position-=new Vector3(0f,0f,1f);
                active=false;
            }
        }
    }
    void OnMouseExit()
    {
        int i,lngt=parts.Length;
        if(active)
        {
            for(i=0;i<lngt;i++) parts[i].material=norm;
            if(Cmt3z_plus) Communtron3.position-=new Vector3(0f,0f,1f);
            active=false;
        }
    }
    void OnDestroy()
    {
        if(active) try{Communtron3.position-=new Vector3(0f,0f,1f);}catch(Exception e){}
    }
}
