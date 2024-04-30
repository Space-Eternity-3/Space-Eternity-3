using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_shield : MonoBehaviour
{
    public Transform VisualShield;
    public string shield_type;
    public bool active;

    void Update()
    {
        if(active) VisualShield.localPosition = new Vector3(0f,0f,0f);
        else VisualShield.localPosition = new Vector3(0f,0f,10000f);
    }
}
