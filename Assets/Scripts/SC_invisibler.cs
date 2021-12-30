using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_invisibler : MonoBehaviour
{
	public Renderer[] parts;
	public bool invisible;
	
    public void LaterUpdate()
	{
		int i, lngt = parts.Length;
		for(i=0;i<lngt;i++)
		{
			parts[i].enabled = !invisible;
		}
	}
}
