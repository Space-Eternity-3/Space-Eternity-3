using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_seeking : MonoBehaviour
{
	public bool enabled = true;
	public string idWord = "";
    public Transform seek;
    public Vector3 offset;

    public void Update()
    {
		if(enabled)
        transform.position=seek.position+offset;
    }
}
