using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_player_follower : MonoBehaviour
{
    public Transform player;
    public Rigidbody playerR;
    public Transform follower;
    public Transform camera;

    public Renderer[] SourceRend, TargetRend;
    public Transform[] SourceTran, TargetTran;

    public bool teleporting = true;

    public SC_players SC_players; //only for pseudostatic functions

    void Update()
    {
        //Position smoothing
        follower.position += playerR.velocity * Time.deltaTime;
        follower.rotation = Quaternion.Euler(0f,0f,SC_players.rotAvg(2,follower.eulerAngles.z,player.eulerAngles.z));
        follower.position = Vector3.Lerp(player.position,follower.position,0.9f);
        if(teleporting) {
            follower.position = player.position;
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
    void FixedUpdate()
    {
        //It somehow works better than in Update, don't touch it!
        //follower.rotation = player.rotation;
    }
}
