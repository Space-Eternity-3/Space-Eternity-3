using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_scaler_special : MonoBehaviour
{
    public float light_range = 0f;
    public Vector3 scaler = new Vector3(1f,1f,1f);

    public void ScaleNow(float percentage)
    {
        Light light = transform.GetComponent<Light>();
        ParticleSystem particles = transform.GetComponent<ParticleSystem>();
        if(light!=null) light.range = light_range * percentage;
        if(particles!=null)
        {
            Transform trn = particles.GetComponent<Transform>();
            trn.localScale = scaler * percentage;
        }
    }
}
