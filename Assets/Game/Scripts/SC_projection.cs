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
    public GameObject VisiblePlane;
    public SC_control SC_control;
    public SC_fun SC_fun;

    void Start()
    {
        int i;
        for(i=0;i<150;i++)
        {
            memPosition[0] = player.position;
            memRotation[0] = 0f;
        }
    }
    void ArrayPusher(Vector3 new_push, float new_rot)
    {
        int i;
        for(i=149;i>0;i--){
            memPosition[i]=memPosition[i-1];
            memRotation[i]=memRotation[i-1];
        }
        memPosition[0]=new_push;
        memRotation[0]=new_rot;
    }
    public void AfterFixedUpdate()
    {
        ArrayPusher(player.position,player.eulerAngles.z);
        MuchLaterUpdate();
    }
    public void MuchLaterUpdate()
    {
        int Dt = SC_control.intPing;
        if(Dt<0) Dt=0;
        transform.position = memPosition[Dt];
        transform.eulerAngles = new Vector3(0f,0f,memRotation[Dt]);
        VisiblePlane.SetActive(IsVisible());
    }
    public Vector3 SpeculateVelocity()
    {
        int Dt = SC_control.intPing;
        if(Dt<0) Dt=0;
        if(Dt==0) Dt++;
        return memPosition[Dt-1] - memPosition[Dt];
    }
    bool IsVisible()
    {
        if((int)SC_control.Communtron4.position.y!=100 || SC_control.intPing<0) return false;

        if(SC_control.living && SC_control.Pitagoras(transform.position-player.position-new Vector3(0f,0f,transform.position.z-player.position.z)) > minDeltaDistance)
        {
            if(devShow) return true;

            List<SC_bullet> buls = SC_control.SC_lists.SC_bullet;
            foreach(SC_bullet bul in buls)
                if(bul.mode=="projection" && bul.projectionOwner!=SC_control.connectionID+"")
                {
                    Vector3 btr = bul.GetComponent<Transform>().position;
                    if(SC_control.Pitagoras(transform.position-btr-new Vector3(0f,0f,transform.position.z-btr.z)) < maxDangerRange)
                        return true;
                }
            List<SC_players> plas = SC_control.SC_lists.SC_players;
            foreach(SC_players pla in plas)
                if(pla.ArtSource%25==2)
                {
                    Vector3 btr = pla.GetComponent<Transform>().position;
                    if(SC_control.Pitagoras(transform.position-btr-new Vector3(0f,0f,transform.position.z-btr.z)) < maxDangerRange)
                        return true;
                }
        }
        
        return false;
    }
}
