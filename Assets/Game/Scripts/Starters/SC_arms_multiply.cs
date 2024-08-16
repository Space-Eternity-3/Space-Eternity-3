using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_arms_multiply : MonoBehaviour
{
    public Transform A;
    public Transform arm_holder;
    public SC_fun SC_fun;

    void Summon(float x, float y, float ang)
    {
        Transform gob = Instantiate(A,new Vector3(0f,0f,0f),Quaternion.identity);
        gob.parent = arm_holder;
        gob.localPosition = new Vector3(x,y,0f);
        gob.eulerAngles = new Vector3(0f,0f,ang);
        gob.localScale = A.localScale;
    }
    void Awake()
    {
        if(SC_fun.arms_did) return;
        SC_fun.arms_did = true;

        float r = A.localPosition.y;
        float sq2 = Mathf.Sqrt(2f)/2;

        Summon(r*sq2,r*sq2,-45f);
        Summon(r,0f,-90f);
        Summon(r*sq2,-r*sq2,-135f);
        Summon(0f,-r,180f);
        Summon(-r*sq2,-r*sq2,135f);
        Summon(-r,0f,90f);
        Summon(-r*sq2,r*sq2,45f);
    }
}
