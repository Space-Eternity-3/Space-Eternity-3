using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_settings : MonoBehaviour
{
    public Slider slider;
    public Text title;
    public string namte;
    public string visibleNamte;
    public float a,b;
    public float music_natural_volume;
    public bool settings_5;
    public bool settings_100;
    public AudioSource music;
    public SC_data SC_data;

    public void valueChanged()
    {
        if(!Input.GetMouseButton(0)) return;
	    if(namte=="volume") SC_data.volume=(a*slider.value+b)+"";
        if(namte=="camera_zoom") SC_data.camera_zoom=(a*slider.value+b)+"";
		if(namte=="music") SC_data.music=(a*slider.value+b)+"";
        if(namte=="graphics") SC_data.graphics=(a*slider.value+b)+"";
        SC_data.Save("settings");
    }
    public void valueRead()
    {
        float reat=0f;
        if(namte=="volume") reat=Parsing.FloatE(SC_data.volume);
        if(namte=="camera_zoom") reat=Parsing.FloatE(SC_data.camera_zoom);
		if(namte=="music") reat=Parsing.FloatE(SC_data.music);
        if(namte=="graphics") reat=Parsing.FloatE(SC_data.graphics);
        slider.value=(reat-b)/a;
    }
    public void updateText()
    {
        if(!settings_100 && !settings_5) title.text=visibleNamte+": "+(slider.value*10)+"%";
        else if(settings_100) title.text=visibleNamte+": "+(slider.value)+"%";
        else if(settings_5) title.text=visibleNamte+": "+(slider.value+1)+"/6";
        if(namte=="music" && SC_data.menu) music.volume=(a*slider.value+b)*music_natural_volume;
    }
    void Update()
    {
        if(namte=="camera_zoom") valueRead();
        updateText();

        if(namte=="graphics" && !Input.GetMouseButton(0))
            SetGraphicsLevel((int)slider.value);
    }
    void Start()
    {
        if(settings_100) a /= 10f;
        valueRead();
    }

    //By ChatGPT
    public void SetGraphicsLevel(int graphics_level)
    {
        if (QualitySettings.GetQualityLevel() != graphics_level)
        {
            QualitySettings.SetQualityLevel(graphics_level, true);
            Debug.Log($"Graphics level set to {GetGraphicsLevelName(graphics_level)}");
        }
    }
    private string GetGraphicsLevelName(int graphics_level)
    {
        switch (graphics_level)
        {
            case 0: return "Very Low";
            case 1: return "Low";
            case 2: return "Medium";
            case 3: return "High";
            case 4: return "Very High";
            case 5: return "Ultra";
            default: return "Unknown";
        }
    }
}
