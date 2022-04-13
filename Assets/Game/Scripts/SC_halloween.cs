using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_halloween : MonoBehaviour
{
    public Renderer background;
    public Material Mt_N,Mt_H;
    public SC_fun SC_fun;
    public SC_inv_number SC_inv_number;
    public SC_control SC_control;

    void Update()
    {
        if(!SC_fun.halloween_theme)
        {
            background.material = Mt_N;
        }
        else
        {
            background.material = Mt_H;
        }
    }
}
