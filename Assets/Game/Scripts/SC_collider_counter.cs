using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_collider_counter : MonoBehaviour
{
    public int age = 0;
    void Start()
    {
        age = 0;
    }
    void FixedUpdate()
    {
        age++;
    }
}
