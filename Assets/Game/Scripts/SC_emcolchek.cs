using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_emcolchek : MonoBehaviour
{
	public SC_fobs SC_fobs;
	
    void OnTriggerEnter(Collider collision)
	{
		SC_fobs.AfterTriggerEnter(collision);
	}
}
