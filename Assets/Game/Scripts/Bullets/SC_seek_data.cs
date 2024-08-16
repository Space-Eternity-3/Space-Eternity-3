using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_seek_data : MonoBehaviour
{
    public int[] bulletID = new int[3844];
    public string[] steerData = new string[3844];

    public SC_lists SC_lists;

    public int getSeekPointer(int bulID)
    {
        int i;
        for(i=0;i<3844;i++)
            if(bulletID[i]==bulID) return i;
        return -1;
    }

    public int createSeekPointer(int bulID)
    {//Only singleplayer
        int i;
        bool[] eliminated = new bool[3844];
        foreach(SC_bullet bul in SC_lists.SC_bullet)
            if(bul.seekPointer!=-1) eliminated[bul.seekPointer]=true;
        for(i=0;i<3844;i++)
            if(!eliminated[i])
            {
                bulletID[i] = bulID;
                steerData[i] = "0000";
                return i;
            }
        return -1;
    }

    public char tryGet(int pID, int ind)
    {
        if(steerData[pID].Length>ind) return steerData[pID][ind];
        Debug.LogWarning("tryGet -> 0, age="+ind);
        return '0';
    }
}
