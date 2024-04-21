using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SC_universe_create : MonoBehaviour
{
    public RectTransform Settings, Creation;
    public RectTransform DatapackOptions, DatapackStatic;
    public Text CreationTitle, ButtonText;

    public int creating_index = 0;

    public SC_main_buttons SC_main_buttons;
    public SC_difficulty SC_difficulty;
    public SC_data SC_data;
    public SC_universe[] SC_universe = new SC_universe[8];
    public SC_bird bird_lockdiff, bird_keepinv;

    void Update()
    {
        if(creating_index==0)
        {
            Settings.localPosition = new Vector3(0f,0f,0f);
            Creation.localPosition = new Vector3(1000f,0f,0f);
        }
        else
        {
            Settings.localPosition = new Vector3(1000f,0f,0f);
            Creation.localPosition = new Vector3(0f,0f,0f);
        }
    }
    void ChangeCreatingIndex(int n)
    {
        creating_index = n;
        CreationTitle.text = " Universe " + n;
        ButtonText.text = "Create & Play";
        SC_difficulty.local_difficulty = 2;
        bird_lockdiff.state = false;
        bird_keepinv.state = false;
        SC_data.ResetDatapack();
        SC_difficulty.SetFileName(n);
    }
    public void StartCreating(int n)
    {
        ChangeCreatingIndex(n);
        SC_main_buttons.SAS(3);

        if(Directory.Exists("../../saves/Universe"+n+"/") && File.Exists("../../saves/Universe"+n+"/UniverseInfo.se3"))
        {
            DatapackOptions.localPosition = new Vector3(10000f,0f,0f);
            DatapackStatic.localPosition = new Vector3(0f,0f,0f);
        }
        else
        {
            DatapackOptions.localPosition = new Vector3(0f,0f,0f);
            DatapackStatic.localPosition = new Vector3(10000f,0f,0f);
        }
    }
    public void EndCreating()
    {
        ChangeCreatingIndex(0);
        SC_main_buttons.SAS(1);
    }
    public void CreateAndPlay()
    {
        SC_difficulty.SaveVariableSGP("difficulty",SC_difficulty.local_difficulty+"");
        
        if(bird_lockdiff.state) SC_difficulty.SaveVariableSGP("lockdiff","1");
        else SC_difficulty.SaveVariableSGP("lockdiff","0");
        
        if(bird_keepinv.state) SC_difficulty.SaveVariableSGP("keepinv","1");
        else SC_difficulty.SaveVariableSGP("keepinv","0");
        
        SC_universe[creating_index].V_PlayDirect();
    }
}
