using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_bg_color : MonoBehaviour
{
    public int gradient_size;
    public Color32[] BgColors = new Color32[32];
    public Renderer Background;
    public SC_data SC_data;
    public SC_fun SC_fun;

    void Start()
    {
        int i;
        for(i=0;i<32;i++) {
            string tag = SC_data.TagSpecialGet(SC_data.BiomeTags[i],"bgcolor");
            string[] tags = tag.Split('-');
            if(tags.Length < 4) continue;
            byte R=0,G=0,B=0;
            try {
                R = (byte)int.Parse(tags[1]);
                G = (byte)int.Parse(tags[2]);
                B = (byte)int.Parse(tags[3]);
            }catch(Exception) {
                continue;
            }
            BgColors[i] = new Color32(R,G,B,255);
        }
    }
    void Update()
    {
        Vector3 pos = SC_fun.SC_control.player.position; pos = new Vector3(pos.x,pos.y,0f);
        Vector3 pos_big = new Vector3((float)Math.Round(pos.x/100f),(float)Math.Round(pos.y/100f),0f);

        if(Vector3.Distance(pos,new Vector3(0f,0f,0f)) > 1000000f) return;
        
        Vector3[] udels = new Vector3[9];
		udels[0] = new Vector3(-1f,1f,0f);
		udels[1] = new Vector3(0f,1f,0f);
		udels[2] = new Vector3(1f,1f,0f);
		udels[3] = new Vector3(-1f,0f,0f);
		udels[4] = new Vector3(0f,0f,0f);
		udels[5] = new Vector3(1f,0f,0f);
		udels[6] = new Vector3(-1f,-1f,0f);
		udels[7] = new Vector3(0f,-1f,0f);
		udels[8] = new Vector3(1f,-1f,0f);

        int i;
        for(i=0;i<9;i++)
            udels[i] = pos_big + udels[i];
        
        int[] types = new int[9];
        float[] to_full = new float[9];
        float[] to_emptiness = new float[9];

        for(i=0;i<9;i++)
        {
            int ulam = SC_fun.CheckID((int)udels[i].x,(int)udels[i].y);
            int type = int.Parse(SC_fun.GetBiomeString(ulam).Split('b')[1]);
            types[i] = type;

            udels[i]*=100;
            if(type==0) {
                to_full[i] = 10000;
                to_emptiness[i] = -10000 + gradient_size;
            }
            else {
                float radius = SC_fun.GetBiomeSize(ulam);
                Vector3 move = SC_fun.GetBiomeMove(ulam);
                udels[i] += move;
                float distance = Vector3.Distance(pos,udels[i]);
                to_full[i] = distance - radius;
                to_emptiness[i] = -to_full[i] + gradient_size;
            }
        }

        for(i=0;i<9;i++)
        {
            //Make better colors
            if(to_full[i]<=0)
                Background.material.color = BgColors[types[i]];
        }
    }
}
