using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_lune_auto_move : MonoBehaviour
{
    public Transform holder;
    public Transform Camera;
    public Transform lune;

    public Transform[] ManualTransparencyFix;
    public Color32[] EnabledFixColors, DisabledFixColors;

    public bool LuneAutoCustomize = false;
    public float[] LuneIntensityTable = new float[7];
    public float[] LuneRangeTable = new float[7];
    public float[] LuneDistanceTable = new float[7];
    public SC_boss SC_boss;

    float deltaZ;

    void Start()
    {
        deltaZ = lune.localPosition.z;
        if(LuneAutoCustomize) {
            Light lgt = lune.GetComponent<Light>();
            lgt.intensity = LuneIntensityTable[SC_boss.type];
            lgt.GetComponent<Light>().range = LuneRangeTable[SC_boss.type];
            deltaZ = LuneDistanceTable[SC_boss.type];
        }
    }
    void Update()
    {
        float x = Camera.position.x - holder.position.x;
        float y = Camera.position.y - holder.position.y;
        float z = Camera.position.z - holder.position.z;
        lune.position = holder.position + deltaZ * new Vector3(x/z,y/z,1f);
        
        for(int i=0;i<ManualTransparencyFix.Length;i++)
        {
            Material mat = ManualTransparencyFix[i].GetComponent<Renderer>().material;
            if(holder.gameObject.activeSelf) mat.color = EnabledFixColors[i];
            else mat.color = DisabledFixColors[i];
        }
    }
}
