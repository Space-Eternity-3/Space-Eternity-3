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
    public string actives;
    public int cooldown;
    public int one_time_id;
    public bool salvic;

    public int projection_id;

    public CShooter(int bul_typ,double angl_deg,double deviat_deg,double precis_deg,double rad,SC_boss ths,bool alway,int freq,string activess,int cld,int otid,bool slvc)
    {
        bullet_type = bul_typ;
        angle = ((float)angl_deg)*3.14159f/180f;
        max_deviation = ((float)deviat_deg)*3.14159f/180f;
        precision = ((float)precis_deg)*3.14159f/180f;
        radius = ((float)rad);
        thys = ths;
        stupid = alway;
        frequency = freq;
        actives = activess;
        cooldown = cld;
        one_time_id = otid; //only prime numbers
        salvic = slvc;

        projection_id = 0;
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
        if(shooter.salvic && thys.dataID[3]%250>=175) return;
        int fram = thys.dataID[19] - thys.dataID[17];
        int true_frequency = shooter.frequency;

        //Shooter frequency change
        if( (thys.type==1 && thys.dataID[18]==4) ||
            (thys.type==2 && thys.dataID[18]==4) ||
            (thys.type==6 && thys.dataID[18]==4)
        ) true_frequency = (int)(shooter.frequency*0.67f);

        if(true_frequency<1) true_frequency=1;
        if(shooter.actives[thys.dataID[18]]=='1' && fram>shooter.cooldown && (fram-shooter.cooldown)%true_frequency==0)
        if(shooter.one_time_id<0 || thys.dataID[20]%shooter.one_time_id!=0) this.ShotCalculate(shooter,players,thys);
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
        if(shooter.one_time_id>=0) thys.dataID[20] *= shooter.one_time_id;
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
        int i;
        List<CShooter> shooters = new List<CShooter>();
        if(type==1) //Protector
        {
            shooters = new List<CShooter>() {
                new CShooter(11, 22.5,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(11, 45,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(11, 135,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(11, 157.5,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(11, 202.5,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(11, 225,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(11, 315,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(11, 337.5,60,7.5, 7,thys,false, 15, "11011",0,-1, false),

                new CShooter(4, 270,120,7.5, 10,thys,false, 1, "01000",50,2, false),
                new CShooter(9, 0,0,0, 7.5,thys,true, 40, "00010",1,-1, false),
                new CShooter(9, 180,0,0, 7.5,thys,true, 40, "00010",1,-1, false),
            };

            for(i=0;i<=7;i++) shooters[i].projection_id = 1;
            shooters[8].projection_id = 4;
            for(i=9;i<=10;i++) shooters[i].projection_id = 9;
        }
        if(type==2) //Adecodron
        {
            shooters = new List<CShooter>() {
                new CShooter(10, 0,0,0, 7.5,thys,true, 40, "00010",1,-1, false),
                new CShooter(10, 90,0,0, 7.5,thys,true, 40, "00010",1,-1, false),
                new CShooter(10, 180,0,0, 7.5,thys,true, 40, "00010",1,-1, false),
                new CShooter(10, 270,0,0, 7.5,thys,true, 40, "00010",1,-1, false),

                new CShooter(12, 45,70,7, 8,thys,false, 15, "00100",30,-1, false),
                new CShooter(12, 135,70,7, 8,thys,false, 15, "00100",30,-1, false),
                new CShooter(12, 225,70,7, 8,thys,false, 15, "00100",30,-1, false),
                new CShooter(12, 315,70,7, 8,thys,false, 15, "00100",30,-1, false),
                
                new CShooter(6, 0,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 20,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 40,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 60,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 80,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 100,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 120,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 140,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 160,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 180,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 200,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 220,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 240,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 260,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 280,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 300,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 320,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(6, 340,0,0, 6.5,thys,true, 40, "01000",1,-1, false),
            };

            for(i=0;i<=3;i++) shooters[i].projection_id = 10;
            for(i=4;i<=7;i++) shooters[i].projection_id = 12;
        }
        if(type==3) //Octogone
        {
            shooters = new List<CShooter>() {
                new CShooter(7, 0,60,10, 6.5,thys,false, 51, "10111",0,-1, false),
                new CShooter(7, 45,60,10, 6.5,thys,false, 48, "10111",0,-1, false),
                new CShooter(7, 90,60,10, 6.5,thys,false, 53, "10111",0,-1, false),
                new CShooter(7, 135,60,10, 6.5,thys,false, 47, "10111",0,-1, false),
                new CShooter(7, 180,60,10, 6.5,thys,false, 50, "10111",0,-1, false),
                new CShooter(7, 225,60,10, 6.5,thys,false, 54, "10111",0,-1, false),
                new CShooter(7, 270,60,10, 6.5,thys,false, 52, "10111",0,-1, false),
                new CShooter(7, 315,60,10, 6.5,thys,false, 49, "10111",0,-1, false),

                new CShooter(7, 0,60,60, 6.5,thys,true, 31, "01000",0,-1, false),
                new CShooter(7, 45,60,60, 6.5,thys,true, 28, "01000",0,-1, false),
                new CShooter(7, 90,60,60, 6.5,thys,true, 33, "01000",0,-1, false),
                new CShooter(7, 135,60,60, 6.5,thys,true, 27, "01000",0,-1, false),
                new CShooter(7, 180,60,60, 6.5,thys,true, 30, "01000",0,-1, false),
                new CShooter(7, 225,60,60, 6.5,thys,true, 34, "01000",0,-1, false),
                new CShooter(7, 270,60,60, 6.5,thys,true, 32, "01000",0,-1, false),
                new CShooter(7, 315,60,60, 6.5,thys,true, 29, "01000",0,-1, false),

                new CShooter(8, 90,60,10, 8,thys,false, 1, "00100",30,2, false),
                new CShooter(8, 210,60,10, 8,thys,false, 1, "00100",30,3, false),
                new CShooter(8, 330,60,10, 8,thys,false, 1, "00100",30,5, false),
            };

            for(i=16;i<=18;i++) shooters[i].projection_id = 8;
        }
        if(type==4) //Starandus
        {
            shooters = new List<CShooter>() {
                new CShooter(5, 0,60,60, 6.5,thys,true, 35, "11000",0,-1, false),
                new CShooter(5, 45,60,60, 6.5,thys,true, 31, "11000",0,-1, false),
                new CShooter(5, 90,60,5, 6.5,thys,false, 36, "11000",0,-1, false),
                new CShooter(5, 135,60,60, 6.5,thys,true, 38, "11000",0,-1, false),
                new CShooter(5, 180,60,60, 6.5,thys,true, 34, "11000",0,-1, false),
                new CShooter(5, 225,60,5, 6.5,thys,false, 33, "11000",0,-1, false),
                new CShooter(5, 270,60,60, 6.5,thys,true, 37, "11000",0,-1, false),
                new CShooter(5, 315,60,5, 6.5,thys,false, 32, "11000",0,-1, false),

                new CShooter(5, 0,60,60, 6.5,thys,true, 14, "00010",0,-1, false),
                new CShooter(5, 45,60,60, 6.5,thys,true, 16, "00010",0,-1, false),
                new CShooter(5, 90,60,10, 6.5,thys,false, 14, "00010",0,-1, false),
                new CShooter(5, 135,60,60, 6.5,thys,true, 16, "00010",0,-1, false),
                new CShooter(5, 180,60,60, 6.5,thys,true, 15, "00010",0,-1, false),
                new CShooter(5, 225,60,10, 6.5,thys,false, 14, "00010",0,-1, false),
                new CShooter(5, 270,60,60, 6.5,thys,true, 15, "00010",0,-1, false),
                new CShooter(5, 315,60,10, 6.5,thys,false, 16, "00010",0,-1, false),
            };
        }
        if(type==6) //Degenerator
        {
            shooters = new List<CShooter>() {
                new CShooter(12, 45,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(12, 135,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(12, 225,60,7.5, 7,thys,false, 15, "11011",0,-1, false),
                new CShooter(12, 315,60,7.5, 7,thys,false, 15, "11011",0,-1, false),

                new CShooter(13, 22.5,60,10, 7,thys,false, 40, "11011",0,-1, false),
                new CShooter(13, 157.5,60,10, 7,thys,false, 40, "11011",0,-1, false),
                new CShooter(13, 202.5,60,10, 7,thys,false, 40, "11011",0,-1, false),
                new CShooter(13, 337.5,60,10, 7,thys,false, 40, "11011",0,-1, false),

                new CShooter(13, 270,70,70, 7,thys,true, 20, "11011",0,-1, false),
                new CShooter(9, 0,0,0, 7.5,thys,true, 40, "01000",1,-1, false),
                new CShooter(9, 180,0,0, 7.5,thys,true, 40, "01000",1,-1, false),

                new CShooter(13, 22.5,20,20, 7,thys,true, 20, "00100",0,-1, false),
                new CShooter(13, 157.5,20,20, 7,thys,true, 20, "00100",0,-1, false),
                new CShooter(13, 202.5,20,20, 7,thys,true, 20, "00100",0,-1, false),
                new CShooter(13, 337.5,20,20, 7,thys,true, 20, "00100",0,-1, false),
                new CShooter(13, 270,70,70, 7,thys,true, 10, "00100",0,-1, false),
            };

            for(i=0;i<=3;i++) shooters[i].projection_id = 2;
            for(i=4;i<=7;i++) shooters[i].projection_id = 3;
            shooters[8].projection_id = 13;
            for(i=9;i<=10;i++) shooters[i].projection_id = 9;
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
        //C# must have
        System.Random random = new System.Random();

        //Constants
        int[] border_times = new int[]{300,500, 250,400, 50, 150}; // S-0/1(state), o-2/3(empty), X-4(boom-state), A-5(wait-for)

        //Pre-defines
        thys.dataID[14] = thys.FloatToScrd((float)random.NextDouble() * 2f*3.14159f);
        thys.dataID[17] = UnityEngine.Random.Range(border_times[2],border_times[3]+1);
        thys.dataID[19] = thys.dataID[17];
        thys.dataID[20] = 1;
    }
    public void _FixedUpdate()
    {
        //C# must have
        System.Random random = new System.Random();

        //Constants
        float bounce_radius = 26f;
        float acceleration = 0.015f;
        float unstable_pulse_force = 0.4f;
        int[] border_times = new int[]{300,500, 250,400, 50, 150, 200+thys.dataID[1]*50, 300}; // S-0/1(state), o-2/3(empty), X-4(boom-state), A-5(wait-for), P-6(shield), R-7(remote)
        char[] state_types = new char[]{
            'o', 'o', 'o', 'o', 'o', //Placeholder
            'o', 'A', 'P', 'X', 'S', //Protector
            'o', 'X', 'S', 'X', 'S', //Adecodron
            'o', 'S', 'X', 'S', 'S', //Octogone
            'o', 'S', 'S', 'S', 'S', //Starandus
            'o', 'o', 'o', 'o', 'o', //Useless
            'o', 'X', 'S', 'R', 'S', //Degenerator
        };
        float[] state_velocities = new float[]{
            0.20f, 0.20f, 0.20f, 0.20f, 0.20f, //Placeholder
            0.20f, 0.10f, 0.10f, 0.10f, 0.40f, //Protector
            0.40f, 0.20f, 0.40f, 0.20f, 0.70f, //Adecodron
            0.30f, 0.15f, 0.10f, 0.15f, 0.50f, //Octogone
            0.00f, 0.00f, 0.00f, 0.00f, 0.00f, //Starandus
            0.20f, 0.20f, 0.20f, 0.20f, 0.20f, //Useless
            0.20f, 0.10f, 0.10f, 0.10f, 0.40f, //Degenerator
        };

        //Pre-defines
        SPlayerInfo[] players = thys.world.GetPlayers();
        float target_velocity = state_velocities[thys.type*5+thys.dataID[18]];
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
        if(rand_unst < unstable_pulse_chance && (thys.dataID[18]==4 && thys.type==6))
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

        //Remote damage
        int reduced_frame = thys.dataID[19] - thys.dataID[17];
        if(thys.type==6 && thys.dataID[18]==3 && reduced_frame%20==0 && reduced_frame!=300)
            if(players.Length>0) thys.SC_control.DamageFLOAT(1f * float.Parse(thys.SC_data.Gameplay[32]));

        //Battle state update
        if(thys.dataID[17] > 0) thys.dataID[17]--;
        else
        {
            thys.dataID[21] = thys.dataID[18];
            if(thys.dataID[18]!=0)
            {
                thys.dataID[18] = 0;
                thys.dataID[17] = UnityEngine.Random.Range(border_times[2],border_times[3]+1);
            }
            else
            {
                thys.dataID[18] = UnityEngine.Random.Range(1,2+thys.dataID[1]+1);
                char time_letter = state_types[5*thys.type+thys.dataID[18]];
                if(time_letter=='S') thys.dataID[17] = UnityEngine.Random.Range(border_times[0],border_times[1]+1); //State
                else if(time_letter=='X') thys.dataID[17] = border_times[4]; //Instant shot
                else if(time_letter=='A') thys.dataID[17] = border_times[5]; //Waiting for shot
                else if(time_letter=='P') thys.dataID[17] = border_times[6]; //Shield not constant
                else if(time_letter=='R') thys.dataID[17] = border_times[7]; //Remote
                else thys.dataID[17] = border_times[4];
            }
            thys.dataID[19] = thys.dataID[17];
            thys.dataID[20] = 1;
        }
    }
    public void _End()
    {
        thys.world.CleanBullets(thys.identifier);
    }
}
