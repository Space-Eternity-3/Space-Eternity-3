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
    private Transform player;
    private SPlayerInfo[] PlayerInfo;
    private CDeltaPos deltaposmem;

    public CInfo(SC_lists lts, Transform pla)
    {
        SC_lists = lts;
        player = pla;
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
}

public class SC_behaviour : MonoBehaviour
{
    public SC_boss thys;

    //thys.type // Boss type (read only)
    //thys.deltapos //Delta position of boss center (read only)
    //thys.dataID // General & Additional data
    //thys.world //Info about world

    public void _Start()
    {
        
    }
    public void _FixedUpdate()
    {
        float angle = (thys.dataID[3]/50f)%(2f*3.14159f);
        thys.dataID[8] = thys.FloatToScrd(22f*Mathf.Cos(angle));
        thys.dataID[9] = thys.FloatToScrd(22f*Mathf.Sin(angle));
        thys.dataID[10] = thys.FloatToScrd(angle*180f/3.14159f);

        //SPlayerInfo[] players = thys.world.GetPlayers();
        //if(players.Length>0) Debug.Log(players.Length+": "+players[0].id+";"+players[0].x+";"+players[0].y);
        //else Debug.Log(players.Length);
    }
    public void _End()
    {
        
    }
}
