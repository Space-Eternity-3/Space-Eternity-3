using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SPlayerInfo
{
    public int id;
    public float x;
    public float y;
    public bool enabled;
}

public class CInfo
{
    private SC_lists SC_lists;
    private SC_control SC_control;
    private SC_bullet SC_bullet;
    private Transform player;
    private SPlayerInfo[] PlayerInfo;
    private CDeltaPos deltaposmem;

    public CInfo(SC_lists lts, Transform pla)
    {
        SC_lists = lts;
        player = pla;
        SC_control = player.GetComponent<SC_control>();
        SC_bullet = SC_control.SC_bullet;
        PlayerInfo = new SPlayerInfo[1];
    }
    public void UpdatePlayers(CDeltaPos deltapos)
    {
        deltaposmem = deltapos;
        PlayerInfo[0].id = 0;
        PlayerInfo[0].x = player.position.x - deltapos.x;
        PlayerInfo[0].y = player.position.y - deltapos.y;
        float px = PlayerInfo[0].x;
        float py = PlayerInfo[0].y;
        bool in_arena_range = (px*px + py*py <= (37f*37f));
        PlayerInfo[0].enabled = (player.position.z<100f && !player.GetComponent<SC_invisibler>().invisible && in_arena_range);
    }
    public SPlayerInfo[] GetPlayers()
    {
        if(PlayerInfo[0].enabled) return PlayerInfo;
        else return new SPlayerInfo[0];
    }
    public void ShotRaw(float px, float py, float vx, float vy, int typ, int bidf)
    {
        SC_bullet.Shot(
			new Vector3(px,py,0f),
			new Vector3(0f,0f,0f),
			new Vector3(vx,vy,0f),
			typ,
			bidf
		);
    }
    public void CleanBullets(int bidf)
    {
        List<SC_bullet> buls = SC_lists.SC_bullet;
        foreach(SC_bullet bul in buls)
        {
            if(bul.gun_owner==bidf)
            {
                bul.block_graphics = true;
                bul.MakeDestroy(false);
            }
        }
    }
}

public class SC_behaviour : MonoBehaviour
{
    public SC_boss thys;

    //thys.type // Boss type (read only)
    //thys.deltapos // Delta position of boss center (read only)
    //thys.dataID // General & Additional data
    //thys.world // Info about world
    //thys.identifier // Object identifier

    public void _Start()
    {
        
    }
    public void _FixedUpdate()
    {
        float angle = (thys.dataID[3]/50f)%(2f*3.14159f);
        thys.dataID[8] = thys.FloatToScrd(22f*Mathf.Cos(angle));
        thys.dataID[9] = thys.FloatToScrd(22f*Mathf.Sin(angle));
        thys.dataID[10] = thys.FloatToScrd(angle*180f/3.14159f);
        
        float[] pak = new float[2]; pak[0]=0; pak[1]=0.35f;
        float[] efwing = thys.RotatePoint(pak,angle+3.14159f/2,false);
        if(thys.dataID[3]%40==0) thys.world.ShotRaw(0+thys.deltapos.x,0+thys.deltapos.y,efwing[0],efwing[1],7,thys.identifier);
        if(thys.dataID[3]%40==20) thys.world.ShotRaw(0+thys.deltapos.x,0+thys.deltapos.y,efwing[0],efwing[1],10,thys.identifier);
    }
    public void _End()
    {
        thys.world.CleanBullets(thys.identifier);
    }
}
