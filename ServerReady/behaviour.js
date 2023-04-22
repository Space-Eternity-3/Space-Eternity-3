const { func } = require("./functions");

function randomInteger(min, max) {
    return Math.round(Math.random() * (max - min)) + func.parseIntU(min);
}

class CBoss
{
    constructor(type,deltapos,dataX,dataY,world)
    {
        this.type = type; // Boss type (read only)
        this.deltapos = deltapos; //Delta position of boss center (read only)
        this.dataX = dataX; // General data
        this.dataY = dataY; // Additional data
        this.world = world; // Communication with the world & bossy functions
        this.identifier = -randomInteger(1,1000000000); // Boss object identifier
        this.shooters = this.world.GetShootersList(this.type,this);
    }

    /*--------------------------//
    //---VARIABLE DESCRIPTION---//
    //--------------------------//
    
    dataX[0] - 1024 (boss identifier)
    dataX[1] - actual_level

    dataY[2-2] - behaviour_state [0,1,2,3,4]
    dataY[3-2] - current_frame_index
    dataY[4-2] - time_left
    dataY[5-2] - max_time
    dataY[6-2] - health_left
    dataY[7-2] - max_health
    dataY[8-2] - actual_position_x [scrd]
    dataY[9-2] - actual_position_y [scrd]
    dataY[10-2] - actual_rotation [scrd, deg]
    dataY[11-2] - rotation_velocity
    dataY[12-2] - target_rotation_velocity
    dataY[13-2] - velocity [scrd]
    dataY[14-2] - velocity_angle [scrd, rad]
    dataY[15-2] - velocity_rotation_speed
    dataY[16-2] - target_velocity_rotation_speed
    
    <...>
    dataY[60-2] - {Other variables are empty at this time. Use them as a data storage between frames or ask Kamiloso to do some special tasks using them.}

    //---------------------//
    //---BASIC FUNCTIONS---//
    //---------------------*/

    Start() //Executes on battle start
    {
        this.dataY[14-2] = func.FloatToScrd(Math.random() * 2*3.14159);
    }
    FixedUpdate() //Executes 50 times per second after starting frame
    {
        //Pre-defines
        var players = this.world.GetPlayers();
        var bounce_radius = 26;
        var acceleration = 0.015;
        var target_velocity = 0.2; if(this.type==2) target_velocity = 0.4; if(this.type==4) target_velocity = 0;
        var unstable_pulse_force = 0.6;
        var unstable_pulse_chance = 0.015; if(this.type!=6) unstable_pulse_chance = 0;
        
        //Rotation
        var rand_rot = Math.random();
        if(this.dataY[11-2]==this.dataY[12-2] && rand_rot>0.8) this.dataY[12-2] = func.randomInteger(-30,30);
        this.dataY[11-2] += Math.sign(this.dataY[12-2]-this.dataY[11-2]);
        this.dataY[10-2] = func.FloatToScrd((func.ScrdToFloat(this.dataY[10-2]) + 0.15*this.dataY[11-2]));

        //Movement rotation
        if(this.dataY[15-2]==this.dataY[16-2]) this.dataY[16-2] = func.randomInteger(-30,30);
        this.dataY[15-2] += Math.sign(this.dataY[16-2]-this.dataY[15-2]);
        this.dataY[14-2] = func.FloatToScrd((func.ScrdToFloat(this.dataY[14-2]) + 0.3*this.dataY[15-2]*3.14159/180));

        //Velocity adjuster
        var current_velocity = func.ScrdToFloat(this.dataY[13-2]);
        var velocity_angle = func.ScrdToFloat(this.dataY[14-2]);
        if(target_velocity > current_velocity) {
            current_velocity += acceleration;
            if(target_velocity < current_velocity) current_velocity = target_velocity;
        } 
        if(target_velocity < current_velocity) {
            current_velocity -= acceleration;
            if(target_velocity > current_velocity) current_velocity = target_velocity;
        }

        //Unstable pulse
        var rand_unst = Math.random();
        if(rand_unst < unstable_pulse_chance)
        {
            var vel_x = Math.cos(velocity_angle) * current_velocity;
            var vel_y = Math.sin(velocity_angle) * current_velocity;
            var angle_unst = Math.random() * 2*3.14159;
            vel_x += unstable_pulse_force * Math.cos(angle_unst);
            vel_y += unstable_pulse_force * Math.sin(angle_unst);
            current_velocity = Math.sqrt(vel_x**2 + vel_y**2);
            velocity_angle = Math.atan2(vel_y,vel_x);
        }
        this.dataY[13-2] = func.FloatToScrd(current_velocity);
        this.dataY[14-2] = func.FloatToScrd(velocity_angle);

        //Movement & Bounce
        var xy = func.RotatePoint([current_velocity,0],velocity_angle,false);
        var x1 = func.ScrdToFloat(this.dataY[8-2]); var y1 = func.ScrdToFloat(this.dataY[9-2]);
        var x2 = x1 + xy[0]; var y2 = y1 + xy[1];
        var ef = func.GetBounceCoords(x1,y1,x2,y2,bounce_radius);
        if(ef[0]**2+ef[1]**2>=bounce_radius**2)
        {//Position correction
            var sqrt = Math.sqrt(ef[0]**2+ef[1]**2);
            ef[0] *= (bounce_radius-0.01)/sqrt;
            ef[1] *= (bounce_radius-0.01)/sqrt;
        }
        this.dataY[8-2] = func.FloatToScrd(ef[0]);
        this.dataY[9-2] = func.FloatToScrd(ef[1]);
        if(ef[3]==1) this.dataY[14-2] = func.FloatToScrd(ef[2]);
        
        //Shooting
        this.shooters.forEach(shooter => {
          this.world.ShotCalculateIfNow(shooter,players,this);
        });
    }
    End() //Executes on battle end directly after last FixedUpdate() Note: dataY will be reseted automatically after execution
    {
        this.world.CleanBullets(this.identifier);
    }
}

module.exports = {
    CBoss
};