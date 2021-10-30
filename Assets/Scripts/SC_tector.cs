using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SC_tector : MonoBehaviour
{
	public Transform Communtron3;
	bool yes=false;

	void Update()
	{
		if(EventSystem.current.IsPointerOverGameObject())
		{
			if(!yes)
			{
				Communtron3.position+=new Vector3(0f,1f,0f);
				yes=true;
			}
		}
		else
		{
			if(yes)
			{
				Communtron3.position-=new Vector3(0f,1f,0f);
				yes=false;
			}
		}
	}
}
