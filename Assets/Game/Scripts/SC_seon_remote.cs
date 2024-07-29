using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_seon_remote : MonoBehaviour
{
    public SC_object_holder SC_object_holder;
    public SC_fun SC_fun;

    public List<string> states = new List<string>();
    public List<int> hide_mode = new List<int>();
    public List<int> extended_mode = new List<int>();

    public Vector3 extension = new Vector3(0f,0f,0f); //position where object is extended
    Vector3 hiddenvector = new Vector3(0f,0f,-500f); //position where object is hidden

    /*
    0 - undone
    1 - doing
    2 - done
    3 - undoing
    */

    public string memState = "non-existing state";
    public int memHide = -1;
    public int memExtended = -1;
    public int animation_type = 0;
    
    bool started_already = false;
    bool abort_remote_update_now = false;

    //local vectors
    public Vector3 localDefault = new Vector3(0f,0f,0f);
    Vector3 localExtended = new Vector3(0f,0f,0f);
    Vector3 localHidden = new Vector3(0f,0f,0f);

    public void HideStateSet(string str, int mode)
    {
        int i,lngt=states.Count;
        for(i=0;i<lngt;i++)
        {
            if(states[i]==str)
            {
                hide_mode[i] = mode;
                return;
            }
        }
        states.Add(str);
        hide_mode.Add(mode);
        extended_mode.Add(0);
    }
    public void ExtendedStateSet(string str, int mode)
    {
        int i,lngt=states.Count;
        for(i=0;i<lngt;i++)
        {
            if(states[i]==str)
            {
                extended_mode[i] = mode;
                return;
            }
        }
        states.Add(str);
        hide_mode.Add(0);
        extended_mode.Add(mode);
    }

    int GetMode(string variant)
    {
        if(memState==SC_object_holder.actual_state) {
            if(variant=="hidden" && memHide!=-1) return memHide;
            if(variant=="extended" && memExtended!=-1) return memExtended;
        }

        int i,lngt=states.Count;
        for(i=0;i<lngt;i++)
        {
            if(states[i]==SC_object_holder.actual_state)
            {
                if(variant=="hidden") return hide_mode[i];
                if(variant=="extended") return extended_mode[i];
                return 0;
            }
        }
        return 0;
    }

    void Start()
    {
        localDefault = transform.localPosition;
        
        localExtended = localDefault + extension;
        localHidden = localDefault + hiddenvector;

        FixedUpdate();
    }
    void FixedUpdate()
    {
        if(transform.GetComponent<SC_boss>()!=null && started_already)
            transform.GetComponent<SC_boss>().FixedUpdateT();

        started_already = true;

        abort_remote_update_now = true;
        LateUpdate();
    }
    void LateUpdate()
    {
        bool jump_hiding = SC_object_holder.scaling_blocker!=0;
        
        int modeH = GetMode("hidden");
        int modeE = GetMode("extended");

        memState = SC_object_holder.actual_state;
        memHide = modeH; memExtended = modeE;

        if(SC_object_holder.SC_boss==null) return;
        SC_boss bos = SC_object_holder.SC_boss;
        float transition_fraction = bos.GetSeonTransitionFraction();

        //Hiding animation
        if(animation_type==1)
        {
            if(jump_hiding)
            {
                transform.localScale = new Vector3(1f,1f,1f);

                if(modeH==1 || modeH==2)
                    transform.localPosition = localHidden;

                if(modeH==0 || modeH==3)
                    transform.localPosition = localDefault;
            }
            else
            {
                Vector3 pscale = new Vector3(0f,0f,0f);

                if(modeH==0)
                    pscale = new Vector3(1f,1f,1f);

                if(modeH==1)
                    pscale = (1f-transition_fraction) * new Vector3(1f,1f,1f);

                if(modeH==2)
                    pscale = new Vector3(0f,0f,0f);

                if(modeH==3)
                    pscale = transition_fraction * new Vector3(1f,1f,1f);

                if(pscale != new Vector3(0f,0f,0f))
                {
                    transform.localPosition = localDefault;
                    transform.localScale = pscale;
                }
                else
                {
                    transform.localPosition = localHidden;
                    transform.localScale = new Vector3(1f,1f,1f);
                }
            }
        }

        //Extending animation
        if(animation_type==2)
        {
            if(modeE==0)
                transform.localPosition = localDefault;

            if(modeE==1)
                transform.localPosition = Vector3.Lerp(localDefault,localExtended,transition_fraction);

            if(modeE==2)
                transform.localPosition = localExtended;

            if(modeE==3)
                transform.localPosition = Vector3.Lerp(localDefault,localExtended,1f-transition_fraction);
        }

        if(!abort_remote_update_now) {
            if(transform.GetComponent<SC_boss>()!=null)
                bos.SC_player_follower3.RemoteUpdate();
        }
        else abort_remote_update_now = false;
    }
}
