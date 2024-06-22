using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_tbs_click : MonoBehaviour
{
    public SC_fobs SC_fobs; //locally set
    public SC_tbase SC_tbase; //locally set
    public SC_control SC_control;
    public SC_data SC_data;
    public SC_slots SC_slots;

    public int TreasureID = 0;

    void OnMouseOver()
    {
        if(SC_fobs.InDistance(15f) && SC_fobs.Communtron1.position.z==0f && SC_tbase.diode_mode==5)
        {
            if(Input.GetMouseButtonDown(0) && SC_fobs.Communtron2.position.x==0f && SC_fobs.Communtron3.position.y==0f)
            {
                if(SC_slots.InvHaveB(-1,1,true,true,true,1))
                {
                    string localDrop = "X";
                    if((int)SC_fobs.Communtron4.position.y!=100)
                    {
                        if(TreasureID==0) localDrop = SC_fobs.getLoot(SC_data.Gameplay[105]);
                        if(TreasureID==1) localDrop = SC_fobs.getLoot(SC_data.Gameplay[106]);
                        if(TreasureID==2) localDrop = SC_fobs.getLoot(SC_data.Gameplay[125]);
                        if(TreasureID==3) localDrop = SC_fobs.getLoot(SC_data.Gameplay[126]);
                        if(TreasureID==4) localDrop = SC_fobs.getLoot(SC_data.Gameplay[127]);
                    }
                    else
                    {
                        List<string> TreasureList = new List<string>();
                        if(TreasureID==0) TreasureList = SC_control.TreasureAllowed;
                        if(TreasureID==1) TreasureList = SC_control.DarkTreasureAllowed;
                        if(TreasureID==2) TreasureList = SC_control.MetalTreasureAllowed;
                        if(TreasureID==3) TreasureList = SC_control.SoftTreasureAllowed;
                        if(TreasureID==4) TreasureList = SC_control.HardTreasureAllowed;

                        if(TreasureList.Count>0)
                        {
                            localDrop = TreasureList[0];
                            TreasureList.RemoveAt(0);
                        }
                        else localDrop = "X";
                    }
                    if(localDrop!="X")
                    {
                        int ldI=Parsing.IntE(localDrop.Split(';')[0]);
                        int ldC=Parsing.IntE(localDrop.Split(';')[1]);
                    
                        int slot = SC_slots.InvChange(ldI,ldC,true,true,false);

                        if(SC_fobs.multiplayer)
                        {
                            // HERE SEND TREASURE BREAKING MESSAGE TO SERVER
                        }
                        SC_tbase.diode_mode = 0;
                    }
                }
            }
        }
    }
}
