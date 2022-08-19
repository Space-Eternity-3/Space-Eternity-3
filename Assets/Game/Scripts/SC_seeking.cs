using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_seeking : MonoBehaviour
{
	public bool enabled = true;
    public bool localpos = false;
	public string idWord = "";
    public Transform seek;
    public Vector3 offset;

    public void Update()
    {
		if(enabled)
        {
            if(!localpos) transform.position=seek.position+offset;
            else transform.localPosition=seek.localPosition+offset;
        }
    }
}
