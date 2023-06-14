using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CElectroLine
{
    public Transform object1;
    public Transform object2;
    public Transform electro;
    public SC_clothes SC_clothes;

    public CElectroLine(Transform obj1, Transform obj2, Transform ele, SC_clothes clt)
    {
        object1 = obj1;
        object2 = obj2;
        electro = ele;
        SC_clothes = clt;
        UpdateVisibility();
    }
    public void UpdateVisibility()
    {
        Vector3 vo = object1.position;
        Vector3 vc = object2.position + 7.5f*0.95f*Vector3.Normalize(vo-object2.position);
        electro.position = (vo+vc)/2;
        if(SC_clothes.get_actual_clothe==6*5+0) electro.position -= new Vector3(0f,0f,electro.position.z);
        else electro.position += new Vector3(0f,0f,1000f);
        electro.eulerAngles = new Vector3(0f,0f,Mathf.Atan2((vc.y-vo.y),(vc.x-vo.x))*180f/Mathf.PI);
        electro.localScale = new Vector3(Mathf.Sqrt((vc.y-vo.y)*(vc.y-vo.y)+(vc.x-vo.x)*(vc.x-vo.x)),1f,1f);
    }
}

public class SC_electro_trails : MonoBehaviour
{
    public Transform player;
    public Transform boss;
    public Transform electro_source;
    public SC_clothes SC_clothes;

    CElectroLine electro_line;
    
    void Start()
    {
        electro_line = new CElectroLine(player,boss,Instantiate(electro_source,new Vector3(0f,0f,0f),Quaternion.identity),SC_clothes);
    }
    void Update()
    {
        electro_line.UpdateVisibility();
    }
}
