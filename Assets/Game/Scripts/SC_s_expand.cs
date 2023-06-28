using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_s_expand : MonoBehaviour
{
    public float distances;
    public int min_x, max_x, min_y, max_y;
    public Transform root_central_object;

    void Awake()
    {
        int i,j;
        for(i=min_x;i<=max_x;i++)
            for(j=min_y;j<=max_y;j++)
                if(i!=0 || j!=0) {
                    Transform trn = Instantiate(root_central_object,
                    root_central_object.position + new Vector3(i*distances,j*distances,0f),root_central_object.rotation);
                    trn.parent = root_central_object.parent;
                }
    }
}
