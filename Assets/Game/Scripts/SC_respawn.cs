using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_respawn : MonoBehaviour
{
    bool on=false;
    public SC_craft2 SC_craft2;
    public Light resp1;
    public Light resp2;
    public Transform player;
    public ParticleSystem resp0;
    public Color32 offed;
    public Color32 oned;
    public Transform RespDestParticles;
    public Transform Communtron3;

    void Update()
    {
        if(transform.position.x==0f&&transform.position.y==0f) transform.position=new Vector3(transform.position.x,transform.position.y,1000f);
        else transform.position=new Vector3(transform.position.x,transform.position.y,1f);

        if(on)
        {
            resp1.color=oned;
            resp2.color=oned;
            if(Input.GetMouseButtonDown(0))
            {
                SC_craft2.ResetSpawn();
            }
        }
        else
        {
            resp1.color=offed;
            resp2.color=offed;
        }
    }
    void OnMouseOver()
    {
        float dX=player.position.x-transform.position.x;
        float dY=player.position.y-transform.position.y;
        float distance=Mathf.Sqrt(dX*dX+dY*dY);

        if(distance<15f && Communtron3.position.y==0f) on=true;
        else on=false;
    }
    void OnMouseExit()
    {
        on=false;
    }
}
