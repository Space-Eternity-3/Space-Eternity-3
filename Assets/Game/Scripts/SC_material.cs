using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_material : MonoBehaviour
{
    public GameObject target;
    public Material[] Materials = new Material[16];
    public Material[] Materials2 = new Material[3];

    public void SetMaterial(int type, bool allow_3_rand)
    {
        if(!allow_3_rand || type >= 2) target.GetComponent<Renderer>().material = Materials[type];
        else target.GetComponent<Renderer>().material = Materials2[3*type + UnityEngine.Random.Range(0,2)];
    }
}
