using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SC_upgrades : MonoBehaviour
{
    public Button[] buttons = new Button[5];
    public int[] MTP_levels = new int[5];
    public int[] UPG_levels = new int[5];
    
    public Button main_button;
    public Text about,namet,countt;
    public RawImage img,item;
    public Transform darkner;
    public Transform countTR;
    public SC_fun SC_fun;
    public SC_control SC_control;
    public SC_data SC_data;
    public SC_slots SC_slots;
    public Transform Communtron4;
    public Transform max_frame,max_about;
    public Vector3 frame_offset,about_offset;

    public string[] namets = new string[5];
    public string[] abouts = new string[5];
    public Texture[] UPG_images = new Texture[5];

    int UPG_selected=0;
    Vector3 startPosDarkner=new Vector3(0f,0f,0f);
    Vector3 startPosCountTR=new Vector3(0f,0f,0f);
    Vector3 startMaxFrame=new Vector3(0f,0f,0f);
    Vector3 startMaxAbout=new Vector3(0f,0f,0f);
	
	public Transform at_disabled, at_enabled;
	Vector3 at_v3_visible=new Vector3(0f,0f,0f);
	Vector3 at_v3_hidden=new Vector3(0f,0f,0f);

    int gI=0;

    public Image[] visibleL = new Image[25];
    public Color32 C_gray,C_green;

    void Start()
    {
        int i;
        if((int)Communtron4.position.y!=100)
        {
            for(i=0;i<5;i++) UPG_levels[i] = int.Parse(SC_data.upgrades[i]);
            for(i=0;i<5;i++) MTP_levels[i] = UPG_levels[i];
        }

        startPosDarkner=darkner.localPosition;
        startPosCountTR=countTR.localPosition;
        startMaxFrame=max_frame.localPosition;
        startMaxAbout=max_about.localPosition;
		
		at_v3_visible=at_disabled.localPosition;
		at_v3_hidden=at_enabled.localPosition;
    }
    public void MTP_loadUpgrades(string entry)
    {
        int i;
        string[] upgS = entry.Split(';');
        for(i=0;i<5;i++)
        {
            UPG_levels[i] = int.Parse(upgS[i]);
            MTP_levels[i] = UPG_levels[i];
        }
    }
    public void Select(int I)
    {
        gI=I;
    }
    public void SelectIN(int I)
    {
        UPG_selected=I;
        img.texture=UPG_images[I];
        if(UPG_levels[I]<5)
        {
            max_frame.localPosition=startMaxFrame;
            max_about.localPosition=startMaxAbout;
            if(true) about.text=abouts[I];
            else
            {
                if(UPG_levels[I]<4) about.text=abouts[I];
                else about.text="Artefact Slot";
            }
        }
        else
        {
            max_frame.localPosition=startMaxFrame+frame_offset;
            max_about.localPosition=startMaxAbout+about_offset;
            about.text="Max Level!";
        }
        namet.text=namets[I];

        int i;
        for(i=0;i<5;i++)
        {
            buttons[i].interactable=true;
        }
        buttons[I].interactable=false;
        //Update();
    }
    int costItemInt(int ust)
    {
        switch(UPG_levels[ust])
        {
            case 0: return 10;
            case 1: return 10;
            case 2: return 10;
            case 3: return 5;
            case 4: return 5;
            case 5: return 1;
        }
        return 0;
    }
    int costCount(int ust)
    {
        switch(UPG_levels[ust])
        {
            case 0: return 3;
            case 1: return 6;
            case 2: return 10;
            case 3: return 12;
            case 4: return 20;
            case 5: return 0;
        }
        return 0;
    }
    void Update()
    {
        SelectIN(gI);

        int i,j;
        int cc=costCount(UPG_selected);
        int cii=costItemInt(UPG_selected);
        
        item.texture=SC_fun.Item[cii];
        countt.text=cc+"";
        
        if(cc==0) countTR.localPosition=new Vector3(1000f,0f,0f);
        else countTR.localPosition=startPosCountTR;

        if(SC_slots.InvHaveB(cii,-cc,true,false,false,0)&&UPG_levels[UPG_selected]<5&&cc!=0)
        {
            main_button.interactable=true;
            darkner.localPosition=new Vector3(10000f,0f,0f);
        }
        else
        {
            main_button.interactable=false;
            darkner.localPosition=startPosDarkner;
        }

        for(i=0;i<5;i++)
        {
            for(j=0;j<5;j++)
            {
                if(UPG_levels[i]>j) visibleL[5*i+j].color=C_green;
                else visibleL[5*i+j].color=C_gray;
            }
        }
		
		if(true)
		{
			at_disabled.localPosition = at_v3_hidden;
			at_enabled.localPosition = at_v3_visible;
		}
		else
		{
			at_disabled.localPosition = at_v3_visible;
			at_enabled.localPosition = at_v3_hidden;
		}
    }
    public void Upgraded()
    {
        int cc=costCount(UPG_selected);
        int cii=costItemInt(UPG_selected);
        
        if(main_button.interactable==false) return;
        if(!(SC_slots.InvHaveB(cii,-cc,true,false,true,0)&&UPG_levels[UPG_selected]<5&&cc!=0)) return;
        
        int slot = SC_slots.InvChange(cii,-cc,true,false,true);
        if(Communtron4.position.y==100) SC_control.SendMTP("/Upgrade "+SC_control.connectionID+" "+cii+" "+cc+" "+UPG_selected+" "+slot);
        else MTP_levels[UPG_selected]++;
        UPG_levels[UPG_selected]++;
        Select(UPG_selected);
    }
}
