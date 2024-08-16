using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_bp_upg : MonoBehaviour
{
    public int state = 0;
    public Transform upg,bp,mng,ch;
    public Text upper, upper_new;
    public Button B,U,M,C,Bmin,Bplu;
    Vector3 upgS,bpS,hidden;
    public SC_inv_mover SC_inv_mover;
	public SC_control SC_control;

    void Start()
    {
        upgS = upg.localPosition;
        bpS = bp.localPosition + new Vector3(-200f,0f,0f);
        hidden = new Vector3(10000f,0f,0f);
    }
    void Update()
    {
        if(state==0)
        {
            upper.text = "Upgrades";
            upper.fontSize = 32;
            upg.localPosition = upgS;
            U.interactable = false;
        }else {upg.localPosition = hidden; U.interactable = true;}
        
        if(state==1)
        {
            upper.text = "Backpack";
            upper.fontSize = 32;
            bp.localPosition = bpS;
            B.interactable = false;
        }else {bp.localPosition = hidden; B.interactable = true;}

        if(state==2)
        {
            upper.text = "Junkyard";
            upper.fontSize = 32;
            mng.localPosition = bpS;
            M.interactable = false;
        }else {mng.localPosition = hidden; M.interactable = true;}

        if(state==3)
        {
            upper.text = "Spaceships";
            upper.fontSize = 30;
            ch.localPosition = bpS;
            C.interactable = false;
        }else {ch.localPosition = hidden; C.interactable = true;}

        upper_new.text = upper.text;
        Bmin.interactable = state > 0;
        Bplu.interactable = state < 2;
    }
    public void add_state(int delta)
    {
        state += delta;
    }
    public void set_state(int st)
    {
        state = st;
    }
}
