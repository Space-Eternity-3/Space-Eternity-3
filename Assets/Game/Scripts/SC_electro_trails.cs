using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CElectroLine
{
    public Transform object1;
    public Transform object2;
    public Transform electro;
    public SC_clothes SC_clothes;
    public SC_terminate SC_terminate;

    public CElectroLine(Transform obj1, Transform obj2, Transform ele)
    {
        object1 = obj1;
        object2 = obj2;
        electro = ele;
        SC_terminate = ele.GetComponent<SC_terminate>();
    }
    public void UpdateVisibility(bool do_visible)
    {
        Vector3 vo = object1.position;
        Vector3 vc = object2.position + 7.5f*0.95f*Vector3.Normalize(vo-object2.position); vc -= new Vector3(0f,0f,vc.z);
        electro.position = (vo+vc)/2;
        electro.position -= new Vector3(0f,0f,electro.position.z);
        electro.eulerAngles = new Vector3(0f,0f,Mathf.Atan2((vc.y-vo.y),(vc.x-vo.x))*180f/Mathf.PI);
        electro.localScale = new Vector3(Mathf.Sqrt((vc.y-vo.y)*(vc.y-vo.y)+(vc.x-vo.x)*(vc.x-vo.x)),1f,1f);
        if(!do_visible) electro.position = new Vector3(0f,0f,10000f);
    }
    public void TerminateUpdate()
    {
        SC_terminate.timer = 500;
    }
}

public class SC_electro_trails : MonoBehaviour
{
    public Transform boss;
    public Transform structure;
    public Transform electro_source;
    public SC_clothes SC_clothes;
    public SC_control SC_control;

    List<CElectroLine> electro_lines = new List<CElectroLine>();
    
    void Start()
    {
        if(SC_clothes.SC_boss.mother) return;

        Transform this_player;
        for(int i=0;i<SC_control.max_players;i++)
        {
            if(i==0) {
                this_player = SC_control.player;
            }
            else {
                this_player = SC_control.PL[i].transform;
            }
            electro_lines.Add(new CElectroLine(this_player,boss,Instantiate(electro_source,new Vector3(0f,0f,0f),Quaternion.identity)));
        }
    }
    void Update()
    {
        if(SC_clothes.SC_boss.mother) return;

        SC_invisibler SC_invisibler;
        Transform this_player;
        for(int i=0;i<SC_control.max_players;i++)
        {
            if(i==0) {
                SC_invisibler = SC_control.SC_invisibler;
                this_player = SC_control.player;
            }
            else {
                SC_invisibler = SC_control.PL[i].SC_invisibler;
                this_player = SC_control.PL[i].transform;
            }
            electro_lines[i].UpdateVisibility(
                !SC_invisibler.invisible &&
                SC_control.Pitagoras(this_player.position-(structure.position-new Vector3(0f,0f,structure.position.z))) <= 37f && //checks if present as well
                SC_clothes.SC_boss.type*5 + SC_clothes.SC_boss.dataID[18] == 6*5 + 3
            );
        }
    }
    void FixedUpdate()
    {
        if(SC_clothes.SC_boss.mother) return;

        for(int i=0;i<SC_control.max_players;i++)
            electro_lines[i].TerminateUpdate();
    }
}
