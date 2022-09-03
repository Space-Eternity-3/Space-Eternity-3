using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_chat : MonoBehaviour
{
    [TextArea(15, 20)]
    public string chat;
    public Text txc;
    public int max_lines;
    public int scroll = -1;
    public bool looking_at_chat = false;
    public bool typing = false;

    void Update()
    {
        string newText = "";
        string[] newLines = chat.Split('\n');
        int i, lngt = newLines.Length, min = 0;

        if (scroll == -1) scroll = lngt;
        if(looking_at_chat)
        {
            if (Input.GetAxisRaw("Mouse ScrollWheel") < 0) scroll++;
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0) scroll--;
        }
        if (scroll <= max_lines) scroll = max_lines;
        if (scroll >= lngt || !looking_at_chat) scroll = -1;

        if (scroll != -1) lngt = scroll;
        if (lngt > max_lines) min = lngt - max_lines;
        for(i=lngt-1;i>=min;i--)
        {
            newText = newLines[i] + newText;
            if(i!=min) newText = "\n" + newText;
        }
        txc.text = newText;
    }
}
