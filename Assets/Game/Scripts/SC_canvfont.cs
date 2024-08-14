using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_canvfont : MonoBehaviour
{
    public Canvas canvas;
    public Text[] texts;
    public float animation_speed = 0.1f;
    public float animation_state = 1f;

    void Update()
    {
        if(canvas.targetDisplay==0)
            animation_state += animation_speed * Time.deltaTime;
        else
            animation_state -= animation_speed * Time.deltaTime;

        animation_state = Mathf.Clamp01(animation_state);

        foreach(Text text in texts)
            SetTextTransparency(text,animation_state);
    }
    public void SetTextTransparency(Text textComponent, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        Color color = textComponent.color;
        color.a = alpha;
        textComponent.color = color;
    }
}
