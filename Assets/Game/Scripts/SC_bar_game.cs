using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_bar_game : MonoBehaviour
{
    public Slider healthUp, healthRe;
    int licznikC = 0;

    public void AfterFixedUpdate()
    {
        if(healthUp.value < healthRe.value)
		{
			if(licznikC < 0)
			healthRe.value-=0.0625f;
			licznikC--;
		}
		else
		{
			healthRe.value = healthUp.value;
			licznikC = 10;
		}
    }
}
