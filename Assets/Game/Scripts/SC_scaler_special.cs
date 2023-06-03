using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_scaler_special : MonoBehaviour
{
    public float light_range = 0f;
    public void ScaleNow(float percentage)
    {
        Light light = transform.GetComponent<Light>();
        ParticleSystem particles = transform.GetComponent<ParticleSystem>();
        if(light!=null) light.range = light_range * percentage;
        if(particles!=null) particles.GetComponent<Transform>().localScale = new Vector3(1f,1f,1f) * percentage;
    }
}
