using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_clothes : MonoBehaviour
{
    public SC_boss SC_boss;
    public int animation_frames;
    public Transform[] clothes = new Transform[35];
    public int[] actual_sizes = new int[35];
    bool[] at_least_once = new bool[35];

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
            if(clothes[i].GetComponent<SC_scacontr>()!=null) clothes[i].GetComponent<SC_scacontr>().ScaleAllNow(1f/animation_frames * actual_sizes[i]);
        }
    }
}
