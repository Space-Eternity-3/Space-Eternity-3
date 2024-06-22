using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_dbase : MonoBehaviour
{
    public GameObject DiamondGroup;
    public GameObject EmptyGroup;
    public Renderer OneDiode;

    public Transform DiamondCreationParticles;
    public Material[] DiodeMaterials;

    public SC_particle_transition SC_particle_transition;
    public SC_control SC_control;

    int nbt1_before = -1;
    public int nbt1 = 0; // 1 -> diamond present

    void LateUpdate()
    {
        if(nbt1!=0 && nbt1!=1) nbt1 = 0;

        if(nbt1==1 && nbt1_before==0) {
            Instantiate(DiamondCreationParticles,DiamondGroup.transform.position,transform.rotation);
        }

        DiamondGroup.SetActive(nbt1==1);
        EmptyGroup.SetActive(nbt1==0);

        if(nbt1==1 && SC_particle_transition.active)
            OneDiode.material = DiodeMaterials[1];
        else
            OneDiode.material = DiodeMaterials[0];

        nbt1_before = nbt1;
    }
    public void DiamondStateLoad()
    {
        SC_fobs SC_fobs = transform.GetComponent<SC_fobs>();
        if(SC_fobs.multiplayer)
        {
            nbt1 = SC_fobs.new_nbt_get_1;
        }
        else
        {
            WorldData.Load(SC_fobs.X,SC_fobs.Y);
            nbt1 = WorldData.GetNbt(SC_fobs.index+1,0);
        }
    }
    public void PlaceDiamond(int slot)
    {
        nbt1 = 1;

        SC_fobs SC_fobs = transform.GetComponent<SC_fobs>();
        if(SC_fobs.multiplayer)
        {
            SC_control.SendMTP("/DiamondPlaceTry "+SC_control.connectionID+" "+SC_fobs.ID+" "+SC_fobs.index+" "+slot);
        }
        else
        {
            WorldData.Load(SC_fobs.X,SC_fobs.Y);
            WorldData.UpdateNbt(SC_fobs.index+1,0,1);
        }
    }
}
