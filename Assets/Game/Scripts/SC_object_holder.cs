using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_object_holder : MonoBehaviour
{
    //Termination variables
    public bool mother = true;
    public int terminate_in = 300;
    public bool aborted_worldgen = false;

    //Must be prepared before unlocking
    public CObjectInfo[] Objects;
    public string holder_name;

    //Seon objects
    public Transform Asteroid;
    public Transform Sphere;
    public Transform Wall;
    public Transform Piston;
    public Transform Respblock;
    public Transform Animator;
    public Transform Star;
    public Transform Monster;
    public Transform Boss;

    //Other variables
    public Dictionary<int, Transform> Transforms = new Dictionary<int, Transform>();
    public string actual_state = "default";
    public int scaling_blocker = 0;
    public int summoned = 0, max_summoned = 0, j_current = 0;
    public SC_boss SC_boss;

    //Script references
    public SC_worldgen SC_worldgen;
    public SC_fun SC_fun;

    public void Unlock()
    {
        mother = false;

        //Generate animator objects
        for(int j=0;j<Objects.Length;j++)
        if(Objects[j]!=null)
        {
            int i;
            Transform trn = null;
            if(Objects[j].obj=="animator")
            {
                trn = Instantiate(Animator, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
                trn.SetParent(transform,true);
            }
            if(Objects[j].obj=="boss")
            {
                trn = Instantiate(Boss, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
                trn.SetParent(transform,true);
                
                SC_boss bos = trn.GetComponent<SC_boss>();
                int[] xy = SC_fun.UlamToXY(Objects[j].ulam);
				bos.bX = xy[0];
                bos.bY = xy[1];
                bos.bID = Objects[j].ulam;
                string[] f_nam = holder_name.Split('_');
                bos.sID = SC_fun.MakeUlam(Parsing.IntE(f_nam[1]),Parsing.IntE(f_nam[2]));
                bos.type = Objects[j].type;
                bos.SC_object_holder = this;
                SC_boss = bos;
				bos.StartFromStructure();
            }
            Transforms.Add(j,trn);

            CObjectInfo obj = Objects[j];
            if(obj.animation_type==1 || obj.animation_type==2)
            {
                SC_seon_remote ser = trn.gameObject.AddComponent<SC_seon_remote>();
                ser.SC_fun = SC_fun;
                ser.SC_object_holder = this;
                
                string[] tags1 = obj.animation_when_doing.Split(';');
                string[] tags2 = obj.animation_when_done.Split(';');
                string[] tags3 = obj.animation_when_undoing.Split(';');
                int lngt1 = tags1.Length-1;
                int lngt2 = tags2.Length-1;
                int lngt3 = tags3.Length-1;
                if(obj.animation_type==1)
                {
                    for(i=0;i<lngt1;i++) ser.HideStateSet(tags1[i],1);
                    for(i=0;i<lngt2;i++) ser.HideStateSet(tags2[i],2);
                    for(i=0;i<lngt3;i++) ser.HideStateSet(tags3[i],3);
                }
                else
                {
                    for(i=0;i<lngt1;i++) ser.ExtendedStateSet(tags1[i],1);
                    for(i=0;i<lngt2;i++) ser.ExtendedStateSet(tags2[i],2);
                    for(i=0;i<lngt3;i++) ser.ExtendedStateSet(tags3[i],3);
                    ser.extension = obj.animation_size;
                }
                ser.animation_type = obj.animation_type;
                ser.PreStart();
            }
        }

        //Disable lazy generation
        Vector3 reduced_player_pos = SC_fun.SC_control.transform.position;
        reduced_player_pos -= new Vector3(0f,0f,reduced_player_pos.z);
        if((transform.position - reduced_player_pos).magnitude < 100f)
            max_summoned = Objects.Length;

        //Generate normal objects
        int already_summoned = GenerateAnimated();
        if(already_summoned < SC_fun.structure_frame_generations)
            GenerateStep(SC_fun.structure_frame_generations-already_summoned);
    }

    int GenerateAnimated()
    {
        int local_summoned = 0;
        for(int j=0;j<Objects.Length;j++)
        if(Objects[j]!=null)
        if(Objects[j].animator!=-1)
        {
            Transform trn = GenerateObject(j);
            if(trn!=null) local_summoned++;
        }
        return local_summoned;
    }
    void GenerateStep(int step_size)
    {
        max_summoned += step_size;
        for(; (j_current < Objects.Length && summoned < max_summoned) ; j_current++)
        if(Objects[j_current]!=null)
        if(Objects[j_current].animator==-1)
        {
            Transform trn = GenerateObject(j_current);
            if(trn!=null) summoned++;
        }
    }
    Transform GenerateObject(int j)
    {
        int i;
        Transform trn = null;
        if(Objects[j].obj=="asteroid")
        {
            trn = Instantiate(Asteroid, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
            trn.SetParent(transform,true);
            SC_asteroid ast = trn.GetComponent<SC_asteroid>();
            int[] xy = SC_fun.UlamToXY(Objects[j].ulam);
            ast.X = xy[0];
            ast.Y = xy[1];
            ast.ID = Objects[j].ulam;
            ast.protsize = Objects[j].size;
            ast.prottype = Objects[j].type;
            ast.protbiome = Objects[j].biome;
            ast.fobCode = Objects[j].fobcode;
            for(i=0;i<20;i++)
            {
                ast.fobInfoPos[i] = Objects[j].fob_positions[i];
                ast.fobInfoRot[i] = Objects[j].fob_rotations[i];
            }
            ast.transform.localScale = new Vector3(1f,1f,0.75f) * Objects[j].size;
            if(Objects[j].hidden)
            {
                ast.GetComponent<SphereCollider>().enabled = false;
                ast.GetComponent<Renderer>().enabled = false;
            }
            ast.SC_object_holder = this;
            scaling_blocker++;
        }
        if(Objects[j].obj=="sphere")
        {
            trn = Instantiate(Sphere, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
            trn.SetParent(transform,true);
            trn.GetComponent<SC_drill>().type = Objects[j].type;
            trn.GetComponent<SC_material>().SetMaterial(Objects[j].type,true);
            trn.localScale = new Vector3(1f,1f,0.75f) * Objects[j].size1;
        }
        if(Objects[j].obj=="wall")
        {
            trn = Instantiate(Wall, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
            trn.SetParent(transform,true);
            trn.GetComponent<SC_drill>().type = Objects[j].type;
            trn.GetComponent<SC_material>().SetMaterial(Objects[j].type,false);
            trn.localScale = new Vector3(Objects[j].size1,Objects[j].size2,Objects[j].size1);
        }
        if(Objects[j].obj=="piston")
        {
            trn = Instantiate(Piston, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
            trn.SetParent(transform,true);
            trn.GetComponent<SC_drill>().type = Objects[j].type;
            trn.GetComponent<SC_adv_colors>().ApplyMaterials(Objects[j].type);
            trn.localScale = new Vector3(Objects[j].size1,Objects[j].size2,1f);
        }
        if(Objects[j].obj=="star")
        {
            trn = Instantiate(Star, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
            trn.SetParent(transform,true);
            trn.localScale = new Vector3(1f,1f,1f) * Objects[j].size1;
        }
        if(Objects[j].obj=="monster")
        {
            trn = Instantiate(Monster, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
            trn.SetParent(transform,true);
            trn.localScale = new Vector3(1f,1f,1f) * Objects[j].size1;
        }
        if(Objects[j].obj=="respblock")
        {
            trn = Instantiate(Respblock, Objects[j].position, Quaternion.Euler(0f,0f,Objects[j].rotation));
            trn.SetParent(transform,true);
            trn.GetComponent<SC_resp_blocker>().radius = Objects[j].range;
        }

        if(!Transforms.ContainsKey(j)) Transforms.Add(j,trn);

        if(trn!=null && Objects[j].animator!=-1)
            trn.SetParent(Transforms[Objects[j].animator],true);

        return trn;
    }
    void FixedUpdate()
    {
        if(!mother) terminate_in--;
        if(terminate_in<=0)
        {
            if(SC_boss!=null) SC_boss.BeforeDestroy();
            SC_worldgen.Holders.Remove(holder_name);
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if(!mother && AsyncData.thread_count==0)
            GenerateStep(SC_fun.structure_frame_generations);
    }
    public void AbortWorldgen()
    {
        terminate_in = 50;
        aborted_worldgen = true;
    }
}
