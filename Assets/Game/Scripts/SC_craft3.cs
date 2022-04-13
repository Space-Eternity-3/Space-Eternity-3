using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_craft3 : MonoBehaviour
{
    public SC_crafting1 SC_crafting1;
    public Transform Communtron2;
    public int selected_page = 0;
    public Button[] buttons = new Button[6];
    public Transform[] btT = new Transform[6];
    Vector3[] btV = new Vector3[6];

    public void PageChange(int n)
    {
        n += (int)Communtron2.position.z*6;
        if(n+1 > SC_crafting1.MaxPageB) return;
        selected_page = n;
    }
    void Start()
    {
        int i;
        for(i=0;i<6;i++)
        {
            btV[i] = btT[i].localPosition;
        }
    }
    void Update()
    {
        int i;
        for(i=0;i<6;i++)
        {
            if((int)Communtron2.position.z*6+i == selected_page) buttons[i].interactable = false;
            else buttons[i].interactable = true;

            if(SC_crafting1.MaxPageB >= (int)Mathf.Round(Communtron2.position.z)*6+i+1)
            {
                buttons[i].enabled = true;
                btT[i].localPosition = btV[i];
            }
            else
            {
                buttons[i].enabled = false;
                btT[i].localPosition = new Vector3(10000f,0f,0f);
            }
        }
    }
}
