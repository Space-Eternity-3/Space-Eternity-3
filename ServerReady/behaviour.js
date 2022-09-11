const { func } = require("./functions");

class CBoss
{
    constructor(type,dataX,dataY,plr,bulletsT)
    {
        this.type = type; // Boss type (read only)
        this.plr = plr; // Players data (read only)
        this.dataX = dataX; // General data (read & write)
        this.dataY = dataY; // Additional data (read & write)
        this.bulletsT = bulletsT; // Bullets data (read & write)
    }

    //--------------------------//
    //---VARIABLE DESCRIPTION---//
    //--------------------------//

    /*
    
    dataX[0] - "1024" (boss identifier)
    dataX[1] - actual_level

    dataY[2-2] - 
    dataY[3-2] - 
    dataY[4-2] - 
    dataY[5-2] - 
    dataY[6-2] - 
    dataY[7-2] - 
    dataY[8-2] - actual_position_x [scrd]
    dataY[9-2] - actual_position_y [scrd]
    dataY[10-2] - actual_rotation [scrd]
    
    */

    //---------------------//
    //---BASIC FUNCTIONS---//
    //---------------------//

    Start() //Executes on battle start
    {
        
    }
    FixedUpdate() //Executes 50 times per second
    {
        var angle = this.dataY[3-2]/50;
        this.dataY[8-2] = func.FloatToScrd(22*Math.cos(angle));
        this.dataY[9-2] = func.FloatToScrd(22*Math.sin(angle));
        this.dataY[10-2] = func.FloatToScrd(angle*180/3.14159);
    }
    End() //Executes on battle end
    {
        
    }

    //-------------//
    //---BULLETS---//
    //-------------//
}

module.exports = {
    CBoss
};