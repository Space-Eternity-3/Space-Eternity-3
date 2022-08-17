using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_bp_upg : MonoBehaviour
{
    public int state = 0;
    public Transform upg,bp;
    public Text upper, star;
    Vector3 upgS,bpS,hidden;
    bool wysuned = false;
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
        if(SC_inv_mover.active && Input.GetKeyDown(KeyCode.D)) change_state();

        if(state==0)
        {
            upper.text = "Upgrades";
            upg.localPosition = upgS;
        }else upg.localPosition = hidden;
        
        if(state==1)
        {
            upper.text = "Backpack";
            bp.localPosition = bpS;
        }else bp.localPosition = hidden;

        if(state==2)
        {
            upper.text = "Manager";
            //bp.localPosition = bpS;
        }//else bp.localPosition = hidden;

        if(state==3)
        {
            upper.text = "Chat";
            //bp.localPosition = bpS;
        }//else bp.localPosition = hidden;
    }
    public void change_state()
    {
        if(state==0) state=1;
        else if(state==1) state=0;
    }
    public void set_state(int st)
    {
        state = st;
    }
}
