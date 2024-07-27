using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_player_follower : MonoBehaviour
{
    public Transform player;
    public Transform follower;
    public Transform camera;

    public Rigidbody playerR;
    public SC_bullet SC_bullet;
    public SC_projection SC_projection;

    public Renderer[] SourceRend, TargetRend;
    public Transform[] SourceTran, TargetTran;

    public bool teleporting = true;
    public bool teleporting_if_far_away = false;
    public int velocity_source = 0;

    public SC_fun SC_fun;

    void Update()
    {
        //Position smoothing
        if(velocity_source==0) // Main player
        {
            follower.position += playerR.velocity * Time.deltaTime;
        }
        if(velocity_source==1) // Bullet
        {
            if(player==null)
            {
                Destroy(gameObject);
                return;
            }
            follower.position += SC_bullet.st_vect * SC_bullet.follow_multiplier * 50f * Time.deltaTime;
        }
        if(velocity_source==2) // White projection
        {
            follower.position += SC_projection.SpeculateVelocity() * 50f * Time.deltaTime;
        }

        follower.rotation = Quaternion.Euler(0f,0f,SC_fun.rotAvg(1,follower.eulerAngles.z,player.eulerAngles.z));
        follower.position = Vector3.Lerp(player.position,follower.position,0.9f);
        if(teleporting || (teleporting_if_far_away && (follower.position - player.position).magnitude > 3f)) {
            follower.position = player.position;
            follower.rotation = player.rotation;
            teleporting=false;
        }
        follower.position = new Vector3(
            follower.position.x,
            follower.position.y,
            player.position.z
        );

        //Camera follow
        if(camera!=null) camera.position = new Vector3(
            follower.position.x,
            follower.position.y,
            camera.position.z
        );

        //Customize projection look
        for(int i=0;i<TargetRend.Length;i++)
        {
            TargetRend[i].material = SourceRend[i].material;
        }
        for(int i=0;i<TargetTran.Length;i++)
        {
            TargetTran[i].localPosition = SourceTran[i].localPosition;
        }
    }
}
