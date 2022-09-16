const { func } = require("./functions");

class CBoss
{
    constructor(type,deltapos,dataX,dataY,world)
    {
        this.type = type; // Boss type (read only)
        this.deltapos = deltapos; //Delta position of boss center (read only)
        this.dataX = dataX; // General data
        this.dataY = dataY; // Additional data
        this.world = world; //Info about world
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
        var angle = (this.dataY[3-2]/50)%(2*3.14159);
        this.dataY[8-2] = func.FloatToScrd(22*Math.cos(angle));
        this.dataY[9-2] = func.FloatToScrd(22*Math.sin(angle));
        this.dataY[10-2] = func.FloatToScrd(angle*180/3.14159);
        console.log(this.world.GetPlayers());
    }
    End() //Executes on battle end directly after last FixedUpdate() Note: dataY will be reseted automatically after execution
    {

    }
}

module.exports = {
    CBoss
};