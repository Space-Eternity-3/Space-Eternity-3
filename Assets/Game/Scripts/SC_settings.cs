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
    public bool settings_100;
    public AudioSource music;
    public SC_data SC_data;

    public void valueChanged()
    {
        if(!Input.GetMouseButton(0)) return;
	    if(namte=="volume") SC_data.volume=(a*slider.value+b)+"";
        if(namte=="camera_zoom") SC_data.camera_zoom=(a*slider.value+b)+"";
		if(namte=="music") SC_data.music=(a*slider.value+b)+"";
        SC_data.Save("settings");
    }
    public void valueRead()
    {
        float reat=0f;
        if(namte=="volume") reat=Parsing.FloatE(SC_data.volume);
        if(namte=="camera_zoom") reat=Parsing.FloatE(SC_data.camera_zoom);
		if(namte=="music") reat=Parsing.FloatE(SC_data.music);
        slider.value=(reat-b)/a;
    }
    public void updateText()
    {
        if(!settings_100) title.text=visibleNamte+": "+(slider.value*10)+"%";
        else title.text=visibleNamte+": "+(slider.value)+"%";
        if(namte=="music" && SC_data.menu) music.volume=(a*slider.value+b)*music_natural_volume;
    }
    void Update()
    {
        if(namte=="camera_zoom") valueRead();
        updateText();
    }
    void Start()
    {
        if(settings_100) a /= 10f;
        valueRead();
    }
}
