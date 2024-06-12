using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_factory_move : MonoBehaviour
{
    public Transform Factory;

    bool ContainsFob81()
    {
        foreach(Transform trn in transform)
        {
            if(trn.gameObject.name=="Fob81(Clone)")
                return true;
        }
        return false;
    }
    public void CreateFactoryIfPossible()
    {
        if(ContainsFob81())
        {
            transform.GetComponent<Renderer>().enabled = false;
            Transform factory = Instantiate(Factory,transform.position,transform.rotation);
            factory.localScale = transform.localScale / 2f;
            factory.localScale = new Vector3(factory.localScale.x,factory.localScale.y,factory.localScale.z*0.8f);
            factory.GetComponent<Renderer>().material = transform.GetComponent<Renderer>().material;
            factory.SetParent(transform,true);

            if(!transform.GetComponent<SphereCollider>().enabled)
                factory.position += new Vector3(0f,0f,1130f);
        }
    }
}
