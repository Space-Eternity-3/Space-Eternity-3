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

	public bool octogone;
	public bool octogone_main;
	public SC_boss SC_boss;
	public SC_seeking SC_seeking;
	
	void LateUpdate()
	{
		if(octogone)
		{
			//Code for Octogone
			invisible = (SC_boss.type==3 && SC_boss.dataID[18]==3);
			LaterUpdate();
		}
	}
    public void LaterUpdate()
	{
		int i, lngt = parts.Length;
		bool eft = !(invisible);

		if(lngt>=0) if(parts[0].enabled != eft)
		for(i=0;i<lngt;i++)
			parts[i].enabled = eft;
		
		if(!this_main && !octogone)
			if(invisible && !visible) nick.color = dis;
			else nick.color = def;

		if(octogone && octogone_main)
		{
			Vector3 v = SC_seeking.offset;
			if(!invisible) SC_seeking.offset = new Vector3(v.x,v.y,-1.15f);
			else SC_seeking.offset = new Vector3(v.x,v.y,10000f);
		}
	}
}
