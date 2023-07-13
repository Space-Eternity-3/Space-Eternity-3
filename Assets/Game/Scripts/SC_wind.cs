using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_wind : MonoBehaviour
{
    public float force;
    public Rigidbody player;

    void OnTriggerStay(Collider collision)
    {
        //Like FixedUpdate
        if(collision.gameObject.name=="pseudoBody")
        {
            float dX,dY,distance;
            dX=transform.position.x-player.position.x;
            dY=transform.position.y-player.position.y;
            distance=Mathf.Sqrt(dX*dX+dY*dY);
            distance-=2f; if(distance<1f) distance=1f;

            float rot=180f-transform.rotation.eulerAngles.z;
            player.velocity-=0.02f*force*(2f/distance)*new Vector3(Mathf.Sin(rot*(3.14159f/180f)),Mathf.Cos(rot*(3.14159f/180f)),0f);
        }
    }
}
