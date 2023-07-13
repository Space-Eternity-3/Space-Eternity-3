using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_scaler_special : MonoBehaviour
{
    public float light_range = 0f;
    public bool get_particles_scale;
    Vector3 scaler = new Vector3(1f,1f,1f);
    bool prer = false;

    void Pre()
    {
        if(get_particles_scale)
            scaler = transform.localScale;
    }
    public void ScaleNow(float percentage)
    {
        if(!prer) {
            Pre();
            prer = true;
        }

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
