using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_behaviour : MonoBehaviour
{
    public SC_boss SC_boss;

    public void _Start()
    {

    }
    public void _FixedUpdate()
    {
        float angle = (SC_boss.dataID[3]/50f)%(2f*3.14159f);
        SC_boss.dataID[8] = SC_boss.FloatToScrd(22f*Mathf.Cos(angle));
        SC_boss.dataID[9] = SC_boss.FloatToScrd(22f*Mathf.Sin(angle));
        SC_boss.dataID[10] = SC_boss.FloatToScrd(angle*180f/3.14159f);
    }
    public void _End()
    {
        
    }
}
