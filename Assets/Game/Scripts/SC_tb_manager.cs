using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_tb_manager : MonoBehaviour
{
    public SC_control SC_control;
    public Transform Communtron4;
    public int TreasureFrame = 0;
    int sgp_counter = 0;

    void FixedUpdate()
    {
        if((int)Communtron4.position.y!=100)
        {
            sgp_counter++;
            if(sgp_counter==15)
            {
                sgp_counter = 0;
                NextFrame();
                
                // Singleplayer [active] treasure generator
                if(TreasureFrame % 2 == 0)
                {
                    foreach(SC_asteroid ast in SC_control.SC_lists.SC_asteroid)
                    foreach(Transform trn in ast.GetComponent<Transform>())
                    {
                        SC_fobs fob = trn.GetComponent<SC_fobs>();
                        if(fob!=null)
                        if(fob.ObjID==81)
                        {
                            SC_tbase tbs = fob.GetComponent<SC_tbase>();
                            if(tbs.nbt1 >= 0 && tbs.nbt1 < 5 && UnityEngine.Random.Range(0,5)==0)
                                tbs.DiodeNextLevel();
                        }
                    }
                }
            }
        }
    }
    public void NextFrame()
    {
        TreasureFrame++;
    }
}
