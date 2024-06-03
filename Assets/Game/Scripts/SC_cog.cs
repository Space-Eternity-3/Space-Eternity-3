using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_cog : MonoBehaviour
{
    public Transform Body;
    public int DeltaRot;
    public SC_fobs SC_fobs;

    bool started = false;
    int rot = 0;

    void FixedUpdate()
    {
        if(!started)
        {
            if(SC_fobs.index % 2 == 0)
            {
                rot += 15;
                DeltaRot *= -1;
            }
            started = true;
        }
        rot += DeltaRot;
        rot %= 360;
        Body.localRotation = Quaternion.Euler(0,rot,0);
    }
}
