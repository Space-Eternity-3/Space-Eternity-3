using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_bg_color : MonoBehaviour
{
    public bool technical_triple_point;
    public int gradient_size;
    public Color32[] BossColors = new Color32[7];
    public Color32[] BgColors = new Color32[32];
    public Renderer Background;
    public SC_data SC_data;
    public SC_fun SC_fun;
    public SC_boss SC_boss; //global mother boss

    struct SBiomeFC
    {
        public int ulam;
        public int type;
        public int priority;
        public float distance;
        public float color_weight;
    }

    Color32 BossBattleLerp(Color32 bgcolor)
    {
        List<SC_boss> boses = SC_fun.SC_control.SC_lists.SC_boss;
        foreach(SC_boss bos in boses)
        {
            float rat = bos.GetTransitionFraction(bos.GetArenaFcr());
            if(rat!=0f) return FluentLerp(bgcolor,BossColors[bos.type],rat);
        }
        return bgcolor;
    }
    Color32 FluentLerp(Color32 A, Color32 B, float perc)
    {
        return Color32.Lerp(A,B,SC_fun.FluentFraction(perc));
    }
    Color32 MultiLerp4(Color32[] colors, float[] weights)
    {
        float w1 = weights[1] / (weights[0]+weights[1]);
        float w2 = weights[3] / (weights[2]+weights[3]);
        float w3 = (weights[2]+weights[3]) / ((weights[0]+weights[1]) + (weights[2]+weights[3]));
        Color32 col1 = FluentLerp(colors[0],colors[1],w1);
        Color32 col2 = FluentLerp(colors[2],colors[3],w2);
        return FluentLerp(col1,col2,w3);
    }
    void Start()
    {
        int i,j;
        for(i=0;i<32;i++) for(j=-1;j<=6;j++)
        {
            string tag;
            if(j==-1) tag = SC_data.TagSpecialGet(SC_data.BiomeTags[i],"bgcolor");
            else tag = SC_data.TagSpecialGet(SC_data.BiomeTags[i],"boss."+j+".color");
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
            Color32 new_color = new Color32(R,G,B,255);
            if(j==-1) BgColors[i] = new_color;
            else BossColors[j] = new_color;
        }
    }
    void LateUpdate()
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

        int i,j;
        for(i=0;i<9;i++)
            udels[i] = pos_big + udels[i];
        
        SBiomeFC[] biomeData = new SBiomeFC[9];

        for(i=0;i<9;i++)
        {
            biomeData[i].ulam = SC_fun.MakeUlam((int)udels[i].x,(int)udels[i].y);
            CBiomeInfo biomeInfo = Generator.GetBiomeData(biomeData[i].ulam);
            biomeData[i].type = biomeInfo.biome;

            udels[i]*=100;
            if(biomeData[i].type==0)
            {
                biomeData[i].distance = 10000f;
                biomeData[i].priority = -1;
            }
            else
            {
                udels[i] += biomeInfo.move;
                float gradient_size = biomeInfo.size;
                if(Generator.tag_gradient[biomeData[i].type] < gradient_size) gradient_size = Generator.tag_gradient[biomeData[i].type];
                biomeData[i].distance = Vector3.Distance(pos,udels[i]) - gradient_size;
                biomeData[i].priority = Generator.tag_priority[biomeData[i].type];
            }
        }

        //Include only local biomes
        List<SBiomeFC> biomes = new List<SBiomeFC>();
        for(i=0;i<9;i++)
            if(biomeData[i].type!=0 && biomeData[i].distance < gradient_size/2) {
                if(biomeData[i].distance < -gradient_size/2) biomeData[i].color_weight = 1f;
                else biomeData[i].color_weight = 1f-(biomeData[i].distance + gradient_size/2)/gradient_size;
                biomes.Add(biomeData[i]);
            }
        if(biomes.Count > 2) UnityEngine.Debug.LogWarning("More than 2 biomes on player.");

        if(biomes.Count == 0) //Set default color
            Background.material.color = BgColors[0];
        else if(biomes.Count == 1) //Set mix of 2 biome colors
            Background.material.color = FluentLerp(BgColors[0],BgColors[biomes[0].type],biomes[0].color_weight);
        else if(biomes.Count == 2) //Biome overlapping handler
        {
            //Biome 0 will have bigger priority
            if(Generator.IsBiggerPriority(biomes[1].ulam,biomes[0].ulam,biomes[1].priority,biomes[0].priority))
            {
                SBiomeFC pom = biomes[0];
                biomes[0] = biomes[1];
                biomes[1] = pom;
            }
            bool full0 = biomes[0].color_weight==1f;
            bool full1 = biomes[1].color_weight==1f;

            if(full0) //Entirely in biome 0
                Background.material.color = BgColors[biomes[0].type];
            else if(full1) {//Biome 0 and 1 gradient
                Background.material.color = FluentLerp(BgColors[biomes[1].type],BgColors[biomes[0].type],biomes[0].color_weight); }
            else { //Triple meeting point
                if(technical_triple_point) SC_fun.SC_control.InfoUp("Triple point",200);
                float x,y,x1,y1;
                x1 = biomes[0].color_weight; x = 1-x1;
                y1 = biomes[1].color_weight; y = 1-y1;
                Background.material.color = MultiLerp4(
                    new Color32[] { BgColors[biomes[0].type], BgColors[biomes[0].type], BgColors[biomes[1].type], BgColors[0] },
                    new float[] { x1*y , x1*y1 , x*y1 , x*y }
                );
            }
        }

        Background.material.color = BossBattleLerp(Background.material.color);
    }
}
