using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_sh_particles : MonoBehaviour
{
    public Transform particles;
    public string type = "shooter";
    public SC_boss SC_boss;
    
    void LateUpdate()
    {
        if(particles.parent.parent.name!="Shooters") {
            if(type=="shooter") particles.gameObject.SetActive(particles.parent.parent.parent.parent.GetComponent<SC_boss>().dataID[2]==2);
            if(type=="normal") particles.gameObject.SetActive(SC_boss.dataID[2]==2);
        }
    }
}
