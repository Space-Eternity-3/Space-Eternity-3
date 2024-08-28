using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_player_follower : MonoBehaviour
{
    public Transform player;
    public Transform follower;
    public Transform camera;
    public SC_seeking SC_seeking;

    public Rigidbody playerR;
    public SC_bullet SC_bullet;
    public SC_projection SC_projection;
    public SC_players SC_players;
    public SC_boss SC_boss;

    public Renderer[] SourceRend, TargetRend;
    public Transform[] SourceTran, TargetTran;

    public bool teleporting = true;
    public bool teleporting_if_far_away = false;
    public bool suc_teleporting = false;
    public bool suc_teleporting_activator = false;
    public bool teleport_until = false;
    
    public int velocity_source = 0;
    public float lerping_ratio = 0.9f;
    public int rotor_int = 1;
    public bool remote_updating = false;
    public bool father = false;
    public bool delete_father = false;

    public SC_fun SC_fun;

    void Update()
    {
        if(!remote_updating)
            RemoteUpdate();
    }
    public void RemoteUpdate()
    {
        if(delete_father)
        {
            delete_father = false;
            father = false;
        }
        if(father) return;

        //Movement speculating
        if(velocity_source==0) { // MAIN PLAYER
            follower.position += playerR.velocity * Time.deltaTime;
        }
        if(velocity_source==1) { // BULLET
            if(player==null) { Destroy(gameObject); return; }
            follower.position += SC_bullet.st_vect * SC_bullet.follow_multiplier * 50f * Time.deltaTime;
        }
        if(velocity_source==2) { // WHITE PROJECTION
            follower.position += SC_projection.SpeculateVelocity() * 50f * Time.deltaTime;
        }
        if(velocity_source==3) { // OTHER PLAYERS
            follower.position += SC_players.SpeculateVelocity() * 50f * Time.deltaTime;
        }
        if(velocity_source==4) { // BOSS
            follower.position += SC_boss.GetVelocity() * 50f * Time.deltaTime;
            follower.rotation = Quaternion.Euler(0f,0f,follower.eulerAngles.z + SC_boss.dataID[11] * 0.15f * 50f * Time.deltaTime);
        }

        //Movement fluent correction
        follower.position = Vector3.Lerp(player.position,follower.position,LerpingMultiplier(lerping_ratio));
        float r = LerpingMultiplier(rotor_int/(rotor_int+1f));
        follower.rotation = Quaternion.Euler(0f,0f,SC_fun.rotAvg((int)Mathf.Ceil(r/(1-r)),follower.eulerAngles.z,player.eulerAngles.z));

        //Teleport frame detect and execute
        if(
            (teleporting) ||
            (teleport_until) ||
            (teleporting_if_far_away && (follower.position - player.position).magnitude > 3f) ||
            (velocity_source==4 && (SC_boss.dataID[2]!=2 || (SC_boss.type*5+SC_boss.dataID[18]==3*5+3 && SC_boss.dataID[17]>10 && SC_boss.dataID[19]-SC_boss.dataID[17]>=30))) ||
            (suc_teleporting && suc_teleporting_activator)
        ) {
            //Position
            if(velocity_source!=4) follower.position = player.position;
            else follower.position = SC_boss.EscapingDynamicPosition;

            //Rotation
            if(!(velocity_source==4 && SC_boss.dataID[2]!=2)) follower.rotation = player.rotation;

            //Other
            teleporting=false;
            if(suc_teleporting && suc_teleporting_activator)
            {
                teleport_until = true;
                suc_teleporting = false;
                suc_teleporting_activator = false;
            }
        }

        //Z-correction
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
        for(int i=0;i<TargetRend.Length;i++) {
            TargetRend[i].material = SourceRend[i].material;
        }
        for(int i=0;i<TargetTran.Length;i++) {
            TargetTran[i].localPosition = SourceTran[i].localPosition;
        }

        //Seeking optional
        if(SC_seeking!=null) SC_seeking.LateUpdate();
    }
    float LerpingMultiplier(float f)
    {
        float frames = 1f / Time.deltaTime;
        if(frames >= 60f) frames = 60f;
        return Mathf.Pow(f,60f/frames);
    }
}
