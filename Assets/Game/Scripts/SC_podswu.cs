using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_podswu : MonoBehaviour
{
    public Renderer[] parts;
    public Renderer[] parts2;
    public Renderer[] parts3;
    public Renderer[] parts4;
    public Renderer[] parts5;
    public Renderer[] parts6;
    public Material norm,high;
    public Material norm2,high2;
    public Material norm3,high3;
    public Material norm4,high4;
    public Material norm5,high5;
    public Material norm6,high6;

    public Transform Communtron3;
    public Transform player;
    public bool Cmt3z_plus;

    bool active=false;

    void Highlight(bool bright)
    {
        int i;
        int[] lngt = { 0,
            parts.Length,
            parts2.Length,
            parts3.Length,
            parts4.Length,
            parts5.Length,
            parts6.Length
        };

        if(!bright) {
            for(i=0;i<lngt[1];i++) parts[i].material=norm;
            for(i=0;i<lngt[2];i++) parts2[i].material=norm2;
            for(i=0;i<lngt[3];i++) parts3[i].material=norm3;
            for(i=0;i<lngt[4];i++) parts4[i].material=norm4;
            for(i=0;i<lngt[5];i++) parts5[i].material=norm5;
            for(i=0;i<lngt[6];i++) parts6[i].material=norm6;
        }
        else {
            for(i=0;i<lngt[1];i++) parts[i].material=high;
            for(i=0;i<lngt[2];i++) parts2[i].material=high2;
            for(i=0;i<lngt[3];i++) parts3[i].material=high3;
            for(i=0;i<lngt[4];i++) parts4[i].material=high4;
            for(i=0;i<lngt[5];i++) parts5[i].material=high5;
            for(i=0;i<lngt[6];i++) parts6[i].material=high6;
        }
    }
    bool InDistance(float dist)
	{
		float dX=player.position.x-transform.position.x;
		float dY=player.position.y-transform.position.y;
		if(Mathf.Sqrt(dX*dX+dY*dY)<dist) return true;
		else return false;
	}
    void OnMouseOver()
    {
        if(!active)
        {
            if(InDistance(15f))
            {
                Highlight(true);
                if(Cmt3z_plus) Communtron3.position+=new Vector3(0f,0f,1f);
                active=true;
            }
        }
        if(active)
        {
            if(!InDistance(15f))
            {
                Highlight(false);
                if(Cmt3z_plus) Communtron3.position-=new Vector3(0f,0f,1f);
                active=false;
            }
        }
    }
    void OnMouseExit()
    {
        if(active)
        {
            Highlight(false);
            if(Cmt3z_plus) Communtron3.position-=new Vector3(0f,0f,1f);
            active=false;
        }
    }
    void OnDestroy()
    {
        if(active && Cmt3z_plus) try{Communtron3.position-=new Vector3(0f,0f,1f);}catch(Exception){}
    }
}
