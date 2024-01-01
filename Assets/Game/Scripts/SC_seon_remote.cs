using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_seon_remote : MonoBehaviour
{
    public SC_structure SC_structure;

    public List<string> states = new List<string>();
    public List<int> hide_mode = new List<int>();
    public List<int> extended_mode = new List<int>();

    public Vector3 extension = new Vector3(0f,0f,0f); //position where object is extended
    public Vector3 hidevector = new Vector3(0f,0f,50f); //position where object disappears
    Vector3 hiddenvector = new Vector3(0f,0f,-500f); //position where object is hidden

    /*
    0 - undone
    1 - doing
    2 - done
    3 - undoing
    */

    public bool jump = true;
    public bool started_already = false;

    public int current_extension = 0;
    public int current_hide = 0;

    public int hiding_time = 1;
    public int extending_time = 1;

    public string memState = "default";
    public int memHide = -1;
    public int memExtended = -1;

    //absolute
    Vector3 localDefault = new Vector3(0f,0f,0f);

    //delta
    Vector3 localExtended = new Vector3(0f,0f,0f);
    Vector3 localHide = new Vector3(0f,0f,0f);
    Vector3 localHidden = new Vector3(0f,0f,0f);
    
    //scale
    Vector3 normalScale = new Vector3(1f,1f,1f);

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
        if(memState==SC_structure.actual_state) {
            if(variant=="hidden" && memHide!=-1) return memHide;
            if(variant=="extended" && memExtended!=-1) return memExtended;
        }

        int i,lngt=states.Count;
        for(i=0;i<lngt;i++)
        {
            if(states[i]==SC_structure.actual_state)
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
        if(SC_structure==null) return;

        localDefault = transform.localPosition;
        
        transform.position += extension;
        localExtended = transform.localPosition - localDefault;
        transform.localPosition = localDefault;
        
        transform.position += hidevector;
        localHide = transform.localPosition - localDefault;
        transform.localPosition = localDefault;

        transform.position += hiddenvector;
        localHidden = transform.localPosition - localDefault;
        transform.localPosition = localDefault;

        normalScale = transform.localScale;

        FixedUpdate();
    }
    void FixedUpdate()
    {
        FixedUpdateM(jump);
        jump = false;
    }
    void FixedUpdateM(bool starting)
    {
        if(SC_structure==null) return;

        int modeH = GetMode("hidden");
        int modeE = GetMode("extended");

        memState = SC_structure.actual_state;
        memHide = modeH; memExtended = modeE;

        if(SC_structure.scaling_blocker!=0 || starting)
        {
            if(modeE==1) modeE=2; if(modeE==3) modeE=0;
            if(modeH==1) modeH=2; if(modeH==3) modeH=0;
        }

        transform.localPosition = localDefault;
        transform.localScale = normalScale;
        
        VariableChanging(modeH,modeE);
        TemporaryBlocking();
        PhysicalChanging();

        if(transform.GetComponent<SC_boss>()!=null && started_already)
            transform.GetComponent<SC_boss>().FixedUpdateT();

        started_already = true;
    }
    void VariableChanging(int modeH, int modeE)
    {
        if(modeH==0) current_hide = 0;
        if(modeH==2) current_hide = hiding_time;
        if(modeH==1 && current_hide < hiding_time) current_hide++;
        if(modeH==3 && current_hide > 0) current_hide--;

        if(modeE==0) current_extension = 0;
        if(modeE==2) current_extension = extending_time;
        if(modeE==1 && current_extension < extending_time) current_extension++;
        if(modeE==3 && current_extension > 0) current_extension--;
    }
    void TemporaryBlocking()
    {
        if(transform.GetComponent<SC_asteroid>()!=null)
        {
            bool bpar,bcur = (current_hide > 0 && current_hide < hiding_time);
            if(transform.parent.GetComponent<SC_seon_remote>()==null) bpar = false;
            else {
                SC_seon_remote ssr = transform.parent.GetComponent<SC_seon_remote>();
                bpar = (ssr.current_hide > 0 && ssr.current_hide < ssr.hiding_time);
            }
            transform.GetComponent<SC_asteroid>().temporary_blocker = (bpar||bcur);
        }
    }
    void PhysicalChanging()
    {
        float fraction_hide = current_hide; fraction_hide /= hiding_time;
        float fraction_extension = current_extension; fraction_extension /= extending_time;

        if(fraction_hide!=1f) transform.localPosition += fraction_hide * localHide;
        else transform.localPosition += localHidden;
        transform.localPosition += fraction_extension * localExtended;
        if(fraction_hide!=1f) transform.localScale = (1f-fraction_hide) * normalScale;
    }
}
