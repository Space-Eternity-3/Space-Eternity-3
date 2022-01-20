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
    public AudioSource music;
    public SC_data SC_data;

    public void valueChanged()
    {
        if(!Input.GetMouseButton(0)) return;
        title.text=visibleNamte+": "+(slider.value*10)+"%";
	    if(namte=="volume") SC_data.volume=(a*slider.value+b)+"";
        if(namte=="camera_zoom") SC_data.camera_zoom=(a*slider.value+b)+"";
		if(namte=="music") SC_data.music=(a*slider.value+b)+"";
        SC_data.Save("settings");
    }
    public void valueRead()
    {
        float reat=0f;
        if(namte=="volume") reat=float.Parse(SC_data.volume);
        if(namte=="camera_zoom") reat=float.Parse(SC_data.camera_zoom);
		if(namte=="music") reat=float.Parse(SC_data.music);
        slider.value=(reat-b)/a;
        title.text=visibleNamte+": "+(slider.value*10)+"%";
    }
    void Update()
    {
		if(namte=="camera_zoom" && !SC_data.menu) valueRead();
        if(namte=="music" && SC_data.menu) music.volume=(slider.value*a+b)*music_natural_volume;
    }
    void Start()
    {
        valueRead();
    }
}
