using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_cursor_follow : MonoBehaviour
{
    public float PositionMultiplier;
    public Text Info;
    public string source_text;

    void LateUpdate()
    {
        Vector2 mpos = Input.mousePosition;
        mpos = new Vector2(mpos.x-Screen.width/2,mpos.y-Screen.height/2);
        transform.localPosition = mpos * PositionMultiplier;
        Info.text = source_text;
        source_text = "";
        if(Info.text=="") transform.localPosition += new Vector3(10000f,0f,0f);
    }
}
