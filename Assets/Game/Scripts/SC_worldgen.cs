using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CWorldDataManager
{
    SC_data SC_data;
    int act_ulam;

    CWorldDataManager(SC_data sdat)
    {
        SC_data = sdat;
    }

    /*
    
    field: 0-60
    place: 1-20
    index: 0-1
    
    */

    //Technical functions
    public void Load(int ulam)
    {
        act_ulam = ulam;
        //data download
    }
    public bool DataExists()
    {
        return false;
    }
    public void DataGenerate(string gencode)
    {
        return;
    }
    
    //Read functions
    public int GetData(int field)
    {
        int gfob = GetFob(field);
        if(gfob!=-1) return gfob;
        else return 0;
    }
    public int GetNbt(int place, int index)
    {
        return GetData(21+index+2*(place-1));
    }
    public int GetFob(int place)
    {
        return 0;
    }

    //Write functions
    public void UpdateData(int field, int data)
    {
        return;
    }
    public void UpdateNbt(int place, int index, int data)
    {
        UpdateData(21+index+2*(place-1),data);
    }
    public void ResetNbt(int place)
    {
        UpdateNbt(place,0,0);
        UpdateNbt(place,1,0);
    }
    public void UpdateFob(int place, int data)
    {
        return;
    }
}

public class SC_worldgen : MonoBehaviour
{
    //Empty class :)
}
