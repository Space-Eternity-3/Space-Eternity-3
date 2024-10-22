using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_snd_loop : MonoBehaviour
{
    public int SoundAmount;

    public AudioSource[] sounds = new AudioSource[3];
    public float[] sound_nvl_ = new float[3];
    public float[] sound_ptc_ = new float[3];

    public Vector3[] sound_pos = new Vector3[2048];
    public int[] sound_id = new int[2048];
    public bool[] sound_locked = new bool[2048];
    public float[] log_base = new float[3];

    public SC_sounds SC_sounds;
    public SC_control SC_control;

    int starter = 5;

    public int AddToLoop(int n, Vector3 vec)
    {
        int i;
        for(i=0;i<2048;i++)
        {
            if(!sound_locked[i])
            {
                sound_locked[i] = true;
                sound_id[i] = n;
                sound_pos[i] = vec;
                return i;
            }
        }
        Debug.LogError("Sound array full!");
        return -1;
    }
    public void RemoveFromLoop(int ind)
    {
        sound_locked[ind] = false;
    }
    void Update()
    {
        if(starter > 0) {
            starter--;
            return;
        }

        int i,j;
        float pom,V;

        for(i=0;i<SoundAmount;i++)
        {
            V = 0f;
            for(j=0;j<2048;j++)
            {
                if(sound_locked[j] && sound_id[j]==i)
                {
                    pom = SC_sounds.GetVolumeDeep(sound_pos[j],0);
                    V += Mathf.Log(Mathf.Pow(log_base[i],pom-V)-Mathf.Pow(log_base[i],-V)+1,log_base[i]);
                }
            }
            float mn = Parsing.FloatE(SC_sounds.SC_data.volume) * sound_nvl_[i] * SC_sounds.SC_data.global_volume;
            V*=0.5f; if(V>=1f) V=1f;
            if(!SC_control.timeStop) sounds[i].volume = V * mn;
			else sounds[i].volume = 0f;
            sounds[i].pitch = sound_ptc_[i];
        }
    }
}
