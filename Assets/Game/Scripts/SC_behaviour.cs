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
    public bool stupid;
    public int frequency;

    public CShooter(int bul_typ,double angl_deg,double deviat_deg,double precis_deg,double rad,SC_boss ths,bool alway,int freq)
    {
        bullet_type = bul_typ;
        angle = ((float)angl_deg)*3.14159f/180f;
        max_deviation = ((float)deviat_deg)*3.14159f/180f;
        precision = ((float)precis_deg)*3.14159f/180f;
        radius = ((float)rad);
        thys = ths;
        stupid = alway;
        frequency = freq;
    }
    public bool CanShoot(float x,float y)
    {
        x -= thys.ScrdToFloat(thys.dataID[8]);
        y -= thys.ScrdToFloat(thys.dataID[9]);
        float[] pat = new float[2];
        pat = thys.RotatePoint(new float[2]{x,y},-(angle+thys.ScrdToFloat(thys.dataID[10])*3.14159f/180f),false);
        x = pat[0]-radius; y = pat[1];

        float target_agl = Mathf.Abs(Mathf.Atan2(y,x));
        return (target_agl <= max_deviation);
    }
    public float BestDeviation(float x,float y)
    {
        x -= thys.ScrdToFloat(thys.dataID[8]);
        y -= thys.ScrdToFloat(thys.dataID[9]);
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
    public void ShotCalculateIfNow(CShooter shooter,SPlayerInfo[] players,SC_boss thys)
    {
        //If multiple shooter
        if(thys.dataID[3]%shooter.frequency==0 && thys.dataID[3]!=0)
            ShotCalculate(shooter,players,thys);
    }
    public void ShotCalculate(CShooter shooter,SPlayerInfo[] players,SC_boss thys)
    {
        //Stupid shooter
        if(shooter.stupid) {
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
        if(type==1) //Protector
        {
            shooters = new List<CShooter>() {
                new CShooter(11, 22.5,60,7.5, 6.5,thys,false, 15),
                new CShooter(11, 45,60,7.5, 6.5,thys,false, 15),
                new CShooter(11, 135,60,7.5, 6.5,thys,false, 15),
                new CShooter(11, 157.5,60,7.5, 6.5,thys,false, 15),
                new CShooter(11, 202.5,60,7.5, 6.5,thys,false, 15),
                new CShooter(11, 225,60,7.5, 6.5,thys,false, 15),
                new CShooter(11, 315,60,7.5, 6.5,thys,false, 15),
                new CShooter(11, 337.5,60,7.5, 6.5,thys,false, 15),

                new CShooter(4, 270,120,7.5, 10,thys,false, 300),
                new CShooter(9, 0,0,0, 7.5,thys,true, 600),
                new CShooter(9, 180,0,0, 7.5,thys,true, 600),
            };
        }
        if(type==2) //Adecodron
        {
            shooters = new List<CShooter>() {
                new CShooter(10, 0,0,0, 7.5,thys,true, 600),
                new CShooter(10, 90,0,0, 7.5,thys,true, 600),
                new CShooter(10, 180,0,0, 7.5,thys,true, 600),
                new CShooter(10, 270,0,0, 7.5,thys,true, 600),

                new CShooter(12, 45,60,7.5, 6.5,thys,false, 20),
                new CShooter(12, 135,60,7.5, 6.5,thys,false, 20),
                new CShooter(12, 225,60,7.5, 6.5,thys,false, 20),
                new CShooter(12, 315,60,7.5, 6.5,thys,false, 20),
                
                new CShooter(6, 0,0,0, 6.5,thys,true, 600),
                new CShooter(6, 20,0,0, 6.5,thys,true, 600),
                new CShooter(6, 40,0,0, 6.5,thys,true, 600),
                new CShooter(6, 60,0,0, 6.5,thys,true, 600),
                new CShooter(6, 80,0,0, 6.5,thys,true, 600),
                new CShooter(6, 100,0,0, 6.5,thys,true, 600),
                new CShooter(6, 120,0,0, 6.5,thys,true, 600),
                new CShooter(6, 140,0,0, 6.5,thys,true, 600),
                new CShooter(6, 160,0,0, 6.5,thys,true, 600),
                new CShooter(6, 180,0,0, 6.5,thys,true, 600),
                new CShooter(6, 200,0,0, 6.5,thys,true, 600),
                new CShooter(6, 220,0,0, 6.5,thys,true, 600),
                new CShooter(6, 240,0,0, 6.5,thys,true, 600),
                new CShooter(6, 260,0,0, 6.5,thys,true, 600),
                new CShooter(6, 280,0,0, 6.5,thys,true, 600),
                new CShooter(6, 300,0,0, 6.5,thys,true, 600),
                new CShooter(6, 320,0,0, 6.5,thys,true, 600),
                new CShooter(6, 340,0,0, 6.5,thys,true, 600),
            };
        }
        if(type==3) //Octogone
        {
            shooters = new List<CShooter>() {
                new CShooter(7, 0,60,10, 6.5,thys,false, 50),
                new CShooter(7, 45,60,10, 6.5,thys,false, 50),
                new CShooter(7, 90,60,10, 6.5,thys,false, 50),
                new CShooter(7, 135,60,10, 6.5,thys,false, 50),
                new CShooter(7, 180,60,10, 6.5,thys,false, 50),
                new CShooter(7, 225,60,10, 6.5,thys,false, 50),
                new CShooter(7, 270,60,10, 6.5,thys,false, 50),
                new CShooter(7, 315,60,10, 6.5,thys,false, 50),

                new CShooter(8, 90,60,10, 8.5,thys,false, 500),
                new CShooter(8, 210,60,10, 8.5,thys,false, 500),
                new CShooter(8, 330,60,10, 8.5,thys,false, 500),
            };
        }
        if(type==4) //Starandus
        {
            shooters = new List<CShooter>() {
                new CShooter(5, 0,60,60, 6.5,thys,true, 25),
                new CShooter(5, 45,60,60, 6.5,thys,true, 21),
                new CShooter(5, 90,60,60, 6.5,thys,true, 26),
                new CShooter(5, 135,60,60, 6.5,thys,true, 28),
                new CShooter(5, 180,60,60, 6.5,thys,true, 24),
                new CShooter(5, 225,60,60, 6.5,thys,true, 23),
                new CShooter(5, 270,60,60, 6.5,thys,true, 27),
                new CShooter(5, 315,60,60, 6.5,thys,true, 22),
            };
        }
        if(type==6) //Degenerator
        {
            shooters = new List<CShooter>() {
                new CShooter(13, 22.5,60,10, 6.5,thys,false, 35),
                new CShooter(12, 45,60,7.5, 6.5,thys,false, 20),
                new CShooter(12, 135,60,7.5, 6.5,thys,false, 20),
                new CShooter(13, 157.5,60,10, 6.5,thys,false, 35),
                new CShooter(13, 202.5,60,10, 6.5,thys,false, 35),
                new CShooter(12, 225,60,7.5, 6.5,thys,false, 20),
                new CShooter(12, 315,60,7.5, 6.5,thys,false, 20),
                new CShooter(13, 337.5,60,10, 6.5,thys,false, 35),
            };
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
        System.Random random = new System.Random();
        thys.dataID[14] = thys.FloatToScrd((float)random.NextDouble() * 2f*3.14159f);
    }
    public void _FixedUpdate()
    {
        //Pre-defines
        System.Random random = new System.Random();
        SPlayerInfo[] players = thys.world.GetPlayers();
        float bounce_radius = 26f;
        float acceleration = 0.015f;
        float target_velocity = 0.2f; if(thys.type==2) target_velocity = 0.4f; if(thys.type==4) target_velocity = 0f;
        float unstable_pulse_force = 0.6f;
        float unstable_pulse_chance = 0.015f; if(thys.type!=6) unstable_pulse_chance = 0f;
        
        //Rotation
        float rand_rot = (float)random.NextDouble();
        if(thys.dataID[11]==thys.dataID[12] && rand_rot>0.8f) thys.dataID[12] = UnityEngine.Random.Range(-30,31);
        thys.dataID[11] += (int)Mathf.Sign(thys.dataID[12]-thys.dataID[11]);
        thys.dataID[10] = thys.FloatToScrd((thys.ScrdToFloat(thys.dataID[10]) + 0.15f*thys.dataID[11]));

        //Movement rotation
        if(thys.dataID[15]==thys.dataID[16]) thys.dataID[16] = UnityEngine.Random.Range(-30,31);
        thys.dataID[15] += (int)Mathf.Sign(thys.dataID[16]-thys.dataID[15]);
        thys.dataID[14] = thys.FloatToScrd((thys.ScrdToFloat(thys.dataID[14]) + 0.3f*thys.dataID[15]*3.14159f/180f));

        //Velocity adjuster
        float current_velocity = thys.ScrdToFloat(thys.dataID[13]);
        float velocity_angle = thys.ScrdToFloat(thys.dataID[14]);
        if(target_velocity > current_velocity) {
            current_velocity += acceleration;
            if(target_velocity < current_velocity) current_velocity = target_velocity;
        } 
        if(target_velocity < current_velocity) {
            current_velocity -= acceleration;
            if(target_velocity > current_velocity) current_velocity = target_velocity;
        }

        //Unstable pulse
        float rand_unst = (float)random.NextDouble();
        if(rand_unst < unstable_pulse_chance)
        {
            float vel_x = Mathf.Cos(velocity_angle) * current_velocity;
            float vel_y = Mathf.Sin(velocity_angle) * current_velocity;
            float angle_unst = (float)random.NextDouble() * 2f*3.14159f;
            vel_x += unstable_pulse_force * Mathf.Cos(angle_unst);
            vel_y += unstable_pulse_force * Mathf.Sin(angle_unst);
            current_velocity = Mathf.Sqrt(vel_x*vel_x + vel_y*vel_y);
            velocity_angle = Mathf.Atan2(vel_y,vel_x);
        }
        thys.dataID[13] = thys.FloatToScrd(current_velocity);
        thys.dataID[14] = thys.FloatToScrd(velocity_angle);

        //Movement & Bounce
        float[] xy = thys.RotatePoint(new float[2]{current_velocity,0},velocity_angle,false);
        float x1 = thys.ScrdToFloat(thys.dataID[8]); float y1 = thys.ScrdToFloat(thys.dataID[9]);
        float x2 = x1 + xy[0]; var y2 = y1 + xy[1];
        float[] ef = thys.GetBounceCoords(x1,y1,x2,y2,bounce_radius);
        if(ef[0]*ef[0]+ef[1]*ef[1]>=bounce_radius*bounce_radius)
        {//Position correction
            float sqrt = Mathf.Sqrt(ef[0]*ef[0]+ef[1]*ef[1]);
            ef[0] *= (bounce_radius-0.01f)/sqrt;
            ef[1] *= (bounce_radius-0.01f)/sqrt;
        }
        thys.dataID[8] = thys.FloatToScrd(ef[0]);
        thys.dataID[9] = thys.FloatToScrd(ef[1]);
        if(ef[3]==1f) thys.dataID[14] = thys.FloatToScrd(ef[2]);

        //Shooting
        foreach(CShooter shooter in thys.shooters) {
          thys.world.ShotCalculateIfNow(shooter,players,thys);
        }
    }
    public void _End()
    {
        thys.world.CleanBullets(thys.identifier);
    }
}
