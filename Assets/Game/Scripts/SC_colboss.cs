using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_colboss : MonoBehaviour
{
    SphereCollider collid;
    public SC_control SC_control;

    void Start()
    {
        collid = gameObject.GetComponent<SphereCollider>();
    }
    void FixedUpdate()
    {
        if(SC_control.impulse_enabled) collid.enabled = false;
        else collid.enabled = true;
    }
}
