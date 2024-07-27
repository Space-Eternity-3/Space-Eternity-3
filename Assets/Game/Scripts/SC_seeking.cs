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

    public SC_control SC_control;

    public void LateUpdate()
    {
		if(enabled)
        {
            if(!localpos) transform.position=seek.position+offset;
            else transform.localPosition=seek.localPosition+offset;
        }
    }
    void Start() {
        SC_control.SC_lists.AddTo_SC_seeking(this);
    }
    void OnDestroy() {
        SC_control.SC_lists.RemoveFrom_SC_seeking(this);
    }
}
