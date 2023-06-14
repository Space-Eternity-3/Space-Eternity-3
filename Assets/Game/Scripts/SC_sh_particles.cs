using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_sh_particles : MonoBehaviour
{
    public Transform particles;
    
    void LateUpdate()
    {
        if(particles.parent.parent.name!="Shooters") {
            particles.gameObject.SetActive(particles.parent.parent.parent.parent.GetComponent<SC_boss>().dataID[2]==2);
        }
    }
}
