using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_bp_disableds : MonoBehaviour
{
    public RectTransform[] BpDisableds = new RectTransform[6];
    public RectTransform[] CopyYFrom = new RectTransform[6];
    Vector3[] normal_vectors = new Vector3[6];

    public SC_upgrades SC_upgrades;

    void Start()
    {
        Vector3 vector = new Vector3(BpDisableds[0].localPosition.x,0f,BpDisableds[0].localPosition.z);
        int i;
        for(i=1;i<=5;i++) {
            BpDisableds[i] = Instantiate(BpDisableds[0],new Vector3(0f,0f,0f),BpDisableds[0].rotation);
            BpDisableds[i].SetParent(BpDisableds[0].parent,false);
            BpDisableds[i].localPosition = vector + new Vector3(0f,CopyYFrom[i].localPosition.y,0f);
            BpDisableds[i].GetChild(0).GetComponent<Text>().text = /*"Upgrade " + i*/ "Locked";
            normal_vectors[i] = BpDisableds[i].localPosition;
        }
    }

    void Update()
    {
        int i;
        for(i=1;i<=5;i++) {
            if(SC_upgrades.MTP_levels[4] >= i) BpDisableds[i].localPosition = new Vector3(0f,10000f,0f);
            else BpDisableds[i].localPosition = normal_vectors[i];
        }
    }
}
