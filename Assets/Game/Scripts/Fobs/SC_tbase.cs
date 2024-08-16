using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_tbase : MonoBehaviour
{
    public SC_control SC_control;
    public SC_tb_manager SC_tb_manager;
    public SC_fobs SC_fobs; //local SC_fobs script
    public Transform Treasure;
    public Renderer[] Diodes = new Renderer[4];
    public Material[] Materials = new Material[3];
    public Transform TreasureCreateParticles;
    public Transform TreasureDestroyParticles;
    public int diode_update_frame = -1;
    public int nbt1 = 0; // diode_mode

    int active_before = -1;

    void LateUpdate()
    {
        string DiodeColors = "0000";
        if((SC_tb_manager.TreasureFrame % 2 == 0) == (nbt1 % 2 == 1))
        {
            if(nbt1==1) DiodeColors = "1000";
            if(nbt1==2) DiodeColors = "1100";
            if(nbt1==3) DiodeColors = "1110";
            if(nbt1==4) DiodeColors = "1111";
        }
        else
        {
            if(nbt1==1) DiodeColors = "0000";
            if(nbt1==2) DiodeColors = "1000";
            if(nbt1==3) DiodeColors = "1100";
            if(nbt1==4) DiodeColors = "1110";
        }
        if(nbt1==0) DiodeColors = "0000";
        if(nbt1==5) DiodeColors = "2222";

        int is_active;
        if(nbt1==5) is_active = 1; else is_active = 0;
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
    void SaveSGP()
    {
        WorldData.Load(SC_fobs.X,SC_fobs.Y);
        WorldData.UpdateNbt(SC_fobs.index+1,0,nbt1);
    }
    public void TreasureStateLoad()
    {
        if(SC_fobs.multiplayer)
        {
            nbt1 = SC_fobs.new_nbt_get_1;
        }
        else
        {
            WorldData.Load(SC_fobs.X,SC_fobs.Y);
            nbt1 = WorldData.GetNbt(SC_fobs.index+1,0);
        }
    }
    public void BreakTreasure(int slot)
    {
        nbt1 = 0;
        if(SC_fobs.multiplayer)
            SC_control.SendMTP("/TreasurePickUpTry "+SC_control.connectionID+" "+SC_fobs.ID+" "+SC_fobs.index+" "+slot);
        else
            SaveSGP();
    }
    public void DiodeNextLevel()
    {
        nbt1++;
        SaveSGP();
    }
}
