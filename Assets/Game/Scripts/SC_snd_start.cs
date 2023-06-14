using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_snd_start : MonoBehaviour
{
    public SC_sounds SC_sounds;
    public SC_control SC_control;
    public int type,id;
    public float deeprange = 0f;
    public bool lonely_deeprange;

    void Start()
    {
        if(transform.position.z<100f)
        {
            if(deeprange==0f)
                SC_sounds.PlaySound(transform.position,type,id);
            else if(lonely_deeprange)
            {
                SC_sounds.deeprange = deeprange;
                SC_sounds.PlaySound(transform.position,type,id);
            }
            else
            {
                List<SC_boss> boses = SC_control.SC_lists.SC_boss;
                SC_boss fav_bos = null;
                float fav_distance = -1f;
                foreach(SC_boss bos in boses)
                {
                    if(bos.mother) continue;
                    float got_distance = SC_control.Pitagoras(transform.position-bos.SC_structure.transform.position);
                    if(fav_distance==-1f || got_distance<fav_distance)
                    {
                        fav_bos = bos;
                        fav_distance = got_distance;
                    }
                }
                if(fav_bos!=null) {
                    SC_sounds.deeprange = deeprange;
                    SC_sounds.PlaySound(fav_bos.SC_structure.transform.position,type,id);
                }
            }
        }
    }
}
