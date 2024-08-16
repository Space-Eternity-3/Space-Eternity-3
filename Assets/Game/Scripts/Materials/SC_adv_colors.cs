using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_adv_colors : MonoBehaviour
{
    public bool piston_assistant;

    public List<Renderer> Rend1 = new List<Renderer>();
    public List<Renderer> Rend2 = new List<Renderer>();
    public List<Renderer> Rend3 = new List<Renderer>();
    public List<Renderer> Rend4 = new List<Renderer>();

    public Material[] M1 = new Material[16];
    public Material[] M2 = new Material[16];
    public Material[] M3 = new Material[16];
    public Material[] M4 = new Material[16];

    public SC_material SC_material;

    public void ApplyMaterials(int type)
    {
        foreach(Renderer rnd in Rend1) rnd.material = M1[type];
        foreach(Renderer rnd in Rend2) rnd.material = M2[type];
        foreach(Renderer rnd in Rend3) rnd.material = M3[type];
        foreach(Renderer rnd in Rend4) rnd.material = M4[type];

        if(piston_assistant)
        {
            foreach(Renderer rnd in Rend2) {
                rnd.material.mainTextureScale = new Vector2(1f,3f);
                rnd.material.mainTextureOffset = new Vector2(0.6f,0f);
            }
        }
    }
}
