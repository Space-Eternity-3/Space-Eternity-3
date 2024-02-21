using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_lune_auto_move : MonoBehaviour
{
    public Transform holder;
    public Transform camera;
    public Transform lune;
    float deltaZ;

    public Transform[] ManualTransparencyFix;
    public Color32 enabledFixColor, disabledFixColor;

    void Start()
    {
        deltaZ = lune.localPosition.z;
    }
    void Update()
    {
        float x = camera.position.x - holder.position.x;
        float y = camera.position.y - holder.position.y;
        float z = camera.position.z - holder.position.z;
        lune.position = holder.position + deltaZ * new Vector3(x/z,y/z,1f);
        
        foreach(Transform trn in ManualTransparencyFix)
            TransparencyFix(trn);
    }
    void TransparencyFix(Transform trn)
    {
        Material mat = trn.GetComponent<Renderer>().material;
        if(holder.gameObject.activeSelf) mat.color = enabledFixColor;
        else mat.color = disabledFixColor;
    }
}
