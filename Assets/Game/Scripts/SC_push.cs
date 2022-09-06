using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_push : MonoBehaviour
{
    public int[] pushMemory = {1,2,3,4,5,6,7,8,9};
    public int clicked_on=0;
    public float a=10f, b=3f;

    public SC_fun SC_fun;
    public SC_control SC_control;
    public SC_selected SC_selected;
    public SC_slots SC_slots;
    public Transform Communtron4;

    void Update()
    {
        if(!Input.GetMouseButton(0)) clicked_on=0;

        float x = Input.mousePosition.x/Screen.width;
        if(Screen.width/Screen.height>=1.7777777f)
        {
            float shouldwidth = 1.7777777f*Screen.height;
            x = (Input.mousePosition.x - (Screen.width-shouldwidth)/2f)/shouldwidth;
        }

        int mouse_over = (int) Mathf.Round(a*x+b);
        if(mouse_over<1||mouse_over>9) return;

        if(clicked_on==0) return;
        int i;
        while(mouse_over!=clicked_on)
        {
            if(mouse_over>clicked_on) //push right
            {
                Push(clicked_on,true);
                clicked_on++;
            }
            else //push left
            {
                Push(clicked_on-1,false);
                clicked_on--;
            }
        }
    }
    void Push(int push_id, bool right)
    {
        int pomX = SC_slots.SlotX[push_id-1];
        int pomY = SC_slots.SlotY[push_id-1];
        int pomZ = pushMemory[push_id-1];
        int pomYA = SC_slots.SlotYA[push_id-1];
        int pomYB = SC_slots.SlotYB[push_id-1];

        SC_slots.SlotX[push_id-1] = SC_slots.SlotX[push_id];
        SC_slots.SlotX[push_id] = pomX;

        SC_slots.SlotY[push_id-1] = SC_slots.SlotY[push_id];
        SC_slots.SlotY[push_id] = pomY;

        pushMemory[push_id-1] = pushMemory[push_id];
        pushMemory[push_id] = pomZ;

        SC_slots.SlotYA[push_id-1] = SC_slots.SlotYA[push_id];
        SC_slots.SlotYA[push_id] = pomYA;

        SC_slots.SlotYB[push_id-1] = SC_slots.SlotYB[push_id];
        SC_slots.SlotYB[push_id] = pomYB;

        SC_fun.pushed_markers[push_id-1] = true;
        SC_fun.pushed_markers[push_id] = true;
        
        if(right) SC_selected.selected++;
        else SC_selected.selected--;
        
        if(Communtron4.position.y==100f){
            SC_control.SendMTP("/InventoryPush "+SC_control.connectionID+" "+push_id);
        }
    }
}
