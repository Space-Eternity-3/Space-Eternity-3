using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_projection : MonoBehaviour
{
    string preBase = "0";
    public string actualBase = "0";
    int lag_counter = 0;
    public int max_allowed_lag;

    void FixedUpdate()
    {
        if(actualBase=="0" || actualBase=="1" || actualBase=="2")
        {
            lag_counter = 0;
            transform.position = new Vector3(0f,0f,10000f);
            preBase = actualBase;
        }
        else
        {
            string[] elements = actualBase.Split(';');
            Vector3 pos = new Vector3(float.Parse(elements[0]),float.Parse(elements[1]),0f);
            float rot = float.Parse(elements[4]);

            string[] elements2; Vector3 pos2;
            if(preBase=="0" || preBase=="1" || preBase=="2")
            {
                elements2 = elements;
                pos2 = pos;
            }
            else
            {
                elements2 = preBase.Split(';');
                pos2 = new Vector3(float.Parse(elements2[0]),float.Parse(elements2[1]),0f);
            }

            transform.position = (pos+pos2)/2;
            //rotation set here
        }
    }
}
