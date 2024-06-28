using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_tb_manager : MonoBehaviour
{
    public SC_control SC_control;
    public SC_data SC_data;
    public Transform Communtron4;
    public int TreasureFrame = 0;
    int sgp_counter = 0;

    void FixedUpdate()
    {
        if((int)Communtron4.position.y!=100)
        {
            sgp_counter++;
            if(sgp_counter==15)
            {
                sgp_counter = 0;
                TreasureFrame++;
                
                if(TreasureFrame % 2 == 0)
                {
                    //Treasure generating
                    foreach(SC_asteroid ast in SC_control.SC_lists.SC_asteroid)
                    foreach(Transform trn in ast.transform)
                    {
                        SC_fobs fob = trn.GetComponent<SC_fobs>();
                        if(fob!=null)
                        if(fob.ObjID==81)
                        {
                            WorldData.Load(fob.X,fob.Y);
                            int diamond_count = WorldData.GetCountOf(82,1);
                            float diode_probability = Mathf.Pow(Parsing.FloatU(SC_data.Gameplay[131]),diamond_count) * Parsing.FloatU(SC_data.Gameplay[130]);

                            if(UnityEngine.Random.Range(0,10000) < diode_probability * 10000f)
                            {
                                int loc_dm = WorldData.GetNbt(fob.index+1,0);
                                if(loc_dm > 0 && loc_dm < 5)
                                    fob.GetComponent<SC_tbase>().DiodeNextLevel();
                            }
                        }
                    }

                    //Treasure generation starting
                    foreach(SC_asteroid ast in SC_control.SC_lists.SC_asteroid)
                    foreach(Transform trn in ast.transform)
                    {
                        SC_fobs fob = trn.GetComponent<SC_fobs>();
                        if(fob!=null)
                        if(fob.ObjID==81)
                        {
                            WorldData.Load(fob.X,fob.Y);
                            int bases_count = WorldData.GetCountOf(81,-1);
                            int empty_bases_count = WorldData.GetCountOf(81,0);
                            int done_bases_count = WorldData.GetCountOf(81,5);

                            if(empty_bases_count + done_bases_count == bases_count && empty_bases_count > 0)
                            {
                                int next_place;
                                do { next_place = UnityEngine.Random.Range(1,21); }
                                while(WorldData.GetFob(next_place) != 81 || WorldData.GetNbt(next_place,0) != 0);

                                foreach(Transform trn2 in ast.transform)
                                {
                                    SC_fobs fob2 = trn2.GetComponent<SC_fobs>();
                                    if(fob2!=null)
                                    if(fob2.index + 1 == next_place)
                                        fob2.GetComponent<SC_tbase>().DiodeNextLevel();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void UpdateNbtsFromServer(string str)
    {
        string[] list = str.Split('|');
        foreach(string s in list)
        {
            if(s=="") continue;

            string[] ar = s.Split(';');
            foreach(SC_asteroid ast in SC_control.SC_lists.SC_asteroid)
            if(ast.ID+"" == ar[0])
            foreach(Transform trn in ast.transform)
            {
                SC_fobs fob = trn.GetComponent<SC_fobs>();
                if(fob!=null)
                if(fob.ObjID==81)
                {
                    if(fob.index+"" == ar[1])
                        fob.GetComponent<SC_tbase>().nbt1 = Parsing.IntU(ar[2]);
                }
            }
        }
    }
}
