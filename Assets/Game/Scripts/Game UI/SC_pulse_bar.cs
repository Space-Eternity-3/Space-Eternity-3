using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_pulse_bar : MonoBehaviour
{
    public Canvas canvas;
    public Image image;
    public Slider bar;
    public Color32 normal;
    public Color32 pulsed;
    public int max_lerp=25;
    public int actual_lerp=0;

    public GameObject detached_nick;

    float memory=1;
    bool mother_minus = true;

    public SC_control SC_control;

    void FixedUpdate()
    {
        if(bar.value < memory) actual_lerp = max_lerp;
        memory = bar.value;
        
        image.color = Color.Lerp(normal,pulsed,actual_lerp*1f/max_lerp);

        if(actual_lerp > 0) actual_lerp--;
    }
    void Start() {
        mother_minus = canvas.GetComponent<Transform>().position.z < -1400f;
        if(!mother_minus)
            SC_control.SC_lists.AddTo_SC_pulse_bar(this);
    }
    void OnDestroy() {
        if(!mother_minus)
            SC_control.SC_lists.RemoveFrom_SC_pulse_bar(this);
    }
}
