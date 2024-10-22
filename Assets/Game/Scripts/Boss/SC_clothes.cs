using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_clothes : MonoBehaviour
{
    public SC_boss SC_boss;
    public int animation_frames;
    public Transform[] clothes = new Transform[35];
    public GameObject fire_clothe;
    public int[] actual_sizes = new int[35];
    bool[] at_least_once = new bool[35];
    public int get_actual_clothe = -1;

    public Transform star_normal_particles;
    public Transform anti_clothe_4_4;

    void Awake()
    {
        foreach(Transform trn in clothes)
            if(trn!=null) {
                trn.localPosition = new Vector3(0f,0f,0f);
                trn.gameObject.SetActive(false);
            }
    }
    void Start()
    {
        int i;
        for(i=0;i<35;i++) {
            actual_sizes[i] = animation_frames;
            at_least_once[i] = false;
        }
    }
    void FixedUpdate()
    {
        int i;
        for(i=0;i<35;i++)
            if(clothes[i]!=null && (!at_least_once[i] || actual_sizes[i]==0)) clothes[i].gameObject.SetActive(false);

        int clothe_index = 5*SC_boss.type+SC_boss.dataID[18];
        if(clothes[clothe_index]!=null) clothes[clothe_index].gameObject.SetActive(true);
        else clothe_index = -1;

        for(i=0;i<35;i++) {
            if(clothes[i]==null) continue;
            if(clothe_index==i) at_least_once[i] = true;
            if(clothe_index==i && actual_sizes[i] < animation_frames) actual_sizes[i]++;
            if(clothe_index!=i && actual_sizes[i] > 0) actual_sizes[i]--;
            clothes[i].localScale = new Vector3(1f,1f,1f) / animation_frames * actual_sizes[i];
            if(i==4*5+4 && SC_boss.SC_control.livTime>100) anti_clothe_4_4.localScale = new Vector3(1f,1f,1f) / animation_frames * (animation_frames - actual_sizes[i]);
            if(clothes[i].GetComponent<SC_scacontr>()!=null) clothes[i].GetComponent<SC_scacontr>().ScaleAllNow(1f/animation_frames * actual_sizes[i]);
            if(SC_boss.type*5+SC_boss.dataID[18]==3*5+3 && SC_boss.dataID[23]==0) clothes[i].gameObject.SetActive(false);
        }

        fire_clothe.SetActive(SC_boss.dataID[24]>0);

        ParticleSystem pss = star_normal_particles.GetComponent<ParticleSystem>();
        var emissionModule = pss.emission;
        if(SC_boss.type==4 && SC_boss.dataID[18]<3) emissionModule.enabled = true;
        else emissionModule.enabled = false;

        get_actual_clothe = clothe_index;
    }
}
