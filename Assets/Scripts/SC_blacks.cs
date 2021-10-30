using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_blacks : MonoBehaviour
{
    public Image[] blacks;
    void Start()
    {
        int i;
        for(i=0;i<4;i++)
        {
            blacks[i].color=new Color32(0,0,0,255);
        }
    }
}
