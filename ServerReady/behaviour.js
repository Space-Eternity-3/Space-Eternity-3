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
    dataY[10-2] - actual_rotation [scrd]
    dataY[11-2] - rotation_velocity
    dataY[12-2] - target_rotation_velocity
    dataY[13-2] - rotation_waiting_timer
    <...>
    dataY[60-2] - {Other variables are empty at this time. Use them as a data storage between frames or ask Kamiloso to do some special tasks using them.}

    //---------------------//
    //---BASIC FUNCTIONS---//
    //---------------------*/

    Start() //Executes on battle start
    {
        
    }
    FixedUpdate() //Executes 50 times per second after starting frame
    {
        var players = this.world.GetPlayers();
        
        var rand_rot = Math.random();
        if(this.dataY[11-2]==this.dataY[12-2] && rand_rot>0.8) this.dataY[12-2] = func.randomInteger(-30,30);
        this.dataY[11-2] += Math.sign(this.dataY[12-2]-this.dataY[11-2]);
        this.dataY[10-2] = func.FloatToScrd((func.ScrdToFloat(this.dataY[10-2]) + 0.15*this.dataY[11-2]));
        
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