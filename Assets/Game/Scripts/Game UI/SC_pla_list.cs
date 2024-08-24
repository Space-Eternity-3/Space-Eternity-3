using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_pla_list : MonoBehaviour
{
    public int page = 1;
    public int max_page = 1;

    public Text NamePages;
    public Text TextConst0, TextConstYou;

    public Color32 color_default, color_you;
    public Color32 color_C_default, color_C_resp;

    public Button butLeft;
    public Button butRight;

    public Image[] bgSpace = new Image[8];
    public Image[] bgCompass = new Image[8];
    public SC_coordinates[] SC_coordinates = new SC_coordinates[8];
    public SC_arrow_rot[] SC_arrow_rot = new SC_arrow_rot[8];

    public Transform resp_main;
    public SC_control SC_control;

    void LateUpdate()
    {
        PageChange(0);

        int i,n,lngt = SC_control.max_players;
        List<int> activeList = new List<int>();
        activeList.Add(0);
        for(i=1;i<lngt;i++)
        {
            if(SC_control.NCT[i].text != "" && SC_control.NCT[i].text != "0")
                activeList.Add(i);
        }
        int lnct = activeList.Count;

        max_page = (lnct-1)/8 + 1;
        if(max_page<=0) max_page=1;
        NamePages.text = "Page " + page + "/" + max_page; //Change using player number variable!!!

        for(i=0;i<8;i++)
        {
            int j = 8*(page-1) + i;

            if(j<lnct) n = activeList[j];
            else n = -1;

            if(n==-1)
            {
                SC_coordinates[i].nik = TextConst0;
                continue;
            }
            else if(n==0)
            {
                SC_coordinates[i].player = SC_control.GetComponent<Transform>();
                SC_coordinates[i].SC_invisibler = SC_control.GetComponent<SC_invisibler>();
                SC_coordinates[i].nik = TextConstYou;
                SC_arrow_rot[i].respawn_point = /*resp_main*/ SC_control.transform;
                SC_arrow_rot[i].silentior = true;
                bgSpace[i].color = color_you;
                bgCompass[i].color = color_C_resp;
            }
            else
            {
                SC_coordinates[i].player = SC_control.PL[n].GetComponent<Transform>();
                SC_coordinates[i].SC_invisibler = SC_control.PL[n].GetComponent<SC_invisibler>();
                SC_coordinates[i].nik = SC_control.NCT[n];
                SC_arrow_rot[i].respawn_point = SC_coordinates[i].player;
                SC_arrow_rot[i].silentior = false;
                bgSpace[i].color = color_default;
                bgCompass[i].color = color_C_default;
            }
        }

        for(i=0;i<8;i++)
        {
            SC_coordinates[i].B_Update();
            SC_arrow_rot[i].B_Update();
        }
    }
    public void PageChange(int delta)
    {
        int new_page = page + delta;
        if(new_page>=1 && new_page<=max_page)
        {
            butLeft.interactable = true;
            butRight.interactable = true;
            page = new_page;
        }
        if(new_page <= 1)
        {
            page = 1;
            butLeft.interactable = false;
        }
        if(new_page >= max_page)
        {
            page = max_page;
            butRight.interactable = false;
        }
    }
}
