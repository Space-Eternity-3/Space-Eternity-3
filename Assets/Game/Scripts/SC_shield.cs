using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_shield : MonoBehaviour
{
    public bool mtp_active = false;
    public int green_time = 0;
    public Transform VisualShield;

    public bool going_active;
    public int going_state = 0; // 0:disabled 6:enabled

    public SC_invisibler SC_invisibler;

    Vector3 NormalShieldSize = new Vector3(1f,1f,1f);

    void Start()
    {
        NormalShieldSize = VisualShield.localScale;
    }
    void FixedUpdate()
    {
        if(green_time > 0)
            green_time--;

        if(going_active) {
            if(going_state < 6)
                going_state=6;
        }
        else {
            if(going_state > 0)
                going_state--;
        }

        VisualShield.gameObject.SetActive(going_state!=0 && !SC_invisibler.invisible);
        if(going_state!=0)
        {
            float size = going_state/6f;
            VisualShield.localScale = size * NormalShieldSize;
        }
    }
    public void SetShieldIfSmaller(int new_value)
    {
        if(new_value > green_time)
            green_time = new_value;
    }
    void Update()
    {
        going_active = (green_time > 0 || mtp_active);
    }
}
