using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_scaler_special : MonoBehaviour
{
    public float light_range = 0f;
    public Transform summonable_particles;

    int particles_cooldown = 0;
    int particles_cooldown_2 = -1;

    public void ScaleNow(float percentage)
    {
        Light light = transform.GetComponent<Light>();
        ParticleSystem particles = transform.GetComponent<ParticleSystem>();
        if(light!=null) light.range = light_range * percentage;
        if(particles!=null)
        {
            Transform trn = particles.GetComponent<Transform>();
            trn.localScale = new Vector3(1f,1f,1f) * percentage;
        }
        if(summonable_particles!=null && particles_cooldown==0 && particles_cooldown_2<=0)
        {
            if(particles_cooldown_2 == -1) {
                particles_cooldown_2 = 40;
                return;
            }
            Transform trn2 = Instantiate(summonable_particles,transform.position,Quaternion.identity);
            trn2.parent = transform;
            particles_cooldown = 150;
            particles_cooldown_2 = -1;
        }
    }
    void FixedUpdate()
    {
        if(particles_cooldown > 0) particles_cooldown--;
        if(particles_cooldown_2 > 0) particles_cooldown_2--;
    }
}
