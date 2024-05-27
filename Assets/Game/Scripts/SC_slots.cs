using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SC_slots : MonoBehaviour
{
    public int[] SlotX = new int[9];
    public int[] SlotY = new int[9];

    public int[] BackpackX = new int[21];
    public int[] BackpackY = new int[21];

    //Server possibilities
    public int[] SlotYA = new int[9];
    public int[] SlotYB = new int[9];
    public int[] BackpackYA = new int[21];
    public int[] BackpackYB = new int[21];

    //Double give poms
    public int SlotUT = -1;

    int comm_time = 0;
    public Text inv_full_text;

    public int comm_time2 = 0;
    public Text inv_full_text2;

    public Color32 iftc, ifmtpc;

    public Transform Communtron1;
    public Transform Communtron4;
    public SC_backpack SC_backpack;
    public SC_bp_upg SC_bp_upg;
    public SC_inv_mover SC_inv_mover;
    public SC_control SC_control;
    public SC_push SC_push;
    public SC_backT[] SC_backT = new SC_backT[9];
    public SC_inv_number[] SC_inv_number = new SC_inv_number[30];

    int GetSlot(int item, int count, bool aI, bool aB, bool potential_change)
    {
        int h,i;
        int[] how = new int[4];

        if((SC_bp_upg.state == 1 && SC_inv_mover.active) || true)
        {
            //Backpack opened (always)
            how[0]=0;
            how[1]=2;
            how[2]=1;
            how[3]=3;
        }
        else
        {
            //Backpack closed
            how[0]=0;
            how[1]=1;
            how[2]=2;
            how[3]=3;
        }
        
        int YA,YB;
        if(count > 0)
        {
            for(h=0;h<4;h++)
            {
                if(how[h]==0) //IP+
                if(aI)
                {
                    for(i=0;i<9;i++)  
                    {
                        if(potential_change)
                        {
                            YA = SlotYA[i];
                            YB = SlotYB[i];
                        }
                        else
                        {
                            YA = SlotY[i];
                            YB = SlotY[i];
                        }
                        
                        if(SlotX[i]==item && YB>0)
                        {
                            if(SlotY[i]>0) return i;
                            else return -1;
                        }
                    }
                }
                

                if(how[h]==1) //IN+
                if(aI)
                {
                    for(i=0;i<9;i++)  
                    {
                        if(potential_change)
                        {
                            YA = SlotYA[i];
                            YB = SlotYB[i];
                        }
                        else
                        {
                            YA = SlotY[i];
                            YB = SlotY[i];
                        }
                        if(YB==0) return i;
                        if(SlotY[i]==0) return -1;
                    }
                }

                if(how[h]==2) //BP+
                if(aB)
                {
                    for(i=0;i<SC_backpack.Lim();i++)  
                    {
                        if(potential_change)
                        {
                            YA = BackpackYA[i];
                            YB = BackpackYB[i];
                        }
                        else
                        {
                            YA = BackpackY[i];
                            YB = BackpackY[i];
                        }
                        
                        if(BackpackX[i]==item && YB>0)
                        {
                            if(BackpackY[i]>0) return i+9;
                            else return -1;
                        }
                    }
                }

                if(how[h]==3) //BN+
                if(aB)
                {
                    for(i=0;i<SC_backpack.Lim();i++)  
                    {
                        if(potential_change)
                        {
                            YA = BackpackYA[i];
                            YB = BackpackYB[i];
                        }
                        else
                        {
                            YA = BackpackY[i];
                            YB = BackpackY[i];
                        }
                        if(YB==0) return i+9;
                        if(BackpackY[i]==0) return -1;
                    }
                }
            }
        }

        //Use find
        if(count < 0)
        {
            for(h=0;h<4;h++)
            {
                if(h==1) //IN-
                if(aI)
                {
                    for(i=0;i<9;i++)  
                    {
                        if(potential_change)
                        {
                            YA = SlotYA[i];
                            YB = SlotYB[i];
                        }
                        else
                        {
                            YA = SlotY[i];
                            YB = SlotY[i];
                        }
                        if(SlotX[i]==item && YA>=-count) return i;
                    }
                }

                if(h==3) //BN-
                if(aB)
                {
                    for(i=0;i<SC_backpack.Lim();i++)  
                    {
                        if(potential_change)
                        {
                            YA = BackpackYA[i];
                            YB = BackpackYB[i];
                        }
                        else
                        {
                            YA = BackpackY[i];
                            YB = BackpackY[i];
                        }
                        if(BackpackX[i]==item && YA>=-count) return i+9;
                    }
                }
            }
        }
        return -1;
    }
    public int InvChange(int item, int count, bool aI, bool aB, bool sureMTP)
    {
        int slot = GetSlot(item,count,aI,aB,true);
        int ret;

        //Change
        if(slot==-1) {Debug.LogError("InvChange error"); return -1;}
        PopInv(slot,count/Mathf.Abs(count));

        if(slot < 9)
        {
            SlotX[slot] = item;
            SlotY[slot] += count;
            if(count<0 || sureMTP || Communtron4.position.y!=100) SlotYA[slot] += count;
            if(count>0 || sureMTP || Communtron4.position.y!=100) SlotYB[slot] += count;
            ret = SC_push.pushMemory[slot]-1;
        }
        else
        {
            slot = slot-9;
            BackpackX[slot] = item;
            BackpackY[slot] += count;
            if(count<0 || sureMTP || Communtron4.position.y!=100) BackpackYA[slot] += count;
            if(count>0 || sureMTP || Communtron4.position.y!=100) BackpackYB[slot] += count;
            ret = slot+9;
        }
        return ret;
    }
    public bool InvHaveB(int item, int count, bool aI, bool aB, bool physical, int commID)
    {
        if(InvHaveM(item,count,aI,aB,physical,commID)>0) return true;
        else return false;
    }
    public int InvHaveM(int item, int count, bool aI, bool aB, bool physical, int commID)
    {
		if(SC_control.pause || !SC_control.living) return 0;

        int slot = GetSlot(item,count,aI,aB,physical);
        int YA,YB;

        if(slot!=-1)
        if(slot<9)
        {
            YA = SlotYA[slot];
            YB = SlotYB[slot];

            if(count<0)
            {
                if(YA==YB && YA==-count) return 2;
            }
            return 1;
        }
        else
        {
            slot = slot-9;
            YA = BackpackYA[slot];
            YB = BackpackYB[slot];

            if(count<0)
            {
                if(YA==YB && YA==-count) return 2;
            }
            return 1;
        }
        
        if(commID != 0)
        {
            if(commID == 1) SC_control.InfoUp("Inventory full",380);
            else if(commID == 2) SC_control.InfoUp("Blocked slot",380);
            else return 0;
        }
        return 0;
    }
    public bool InvHaving(int item)
    {
        if(SC_control.pause || !SC_control.living) return false;

        int selected = (int)Communtron1.position.y-1;
        if(SlotX[selected]==item && SlotYA[selected]>0) return true;
        else return false;
    }
    public int SelectedItem()
    {
        if(SC_control.pause || !SC_control.living) return 0;

        int selected = (int)Communtron1.position.y-1;
        if(SlotYA[selected]>0) return SlotX[selected];
        else return 0;
    }
    int memNps(int n)
    {
        int i;
        for(i=0;i<9;i++) if(SC_push.pushMemory[i] == n+1) return i;
        return -1; //Error making
    }
    public void InvCorrectionMTP(int item, int deltaCount, int slot, int propCount)
    {
        if(slot < 9)
        {
            slot = memNps(slot);
            if(SlotX[slot] == item)
            {
                if(propCount>0) SlotYB[slot] -= propCount;
                if(propCount<0) SlotYA[slot] -= propCount;
                SlotY[slot] += deltaCount;
            }
            else SC_control.MenuReturn();
        }
        else
        {
            slot -= 9;
            if(BackpackX[slot] == item)
            {
                if(propCount>0) BackpackYB[slot] -= propCount;
                if(propCount<0) BackpackYA[slot] -= propCount;
                BackpackY[slot] += deltaCount;
            }
            else SC_control.MenuReturn();
        }
    }
    public void ResetYAB()
    {
        int i;
        for(i=0;i<9;i++)
        {
            SlotYA[i] = SlotY[i];
            SlotYB[i] = SlotY[i];
        }
        for(i=0;i<21;i++)
        {
            BackpackYA[i] = BackpackY[i];
            BackpackYB[i] = BackpackY[i];
        }
    }
    public void PopInv(int slot, int signum)
    {
        int i;
        if(slot<9)
        {
            SC_inv_number[slot].smallerIN = 6 * signum;
            return;
        }
        for(i=9;i<30;i++)
        {
            if(SC_inv_number[i].SlotID==slot-9)
            {
                SC_inv_number[i].smallerIN = 6 * signum;
                return;
            }
        }
        Debug.LogError("PopInv error");
    }
    void FixedUpdate()
    {
        //Inv comm
        if(comm_time > 5) comm_time -= 5;
        else comm_time = 0;

        if(comm_time > 255) inv_full_text.color = new Color32(iftc.r,iftc.g,iftc.b,255);
        else inv_full_text.color = new Color32(iftc.r,iftc.g,iftc.b,(byte)comm_time);

        //Mtp comm
        if(comm_time2 > 5) comm_time2 -= 5;
        else comm_time2 = 0;

        if(comm_time2 > 255) inv_full_text2.color = new Color32(ifmtpc.r,ifmtpc.g,ifmtpc.b,255);
        else inv_full_text2.color = new Color32(ifmtpc.r,ifmtpc.g,ifmtpc.b,(byte)comm_time2);
    }
}
