using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_chat : MonoBehaviour
{
    public string chat;
    public Text txc;

    void Update()
    {
        string[] messages = chat.Split('`');
        int i,lngt=messages.Length;
        string pom="";
        for(i=0;(i<20&&i<lngt);i++) pom="\n"+messages[lngt-i-1]+pom;
        txc.text=pom;
    }
}
