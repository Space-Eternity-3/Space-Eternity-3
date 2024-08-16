using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_clock_arrow : MonoBehaviour
{
    public float distance;
    public Image clock;

    public void LaterUpdate()
    {
        float alpha = (clock.fillAmount-0.25f) * 2f * Mathf.PI;
        transform.eulerAngles = new Vector3(0f,0f,-360f*(clock.fillAmount-0.25f)+90f);
        transform.localPosition = distance * new Vector3(
            Mathf.Cos(-alpha),
            Mathf.Sin(-alpha),
            0f
        );
    }
}
