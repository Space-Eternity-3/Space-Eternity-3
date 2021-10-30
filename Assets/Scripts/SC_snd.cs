using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_snd : MonoBehaviour
{
    public float naturalVolume;
    public float naturalPitch;
    public float maxDeltaPitch;

    public float volume;
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
        if(!updator) audio.volume=volume;
        activated=true;
    }
    void Update()
    {
        if(activated && !terminated) audio.volume=SC_sounds.GetVolume(transform.position,naturalVolume);
        if(terminated) audio.volume = 0f;
    }
}
