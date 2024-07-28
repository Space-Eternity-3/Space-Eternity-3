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
    public SC_players SC_players;
    public SC_boss SC_boss;

    public Renderer[] SourceRend, TargetRend;
    public Transform[] SourceTran, TargetTran;

    public bool teleporting = true;
    public bool teleporting_if_far_away = false;
    public bool teleporting_unsynced = false;
    public bool teleporting_unsynced_catalizator = false;
    public int velocity_source = 0;
    public float lerping_ratio = 0.9f;

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
        if(velocity_source==3) // Other players
        {
            follower.position += SC_players.SpeculateVelocity() * 50f * Time.deltaTime;
        }
        if(velocity_source==4) //Boss
        {
            follower.position += SC_boss.GetVelocity() * 50f * Time.deltaTime;
        }

        follower.rotation = Quaternion.Euler(0f,0f,SC_fun.CalculateAverageAngle(follower.eulerAngles.z,player.eulerAngles.z));
        follower.position = Vector3.Lerp(player.position,follower.position,lerping_ratio);
        if(
            teleporting ||
            (teleporting_if_far_away && (follower.position - player.position).magnitude > 3f) ||
            (teleporting_unsynced && teleporting_unsynced_catalizator) ||
            (velocity_source==4 && (SC_boss.dataID[2]!=2 || SC_boss.type*5+SC_boss.dataID[18]==3*5+3))
        ) {
            follower.position = player.position;
            if(!(velocity_source==4 && SC_boss.dataID[2]!=2)) follower.rotation = player.rotation;
            teleporting=false;
            teleporting_unsynced = false;
            if(velocity_source==3)
            {
                SC_players.positionBefore = new Vector3(0f,0f,300f);
                SC_players.positionBeforeB = new Vector3(0f,0f,300f);
                SC_players.positionBeforeC = new Vector3(0f,0f,300f);
            }
        }
        teleporting_unsynced_catalizator = false;
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
