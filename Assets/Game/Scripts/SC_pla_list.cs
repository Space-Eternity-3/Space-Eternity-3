using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_pla_list : MonoBehaviour
{
    public int page = 1;

    public Text NamePages;
    public Text TextConst0;

    public SC_coordinates[] SC_coordinates = new SC_coordinates[8];
    public SC_arrow_rot[] SC_arrow_rot = new SC_arrow_rot[8];
    public SC_control SC_control;

    void Update()
    {
        NamePages.text = "Page " + page + "/0"; //Change using player number variable!!!

        int i,n,lngt = SC_control.max_players;
        for(i=0;i<8;i++)
        {
            n = 8*(page-1) + i + 1;
            if(n>=lngt)
            {
                SC_coordinates[i].nik = TextConst0;
                continue;
            }

            SC_coordinates[i].player = SC_control.PL[n].GetComponent<Transform>();
            SC_coordinates[i].SC_invisibler = SC_control.PL[n].GetComponent<SC_invisibler>();
            SC_coordinates[i].nik = SC_control.NCT[n];
            SC_arrow_rot[i].respawn_point = SC_coordinates[i].player;
        }

        for(i=0;i<8;i++)
        {
            SC_coordinates[i].B_Update();
            SC_arrow_rot[i].B_Update();
        }
    }
}
