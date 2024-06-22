using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_dbs_click : MonoBehaviour
{
    public SC_fobs SC_fobs; //locally set
    public SC_dbase SC_dbase; //locally set
    public SC_control SC_control;
    public SC_slots SC_slots;
    public Renderer localRenderer;

    bool emptyShow=false; int onu=2;
    int ReR_int = 0;
    bool localRendererMem = true; //Local temp variable
    bool localRendererMem2 = true; //Memory of renderer state

    void Start()
    {
        localRendererMem = false;
        localRendererMem2 = false;
        localRenderer.enabled=false;
    }
    void Update()
    {
        if(onu>0) onu--;
        if(ReR_int>0) ReR_int--;
        if(localRendererMem2 != localRendererMem) {
            localRendererMem2 = localRendererMem;
            localRenderer.enabled = localRendererMem;
        }
        localRendererMem=false;
    }
    bool topDistance(float minDis)
    {
        float oX = transform.position.x;
        float oY = transform.position.y;
        float pX,pY,dist;
        int i;
        for(i=1;i<SC_control.max_players;i++)
        {
            if(SC_control.PL[i].GetComponent<Transform>().position.z<100f)
            {
                pX=SC_control.PL[i].GetComponent<Transform>().position.x;
                pY=SC_control.PL[i].GetComponent<Transform>().position.y;
                dist=Mathf.Sqrt((oX-pX)*(oX-pX)+(oY-pY)*(oY-pY));
                if(dist<minDis) return false;
            } 
        }
        return true;
    }
    void OnMouseOver()
    {
        float oX=transform.position.x, oY=transform.position.y;
		float pX=SC_control.transform.position.x, pY=SC_control.transform.position.y;
		float distance=Mathf.Sqrt((oX-pX)*(oX-pX)+(oY-pY)*(oY-pY));

        if(SC_fobs.InDistance(15f) && SC_fobs.Communtron3.position.y==0f && distance>=2f && topDistance(2f) &&
        !Input.GetMouseButton(0) && SC_fobs.Communtron2.position.x==0f && SC_slots.SelectedItem()==33)
        {
            if(!Input.GetMouseButton(1)&&emptyShow)
                localRendererMem=true;

            if(Input.GetMouseButtonDown(1)&&ReR_int==0)
            {
                ReR_int=5;
                int slot = SC_slots.InvChange(33,-1,true,false,false);
				SC_control.public_placed = true;
                SC_dbase.PlaceDiamond(slot); // both SGP & MTP
            }
        }
    }
    void OnMouseEnter()
    {
        if(onu==0) emptyShow = true;
    }
}
