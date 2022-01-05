using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_invisibler : MonoBehaviour
{
	public Renderer[] parts;
	public TextMesh nick;
	public Color32 def, dis;
	public bool invisible;
	public bool visible;
	public bool this_main;
	
    public void LaterUpdate()
	{
		int i, lngt = parts.Length;
		for(i=0;i<lngt;i++)
		{
			parts[i].enabled = !invisible;
		}
		
		if(!this_main)
		if(invisible && !visible) nick.color = dis;
		else nick.color = def;
	}
}
