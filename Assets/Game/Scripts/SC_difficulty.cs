using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class SC_difficulty : MonoBehaviour
{
    public SC_fun SC_fun;
    public Image otp_image;
    public Text otp_text;
    public Text difficulty_title;

    public Button Arrow1, Arrow2;

    public string[] TextTexts = new string[]{"safe","easy","normal","hard","extreme","impossible"};
    public float[] ConvertDifficulty = new float[]{0f,0.7f,1f,1.43f,2f,10000f};
    public Color32[] ImageColors = new Color32[6];

    public int min_difficulty = 1, max_difficulty = 4;
    public int min_ctrl = 1, max_ctrl = 5;
    public int local_difficulty = 2;

    string fileName="";

    void Start()
    {
        fileName = SC_fun.SC_data.worldDIR + "difficulty.txt";
        if((int)SC_fun.SC_control.Communtron4.position.y==100) HardSet(0);
        else {
            string read_diff = ReadVariableSGP("difficulty");
            if(read_diff!="") local_difficulty = Parsing.IntU(read_diff);
            if(local_difficulty<0 || local_difficulty>5) local_difficulty=2;
        }
    }
    void Update()
    {
        SC_fun.difficulty = ConvertDifficulty[local_difficulty];
        otp_image.color = ImageColors[local_difficulty];
        otp_text.text = TextTexts[local_difficulty];

        bool ctrl = Input.GetKey(KeyCode.LeftControl);
        Arrow1.interactable = (local_difficulty > min_difficulty || (ctrl && local_difficulty > min_ctrl));
        Arrow2.interactable = (local_difficulty < max_difficulty || (ctrl && local_difficulty < max_ctrl));
    }
    public void HardSet(int diff)
    {
        difficulty_title.text = "Difficulty (server)";
        local_difficulty = diff;
        min_difficulty = diff;
        max_difficulty = diff;
        Arrow1.GetComponent<Transform>().position = new Vector3(0f,10000f,0f);
        Arrow2.GetComponent<Transform>().position = new Vector3(0f,10000f,0f);
    }
    public void ChangeDifficulty(bool right)
    {
        if(right) local_difficulty++; else local_difficulty--;
        SaveVariableSGP("difficulty",local_difficulty+"");
    }

    //Saving and reading sgp variables
    public void SaveVariableSGP(string variable_name, string value)
    {
        string all_text = "";
        bool included = false;
        string[] str = ReadDataSGP().Split(';');
        foreach(string st in str)
        {
            string[] s = st.Split(':');
            if(s[0]==variable_name)
            {
                s[1] = value;
                included = true;
            }
            if(s[0]!="") all_text += s[0] + ":" + s[1] + ";";
        }
        if(!included) all_text += variable_name + ":" + value + ";";
        try
        {
            string file = SC_fun.SC_data.worldDIR + "SgpData.se3";
            File.WriteAllText(file,all_text);
        }
        catch(Exception) {}
    }
    public string ReadVariableSGP(string variable_name)
    {
        string[] str = ReadDataSGP().Split(';');
        foreach(string st in str)
        {
            string[] s = st.Split(':');
            if(s[0]==variable_name) return s[1];
        }
        return "";
    }
    public string ReadDataSGP()
    {
        try
        {
            string file = SC_fun.SC_data.worldDIR + "SgpData.se3";
            if(File.Exists(file)) return File.ReadAllText(file);
            else return "";
        }
        catch(Exception) { return ""; }
    }
}
