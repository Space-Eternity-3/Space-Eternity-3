using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_snd : MonoBehaviour
{
    public float naturalVolume;
    public float naturalPitch;
    public float maxDeltaPitch;

    public float volume;
    public float deeprange = 0;
    AudioSource audio;
    public bool updator = false;
    bool activated = false;
    public bool terminated = false;
    public SC_sounds SC_sounds;

    void Awake()
    {
        audio = transform.GetComponent<AudioSource>();
    }
    float SetPitch()
    {
        int rand = Random.Range(0,11)-5;
        return naturalPitch+rand*maxDeltaPitch/5f;
    }
    public void Active()
    {
        audio.pitch=SetPitch();
        activated=true;
        Update();
    }
    void Update()
    {
        if(activated && !terminated)
        {
            volume=SC_sounds.GetVolume(transform.position,naturalVolume,deeprange);
            audio.volume = volume;
        }
        if(terminated) audio.volume = 0f;
    }
}
