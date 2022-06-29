using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_projection : MonoBehaviour
{
    Vector3[] memPosition = new Vector3[150];
    float[] memRotation = new float[150];

    public float minDeltaDistance;
    public float maxDangerRange;
    public bool devShow;

    public Transform player;
    public SC_control SC_control;
    public SC_fun SC_fun;

    void Start()
    {
        int i;
        for(i=0;i<20;i++)
        {
            memPosition[0] = new Vector3(0f,0f,300f);
            memRotation[0] = 0f;
        }
    }
    void ArrayPusher(Vector3 new_push, float new_rot)
    {
        int i;
        for(i=19;i>0;i--){
            memPosition[i]=memPosition[i-1];
            memRotation[i]=memRotation[i-1];
        }
        memPosition[0]=new_push;
        memRotation[0]=new_rot;
    }
    public void AfterFixedUpdate()
    {
        ArrayPusher(player.position,player.eulerAngles.z);
    }
    public void MuchLaterUpdate()
    {
        int Dt = SC_control.intPing;
        if(Dt!=-1) transform.position = memPosition[Dt];
        if(IsVisible()) transform.eulerAngles = new Vector3(memRotation[Dt]-90f,-90f,90f);
        else transform.position = new Vector3(0f,0f,300f);
    }
    bool IsVisible()
    {
        if((int)SC_control.Communtron4.position.y!=100 || SC_control.intPing==-1) return false;

        if(SC_control.living && SC_control.Pitagoras(transform.position-player.position-new Vector3(0f,0f,transform.position.z-player.position.z)) > minDeltaDistance)
        {
            if(devShow) return true;

            SC_bullet[] buls = FindObjectsOfType<SC_bullet>();
            foreach(SC_bullet bul in buls)
                if(bul.mode=="projection" && bul.projectionOwner!=SC_control.connectionID+"")
                {
                    Vector3 btr = bul.GetComponent<Transform>().position;
                    if(SC_control.Pitagoras(transform.position-btr-new Vector3(0f,0f,transform.position.z-btr.z)) < maxDangerRange)
                        return true;
                }
        }
        
        return false;
    }
}
