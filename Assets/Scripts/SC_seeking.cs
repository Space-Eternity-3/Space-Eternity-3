using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_seeking : MonoBehaviour
{
    public Transform seek;
    public Vector3 offset;

    public void Update()
    {
        transform.position=seek.position+offset;
    }
}
