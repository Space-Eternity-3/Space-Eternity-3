using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_diode : MonoBehaviour
{
    public Transform LuneHolder;
    public Material DiodeOff, DiodeOn;
    public int min, max;
    public int counter;
    public bool is_active = false;

    void Start()
    {
        counter = 0;
        FixedUpdate();
        is_active = false;
        int rand = UnityEngine.Random.Range(0,2);
    }
    void FixedUpdate()
    {
        if(counter==0)
        {
            is_active = !is_active;
            counter = UnityEngine.Random.Range(min,max+1);
        }
        counter--;
    }
    void Update()
    {
        Renderer rend = gameObject.GetComponent<Renderer>();
        if(is_active)
        {
            LuneHolder.localPosition = new Vector3(0f,0f,0f);
            rend.material = DiodeOn;
        }
        else
        {
            LuneHolder.localPosition = new Vector3(0f,0f,10000f);
            rend.material = DiodeOff;
        }
    }
}
