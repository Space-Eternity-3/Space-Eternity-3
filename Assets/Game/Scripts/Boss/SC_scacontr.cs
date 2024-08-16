using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_scacontr : MonoBehaviour
{
    public List<SC_scaler_special> SC_scaler_special = new List<SC_scaler_special>();
    public void ScaleAllNow(float percentage)
    {
        foreach(SC_scaler_special scl in SC_scaler_special)
            scl.ScaleNow(percentage);
    }
}
