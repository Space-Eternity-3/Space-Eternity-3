using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_tbase : MonoBehaviour
{
    public SC_control SC_control;
    public SC_fobs SC_fobs; //local SC_fobs script
    public Transform Treasure;
    public Renderer[] Diodes = new Renderer[4];
    public Material[] Materials = new Material[3];
    public Transform TreasureCreateParticles;
    public Transform TreasureDestroyParticles;
    public int diode_mode = 0;

    int active_before = -1;

    void FixedUpdate()
    {
        if(diode_mode < 5)
        {
            int rand = UnityEngine.Random.Range(0,300);
            if(rand==0) diode_mode++;
        }
    }
    void LateUpdate()
    {
        string DiodeColors = "0000";

        if(((SC_control.livTime / 15) % 2 == 0) == (diode_mode % 2 == 1))
        {
            if(diode_mode==1) DiodeColors = "1000";
            if(diode_mode==2) DiodeColors = "1100";
            if(diode_mode==3) DiodeColors = "1110";
            if(diode_mode==4) DiodeColors = "1111";
        }
        else
        {
            if(diode_mode==1) DiodeColors = "0000";
            if(diode_mode==2) DiodeColors = "1000";
            if(diode_mode==3) DiodeColors = "1100";
            if(diode_mode==4) DiodeColors = "1110";
        }
        if(diode_mode==0) DiodeColors = "0000";
        if(diode_mode==5) DiodeColors = "2222";

        int is_active;
        if(diode_mode==5) is_active = 1; else is_active = 0;
        if(active_before == -1) active_before = is_active;

        Treasure.gameObject.SetActive(is_active==1);
        if(is_active != active_before)
        {
            if(is_active==1) Instantiate(TreasureCreateParticles,Treasure.position,Treasure.rotation);
            else Instantiate(TreasureDestroyParticles,Treasure.position,Treasure.rotation);
        }

        for(int i=0;i<4;i++)
        {
            Diodes[i].material = Materials[Parsing.IntU(DiodeColors[i]+"")];
        }

        active_before = is_active;
    }
}
