using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_ui_bar_amount : MonoBehaviour
{
    public Image bar;
    public float value;
    Vector3 def;
    public float bar_length;

    void Awake()
    {
        def = bar.GetComponent<RectTransform>().localScale;
    }
    public void MuchLaterUpdate()
    {
        RectTransform trn = bar.GetComponent<RectTransform>();
        trn.localScale = new Vector3(def.x*value,def.y,def.z);
    }
}
