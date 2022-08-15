using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_boss : MonoBehaviour
{
    public Transform TestBossNick;
    public Transform CanvPS;
    Transform CanvP;

    bool mother = true;

    void Start()
    {
        if(transform.position.z<100f) mother=false;
        if(mother) return;

        CanvP = Instantiate(CanvPS,new Vector3(0f,0f,0f),Quaternion.identity);
        CanvP.SetParent(TestBossNick,false); CanvP.name = "CanvBS";
    }
}
