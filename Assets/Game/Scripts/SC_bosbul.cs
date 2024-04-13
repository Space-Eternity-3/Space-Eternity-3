using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// ENEMY BULLET COLLIDER SYSTEM (bosbul)

public class CExactCollider
{
    public string type;
    public float x, y;              // universal
    public float r;                 // circles
    public float rx, ry, angle;     // rectangles

    //Initializers
    public void Circle(float _x, float _y, float _r)
    {
        type = "circle";
        x = _x;
        y = _y;
        r = _r;
    }
    public void Rectangle(float _x, float _y, float _rx, float _ry, float _angle)
    {
        type = "rectangle";
        x = _x;
        y = _y;
        rx = _rx;
        ry = _ry;
        angle = _angle;
    }

    //Checkers
    public static bool CheckCollision(CExactCollider C1, CExactCollider C2)
    {
        if(C1==null || C2==null) return false;
        if(C1.type=="circle" && C2.type=="circle") return CollideCC(C1,C2);
        if(C1.type=="rectangle" && C2.type=="rectangle") return false; // such collisions are not needed nor implemented
        if(C1.type=="circle" && C2.type=="rectangle") return CollideCR(C1,C2);
        if(C1.type=="rectangle" && C2.type=="circle") return CollideCR(C2,C1);
        return false;
    }
    private static bool CollideCC(CExactCollider C1, CExactCollider C2)
    {
        //Primitive circle collision
        float dx = C2.x - C1.x;
        float dy = C2.y - C1.y;
        float rs = C1.r + C2.r;
        return (rs*rs > dx*dx + dy*dy);
    }
    private static bool CollideCR(CExactCollider C1, CExactCollider C2)
    {
        //Performance boost
        float mcd = C1.r + C2.rx + C2.ry;
        float dx = C2.x - C1.x;
        float dy = C2.y - C1.y;
        if(mcd*mcd < dx*dx + dy*dy) return false;

        //Exclude null rectangles
        if(C2.rx==0f && C2.ry==0f) return false;

        //Initialize temporary objects
        CExactCollider B1 = new CExactCollider();
        CExactCollider B2 = new CExactCollider();

        //Move to center
        B1.Circle(C1.x-C2.x,C1.y-C2.y,C1.r);
        B2.Rectangle(0f,0f,C2.rx,C2.ry,C2.angle);
        
        //Rotate objects
        float[] sph_rot = SC_fun.RotateVector(B1.x,B1.y,-B2.angle);
        B1.Circle(sph_rot[0],sph_rot[1],B1.r);
        B2.Rectangle(0f,0f,B2.rx,B2.ry,0f);

        //Detect sector
        int sec_x = 0, sec_y = 0;
        if(B1.x > B2.rx) sec_x = 1; if(B1.x < -B2.rx) sec_x = -1;
        if(B1.y > B2.ry) sec_y = 1; if(B1.y < -B2.ry) sec_y = -1;

        //Check collision in trivial sectors
        if(sec_x == 0) return (Mathf.Abs(B1.y) < B1.r + B2.ry);
        if(sec_y == 0) return (Mathf.Abs(B1.x) < B1.r + B2.rx);

        //Check collision in corner sectors
        CExactCollider W1 = new CExactCollider(); W1.Circle(sec_x*B2.rx,sec_y*B2.ry,0f);
        return CollideCC(B2,W1);
    }

    //Clone
    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public class CBosbulCollider
{
    private string[] WhenAnimated = new string[0];
    private CExactCollider ColliderDefault = null;
    private CExactCollider ColliderAnimated = null;
    private float x, y, ax, ay;
    
    public CBosbulCollider(CObjectInfo obj, int fob)
    {
        if(fob==-1) {
            x = obj.position.x;
            y = obj.position.y;
        }
        else {
            x = obj.fob_positions[fob].x;
            y = obj.fob_positions[fob].y;
        }
        ax = x;
        ay = y;

        //Create base colliders
        ColliderDefault = new CExactCollider();
        if(obj.obj=="asteroid")
        {
            if(fob==-1) ColliderDefault.Circle(x,y,obj.size/2f);
            else ColliderDefault.Rectangle(x,y,0f,0f,obj.fob_rotations[fob]);
        }
        else if(obj.obj=="sphere") ColliderDefault.Circle(x,y,obj.size1/2f);
        else if(obj.obj=="star") ColliderDefault.Circle(x,y,9.6f*obj.size1);
        else if(obj.obj=="monster") ColliderDefault.Circle(x,y,9.6f*obj.size1);
        else if(obj.obj=="wall") ColliderDefault.Rectangle(x,y,1.5f*obj.size1,5f*obj.size2,obj.rotation);
        else if(obj.obj=="piston") ColliderDefault.Rectangle(x,y,1.5f*obj.size1,3f*obj.size2,obj.rotation);
        else ColliderDefault = null;

        //Update to state colliders
        if(obj.animator_reference==null)
        {
            WhenAnimated = new string[0];
            ColliderAnimated = null;
        }
        else if(obj.animator_reference.animation_type!=0)
        {
            WhenAnimated = obj.animator_reference.animation_when_done.Split(";");
            if(obj.animator_reference.animation_type==2)
            {
                ColliderAnimated = (CExactCollider)ColliderDefault.Clone();
                ColliderAnimated.x += obj.animator_reference.animation_size.x;
                ColliderAnimated.y += obj.animator_reference.animation_size.y;
                ax = ColliderAnimated.x;
                ay = ColliderAnimated.y;
            }
            else ColliderAnimated = null;
        }
    }

    public bool CheckCollision(CExactCollider C1, string reduced_state)
    {
        if(Array.IndexOf(WhenAnimated,reduced_state)==-1) return CExactCollider.CheckCollision(ColliderDefault,C1);
        else return CExactCollider.CheckCollision(ColliderAnimated,C1);
    }

    public void UpdateFobCollider(int num)
    {
        float RX = 0f;
        float RY = 0f;
        float OF = 0f;

        //More general
        if((new List<int>{8,9,10,11,16,30,50,56,58,60,62,66}).Contains(num))
            { RX = 0.9f; RY = 1f; OF = 0f; } // stones & elements
        if((new List<int>{17,18,19,22,26,31,49,67,32}).Contains(num))
            { RX = 1.4f; RY = 1.6f; OF = 0.5f; } // packeds & big diamond
        if((new List<int>{41,42,43,44,45,46,47}).Contains(num))
            { RX = 1.15f; RY = 1.2f; OF = 0f; } // artefacts
        if((new List<int>{13,25,27,40}).Contains(num))
            { RX = 1.4f; RY = 2.6f; OF = 0.5f; } // smaller aliens
        if((new List<int>{37,68,73,74,75}).Contains(num))
            { RX = 1.4f; RY = 2.2f; OF = 0.5f; } // treasures

        //More individual
        if((new List<int>{23,53}).Contains(num))    { RX = 1.4f; RY = 3f; OF = 0.5f; } // bigger aliens
        if((new List<int>{1}).Contains(num))        { RX = 1.15f; RY = 1.5f; OF = 0f; } // stone with crystals
        if((new List<int>{34,36}).Contains(num))    { RX = 0.4f; RY = 2.3f; OF = 0.5f; } // drills
        if((new List<int>{35}).Contains(num))       { RX = 0.4f; RY = 3f; OF = 0.5f; } // magnetic lamp
        if((new List<int>{54}).Contains(num))       { RX = 1f; RY = 2.2f; OF = 0.5f; } // bone
        if((new List<int>{51}).Contains(num))       { RX = 1.4f; RY = 2f; OF = 0.5f; } // metal piece
        if((new List<int>{29,69}).Contains(num))    { RX = 1.4f; RY = 3.6f; OF = 1f; } // tombs
        if((new List<int>{28}).Contains(num))       { RX = 1.4f; RY = 1.2f; OF = 0f; } // red spikes
        if((new List<int>{33}).Contains(num))       { RX = 0.9f; RY = 1.1f; OF = 0f; } // small diamond
        if((new List<int>{38,70}).Contains(num))    { RX = 1.5f; RY = 1.6f; OF = 0f; } // normal and lava geyzer
        if((new List<int>{3,4}).Contains(num))      { RX = 1.4f; RY = 2.2f; OF = 0f; } // pumpkin & mega geyzer
        if((new List<int>{5}).Contains(num))        { RX = 0.6f; RY = 1f; OF = 0f; } // small amethyst
        if((new List<int>{6}).Contains(num))        { RX = 1f; RY = 1.6f; OF = 0f; } // medium amethyst
        if((new List<int>{7}).Contains(num))        { RX = 1.4f; RY = 2.2f; OF = 0f; } // big amethyst
        if((new List<int>{2}).Contains(num))        { RX = 1.4f; RY = 3.2f; OF = 1f; } // driller
        if((new List<int>{21,52}).Contains(num))    { RX = 1.4f; RY = 2.4f; OF = 1f; } // storages
        if((new List<int>{15}).Contains(num))       { RX = 1.8f; RY = 8f; OF = 3.7f; } // copper chimney

        float[] dXY = SC_fun.RotateVector(0f,OF,ColliderDefault.angle);

        ColliderDefault.x = x + dXY[0];
        ColliderDefault.y = y + dXY[1];
        ColliderDefault.rx = RX/2f;
        ColliderDefault.ry = RY/2f;

        if(ColliderAnimated!=null)
        {
            ColliderAnimated.x = ax + dXY[0];
            ColliderAnimated.y = ay + dXY[1];
            ColliderAnimated.rx = RX/2f;
            ColliderAnimated.ry = RY/2f;
        }
    }
}

public static class Bosbul
{
    public static SC_fun SC_fun;

    public static Dictionary<int, Dictionary<string, CBosbulCollider>> BosbulSectors = new Dictionary<int, Dictionary<string, CBosbulCollider>>();
    public static int max_dict_size = 64;

    public static Dictionary<string, CBosbulCollider> GetBosbuls(int X, int Y) // X and Y must be even numbers and specify structure sector
    {
        int i,key = SC_fun.MakeUlam(X,Y);
        if(BosbulSectors.ContainsKey(key)) return BosbulSectors[key];
        else
        {
            List<CObjectInfo> Surroundings = new List<CObjectInfo>();

            Surroundings.AddRange( Universe.GetSector("S_"+X+"_"+Y) );
            Surroundings.AddRange( Universe.GetSector("B_"+X+"_"+Y) );
            Surroundings.AddRange( Universe.GetSector("B_"+(X-1)+"_"+Y) );
            Surroundings.AddRange( Universe.GetSector("B_"+X+"_"+(Y-1)) );
            Surroundings.AddRange( Universe.GetSector("B_"+(X-1)+"_"+(Y-1)) );

            Dictionary<string, CBosbulCollider> Build = new Dictionary<string, CBosbulCollider>();
            foreach(CObjectInfo obj in Surroundings)
                if(obj!=null)
                {
                    if(obj.obj=="asteroid")
                    {
                        if(!obj.hidden) Build.Add(("ast_"+obj.ulam),new CBosbulCollider(obj,-1));
                        for(i=0;i<obj.size*2;i++)
                            Build.Add(("fob_"+obj.ulam+"_"+i),new CBosbulCollider(obj,i));
                        UpdateFobCollidersInDictionary(Build,obj.ulam);
                    }
                    else if((new List<string>{"sphere","star","monster","wall","piston"}).Contains(obj.obj))
                    {
                        int random_key = UnityEngine.Random.Range(0,1000000000);
                        Build.Add((random_key+""),new CBosbulCollider(obj,-1));
                    }
                }

            if(BosbulSectors.Count >= max_dict_size) BosbulSectors.Clear();
            BosbulSectors.Add(key,Build);
            return BosbulSectors[key];
        }
    }

    public static bool CollidesWithBosbul(CExactCollider C1, string reduced_state)
    {
        int X = (int)Mathf.Round(C1.x/200f) * 2;
        int Y = (int)Mathf.Round(C1.y/200f) * 2;
        ICollection<CBosbulCollider> LocalBosbuls = GetBosbuls(X,Y).Values;
        foreach(CBosbulCollider bbc in LocalBosbuls)
        {
            if(bbc.CheckCollision(C1,reduced_state))
            {
                return true;
            }
        }
        return false;
    }

    public static void UpdateFobColliders(int ulam)
    {
        string[] nums = Universe.GetSectorNameByUlam(ulam).Split('_');
        int X = Parsing.IntE(nums[1]); if(X%2!=0) X++;
        int Y = Parsing.IntE(nums[2]); if(Y%2!=0) Y++;
        Dictionary<string, CBosbulCollider> LocalDictionary = GetBosbuls(X,Y);
        UpdateFobCollidersInDictionary(LocalDictionary,ulam);
    }

    public static void UpdateFobCollidersInDictionary(Dictionary<string, CBosbulCollider> LocalDictionary, int ulam)
    {
        int[] xy = SC_fun.UlamToXY(ulam);
        WorldData.Load(xy[0],xy[1]);
        for(int i=0;i<20;i++)
        {
            string key = "fob_"+ulam+"_"+i;
            if(LocalDictionary.ContainsKey(key))
                LocalDictionary[key].UpdateFobCollider(WorldData.GetFob(i+1));
        }
    }
}

public class SC_bosbul : MonoBehaviour
{
    //script doesn't exist, just holder for bosbul classes
}
