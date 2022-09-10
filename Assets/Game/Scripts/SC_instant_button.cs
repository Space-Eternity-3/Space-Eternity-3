using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SC_instant_button : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int ID;
    public int index;
	public bool special;
    public bool ASAC; //Allow Spam After Cooldown
    public int ASAC_cooldown;
    public int ASAC_mode;

    bool ASAC_pressed=false;
    int press_time=0;

    public SC_arrow_rot SC_arrow_rot;
    public SC_selected SC_selected;
    public SC_upgrades SC_upgrades;
    public SC_backpack SC_backpack;
    public SC_craft2 SC_craft2;
    public SC_push SC_push;
    public SC_data SC_data;
    public SC_craft3 SC_craft3;
	public SC_resume SC_resume;
    public SC_Fob21 SC_Fob21;
    public SC_control SC_control;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(Input.GetMouseButtonDown(0)) Activated(0);
        if(Input.GetMouseButtonDown(1)) Activated(1);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if(Input.GetMouseButtonUp(ASAC_mode)) ASAC_pressed=false;
        press_time=0;
    }
    void FixedUpdate()
    {
        if(ASAC_pressed&&press_time>=ASAC_cooldown&&press_time%2==0&&ASAC)
        {
            if(Input.GetMouseButton(ASAC_mode)) Activated(ASAC_mode);
        }
        if(ASAC_pressed) press_time++;
    }
    void Activated(int mode)
    {
        if(ASAC&&mode==ASAC_mode) ASAC_pressed=true;
        Button button=gameObject.GetComponent<Button>();

        if(mode==0)
        {
            if(ID==1) SC_arrow_rot.ChangePosVisible();
            if(ID==2)
            {
                SC_selected.set_selected(index);
                SC_push.clicked_on=index;
            }
            if(ID==3) SC_upgrades.Select(index);
            if(ID==4) SC_backpack.Select(index,true);
            if(ID==5)
			{
				if(!special) SC_backpack.Import(index,true);
				else SC_backpack.ImportArt(index);
			}
            if(ID==6) SC_upgrades.Upgraded();
            if(ID==7) if(button.interactable) SC_craft2.Crafted();
            if(ID==8) SC_data.RemoveWarning();
            if(ID==9) SC_craft3.PageChange(index);
			if(ID==10)
			{
				if(index==1) 
				{
					SC_backpack.destroyLock = true;
					SC_resume.Resume();
				}
				if(index==2) SC_resume.Quit();
                if(index==3) SC_resume.GiveUp();
			}
        }
        else if(mode==1)
        {
            if(ID==4) SC_backpack.Select(index,false);
            if(ID==5)
			{
				if(!special) SC_backpack.Import(index,false);
				else SC_backpack.ImportArt(index);
			}
        }
        if(mode==0 || mode==1)
        {
            if(ID==11) if(button.interactable && button.enabled)
            {
                SC_structure rts = transform.root.GetComponent<SC_structure>();
                if(rts!=null)
                    if(rts.st_structs[0]!=null)
                    {
                        SC_boss bts = rts.st_structs[0].GetComponent<SC_boss>();
                        if(bts!=null && SC_Fob21.item==5 && SC_Fob21.count>=10)
                        {
                            if(!bts.multiplayer)
                            {
                                SC_Fob21.count -= 10;
                                if(SC_Fob21.count==0) SC_Fob21.item = 0;
                                SC_Fob21.SaveSGP();

                                bts.dataID[2] = 1;
                                bts.resetScr();
                            }
                            else
                            {
                                SC_control.SendMTP("/TryBattleStart "+SC_control.connectionID+" "+bts.bID+" "+SC_Fob21.ID+" "+SC_Fob21.uID);
                                SC_Fob21.screen_button_cooldown = 125;
                            }
                        }
                    }
            }
        }
    }
}
