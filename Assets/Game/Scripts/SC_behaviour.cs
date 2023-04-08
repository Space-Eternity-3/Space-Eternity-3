using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct SPlayerInfo
{
    public int id;
    public float x;
    public float y;
    public bool enabled;
}

public class CShooter
{
    public int bullet_type;
    public float angle;
    public float max_deviation;
    public float precision;
    public float radius;
    public SC_boss thys;
    public bool always;

    public CShooter(int bul_typ,float angl_deg,float deviat_deg,float precis_deg,float rad,SC_boss ths,bool alway)
    {
        bullet_type = bul_typ;
        angle = angl_deg*3.14159f/180f;
        max_deviation = deviat_deg*3.14159f/180f;
        precision = precis_deg*3.14159f/180f;
        radius = rad;
        thys = ths;
        always = alway;
    }
    public bool CanShoot(float x,float y)
    {
        float[] pat = new float[2];
        pat = thys.RotatePoint(new float[2]{x,y},-(angle+thys.ScrdToFloat(thys.dataID[10])*3.14159f/180f),false);
        x = pat[0]-radius; y = pat[1];

        float target_agl = Mathf.Abs(Mathf.Atan2(y,x));
        return (target_agl <= max_deviation);
    }
    public float BestDeviation(float x,float y)
    {
        float[] pat = new float[2];
        pat = thys.RotatePoint(new float[2]{x,y},-(angle+thys.ScrdToFloat(thys.dataID[10])*3.14159f/180f),false);
        x = pat[0]-radius; y = pat[1];
        return Mathf.Atan2(y,x);
    }
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
        PlayerInfo[0].enabled = (player.position.z<100f && (!player.GetComponent<SC_invisibler>().invisible || SC_control.SC_effect.effect!=0) && in_arena_range);
    }
    public SPlayerInfo[] GetPlayers()
    {
        if(PlayerInfo[0].enabled) return PlayerInfo;
        else return new SPlayerInfo[0];
    }
    public void ShotCalculate(CShooter shooter,SPlayerInfo[] players,SC_boss thys)
    {
        //Stupid shooter
        if(shooter.always) {
            ShotUsingShooter(shooter,0,thys);
            return;
        }

        //Intelligent shooter
        float min_deviation = 10000f;
        foreach(SPlayerInfo player in players) {
            if(shooter.CanShoot(player.x,player.y)) {
                float cand_deviation = shooter.BestDeviation(player.x,player.y);
                if(Mathf.Abs(cand_deviation) < Mathf.Abs(min_deviation)) min_deviation = cand_deviation;
            }
        }
        if(min_deviation!=10000f) ShotUsingShooter(shooter,min_deviation,thys);
    }
    public void ShotUsingShooter(CShooter shooter,float best_deviation,SC_boss thys)
    {
        System.Random random = new System.Random();
        float deviation_angle = ((float)random.NextDouble()-0.5f)*2f*shooter.precision + best_deviation;
        if(deviation_angle < -shooter.max_deviation) deviation_angle = -2*shooter.max_deviation - deviation_angle;
        if(deviation_angle > shooter.max_deviation) deviation_angle = 2*shooter.max_deviation - deviation_angle;
        ShotCooked(shooter.angle,shooter.bullet_type,thys,deviation_angle,shooter.radius);
    }
    public void ShotCooked(float delta_angle_rad,int btype,SC_boss thys,float deviation_angle,float rad)
    {
        float lx = thys.ScrdToFloat(thys.dataID[8]);
        float ly = thys.ScrdToFloat(thys.dataID[9]);
        float angle = delta_angle_rad + thys.ScrdToFloat(thys.dataID[10])*3.14159f/180f;
        float[] pak = new float[2]; pak[1]=0f;
        if(btype==9 || btype==10) pak[0]=0.25f; else pak[0]=0.35f;
        float[] efwing = thys.RotatePoint(pak,angle+deviation_angle,false);
        ShotRaw(rad*Mathf.Cos(angle)+thys.deltapos.x+lx,rad*Mathf.Sin(angle)+thys.deltapos.y+ly,efwing[0],efwing[1],btype,thys.identifier);
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

    //bossy functions
    public List<CShooter> GetShootersList(int type,SC_boss thys)
    {
        List<CShooter> shooters = new List<CShooter>();
        if(true) //All bosses (temporary)
        {
            shooters.Add(new CShooter(1,0f,70f,7.5f,6.5f,thys,false));
            shooters.Add(new CShooter(2,90f,70f,70f,6.5f,thys,true));
            shooters.Add(new CShooter(3,180f,70f,7.5f,6.5f,thys,false));
            shooters.Add(new CShooter(4,270f,120f,7.5f,10f,thys,false));
        }
        return shooters;
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
    //thys.shooters // Shooters

    public void _Start()
    {
        
    }
    public void _FixedUpdate()
    {
        SPlayerInfo[] players = thys.world.GetPlayers();

        if(thys.dataID[3]%15==0) foreach(CShooter shooter in thys.shooters) {
          thys.world.ShotCalculate(shooter,players,thys);
        }
    }
    public void _End()
    {
        thys.world.CleanBullets(thys.identifier);
    }
}
