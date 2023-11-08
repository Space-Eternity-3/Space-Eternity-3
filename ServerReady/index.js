const WebSocket = require("ws");
const fs = require("fs");
const path = require("path");
const config = require("./config.json");
const authConfig = require("./authConfig.json");
const { runInNewContext } = require("vm");
const { parse } = require("path");
const { func } = require("./functions");
const { CBoss } = require("./behaviour");

//Variable functions
String.prototype.replaceAll = function replaceAll(search, replace) {
  return this.split(search).join(replace);
};
String.prototype.replaceAt = function(index, replacement) {
  return this.substring(0, index) + replacement + this.substring(index + replacement.length);
}
String.prototype.insertAt = function(index, insertion) {
  return this.substring(0, index) + insertion + this.substring(index);
}
Array.prototype.remove = function (ind) {
  this.splice(ind, 1);
  return this;
};

console.log("Starting procedures...");

//Authorization server variables
var waiting_authorized = [];

/* ---------AUTHORIZATION SERVER CONNECTION---------- */

const authServerUrl = authConfig.auth_server_url;

function connectToAuthServer() {
  return new Promise((resolve, reject) => {
    
    if(config.require_se3_account) {
    
    if(authConfig.is_configured) {
      
    const ws = new WebSocket(authServerUrl);

    ws.on('open', () => {
      console.log('Sending information to the authorization server...');
      ws.send("/RunningServerAdd "+
      (authConfig.host_nickname).replaceAll(" ","\n")+" "+(authConfig.host_password).replaceAll(" ","\n")+" "+
      (authConfig.server_name).replaceAll(" ","\n")+" "+(authConfig.server_redirect_address).replaceAll(" ","\n"));

      ws.on("message", (msg) => {
        var arg = msg.toString().split(" ");
        if(arg[0]=="/RetServer")
        {
          if(arg[1]=="10") {
            console.log("Server registered by user "+authConfig.host_nickname);
            resolve(ws);
          }
          else if(arg[1]=="13") {
            console.log("Server reloaded by user "+authConfig.host_nickname);
            resolve(ws);
          }

          else if(arg[1]=="9") {
            console.log("Server name '"+authConfig.server_name+"' is already registered by another player. Choose a different name.");
            reject("code 9");
          }
          else if(arg[1]=="11") {
            console.log("Server name should not contain \\n or space character.");
            reject("code 11");
          }
          else if(arg[1]=="12") {
            console.log("Server address should not contain \\n or space character.");
            reject("code 12");
          }
          else {
            console.log("Failied to login as "+authConfig.host_nickname);
            reject("code L");
          }
        }
        else if(arg[0]=="/RetAuthorizeUser")
        {
          //Add nick with conID for 8 seconds
          waiting_authorized.push([arg[1],arg[2],8]);
        }
        else
        {
          console.log("Unknown authorization answer.");
          reject("code W");
        }
      });
      ws.on("close", (code,reason) => {
        console.log("Connection with authorization server closed. Stopping the server...");
        process.emit("SIGINT");
      });
    });

    ws.on('error', (error) => {
      console.log('Error connecting to the authorization server.');
      console.log("You can disable 'require_se3_account' in the config.json file to ignore that.");
      console.log("Do it only when hosting a non-comercial server, as this will result in removing the nickname protection.");
      console.log("Such servers do not provide the server name system. Instead, they use raw IP or DNS addresses.");
      reject("code E");
    });
    
    } else {
      //not configured
      console.log("You must configure the authConfig.json file or disable 'require_se3_account' in the config.json file.");
      console.log("Here is the tutorial how to configure values:\n");
      console.log("'is_configured' - set this to true");
	    console.log("'auth_server_url' - leave it as it is, unless authorization address has changed");
	    console.log("'host_nickname' - your se3 account nickname");
	    console.log("'host_password' - your se3 account password");
	    console.log("'server_name' - SE3 server name, use it as se3://SERVER_NAME to connect to the server");
	    console.log("'server_redirect_address' - IP or DNS address which will be sent to users, who are connecting to your server");
      console.log("\nNote, that every user can have up to one server. Every next will overwrite the server_name reservation.");
      reject("code C");
    }

    } else {
      //doesn't require se3 account
      resolve("NO SOCKET");
    }
  });
}

connectToAuthServer()
  .then((AuthWs) => {
    console.log("Starting the server...");
    if(!config.require_se3_account) console.log("Warning: Running without the nickname protection!\nEnable 'require_se3_account' in file config.json to fix.");
    //(...)

/* -------------------------------------------------- */

const health_base = 1.0985;
const unit = 0.0008;
const in_arena_range = 37;

//Global variables
var serverVersion = "Beta 2.2";
var serverRedVersion = "Beta_2_2";
var clientDatapacksVar = "";
var seed;
var biome_memories = new Array(16000);
var hourHeader = "";
var gpl_number = 122;
var max_players = 10;
var verF;
var connectionAddress = "IP or DNS + port";
if(config.require_se3_account) connectionAddress = "se3://" + authConfig.server_name;

var boss_damages = [0,0,0,0,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,0,0 ,-1,-1,0,0,0,0,0,0,0,0,0,0,0,0,0,0];
var other_bullets_colliders = [0,0.14,0.14,0.12,1,0.25,0.25,1.2,1.68,0.92,0.92,0.25,0.25,0.25,0.14,0.08 ,1.68,0.25,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08];
var bullet_air_consistence = [0,0,0,1,0,1,0,1,0,0,0,0,0,1,0,1 ,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0];

if(Number.isInteger(config.max_players))
  max_players = config.max_players;

var pvp = true;
if(config.pvp==false) pvp = false;

var show_positions = true;
if(config.show_positions==false) show_positions = false;

var universe_name = "ServerUniverse";
if(config.universe_name) universe_name = config.universe_name;

var chunk_data = [];
var chunk_names = [];
var chunk_waiter = [];

var se3_ws = new Array(max_players);
var se3_wsS = new Array(max_players);
se3_ws.fill(""); Object.seal(se3_ws);
se3_wsS.fill(""); Object.seal(se3_wsS);

var memTemplate = {
  nicks: "0",
  health: "1",
  others1: "0",
  others2: "0",
  rposX: "0",
  rposY: "0",
};

var unstable_sprobability = 1;

function TreasureDrop(str)
{
  try {
      var marray = [];
      var i,j,rander=func.randomInteger(0,9999);
      var arra = str.split("-");
      var lngt=Math.floor(arra.length/5);
      for(i=0;i<lngt;i++)
      {
        for(j=0;j<5;j++) marray[j]=func.parseIntE(arra[5*i+j]);
        if(rander>=marray[3]&&rander<=marray[4]) return marray[0]+";"+func.randomInteger(marray[1],marray[2]);
      }
    } catch { return "8;1"; }
    return "8;1";
}

class CPlayer {
  constructor(pid) {
    this.Reset();
    this.gpid = pid;
    this.TreasureNextDrops = [];
    this.DarkTreasureNextDrops = [];
    for(var i=0;i<4;i++) {
      this.TreasureNextDrops.push(TreasureDrop(gameplay[105]));
      this.DarkTreasureNextDrops.push(TreasureDrop(gameplay[106]));
    }
  }
  Reset() {
    this.respawn_x = 0;
    this.respawn_y = 0;
    this.drill_counter = 0;
    this.drill_asteroid = 0;
    this.drill_group = 0;
    this.drill_list = [];
    this.unstable_pulses_available = 0;
    this.last_pos_changes = [];
    this.allowed_teleport_small = false;
    this.force_teleport_respawn = false;
    this.spam_handler = { //[last, max, wait_time, mode, kick, last_date] (TPS independent system)
      pre_command: [0,3,0,"limit",true,0],
      player_update: [0,60,1000,"spam",true,0],
      impulse_start: [0,1,(GplGet("impulse_cooldown")-1)*20,"cooldown",true,0],
      invisibility_pulse: [0,10,1000,"spam",false,0],
    };
  }
  DataImport(rsp_x,rsp_y) {
    this.Reset();
    this.ModifyRespawn(rsp_x,rsp_y);
  }
  ModifyRespawn(rsp_x,rsp_y) {
    this.respawn_x = func.parseFloatU(rsp_x);
    this.respawn_y = func.parseFloatU(rsp_y);
  }
  DrillAsk(drillID,pid,drillGroup) {
    this.drill_asteroid = drillID;
    this.drill_group = drillGroup;
    this.drill_counter = handDrillTimeGet(pid);
  }
  DrillReady(pid) {
    var mined = drillItemGet(this.drill_asteroid,0);
    if(mined==0) return;
    this.drill_list.push(mined);
    sendTo(se3_ws[pid],"/RetDrillReady "+mined+" "+this.drill_group+" X X");
  }
  DrillGet(pid,item,slot) {
    var iof = this.drill_list.indexOf(item);
    if(iof!=-1) {
      this.drill_list.remove(iof);
      return invChangeTry(pid,item,"1",slot);
    }
    else return false;
  }
  DrillDiscard(item) {
    var iof = this.drill_list.indexOf(item);
    if(iof!=-1) this.drill_list.remove(iof);
  }
  TreasureArrayUpdate(treasure_type,generate_new_loot) {
    var spliced;
    if(generate_new_loot) {
      if(treasure_type==0) {
        spliced = this.TreasureNextDrops[0];
        this.TreasureNextDrops.remove(0);
        this.TreasureNextDrops.push(TreasureDrop(gameplay[105]));
      }
      else {
        spliced = this.DarkTreasureNextDrops[0];
        this.DarkTreasureNextDrops.remove(0);
        this.DarkTreasureNextDrops.push(TreasureDrop(gameplay[106]));
      }
    }
    else {
      if(treasure_type==0) {
        spliced = this.TreasureNextDrops[0];
        this.TreasureNextDrops.remove(0);
        this.TreasureNextDrops.push(spliced);
      }
      else {
        spliced = this.DarkTreasureNextDrops[0];
        this.DarkTreasureNextDrops.remove(0);
        this.DarkTreasureNextDrops.push(spliced);
      }
    }
    if(treasure_type==0) sendTo(se3_ws[this.gpid],"/RetTreasureLoot "+plr.pclass[this.gpid].TreasureNextDrops[3]+" "+treasure_type+" X X");
    else sendTo(se3_ws[this.gpid],"/RetTreasureLoot "+plr.pclass[this.gpid].DarkTreasureNextDrops[3]+" "+treasure_type+" X X");
  }
  PosChangeAdd(distance) {
    if(this.last_pos_changes.length >= config.anti_cheat.period_in_ticks) this.last_pos_changes.shift();
    this.last_pos_changes.push(distance);
  }
  PosChangeSum() {
    var sum=0,i,lngt = this.last_pos_changes.length;
    for(i=0;i<lngt;i++) sum += this.last_pos_changes[i];
    return sum;
  }
}

var plr = {
  waiter: [0],
  players: ["0"],
  nicks: ["0"],
  conID: ["0"],
  livID: ["0"],
  immID: ["0"],

  connectionTime: [-1],
  sHealth: [0],
  sRegTimer: [-1],

  data: ["0"],
  inventory: ["0"],
  backpack: ["0"],
  upgrades: ["0"],
  pushInventory: ["0"],

  mems: [Object.assign({},memTemplate)],
  impulsed: [[]],
  bossMemories: [[]],

  pclass: [],
};

var ki, kj, kd = Object.keys(plr);
var klngt = kd.length;
for(ki=1;ki<max_players;ki++)
  for(kj=0;kj<klngt;kj++)
  {
    if(kd[kj]=="mems")
      plr["mems"].push(Object.assign({},plr["mems"][0]));
    else if(kd[kj]=="impulsed"||kd[kj]=="bossMemories")
      plr[kd[kj]].push([]);
    else if(kd[kj]!="pclass")
      plr[kd[kj]].push(plr[kd[kj]][0]);
  }

var growT = [];
var growW = [];

var drillT = [];
var drillW = [];
var drillC = [];

const bulletTemplate = {
  ID: 0,
  owner: -1,
  type: 0,
  age: 0,
  max_age: 100,

  steerPtr: -1,
  boss_owner: 0,

  normal_damage: 0,
  is_unstable: false,
  turn_used: false,
  unstable_virtual: false,
  
  start: {
    x: 0,
    y: 0
  },
  vector: {
    x: 0,
    y: 0
  },
  pos: {
    x: 0,
    y: 0
  },

  damaged: [],
  immune: [],
};

const scrTemplate = {
  dataX: [],
  dataY: [],
  bulCols: [],
  bID: -1,
  type: 0,
  posCX: 0,
  posCY: 0,
  timeToDisappear: 1000,
  timeToLose: 1000,
  behaviour: -1,
};

var bulletsT = [];
var scrs = [];

var growSolid = [];
growSolid[5] = "-1;-1;6";
growSolid[6] = "-1;-1;7";
growSolid[25] = "-1;-1;23";

var jse3Var = [];
var jse3Dat = [];

var datName = "";
var version = "";
var craftings = "";
var craftMaxPage = "";
var biomeChances = "";
var drillLoot = new Array(16);
var fobGenerate = new Array(64);
var biomeTags = new Array(32);
var customStructures = new Array(32);
var typeSet = new Array(224);
var gameplay = new Array(256);
var modifiedDrops = new Array(128);
var translateFob = [];
var translateAsteroid = [];

var size_download = 0;
var size_upload = 0;
var size_updates = 0;
var size_tps = 1;

var current_tick = 0;

drillLoot.fill(""); Object.seal(drillLoot);
fobGenerate.fill(""); Object.seal(fobGenerate);
biomeTags.fill(""); Object.seal(biomeTags);
customStructures.fill(""); Object.seal(customStructures);
typeSet.fill(""); Object.seal(typeSet);
gameplay.fill(""); Object.seal(gameplay);
modifiedDrops.fill(""); Object.seal(modifiedDrops);

function Num31ToChar(num) {
  if (num < 10) return String.fromCharCode(48 + num)+"";
  return String.fromCharCode(55 + num)+"";
}

function replaceCharAtIndex(inputStr, index, newChar) {
  if (index < 0 || index >= inputStr.length) {
      return inputStr; // Jeśli indeks jest poza zakresem, zwróć oryginalny string
  }

  return inputStr.slice(0, index) + newChar + inputStr.slice(index + 1);
}

//Classes
class CShooter
{
  constructor(bul_typ,angl_deg,deviat_deg,precis_deg,rad,ths,alway,freq,activess,cld,otid,slvc) {
    this.bullet_type = bul_typ;
    this.angle = angl_deg*3.14159/180;
    this.max_deviation = deviat_deg*3.14159/180;
    this.precision = precis_deg*3.14159/180;
    this.radius = rad;
    this.thys = ths;
    this.stupid = alway;
    this.frequency = freq;
    this.actives = activess;
    this.cooldown = cld;
    this.one_time_id = otid; //only prime numbers
    this.salvic = slvc;
  }
  CanShoot(x,y)
  {
    x -= func.ScrdToFloat(this.thys.dataY[8-2]);
    y -= func.ScrdToFloat(this.thys.dataY[9-2]);
    var pat = func.RotatePoint([x,y],-(this.angle+func.ScrdToFloat(this.thys.dataY[10-2])*3.14159/180),false);
    x = pat[0]-this.radius; y = pat[1];

    var target_agl = Math.abs(Math.atan2(y,x));
    return (target_agl <= this.max_deviation);
  }
  BestDeviation(x,y)
  {
    x -= func.ScrdToFloat(this.thys.dataY[8-2]);
    y -= func.ScrdToFloat(this.thys.dataY[9-2]);
    var pat = func.RotatePoint([x,y],-(this.angle+func.ScrdToFloat(this.thys.dataY[10-2])*3.14159/180),false);
    x = pat[0]-this.radius; y = pat[1];
    return Math.atan2(y,x);
  }
}
class CShape
{
  constructor(neme1,shape_description1,default_offset1,col_id) {
    this.neme = neme1;
    this.sds = shape_description1; // sx sy
    this.dof = default_offset1; // cx cy lx ly lrot
    this.cop = {cx:0,cy:0};
    this.col_identifier = col_id;
  }
  SetShapeCollider(dx,dy,drot)
  {
    var alpha = (drot) * Math.PI / 180;
    var beta = (this.dof.lrot+drot) * Math.PI / 180;

    var lroted = func.RotatePoint([this.dof.lx,this.dof.ly],alpha,false);
    var cx = this.dof.cx + dx + lroted[0];
    var cy = this.dof.cy + dy + lroted[1];
    this.cop.cx = cx; this.cop.cy = cy;

    if(this.neme=="sphere") return;

    var srt1 = func.RotatePoint([0,this.sds.sy],beta,false)
    var srt2 = [-srt1[0],-srt1[1]];
    srt1[0] += cx; srt1[1] += cy;
    srt2[0] += cx; srt2[1] += cy;

    if(this.neme=="cylinder" || this.neme=="capsule") func.CollisionLinearBulletSet(srt1[0],srt1[1],srt2[0],srt2[1],this.sds.sx);
  }
  IsSphereColliding(cx,cy,r1)
  {
    if(this.neme=="sphere") return func.CollisionPointCheck(this.cop.cx,this.cop.cy,cx,cy,this.sds.sx,r1);
    if(this.neme=="cylinder" || this.neme=="capsule") return func.CollisionLinearCheck(cx,cy,r1,(this.neme=="capsule"));
    return false;
  }
}
class CInfo
{
  constructor(plas,buls) {
    this.plas = plas;
    this.buls = buls;
    this.players = [];
    this.deltaposmem = 0;
  }

  UpdatePlayers(deltapos)
  {
    this.players = [];
    this.deltaposmem = deltapos;
    var i;
    for(i=0;i<max_players;i++)
      if(plr.players[i]!="0" && plr.players[i]!="1")
      {
        var splitted = plr.players[i].split(";");
        var tpl = {id:0,x:0,y:0};
        tpl.id = i;
        tpl.x = func.parseFloatU(splitted[0]) - deltapos.x;
        tpl.y = func.parseFloatU(splitted[1]) - deltapos.y;
        var px = tpl.x;
        var py = tpl.y;
        if(func.parseIntU(splitted[5].split("&")[1]) % 100 != 1) //not invisible (must be %100)
        if(px**2 + py**2 <= in_arena_range**2) //in arena range
          this.players.push(tpl);
      }
  }
  GetPlayers()
  {
    return this.players;
  }
  CreateTelepPulse(xx,yy)
  {
    xx = (xx+"").replaceAll(".",",");
    yy = (yy+"").replaceAll(".",",");
    sendToAllPlayers("/RetEmitParticles -1 16 "+xx+" "+yy+" X X");
  }
  RemoteDamage(id)
  {
    DamageFLOAT(id,GplGet("cyclic_remote_damage")*gameplay[36]);
  }
  ShotCalculateIfNow(shooter,players,thys)
  {
    if(shooter.salvic && thys.dataY[3-2]%250>=175) return;
    var fram = thys.dataY[19-2] - thys.dataY[17-2];
    var true_frequency = shooter.frequency;

    //Shooter frequency change (quakes)
    if(
      (thys.type==1 && thys.dataY[18-2]==4) ||
      (thys.type==2 && thys.dataY[18-2]==4) ||
      (thys.type==3 && thys.dataY[18-2]==4) ||
      (thys.type==6 && thys.dataY[18-2]==4)
    ) true_frequency = Math.floor(shooter.frequency*0.67);

    if(true_frequency<1) true_frequency=1;
    if(shooter.actives[thys.dataY[18-2]]=='1' && fram>shooter.cooldown && (fram-shooter.cooldown)%true_frequency==0)
      if(shooter.one_time_id<0 || thys.dataY[20-2]%shooter.one_time_id!=0) this.ShotCalculate(shooter,players,thys);
  }
  ShotCalculate(shooter,players,thys)
  {
    //Stupid shooter
    if(shooter.stupid) {
      this.ShotUsingShooter(shooter,0,thys);
      return;
    }

    //Intelligent shooter
    var min_deviation = 10000;
    players.forEach( player => {
      if(shooter.CanShoot(player.x,player.y)) {
        var cand_deviation = shooter.BestDeviation(player.x,player.y);
        if(Math.abs(cand_deviation) < Math.abs(min_deviation)) min_deviation = cand_deviation;
      }
    });
    if(min_deviation!=10000) this.ShotUsingShooter(shooter,min_deviation,thys);
  }
  ShotUsingShooter(shooter,best_deviation,thys)
  {
    var deviation_angle = (Math.random()-0.5)*2*shooter.precision + best_deviation;
    if(deviation_angle < -shooter.max_deviation) deviation_angle = -2*shooter.max_deviation - deviation_angle;
    if(deviation_angle > shooter.max_deviation) deviation_angle = 2*shooter.max_deviation - deviation_angle;
    if(shooter.one_time_id>=0) thys.dataY[20-2] *= shooter.one_time_id;
    this.ShotCooked(shooter.angle,shooter.bullet_type,thys,deviation_angle,shooter.radius);
  }
  ShotCooked(delta_angle_rad,btype,thys,deviation_angle,rad)
  {
    var lx = func.ScrdToFloat(thys.dataY[8-2]);
    var ly = func.ScrdToFloat(thys.dataY[9-2]);
    var angle = delta_angle_rad + func.ScrdToFloat(thys.dataY[10-2])*3.14159/180;
    var pak = ["?",0];
    if(btype==9 || btype==10) pak[0]=GplGet("boss_seeker_speed"); else pak[0]=GplGet("boss_bullet_speed");
    var efwing = func.RotatePoint(pak,angle+deviation_angle,false);
    this.ShotRaw(rad*Math.cos(angle)+thys.deltapos.x+lx,rad*Math.sin(angle)+thys.deltapos.y+ly,efwing[0],efwing[1],btype,thys.identifier);
  }
  ShotRaw(px,py,vx,vy,type,bidf)
  {
      var tpl = Object.assign({},bulletTemplate);
      tpl.start = Object.assign({},bulletTemplate.start);
      tpl.vector = Object.assign({},bulletTemplate.vector);
      tpl.pos = Object.assign({},bulletTemplate.pos);
      tpl.damaged = [];
      tpl.immune = [];

      tpl.ID = func.randomInteger(0,1000000000);
      tpl.owner = bidf;
      tpl.type = type;
      tpl.start.x = px;
      tpl.start.y = py;
      tpl.vector.x = vx;
      tpl.vector.y = vy;
      tpl.pos.x = tpl.start.x;
      tpl.pos.y = tpl.start.y;

      tpl.normal_damage = boss_damages[type] * func.parseFloatU(gameplay[32]);
      if(type==3||type==13) tpl.is_unstable = true;
      else tpl.is_unstable = false;

      if(tpl.vector.x==0) tpl.vector.x = 0.00001;
      if(tpl.vector.y==0) tpl.vector.y = 0.00001;

      var arg = ("/ "+bidf+" "+type+" "+px+" "+py+" "+vx+" "+vy+" "+tpl.ID).replaceAll(".",",").split(" ");
      arg.push("0");
      spawnBullet(tpl,arg,bidf);
  }
  CleanBullets(bidf)
  {
    var i,lngt=bulletsT.length;
    for(i=0;i<lngt;i++)
      if(bulletsT[i].owner==bidf)
        destroyBullet(i, ["", bulletsT[i].owner, bulletsT[i].ID, bulletsT[i].age], false);
  }

  //bossy functions
  GetShootersList(type,thys)
  {
    var shooters = [];
    if(true)
    {
        if(type==1) //Protector
        {
            shooters = [
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
            ];
        }
        if(type==2) //Adecodron
        {
            shooters = [
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
            ];
        }
        if(type==3) //Octogone
        {
            shooters = [
              new CShooter(7, 0,60,10, 6.5,thys,false, 50, "10111",0,-1, false),
              new CShooter(7, 45,60,10, 6.5,thys,false, 50, "10111",0,-1, false),
              new CShooter(7, 90,60,10, 6.5,thys,false, 50, "10111",0,-1, false),
              new CShooter(7, 135,60,10, 6.5,thys,false, 50, "10111",0,-1, false),
              new CShooter(7, 180,60,10, 6.5,thys,false, 50, "10111",0,-1, false),
              new CShooter(7, 225,60,10, 6.5,thys,false, 50, "10111",0,-1, false),
              new CShooter(7, 270,60,10, 6.5,thys,false, 50, "10111",0,-1, false),
              new CShooter(7, 315,60,10, 6.5,thys,false, 50, "10111",0,-1, false),

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
            ];
        }
        if(type==4) //Starandus
        {
            shooters = [
              new CShooter(5, 30,60,12, 6.5,thys,false, 30, "10000",15,-1, false), //Normal
              new CShooter(5, 60,60,12, 6.5,thys,false, 30, "10000",0,-1, false),
              new CShooter(5, 120,60,12, 6.5,thys,false, 30, "10000",15,-1, false),
              new CShooter(5, 150,60,12, 6.5,thys,false, 30, "10000",0,-1, false),
              new CShooter(5, 210,60,12, 6.5,thys,false, 30, "10000",15,-1, false),
              new CShooter(5, 240,60,12, 6.5,thys,false, 30, "10000",0,-1, false),
              new CShooter(5, 300,60,12, 6.5,thys,false, 30, "10000",15,-1, false),
              new CShooter(5, 330,60,12, 6.5,thys,false, 30, "10000",0,-1, false),

              new CShooter(5, 30,60,60, 6.5,thys,true, 34, "01000",18,-1, false), //Fire
              new CShooter(5, 60,60,60, 6.5,thys,true, 37, "01000",0,-1, false),
              new CShooter(5, 120,60,60, 6.5,thys,true, 35, "01000",18,-1, false),
              new CShooter(5, 150,60,60, 6.5,thys,true, 36, "01000",0,-1, false),
              new CShooter(5, 210,60,60, 6.5,thys,true, 35, "01000",18,-1, false),
              new CShooter(5, 240,60,60, 6.5,thys,true, 34, "01000",0,-1, false),
              new CShooter(5, 300,60,60, 6.5,thys,true, 37, "01000",18,-1, false),
              new CShooter(5, 330,60,60, 6.5,thys,true, 36, "01000",0,-1, false),

              new CShooter(5, 30,60,60, 6.5,thys,true, 14, "00100",8,-1, false), //Supernova
              new CShooter(5, 60,60,60, 6.5,thys,true, 17, "00100",0,-1, false),
              new CShooter(5, 120,60,60, 6.5,thys,true, 15, "00100",8,-1, false),
              new CShooter(5, 150,60,60, 6.5,thys,true, 16, "00100",0,-1, false),
              new CShooter(5, 210,60,60, 6.5,thys,true, 15, "00100",8,-1, false),
              new CShooter(5, 240,60,60, 6.5,thys,true, 14, "00100",0,-1, false),
              new CShooter(5, 300,60,60, 6.5,thys,true, 17, "00100",8,-1, false),
              new CShooter(5, 330,60,60, 6.5,thys,true, 16, "00100",0,-1, false),

              new CShooter(16, 45,60,60, 6.5,thys,true, 24, "00010",13,-1, false), //Gravitons
              new CShooter(16, 90,60,60, 6.5,thys,true, 27, "00010",0,-1, false),
              new CShooter(16, 135,60,60, 6.5,thys,true, 25, "00010",13,-1, false),
              new CShooter(16, 180,60,60, 6.5,thys,true, 26, "00010",0,-1, false),
              new CShooter(16, 225,60,60, 6.5,thys,true, 25, "00010",13,-1, false),
              new CShooter(16, 270,60,60, 6.5,thys,true, 24, "00010",0,-1, false),
              new CShooter(16, 315,60,60, 6.5,thys,true, 27, "00010",13,-1, false),
              new CShooter(16, 360,60,60, 6.5,thys,true, 26, "00010",0,-1, false),

              new CShooter(17, 0,15,15, 5,thys,true, 10, "00001",0,-1, false), //Electrons
              new CShooter(17, 180,15,15, 5,thys,true, 10, "00001",0,-1, false),
            ];
        }
        if(type==6) //Degenerator
        {
            shooters = [
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

              new CShooter(13, 22.5,20,20, 7,thys,true, 15, "00100",0,-1, false),
              new CShooter(13, 157.5,20,20, 7,thys,true, 15, "00100",0,-1, false),
              new CShooter(13, 202.5,20,20, 7,thys,true, 15, "00100",0,-1, false),
              new CShooter(13, 337.5,20,20, 7,thys,true, 15, "00100",0,-1, false),
              new CShooter(13, 270,70,70, 7,thys,true, 8, "00100",0,-1, false),
            ];
        }
    }
    return shooters;
  }
}
let visionInfo = new CInfo(plr,bulletsT);

//Websocket functions
let connectionOptions = {
  port: 27683,
};
if(Number.isInteger(config.port))
  connectionOptions.port = config.port;

const wss = new WebSocket.Server(connectionOptions);

function sendToAllClients(data) {
  console.log("Sending should be to all players, not clients");
  wss.clients.forEach(function (client) {
    sendTo(client,data);
  });
}
function sendToAllPlayers(data) {
  for(var i=0;i<max_players;i++)
    if(se3_wsS[i]=="game") sendTo(se3_ws[i],data);
}
function sendTo(ws,data) {
  try{
    ws.send(data);
    size_upload += data.length;
  }catch{return;}
}

//Funny functions
function getRandomFunnyText()
{
  var rfn = func.randomInteger(0,19);
  switch(rfn)
  {
    case 0: return "Eating a dinner..."; case 1: return "Breaking bones...";
    case 2: return "Waiting for a waiter..."; case 3: return "Talking with somebody...";
    case 4: return "Washing teeth..."; case 5: return "Installing something...";
    case 6: return "Breaking internet..."; case 7: return "Uploading private videos...";
    case 8: return "Lending money..."; case 9: return "Hacking SE3...";
    case 10: return "Kicking players..."; case 11: return "Being behind you...";
    case 12: return "Playing football..."; case 13: return "Watching Netflix...";
    case 14: return "Calling Kamiloso..."; case 15: return "Removing your life...";
    case 16: return "Playing SE3..."; case 17: return "Sending email...";
    case 18: return "Hacking your computer..."; case 19: return "Searching for cheaters...";
  }
  return "Something went wrong...";
}

//File functions
function readF(nate) {
  if (existsF(nate)) return fs.readFileSync(nate, { flag: "r" }).toString();
  else crash("Can't read file " + nate);
}
function existsF(nate) {
  return fs.existsSync(nate);
}
function writeF(nate, text) {
  var i,
    fold = path.dirname(nate);
  var foldT = fold.split("/");
  var lngt = foldT.length;
  var pathCurrent = "";
  for (i = 0; i < lngt; i++) {
    pathCurrent += foldT[i] + "/";
    if (!existsF(pathCurrent)) fs.mkdirSync(pathCurrent);
  }
  fs.writeFileSync(nate, String(text));
}
function removeF(nate) {
  fs.unlinkSync(nate);
}

//File readable functions
function fileReadablePlayer(elemT) {
  if (elemT.length < 4) return false;

  var elem0 = elemT[0].split(";");
  if (elem0.length < 8) return false;
  var elem1 = elemT[1].split(";");
  if (elem1.length < 18) return false;
  var elem2 = elemT[2].split(";");
  if (elem2.length < 42) return false;
  var elem3 = elemT[3].split(";");
  if (elem3.length < 5) return false;

  var i;
  for (i = 0; i < 6; i++) if (isNaN(func.parseFloatP(elem0[i]))) return false;
  if (isNaN(func.parseIntP(elem0[6]))) return false;
  if (isNaN(func.parseFloatP(elem0[7]))) return false;
  for (i = 0; i < 18; i++) if (isNaN(func.parseIntP(elem1[i]))) return false;
  for (i = 0; i < 42; i++) if (isNaN(func.parseIntP(elem2[i]))) return false;
  for (i = 0; i < 5; i++) if (isNaN(func.parseIntP(elem3[i]))) return false;

  return true;
}

//File player data functions
function msgToPlayer(str) {
  var strT = str.split(";");
  return [
    strT[0],
    strT[1],
    strT[8],
    strT[9],
    strT[6],
    strT[7],
    strT[10],
    strT[11],
  ].join(";");
}
function playerToMsg(str) {
  var strT = str.split(";");
  return [
    strT[0],
    strT[1],
    "",
    "",
    0,
    "0&0",
    strT[4],
    strT[5],
    strT[2],
    strT[3],
    strT[6],
    strT[7],
  ].join(";");
}
function pushConvert(inv, psh) {
  var invT = inv.split(";");
  var pshT = psh.split(";");
  var i,
    effect = [];
  for (i = 0; i < 9; i++) {
    effect[2 * i] = invT[2 * (pshT[i] - 1)];
    effect[2 * i + 1] = invT[2 * (pshT[i] - 1) + 1];
  }
  return effect.join(";");
}
function savePlayer(n) {
  var effTab = [
    msgToPlayer(plr.data[n]),
    pushConvert(plr.inventory[n], plr.pushInventory[n]),
    plr.backpack[n],
    plr.upgrades[n],
  ];
  var effect = effTab.join(";\r\n") + ";\r\n";
  writeF(universe_name + "/Players/" + plr.nicks[n] + ".se3", effect);
}
function readPlayer(n) {
  var srcTT, i, dd, di, db, du, eff, lngt;
  if (existsF(universe_name + "/Players/" + plr.nicks[n] + ".se3"))
    srcTT = readF(universe_name + "/Players/" + plr.nicks[n] + ".se3").split("\r\n");
  else return;

  if (!fileReadablePlayer(srcTT)) return;

  plr.data[n] = playerToMsg(srcTT[0]);
  di = srcTT[1].split(";");
  db = srcTT[2].split(";");
  du = srcTT[3].split(";");

  eff = di[0];
  for (i = 1; i < 18; i++) eff += ";" + di[i];
  plr.inventory[n] = eff;
  eff = db[0];
  for (i = 1; i < 42; i++) eff += ";" + db[i];
  plr.backpack[n] = eff;
  eff = du[0];
  for (i = 1; i < 5; i++) eff += ";" + du[i];
  plr.upgrades[n] = eff;
}

//File asteroid data function
function ulamToXY(ulam) {
  var sqrt = Math.floor(Math.sqrt(ulam));
  if (sqrt % 2 == 0) sqrt--;
  var x = sqrt / 2 + 0.5;
  var y = -sqrt / 2 - 0.5;
  var pot = sqrt ** 2;
  var delta = ulam - pot;
  var cwr = Math.floor(delta / (sqrt + 1));
  var dlt = delta % (sqrt + 1);
  if (cwr == 0 && dlt == 0) return [x - 1, y + 1];

  if (cwr > 0) y += sqrt + 1;
  if (cwr > 1) x -= sqrt + 1;
  if (cwr > 2) y -= sqrt + 1;

  if (cwr == 0) y += dlt;
  if (cwr == 1) x -= dlt;
  if (cwr == 2) y -= dlt;
  if (cwr == 3) x += dlt;

  return [x, y];
}
function chunkRead(ind) {
  var i, j, eff = seed + "\r\n";
  if (existsF(universe_name + "/Asteroids/Generated_" + ind + ".se3"))
  {
    var datT = readF(universe_name + "/Asteroids/Generated_" + ind + ".se3").split("\r\n");
    var pom;

    try {
      if (datT[0] == seed) {
        for (i = 1; i <= 100; i++) {
          var datM = datT[i].split(";");
          var lnt = datM.length;
          if (lnt > 61) lnt = 61;
          for (j = 0; j < lnt; j++) if (datM[j] != "") pom = func.parseIntE(datM[j]);
          for (j = lnt; j < 61; j++) datT[i] += ";";
          eff += datT[i] + "\r\n";
        }
        return eff;
      } else throw "error";
    } catch {
      console.log("Asteroid file [" + ind + "] is invalid. Generating new data...");
    }
  }
  for (i = 0; i < 100; i++) eff += ";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;" + "\r\n";
  return eff;
}
function removeEnds(str) {
  var i,
    lngt = str.length,
    eff = "";
  for (i = lngt; i > 0; i--) if (str[i - 1] != ";") break;
  lngt = i;
  for (i = 0; i < lngt; i++) eff += str[i];
  return eff;
}
function chunkSave(n) {
  var i,
    j,
    eff = chunk_data[n][0] + "\r\n";
  var lines = [];
  for (i = 0; i < 100; i++) {
    lines[i] = chunk_data[n][i + 1].join(";");
    lines[i] = removeEnds(lines[i]);
  }
  eff += lines.join("\r\n") + "\r\n";
  writeF(universe_name + "/Asteroids/Generated_" + chunk_names[n] + ".se3", eff);
}
function asteroidIndex(ulam) {
  var xy = ulamToXY(ulam);
  var x = xy[0],
    y = xy[1];

  // X - Real position <INF;INF>
  // gX - Sector position <INF;INF>
  // rX - Reduced position <0;9>

  var gX, gY, rX, rY, rSS;
  gX = Math.floor(x / 10);
  gY = Math.floor(y / 10);
  if (x < 0) rX = -(x % 10);
  else rX = x % 10;
  if (y < 0) rY = -(y % 10);
  else rY = y % 10;
  rSS = rX * 10 + rY;

  var i,
    lngt = chunk_names.length;
  for (i = 0; i < lngt; i++) {
    if (chunk_names[i] == gX + "_" + gY) {
      chunk_waiter[i] = 100;
      return [i, rSS + 1];
    }
  }

  chunk_names.push(gX + "_" + gY);
  chunk_waiter.push(100);

  var i,
    lines = chunkRead(chunk_names[lngt]).split("\r\n");
  for (i = 1; i <= 100; i++) lines[i] = lines[i].split(";");

  chunk_data.push(lines);

  return [lngt, rSS + 1];
}

//Data functions
function nickWrong(stam) {
  if (
    stam.length > 16 ||
    stam == "0" ||
    stam == ""
  ) return true;
  var i,lngt = stam.length;
  for(i=0;i<lngt;i++)
    if(![
      'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
      'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
      '1','2','3','4','5','6','7','8','9','0','_','-'
    ].includes(stam[i])) return true;
  return false;
}
function clientDatapacks() {
  // ' element separator
  // ~ array separator

  return [
    craftings,
    craftMaxPage,
    drillLoot.join("'"),
    fobGenerate.join("'"),
    typeSet.join("'"),
    gameplay.join("'").replaceAll(".", ","),
    modifiedDrops.join("'"),
    biomeTags.join("'").replaceAll(" ", "_"),
    biomeChances,
    customStructures.join("'").replaceAll(" ", "^"),
  ].join("~");
}

//On exit functions
process.stdin.resume();
function exitHandler(options, exitCode) {
  try {
    SaveAllNow();
  } catch {}
  console.log("Data saved");
  if (options.exit) process.exit();
}

process.on("SIGINT", exitHandler.bind(null, { exit: true })); //On ctrl+C
process.on("SIGHUP", exitHandler.bind(null, { exit: true })); //On cmd closed
process.on("SIGUSR1", exitHandler.bind(null, { exit: true })); //???
process.on("SIGUSR2", exitHandler.bind(null, { exit: true })); //???

//process.on('uncaughtException', exitHandler.bind(null, {exit:true})); //On error
//process.on('exit', exitHandler.bind(null,{cleanup:true})); //???

//Save optimalize functions
function SaveAllNow() {
  var i,
    lngt = chunk_data.length;
  for (i = 0; i < max_players; i++) if (checkPlayerCn(i, plr.conID[i])) savePlayer(i);
  for (i = 0; i < lngt; i++) chunkSave(i);
  writeF(universe_name + "/Biomes.se3", biome_memories.join("\r\n") + "\r\n");
}

//Save all once per 15 seconds (or less if lags)
setInterval(function () { // <interval #1>
  SaveAllNow();
  var i,
    lngt = chunk_waiter.length;
  for (i = 0; i < lngt; i++) {
    if (chunk_waiter[i] == 0) {
      chunk_waiter.remove(i);
      chunk_names.remove(i);
      chunk_data.remove(i);
      lngt--;
      i--;
    }
  }
}, 15000);

function getProtLevelAdd(art)
{
	if(art!=1) return 0;
	else return func.parseFloatU(gameplay[16]);
}
function getProtRegenMultiplier(art)
{
	if(art!=1) return 1;
	else return func.parseFloatU(gameplay[17]);
}

function DamageFLOAT(pid,dmg)
{
  dmg = func.parseFloatU(dmg);
  if(dmg>0 && plr.players[pid].split(";").length!=1 && plr.connectionTime[pid]>=50)
  {
    var artid = plr.backpack[pid].split(";")[30] - 41;
    if(plr.backpack[pid].split(";")[31]=="0") artid = -41;
    if(func.parseFloatU(plr.players[pid].split(";")[5].split("&")[1])%25==2) return;
    var potHHH = func.parseFloatU(plr.upgrades[pid].split(";")[0]) + getProtLevelAdd(artid) + func.parseFloatU(gameplay[26]);
		if(potHHH<-50) potHHH = -50; if(potHHH>56.397) potHHH = 56.397;
		dmg=0.02*dmg/(Math.ceil(50*Math.pow(health_base,potHHH))/50);
		CookedDamage(pid,dmg,false);

    var info = "d";
    if(plr.sHealth[pid]<=0)
    {
      if(artid != 4) info = "K";
      else info = "I";
    }
		sendTo(se3_ws[pid],"/RetServerDamage "+pid+" "+(dmg+"").replaceAll(".",",")+" "+plr.immID[pid]+" "+plr.livID[pid]+" "+info+" X X");

    if(info=="K")
    {
      kill(pid);
      plr.players[pid] = "1";
    }
    if(info=="I")
    {
      immortal(pid);
      var cens = Censure(plr.players[pid],pid,plr.livID[pid]);
      plr.players[pid] = cens;
      if(cens!="1") plr.data[pid] = cens;
    }
    return info;
  }
}
function CookedDamage(pid,dmg,if_ringed)
{
  dmg = func.parseFloatU(dmg);
  plr.sHealth[pid] -= dmg;
  if(Math.round(plr.sHealth[pid]*10000) == 0) plr.sHealth[pid] = 0;
  plr.sRegTimer[pid] = Math.floor(50*func.parseFloatU(gameplay[4]));

  if(plr.players[pid]=="0" || plr.players[pid]=="1") return;

  var xx = plr.players[pid].split(";")[0];
  var yy = plr.players[pid].split(";")[1];
  if(!if_ringed) sendToAllPlayers("/RetEmitParticles "+pid+" 2 "+xx+" "+yy+" X X");
  else sendToAllPlayers("/RetEmitParticles "+pid+" 18 "+xx+" "+yy+" X X");
  sendToAllPlayers("/RetInvisibilityPulse "+pid+" wait X X");
}
function getBulletDamage(type,owner,pid,bll)
{
  var artid,is_bos;
  if(pid==-2) { //unstable boss
    artid = 6;
    is_bos = true;
  }
  else if(pid==-1 || pid==-3) { //normal boss
    artid = -41;
    is_bos = true;
  }
  else { //player
    if(bll.immune.includes(pid+"")) return 0;
    artid = plr.backpack[pid].split(";")[30] - 41;
    if(plr.backpack[pid].split(";")[31]==0) artid = -41;
    is_bos = false;
  }
  if(artid!=6 || !bll.is_unstable)
  {
    if(!is_bos) return bll.normal_damage;
    else {
      var damage_modifier = 1;
      if(bll.type==3) damage_modifier = func.parseFloatU(gameplay[37]);
      if(bll.type==15) damage_modifier = func.parseFloatU(gameplay[38]);
      if(bll.type==15 && pid==-3) damage_modifier = 0;
      return bll.normal_damage*damage_modifier;
    }
  }
  else return 0;
}

//HUB INTERVAL <interval #0>
var date_before = Date.now();
var date_start = Date.now();
var time_loan = 0;
setInterval(function () { // <interval #2>
  while(Date.now() > date_before)
  {
    date_before+=20;
    if((date_before-date_start) % 1000 == 0) //precisely 1 times per second (never affected)
    {
      updateHourHeader();
      setTerminalTitle("SE3 server | " + serverVersion +
        " | Download: " + size_download/1000 + "KB/s" +
        " | Upload: " + size_upload/1000 + "KB/s" +
        " | Packet frequency: " + size_updates + "/s" +
        " | TPS: " + size_tps
      );
      size_download = 0;
      size_upload = 0;
      size_updates = 0;
      size_tps = 0;

      var vi,wa_lngt = waiting_authorized.length;
      for(vi=0;vi<wa_lngt;vi++) {
        waiting_authorized[vi][2]--;
        if(waiting_authorized[vi][2]<=0) {
          waiting_authorized.splice(vi,1);
          vi--; wa_lngt--;
        }
      }
    }

    //LAG PREVENTING
    if(time_loan>=15) {
      time_loan-=15;
      continue;
    }

    var v1_date_now = Date.now();

    //FRAME UPDATE (50TPS by default):

    //lagger
    //while(Date.now()<v1_date_now+40) { console.log("lagger enabled"); }

      //Bullet movement && check collision
      var i, j, lngt=bulletsT.length, slngt=scrs.length;
      for(j=0;j<slngt;j++)
      {
        var q,qlngt=scrs[j].bulCols.length;
        for(q=0;q<qlngt;q++)
        {
          scrs[j].bulCols[q].SetShapeCollider(0,0,0);
          for(i=0;i<lngt;i++)
          {
            if(bulletsT[i].age != bulletsT[i].max_age && bulletsT[i].owner<0)
              if(scrs[j].bulCols[q].IsSphereColliding(bulletsT[i].pos.x + bulletsT[i].vector.x, bulletsT[i].pos.y + bulletsT[i].vector.y, other_bullets_colliders[bulletsT[i].type]))
                  destroyBullet(i, ["", bulletsT[i].owner, bulletsT[i].ID, bulletsT[i].age], true);
          }
        }
      }
      for(i=0;i<lngt;i++)
      {
        if(bulletsT[i].steerPtr!=-1)
        {
          //Seek decisions
          var nc="0";
          for(j=0;j<slngt;j++) {
            if(scrs[j].behaviour.identifier==bulletsT[i].boss_owner) break;
          }
          if(j!=slngt)
          {
            var deltapos = {x:0,y:0};
            deltapos.x = scrs[j].posCX;
            deltapos.y = scrs[j].posCY;
            visionInfo.UpdatePlayers(deltapos);

            var player_candidates = visionInfo.GetPlayers();
            var nbx = bulletsT[i].pos.x - deltapos.x;
            var nby = bulletsT[i].pos.y - deltapos.y;
            var cl,cnajm,cnajwart,curwart,clngt = player_candidates.length;
            if(clngt>0)
            {
              cnajm = 0;
              cnajwart = (nbx-player_candidates[0].x)**2 + (nby-player_candidates[0].y)**2;
              for(cl=1;cl<clngt;cl++) {
                curwart = (nbx-player_candidates[cl].x)**2 + (nby-player_candidates[cl].y)**2;
                if(curwart < cnajwart)
                {
                  cnajm = cl;
                  cnajwart = curwart;
                }
              }

              //Choosing
              var dif_x = nbx - player_candidates[cnajm].x;
              var dif_y = nby - player_candidates[cnajm].y;
              var angle = func.AngleBetweenVectorAndOX(bulletsT[i].vector.x,bulletsT[i].vector.y) - func.AngleBetweenVectorAndOX(dif_x,dif_y);
              while(angle<-180) angle+=360;
              while(angle>180) angle-=360;
              if(angle>0) nc = "L";
              else nc = "P";
            }
          }
          steer_steerData[bulletsT[i].steerPtr]+=nc;
          steer_steerSend[bulletsT[i].steerPtr]+=nc;

          //Seek update
          var c = steer_tryGet(bulletsT[i].steerPtr,bulletsT[i].age);
          if(c!='0')
          {
              var get, pak = [bulletsT[i].vector.x,bulletsT[i].vector.y];
              if(c=='L') get = func.RotatePoint(pak,2.14,true);
              if(c=='P') get = func.RotatePoint(pak,-2.14,true);
              bulletsT[i].vector.x = get[0];
              bulletsT[i].vector.y = get[1];
          }
        }

        var xv = bulletsT[i].vector.x;
        var yv = bulletsT[i].vector.y;

        var xa = bulletsT[i].pos.x;
        var ya = bulletsT[i].pos.y;
        var xb = xa + bulletsT[i].vector.x;
        var yb = ya + bulletsT[i].vector.y;
        func.CollisionLinearBulletSet(xa,ya,xb,yb,other_bullets_colliders[bulletsT[i].type]);

        if(pvp || bulletsT[i].owner<0)
        for(j=0;j<max_players;j++)
        {
          // player/boss -> player
          if(plr.players[j]!="0" && plr.players[j]!="1" && bulletsT[i].owner!=j)
          {
            var plas = plr.players[j].split(";");
            var xc = func.parseFloatU(plas[0]);
            var yc = func.parseFloatU(plas[1]);
            if(func.CollisionLinearCheck(xc,yc,0.92))
            {
              if(bullet_air_consistence[bulletsT[i].type]==0) {
                if( DamageFLOAT(j, getBulletDamage(bulletsT[i].type, bulletsT[i].owner, j, bulletsT[i]) ) != "K")
                  sendTo(se3_ws[j],"/RetDamageUsing "+bulletsT[i].type+" X "+plr.livID[j]);
                destroyBullet(i, ["", bulletsT[i].owner, bulletsT[i].ID, bulletsT[i].age], false);
                break;
              }
              else if(!bulletsT[i].damaged.includes(j)) {
                if( DamageFLOAT(j, getBulletDamage(bulletsT[i].type, bulletsT[i].owner, j, bulletsT[i]) ) != "K")
                  sendTo(se3_ws[j],"/RetDamageUsing "+bulletsT[i].type+" X "+plr.livID[j]);
                bulletsT[i].damaged.push(j);
              }
            }
          }
        }

        var lngts = scrs.length;
        if(bulletsT[i].owner>=0)
        for(j=0;j<lngts;j++)
        {
          // player -> boss
          var l = scrs[j].bID;
          if(scrs[j].dataY[2-2]==2)
          {
            var xc = scrs[j].posCX + func.ScrdToFloat(scrs[j].dataY[8-2]);
            var yc = scrs[j].posCY + func.ScrdToFloat(scrs[j].dataY[9-2]);
            if(func.CollisionLinearCheck(xc,yc,7.5))
            {
              if(scrs[j].type==6) DamageBoss(j, getBulletDamage(bulletsT[i].type, bulletsT[i].owner, -2, bulletsT[i]) );
              else if(scrs[j].type==4) DamageBoss(j, getBulletDamage(bulletsT[i].type, bulletsT[i].owner, -3, bulletsT[i]) );
              else DamageBoss(j, getBulletDamage(bulletsT[i].type, bulletsT[i].owner, -1, bulletsT[i]) );
              destroyBullet(i, ["", bulletsT[i].owner, bulletsT[i].ID, bulletsT[i].age], true);
              break;
            }
          }
        }

        bulletsT[i].age++;
        if(bulletsT[i].age>=bulletsT[i].max_age)
        {
          steer_removeSeekPointer(bulletsT[i].steerPtr);
          bulletsT.remove(i);
          lngt--; i--; continue;
        }

        bulletsT[i].pos.x += xv;
        bulletsT[i].pos.y += yv;
      }

      //CPlayer all frame updates
      for(i=0;i<max_players;i++)
      {
        if(plr.pclass[i].drill_counter!=0) {
          plr.pclass[i].drill_counter--;
          if(plr.pclass[i].drill_counter==0)
            plr.pclass[i].DrillReady(i);
        }
        var bpckArray = plr.backpack[i].split(";");
        var loc_artid = func.parseIntU(bpckArray[30]) - 41;
        if(func.parseIntU(bpckArray[31]) < 1) loc_artid = -42;
        if(plr.connectionTime[i] > 50 && plr.pclass[i].unstable_pulses_available < 5 && loc_artid==6) {
          if(func.randomInteger(0,unstable_sprobability-1)==0) {
            plr.pclass[i].unstable_pulses_available++;
            sendTo(se3_ws[i],"/RetUnstablePulse X X");
          }
        }
      }

      //Health regeneration
      for(i=0;i<max_players;i++)
      {
        if(plr.data[i]!="0" && plr.data[i]!="1" && se3_wsS[i]=="game")
        {
          var artid = plr.backpack[i].split(";")[30] - 41;
          if(plr.backpack[i].split(";")[31]==0) artid = -41;

          var sth1 = plr.sHealth[i];

          if(plr.sHealth[i]<1 && plr.sRegTimer[i]==0)
		      {
			      var potHH = func.parseFloatU(plr.upgrades[i].split(";")[0]) + getProtLevelAdd(artid) + func.parseFloatU(gameplay[26]);
			      if(potHH<-50) potHH = -50; if(potHH>56.397) potHH = 56.397;
            var true_add = unit * getProtRegenMultiplier(artid) * func.parseFloatU(gameplay[5]) / (Math.ceil(50*Math.pow(health_base,potHH))/50);
			      if(true_add>0) plr.sHealth[i] += true_add;
		      }
          if(plr.sHealth[i]>1) plr.sHealth[i]=1;
          var sth2 = plr.sHealth[i];

          var del = ((sth2-sth1)+"").replaceAll(".",",");
          var abl = ((sth2)+"").replaceAll(".",",");
          sendTo(se3_ws[i],"R "+del+" "+plr.immID[i]+" "+plr.livID[i]+" "+abl); //Medium type message
          
          if(plr.sRegTimer[i]>0)
	        {
      			if(artid!=1) plr.sRegTimer[i]--;
			      else plr.sRegTimer[i]-=2;
			      if(plr.sRegTimer[i]<0) plr.sRegTimer[i]=0;
		      }
        }
      }

      //[Scrs: Multiplayer boss mechanics update]
      lngt = scrs.length;
      for(i=0;i<lngt;i++)
      {
        var sta = scrs[i].dataY[2-2]+"";
        scrs[i].dataY[3-2]++;
        if(sta=="2")
        {
          scrs[i].dataY[4-2]--;
          var deltapos = {x:0,y:0};
          deltapos.x = scrs[i].posCX;
          deltapos.y = scrs[i].posCY;
          visionInfo.UpdatePlayers(deltapos);
          scrs[i].behaviour.FixedUpdate();
        }
        if((sta=="1" || sta=="3" || sta=="4") && scrs[i].dataY[3-2]>=50)
        {
          if(sta=="1") scrs[i].dataY[2-2] = 2;
          else if(sta=="3" || sta=="4") scrs[i].dataY[2-2] = 0;
          resetScr(i);
        }
        else if(sta=="2" && scrs[i].dataY[4-2]<=0)
        {
          scrs[i].dataY[2-2] = 3;
          resetScr(i);
        }
        else if(sta=="2" && scrs[i].dataY[6-2]<=0)
        {
          scrs[i].dataY[2-2] = 4;
          resetScr(i);
        }
      }

      //Connection time
      for(i=0;i<max_players;i++)
      {
        if(plr.connectionTime[i]>=0)
          plr.connectionTime[i]++;
      }

    if((date_before-date_start) % 100 == 0) //precisely 10 times per second
    {
      //[Grow]
       var i,
       lngt = growT.length;
      for (i = 0; i < lngt; i++) {
        growW[i]--;
        if (growW[i] > 0) {
         var ulam = growT[i].split("g")[0];
          var place = growT[i].split("g")[1];
         var det = asteroidIndex(ulam);
          var tume = chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)];
          tume -= 5;

         if (tume > 0) {
            chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] = tume;
          } else {
           serverGrow(ulam, place);
           growT.remove(i);
            growW.remove(i);
            lngt--;
            i--;
          }
        } else {
          growT.remove(i);
          growW.remove(i);
         lngt--;
         i--;
       }
      }

      //[Driller]
      lngt = drillT.length;
      for (i = 0; i < lngt; i++) {
       drillW[i]--;
        if (drillW[i] > 0) {
           drillC[i] -= 5;
        if (drillC[i] <= 0) {
           var ulam = drillT[i].split("w")[0];
           var place = drillT[i].split("w")[1];
           serverDrill(ulam, place);
           drillC[i] = func.randomInteger(180, 420); //it is two times in a code
         }
        } else {
          drillT.remove(i);
          drillW.remove(i);
          drillC.remove(i);
          lngt--;
          i--;
        }
      }

      //[Chunks]
      lngt = chunk_waiter.length;
      for (i = 0; i < lngt; i++) {
        if (chunk_waiter[i] > 0) chunk_waiter[i]--;
      }

      //[Scrs: Server counters actions]
      lngt = scrs.length;
      for(i=0;i<lngt;i++) {
        scrs[i].timeToDisappear-=5;
        scrs[i].timeToLose-=5;
        if(scrs[i].timeToDisappear<=0)
        {
          scrs.remove(i);
          lngt--; i--;
          continue;
        }
        if(scrs[i].timeToLose<=0 && scrs[i].dataY[2-2]==2)
        {
          scrs[i].dataY[2-2] = 3;
          resetScr(i);
        }
      }

      //Steer cooldown
      for(i=0;i<1024;i++) {
        if(steer_cooldown[i]==5) steer_bulletID[i]=-1;
        if(steer_cooldown[i]>0) steer_cooldown[i]-=5;
      }
    }

    var v2_date_now = Date.now();
    var date_dif = v2_date_now - v1_date_now;
    if(date_dif>15) time_loan += date_dif-15;

    current_tick++;
    size_tps++;
  }
}, 5);
function setTerminalTitle(title)
{
  process.stdout.write(
    String.fromCharCode(27) + "]0;" + title + String.fromCharCode(7)
  );
}
function updateHourHeader()
{
  var date_ob = new Date();
  var years = date_ob.getYear()+1900;
  var months = ToTwo(date_ob.getMonth()+1);
  var days = ToTwo(date_ob.getDate());
  var hours = ToTwo(date_ob.getHours());
  var minutes = ToTwo(date_ob.getMinutes());
  var seconds = ToTwo(date_ob.getSeconds());
  hourHeader = `[${years}-${months}-${days} ${hours}:${minutes}:${seconds}] `;
}
function ToTwo(n) {
  if(n>9) return n+"";
  else return "0"+n;
}
function getTimeSize(n)
{
  return Math.floor(GplGet("boss_battle_time") * 50);
}
function getBossHealth(n,typ)
{
  var ret = 10000;
  if(typ==1) {
      if(n==0) ret = Math.floor(GplGet("boss_hp_protector_1") * 100);
      if(n==1) ret = Math.floor(GplGet("boss_hp_protector_2") * 100);
      if(n==2) ret = Math.floor(GplGet("boss_hp_protector_3") * 100);
  }
  if(typ==2) {
      if(n==0) ret = Math.floor(GplGet("boss_hp_adecodron_1") * 100);
      if(n==1) ret = Math.floor(GplGet("boss_hp_adecodron_2") * 100);
      if(n==2) ret = Math.floor(GplGet("boss_hp_adecodron_3") * 100);
  }
  if(typ==3) {
      if(n==0) ret = Math.floor(GplGet("boss_hp_octogone_1") * 100);
      if(n==1) ret = Math.floor(GplGet("boss_hp_octogone_2") * 100);
      if(n==2) ret = Math.floor(GplGet("boss_hp_octogone_3") * 100);
  }
  if(typ==4) {
      if(n==0) ret = Math.floor(GplGet("boss_hp_starandus_1") * 100);
      if(n==1) ret = Math.floor(GplGet("boss_hp_starandus_2") * 100);
      if(n==2) ret = Math.floor(GplGet("boss_hp_starandus_3") * 100);
  }
  if(typ==6) {
      if(n==0) ret = Math.floor(GplGet("boss_hp_degenerator_1") * 100);
      if(n==1) ret = Math.floor(GplGet("boss_hp_degenerator_2") * 100);
      if(n==2) ret = Math.floor(GplGet("boss_hp_degenerator_3") * 100);
  }
  if(ret<=0) return 1;
  else return ret;
}
function resetScr(i)
{
  var mem6 = scrs[i].dataY[6-2];
  var mem7 = scrs[i].dataY[7-2];
  var mem8 = scrs[i].dataY[8-2];
  var mem9 = scrs[i].dataY[9-2];
  var mem10 = scrs[i].dataY[10-2];

  var j;
  var sta = scrs[i].dataY[2-2]+"";
  if(sta=="3"||sta=="4") scrs[i].behaviour.End();
  for(j=3;j<=60;j++) scrs[i].dataY[j-2] = 0;

  scrs[i].timeToLose = 1000;

  if(sta=="0")
  {
    
  }
  else if(sta=="1")
  {
    scrs[i].dataY[5-2] = getTimeSize(scrs[i].dataX[1]); //Max time
    scrs[i].dataY[4-2] = scrs[i].dataY[5-2]; //Time left
    scrs[i].dataY[7-2] = getBossHealth(scrs[i].dataX[1],scrs[i].type); //Max health
    scrs[i].dataY[6-2] = scrs[i].dataY[7-2]; //Health left
  }
  else if(sta=="2")
  {
    scrs[i].dataY[5-2] = getTimeSize(scrs[i].dataX[1]); //Max time
    scrs[i].dataY[4-2] = scrs[i].dataY[5-2]; //Time left
    scrs[i].dataY[7-2] = mem7; //Max health
    scrs[i].dataY[6-2] = scrs[i].dataY[7-2]; //Health left
    scrs[i].behaviour.Start();
  }
  else if(sta=="3")
  {
    scrs[i].dataY[7-2] = mem7; //Max health
    scrs[i].dataY[6-2] = mem6; //Health left
    scrs[i].dataY[8-2] = mem8; scrs[i].dataY[9-2] = mem9; scrs[i].dataY[10-2] = mem10; //Position & Rotation
  }
  else if(sta=="4")
  {
    scrs[i].dataY[7-2] = mem7; //Max health
    scrs[i].dataY[8-2] = mem8; scrs[i].dataY[9-2] = mem9; scrs[i].dataY[10-2] = mem10; //Position & Rotation

    //Add 1 to wave number
    scrs[i].dataX[1]++;
    var det = asteroidIndex(scrs[i].bID);
    chunk_data[det[0]][det[1]][1] = scrs[i].dataX[1]+"";
    sendToAllPlayers(
      "/RetEmitParticles -1 10 "+
      ((func.ScrdToFloat(mem8)+scrs[i].posCX)+"").replaceAll(".",",")+" "+
      ((func.ScrdToFloat(mem9)+scrs[i].posCY)+"").replaceAll(".",",")+
      " X X"
    );
  }
}
function DamageBoss(i,dmg)
{
  if(scrs[i].dataY[2-2]!=2 || ((scrs[i].behaviour.type-1)==0 && scrs[i].dataY[18-2]==2)) return;
  var actualHealth = scrs[i].dataY[6-2]/100;
  actualHealth -= dmg;
  scrs[i].dataY[6-2] = Math.floor(actualHealth*100);
  if(dmg!=0) sendToAllPlayers(
    "/RetEmitParticles -1 9 "+
    ((func.ScrdToFloat(scrs[i].dataY[8-2])+scrs[i].posCX)+"").replaceAll(".",",")+" "+
    ((func.ScrdToFloat(scrs[i].dataY[9-2])+scrs[i].posCY)+"").replaceAll(".",",")+
    " X X"
  );
}

//RetPlayerUpdate (25 times per second by default)
setInterval(function () {  // <interval #2>

  size_updates++;
  var eff,lngt = plr.players.length;

  //RPC - Static data
  var gtt = GetRPC(plr.players,lngt,false);
  if(gtt != "DONT SEND") //space important here
  {
    eff = "/RPC " + max_players + " " + gtt + " X X";
    sendToAllPlayers(eff);
  }

  //RPU - Dynamic data
  if(show_positions) eff = "/RPU " + max_players + " ";
  else eff = ".RPU " + max_players + " ";
  eff += GetRPU(plr.players,lngt) + " ";
  eff += current_tick;
  eff += " X X"
  sendToAllPlayers(eff);

  //BRP - Bullet Rotation Packet
  var gbrp = GetBRP();
  eff = "/BRP " + gbrp + " X X";
  if(gbrp!="") sendToAllPlayers(eff);

  var i,j;
  for(i=0;i<scrs.length;i++)
  {
    var lc3 = scrs[i].dataX.join(";") + ";" + scrs[i].dataY.join(";");
    var lc4 = GetRSD(lc3);
    
    //RSD - Boss data
    for(j=0;j<max_players;j++)
    {
      if(plr.bossMemories[j].includes(scrs[i].bID+""))
        sendToAllPlayers(
          "/RSD " +
          scrs[i].bID +
          " " +
          lc4 +
          " 1024 X X"
      );
    }
  }

  for(i=0;i<max_players;i++)
  {
    if(se3_wsS[i]=="game")
      sendTo(se3_ws[i],"I "+plr.immID[i]+" "+plr.livID[i]+" X X"); //medium type message
  }

}, 40);

//Waiter kicker (50 times per second by default)
setInterval(function () { //<interval #3>
  var i;
  for (i = 0; i < max_players; i++) {
    if (plr.waiter[i] > 0) {
      plr.waiter[i]--;
      if (plr.waiter[i] == 0) kick(i);
    }
  }
}, 20);

//Kick functions
function kick(i) {
  SaveAllNow();
  console.log(hourHeader + plr.nicks[i] + " disconnected");
  var pom = se3_ws[i];
  se3_ws[i] = "";
  se3_wsS[i] = "";
  try{
    sendTo(pom,"/RetAllowConnection -6 X X");
    pom.close();
  }catch{}
  if (plr.players[i] != "0")
    sendToAllPlayers(
      "/RetInfoClient " +
        (plr.nicks[i] + " left the game").replaceAll(" ", "`") +
        " " +
        i +
        " X X"
    );
  plr.waiter[i] = 0;
  plr.nicks[i] = "0";
  plr.players[i] = "0";
  plr.data[i] = "0";
  plr.conID[i] = "0";
  plr.livID[i] = "0";
  plr.immID[i] = "0";
  plr.connectionTime[i] = -1;
  plr.sHealth[i] = 0;
  plr.sRegTimer[i] = -1;
  plr.upgrades[i] = "0";
  plr.backpack[i] = "0";
  plr.inventory[i] = "0";
  plr.pushInventory[i] = "0";
  plr.impulsed[i] = [];
  plr.bossMemories[i] = [];
}

//Bullet functions
function MaxAgeOfPlayerBullet(n)
{
  var ret = 0;
  if(n==1) ret = func.parseFloatU(gameplay[45]);
  if(n==2) ret = func.parseFloatU(gameplay[46]);
  if(n==3) ret = func.parseFloatU(gameplay[47]);
  if(n==14) ret = func.parseFloatU(gameplay[49]);
  if(n==15) ret = func.parseFloatU(gameplay[48]);
  if(ret==0) ret = 0.001;
        
  var ret2 = 100;
  if(n==1) ret2 = Math.floor(35*func.parseFloatU(gameplay[92])/ret) + 1;
  if(n==2) ret2 = Math.floor(35*func.parseFloatU(gameplay[93])/ret) + 1;
  if(n==3) ret2 = Math.floor(35*func.parseFloatU(gameplay[96])/ret) + 1;
  if(n==14) ret2 = Math.floor(35*func.parseFloatU(gameplay[94])/ret) + 1;
  if(n==15) ret2 = Math.floor(35*func.parseFloatU(gameplay[95])/ret) + 1;

  if(ret2>=10000) return 10000;
  else if(ret2<=1) return 1;
  else return ret2;
}
function spawnBullet(tpl,arg,bow)
{
  if(tpl.type==1 || tpl.type==2 || tpl.type==3 || tpl.type==14 || tpl.type==15)
    tpl.max_age = MaxAgeOfPlayerBullet(tpl.type);

  if(tpl.type==9 || tpl.type==10)
  {
    tpl.steerPtr = steer_createSeekPointer(tpl.ID);
    if(tpl.steerPtr!=-1)
    {
      tpl.boss_owner = bow;
      tpl.max_age = 300;
      sendToAllPlayers("/RetSeekData "+tpl.steerPtr+" "+tpl.ID+" X X");
    }
  }
  bulletsT.push(tpl);
  sendToAllPlayers(
    "/RetNewBulletSend " +
      arg[1]+" " +
      arg[2]+" " +
      arg[3]+" " +
      arg[4]+" " +
      arg[5]+" " +
      arg[6]+" " +
      arg[7]+" " +
      arg[8]+" " +
      " X X"
  );
}
function destroyBullet(n,arg,noise)
{
  if(func.parseIntU(arg[3]) <= bulletsT[n].max_age) bulletsT[n].max_age = func.parseIntU(arg[3]);
  sendToAllPlayers(
    "/RetNewBulletDestroy "+
    arg[1]+" "+
    arg[2]+" "+
    arg[3]+" "+
    noise +" "+
    " X X"
  );
}

//Seek data functions
var steer_bulletID = [];
var steer_steerData = [];
var steer_steerSend = [];
var steer_cooldown = [];
var steer_it;
for(steer_it=0;steer_it<1024;steer_it++)
{
  steer_bulletID[steer_it] = -1;
  steer_steerData[steer_it] = "";
  steer_steerSend[steer_it] = "";
  steer_cooldown[steer_it] = 0;
}

function steer_createSeekPointer(bulID)
{
  var i;
  for(i=0;i<1024;i++)
  {
    if(steer_bulletID[i]==-1 && steer_cooldown[i]<=0)
    {
      steer_bulletID[i] = bulID;
      steer_steerData[i] = "0000";
      steer_steerSend[steer_it] = "";
      return i;
    }
  }
  return -1;
}

function steer_removeSeekPointer(pID)
{
    steer_cooldown[pID] = 1500;
}

function steer_tryGet(pID,ind)
{
    if(steer_steerData[pID].length>ind) return steer_steerData[pID][ind];
    return '0';
}

//Check functions
function checkPlayerM(n,ws) {
  return (se3_wsS[n]=="menu" && checkPlayer(n,ws));
}
function checkPlayerJ(n,cn) {
  return (se3_wsS[n]=="joining" && checkPlayerCn(n,cn));
}
function checkPlayerG(n,ws) {
  return (se3_wsS[n]=="game" && checkPlayer(n,ws));
}
function checkPlayer(n,ws) {
  return (plr.nicks[n]!="0" && se3_ws[n]==ws);
}
function checkPlayerCn(idm, cn) {
  return (plr.nicks[idm]!="0" && plr.conID[idm]==cn);
}

function intToRASCII(int)
{
  if(int>=0 && int<=30) return String.fromCharCode(int+1);
  if(int>=31 && int<=123) return String.fromCharCode(int+4);
  return String.fromCharCode(1);
}
function insertFloatToChar4(str,delta,float)
{
  var i,lngt=str.length;

  var minus = (float<0);
  if(minus) float = -float;
  
  float *= 124;
  float = Math.round(float);

  var bs = [1906624,15376,124,1];
  for(i=0;i<4;i++)
  {
    var ii = lngt+delta+i;
    var num = Math.floor(float/bs[i]);
    float = float % bs[i];

    if(i==0)
    {
      num = num%62;
      if(minus) num+=62;
    }
    str = str.replaceAt(ii,intToRASCII(num));
  }

  return str;
}
function insertIntToChar3(str,delta,int)
{
  var i,lngt=str.length;

  var minus = (int<0);
  if(minus) int = -int;

  var bs = [15376,124,1];
  for(i=0;i<3;i++)
  {
    var ii = lngt+delta+i;
    var num = Math.floor(int/bs[i]);
    int = int % bs[i];

    if(i==0)
    {
      num = num%62;
      if(minus) num+=62;
    }
    str = str.replaceAt(ii,intToRASCII(num));
  }

  return str;
}
function insertRotToChar1(str,delta,rot)
{
  var lngt=str.length;
  rot = 

  rot /= 360;
  rot *= 124;
  rot = Math.round(rot);
  if(rot==124) rot=0;
  if(rot<0) rot+=124;
  str = str.replaceAt(lngt+delta,intToRASCII(rot));

  return str;
}

function insertOthersToChar3(str,delta,others)
{
  var lngt=str.length;
  var spt = others.split("&");
  
  str = str.replaceAt(lngt+delta+0,intToRASCII(spt[0] * 1));
  str = str.replaceAt(lngt+delta+1,intToRASCII(spt[1] / 124));
  str = str.replaceAt(lngt+delta+2,intToRASCII(spt[1] % 124));

  return str;
}

function insertHealthToChar1(str,delta,health)
{
  var lngt=str.length;
  
  health *= 123;
  health = Math.ceil(health);
  str = str.replaceAt(lngt+delta,intToRASCII(health));

  return str;
}

function GetRPU(players,lngt)
{
  var i,splitted,eff="";
  for(i=0;i<lngt;i++)
  {
    if(players[i]=="0") eff+="!";
    else if(players[i]=="1") eff+="\"";
    else
    {
      splitted = players[i].split(";");
      eff+="XXXXXXXXX";
      eff = insertFloatToChar4(eff,-9,func.parseFloatU(splitted[0]));
      eff = insertFloatToChar4(eff,-5,func.parseFloatU(splitted[1]));
      eff = insertRotToChar1(eff,-1,func.parseFloatU(splitted[4]));
    }
  }

  return eff;
}

function GetBRP()
{
  var i,j,kngt,eff="";
  for(i=0;i<1024;i++)
  {
    if(steer_steerSend[i]!="")
    {
      var awt1 = [0,0,0,0,0,0];
      var awt2 = [0,0,0,0,0,0];
      var i_t = i;
      for(j=3;j>=0;j--) {
        awt2[j] = i_t % 2;
        i_t=Math.floor(i_t/2);
      }
      for(j=5;j>=0;j--) {
        awt1[j] = i_t % 2;
        i_t=Math.floor(i_t/2);
      }

      kngt = steer_steerSend[i].length;
      for(j=0;j<kngt;j++)
      {
        var awn1 = awt1;
        var awn2 = awt2;
        if(steer_steerSend[i][j]=='L') {awn2[4] = 0; awn2[5] = 1;}
        if(steer_steerSend[i][j]=='P') {awn2[4] = 1; awn2[5] = 0;}
        if(steer_steerSend[i][j]=='0') {awn2[4] = 1; awn2[5] = 1;}
        eff += bool6ToChar1(awn1) + bool6ToChar1(awn2);
      }
      steer_steerSend[i] = "";
    }
  }
  return eff;
}

function bool6ToChar1(array_what)
{
  var i,j=32,eff = 0;
  for(i=0;i<6;i++)
  {
    eff += j*array_what[i];
    j/=2;
  }
  return intToRASCII(eff);
}

function GetRootByte(players,n)
{
  // [0] -> if have sth
  // [1] -> array what
  // [2] -> root byte

  // i0 - others
  // i1 - health
  // i2 - resp pos
  // i3 - nick

  var ret = ["","",""];
  var array_what = [0,0,0,0,0,0];
  var splitted;

  if(players[n]=="0" || players[n]=="1") splitted = [0,0,0,0,0,"0&0","0","0","1"];
  else splitted = players[n].split(";");

  if(splitted[5] == (plr.mems[n].others1 + "&" + plr.mems[n].others2)) array_what[0] = 0;
  else array_what[0] = 1;

  if(splitted[8] == (plr.mems[n].health)) array_what[1] = 0;
  else array_what[1] = 1;

  if(splitted[6] == (plr.mems[n].rposX) &&
     splitted[7] == (plr.mems[n].rposY) ) array_what[2] = 0;
  else array_what[2] = 1;

  if(plr.nicks[n] == (plr.mems[n].nicks)) array_what[3] = 0;
  else array_what[3] = 1;

  ret[0] = (!(array_what[0]==0 && array_what[1]==0 && array_what[2]==0 && array_what[3]==0 && array_what[4]==0 && array_what[5]==0))
  ret[1] = array_what;
  ret[2] = bool6ToChar1(array_what);

  return ret;
}

function GetRPC(players,lngt,sendAll)
{
  var eff = "", not_empty = false;
  var splitted;

  for(i=0;i<lngt;i++)
  {
    if(players[i]=="0" || players[i]=="1") splitted = [0,0,0,0,0,"0&0","0","0","1"];
    else splitted = players[i].split(";");

    var rbt;
    if(!sendAll) rbt = GetRootByte(players,i);
    else rbt = [true, [1,1,1,1,1,1], bool6ToChar1([1,1,1,1,1,1])];

    if(rbt[0])
    {
      not_empty = true;

      eff += rbt[2];

      if(rbt[1][0]==1) //others
      {
        eff += "XXX";
        eff = insertOthersToChar3(eff,-3,splitted[5]);
        if(!sendAll) plr.mems[i].others1 = splitted[5].split("&")[0];
        if(!sendAll) plr.mems[i].others2 = splitted[5].split("&")[1];
      }
      if(rbt[1][1]==1) //health
      {
        eff += "X";
        eff = insertHealthToChar1(eff,-1,func.parseFloatU(splitted[8]));
        if(!sendAll) plr.mems[i].health = splitted[8];
      }
      if(rbt[1][2]==1) //resp pos
      {
        eff += "XXXXXXXX";
        eff = insertFloatToChar4(eff,-8,func.parseFloatU(splitted[6]));
        eff = insertFloatToChar4(eff,-4,func.parseFloatU(splitted[7]));
        if(!sendAll) plr.mems[i].rposX = splitted[6];
        if(!sendAll) plr.mems[i].rposY = splitted[7];
      }
      if(rbt[1][3]==1) //nick
      {
        eff += plr.nicks[i].replaceAll(" ","|"); eff += ":";
        if(!sendAll) plr.mems[i].nicks = plr.nicks[i];
      }
    }
    else eff += "!";
  }

  //Reducer of !!!
  lngt = eff.length;
  var in1,in2;
  for(i=0;i<lngt;i++)
  {
    var amt = 0;
    while(eff[i]=="!")
    {
      amt++;
      eff = eff.replaceAt(i," ");
      eff = eff.replaceAll(" ","");
      lngt--;
    }

    if(amt==1)
    {
      eff = eff.insertAt(i,"!");
      i++;
      lngt++;
    }
    else if(amt==2)
    {
      eff = eff.insertAt(i,"!!");
      lngt+=2;
      i+=2;
    }
    else if(amt>=3)
    {
      in1 = amt/124;
      in2 = amt%124;
      eff = eff.insertAt(i, "\"" + intToRASCII(in1) + intToRASCII(in2) );
      lngt+=3;
      i+=3;
    }
  }

  if(!not_empty) return "DONT SEND";
  else return eff;
}
function GetRSD(str)
{
  var i,eff="";
  str = str.split(";");
  for(i=0;i<=60;i++)
  {
    eff+="XXX";
    eff = insertIntToChar3(eff,-3,func.parseFloatU(str[i]));
  }
  return eff;
}

function serverGrow(ulam, place) {
  var det = asteroidIndex(ulam);
  if (chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] != "") {
    var bef = chunk_data[det[0]][det[1]][func.parseIntU(place) + 1];
    if (!["5", "6", "25"].includes(bef)) return;
    chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] = "";
    chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)] = "";
    chunk_data[det[0]][det[1]][func.parseIntU(place) + 1] =
      growSolid[bef].split(";")[2];
    sendToAllPlayers("/RetGrowNow " + ulam + " " + place + " X X");
  }
}
function serverDrill(ulam, place) {
  var det = asteroidIndex(ulam);
  if (chunk_data[det[0]][det[1]][func.parseIntU(place) + 1] == "2") {
    var gItem = drillGet(det,chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)]);
    if (gItem == "0") return;
    var gCountEnd = func.parseIntU(
      chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)]
    );
    gCountEnd++;

    chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] = gItem;
    chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)] = gCountEnd;

    sendToAllPlayers(
      "/RetFobsDataChange " +
        ulam +
        " " +
        place +
        " " +
        gItem +
        " 1 -1 " +
        gCountEnd +
        " 2 X X"
    );
  }
}
function handDrillTimeGet(pid) {
  var upg3hugity = 1.12;
  var upg3down = 90, upg3up = 210;
  var matpow = upg3hugity ** (func.parseFloatU(plr.upgrades[pid].split(";")[2])+func.parseFloatU(gameplay[2]));
	down = Math.round(upg3down/matpow);
	up = Math.round(upg3up/matpow);
	return func.randomInteger(down,up);
}
function drillItemGet(ast,stackedItem) {
  var ltdt = drillLoot[ast].split(";");
  var lngt = ltdt.length;
  var rnd = func.randomInteger(0, 999);
  var i;
  for (i=0;i*3+2<lngt;i++) {
    if (rnd >= ltdt[i*3+1] && rnd <= ltdt[i*3+2]) {
      if(stackedItem==0 || stackedItem=="" || stackedItem==ltdt[i*3]) return ltdt[i*3];
      else return 0;
    }
  }
  return 0;
}
function drillGet(det,stackedItem) {
  var typp = func.parseIntU(chunk_data[det[0]][det[1]][0]+"");
  if(typp<0) typp=0;
  else typp = typp % 16;
  return drillItemGet(typp,stackedItem);
}
function growActive(ulam) {
  var i, block, tim, ind;
  var tab = [];
  var blockTab = [];

  var det = asteroidIndex(ulam);
  for (i = 0; i < 20; i++) blockTab[i] = chunk_data[det[0]][det[1]][i + 1];

  var typpu = func.parseIntU(chunk_data[det[0]][det[1]][0]+"");
  if(typpu<0) typpu = 0;
  else typpu = typpu % 16;

  for (i = 0; i < 20; i++) {
    block = blockTab[i];
    if (
      ["25"].includes(block) ||
      (["5", "6"].includes(block) && typpu == 6)
    ) {
      if (!growT.includes(ulam + "g" + i)) {
        if (chunk_data[det[0]][det[1]][21 + 2 * i] == "") {
          tab = growSolid[block].split(";");
          tim = func.randomInteger(tab[0], tab[1]);
          chunk_data[det[0]][det[1]][21 + 2 * i] = tim;
        }
        growT.push(ulam + "g" + i);
        growW.push(100);
      }
      ind = growT.indexOf(ulam + "g" + i);
      growW[ind] = 100;
    }
    if (
      ["2"].includes(block) &&
      (chunk_data[det[0]][det[1]][22 + 2 * i] == "" ||
        chunk_data[det[0]][det[1]][22 + 2 * i] < 5)
    ) {
      if (!drillT.includes(ulam + "w" + i)) {
        tim = func.randomInteger(180, 420); //it is two times in a code
        drillT.push(ulam + "w" + i);
        drillW.push(100);
        drillC.push(tim);
      }
      ind = drillT.indexOf(ulam + "w" + i);
      drillW[ind] = 100;
    }
  }
}
function nbtReset(ulam, place) {
  var ind,
    det = asteroidIndex(ulam);
  chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] = "";
  chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)] = "";

  if (growT.includes(ulam + "g" + place)) {
    ind = growT.indexOf(ulam + "g" + place);
    growT.remove(ind);
    growW.remove(ind);
  }

  if (drillT.includes(ulam + "g" + place)) {
    ind = drillT.indexOf(ulam + "g" + place);
    drillT.remove(ind);
    drillW.remove(ind);
    drillC.remove(ind);
  }
}

//Inventory functions
function invChangeTry(invID, item, count, slot) {
  if (slot == -1) return true;
  var itemS, countS, effTab, mode;

  if(slot>=9) {
    var bpkUpgrade = func.parseFloatU(plr.upgrades[invID].split(";")[4]);
    if(Math.floor((slot-9)/3)+1 > bpkUpgrade && !(slot-9>=15 && slot-9<=16)) return false;
  }

  if (slot < 9) {
    effTab = plr.inventory[invID].split(";");
    itemS = effTab[slot * 2];
    countS = effTab[slot * 2 + 1];
    mode = "INV";
  } else {
    slot -= 9;
    effTab = plr.backpack[invID].split(";");
    itemS = effTab[slot * 2];
    countS = effTab[slot * 2 + 1];
    mode = "BPK";
  }

  if (count > 0) {
    //Add
    if (!(itemS == item || countS == 0)) return false;
  } else if (count < 0) {
    //Remove
    if (!(itemS == item && func.parseIntU(countS) + func.parseIntU(count) >= 0))
      return false;
  } else return true;

  effTab[slot * 2] = item;
  effTab[slot * 2 + 1] = func.parseIntU(effTab[slot * 2 + 1]) + func.parseIntU(count);

  if (mode == "INV") plr.inventory[invID] = effTab.join(";");
  else plr.backpack[invID] = effTab.join(";");

  return true;
}

//Fobs change functions
function checkFobChange(ulam, place, start1, start2) {
  var det = asteroidIndex(ulam);
  if(chunk_data[det[0]][det[1]][0]=="1024") return false;

  if (
    chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)] != "" &&
    (start1 == 21 || start2 == 21) //2 not required, driller item might disappear
  ) return false;

  if (
    chunk_data[det[0]][det[1]][func.parseIntU(place) + 1] == start1 ||
    chunk_data[det[0]][det[1]][func.parseIntU(place) + 1] == start2
  ) return true;
  else return false;
}
function fobChange(ulam, place, end) {
  var det = asteroidIndex(ulam);
  chunk_data[det[0]][det[1]][func.parseIntU(place) + 1] = end;
  nbtReset(ulam, place);
  growActive(ulam);
}

//Fob21 change functions
function checkFobDataChange(ulam, place, item, deltaCount, id21) {
  var det = asteroidIndex(ulam);
  if (chunk_data[det[0]][det[1]][func.parseIntU(place) + 1] != id21) {
    return false;
  }

  var max_count = -1;
  if (id21 == 21) max_count = 35;
  if (id21 == 2) max_count = 5;
  if (id21 == 52) max_count = 10;

  if (
    chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] == "" ||
    chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] == 0 ||
    chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] == item
  ) {
    var countEnd = func.parseIntU(
      chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)]
    );
    countEnd += func.parseIntU(deltaCount);
    if (countEnd >= 0 && countEnd <= max_count) return true;
    else {
      return false;
    }
  } else {
    return false;
  }
}
function fobDataChange(ulam, place, item, deltaCount) {
  var det = asteroidIndex(ulam);
  var countEnd = func.parseIntU(chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)]);
  countEnd += func.parseIntU(deltaCount);

  if (countEnd != 0) {
    chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] = item;
    chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)] = countEnd;
  } else {
    chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] = "";
    chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)] = "";
  }
  return countEnd;
}

//Fobs data functions
function nbts(ulam) {
  var i,
    det = asteroidIndex(ulam),
    tabR = [],
    newR;
  for (i = 21; i < 61; i += 2) {
    if (
      chunk_data[det[0]][det[1]][i] != "" &&
      chunk_data[det[0]][det[1]][i + 1] != ""
    )
      tabR.push(
        chunk_data[det[0]][det[1]][i] + ";" + chunk_data[det[0]][det[1]][i + 1]
      );
    else tabR.push("0;0");
  }
  newR = tabR.join(" ");
  return newR;
}
function nbt(ulam, place, lt, nw) {
  var det = asteroidIndex(ulam);
  if (lt == "n") {
    if (
      chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] != "" &&
      chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)] != ""
    )
      return (
        chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] +
        ";" +
        chunk_data[det[0]][det[1]][22 + 2 * func.parseIntU(place)]
      );
    else return nw;
  }
  if (lt == "g") {
    if (chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)] != "")
      return chunk_data[det[0]][det[1]][21 + 2 * func.parseIntU(place)];
    else return nw;
  }
}
function getBlockAt(ulam, place) {
  var det = asteroidIndex(ulam);
  var eff = chunk_data[det[0]][det[1]][func.parseIntU(place) + 1];
  if (eff != "") return eff;
  else return -1;
}

//ScrShapeAdd
function ScrShapeAdd(arg)
{
  var bID = arg[2];
  var lngts = scrs.length;
  for(i=0;i<lngts;i++)
  {
    if(scrs[i].bID==bID)
    {
      var q,qlngt=scrs[i].bulCols.length;
      for(q=0;q<qlngt;q++)
        if(scrs[i].bulCols[q].col_identifier==arg[9]) return;

      shape_description = {sx:0,sy:0};
      default_offset = {cx:0,cy:0,lx:0,ly:0,lrot:0};
          
      shape_description.sx = func.parseFloatU(arg[4]);
      shape_description.sy = func.parseFloatU(arg[5]);
      default_offset.cx = scrs[i].posCX;
      default_offset.cy = scrs[i].posCY;
      default_offset.lx = func.parseFloatU(arg[6]);
      default_offset.ly = func.parseFloatU(arg[7]);
      default_offset.lrot = func.parseFloatU(arg[8]);

      scrs[i].bulCols.push(new CShape(
        arg[3],
        shape_description,
        default_offset,
        arg[9]
      ));
    }
  }
}

//Death functions
function kill(pid)
{
  plr.livID[pid]++;

  plr.data[pid] =
  plr.data[pid].split(";")[6] + ";" +
  plr.data[pid].split(";")[7] + ";0;0;0;0;" +
  plr.data[pid].split(";")[6] + ";" +
  plr.data[pid].split(";")[7] + ";1;0;0;0";
  plr.inventory[pid] = "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
  plr.upgrades[pid] = "0;0;0;0;0";
  plr.backpack[pid] = "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";

  plr.sHealth[pid] = 1;
  plr.sRegTimer[pid] = 0;

  plr.pclass[pid].force_teleport_respawn = true;
  plr.pclass[pid].last_pos_changes = [];

  if(plr.players[pid]=="0" || plr.players[pid]=="1") return;

  var xx = plr.players[pid].split(";")[0];
  var yy = plr.players[pid].split(";")[1];
  sendToAllPlayers("/RetEmitParticles "+pid+" 1 "+xx+" "+yy+" X X");
}
function immortal(pid)
{
  plr.immID[pid]++;

  plr.sHealth[pid] = 1;
  plr.sRegTimer[pid] = 0;

  var bpck = plr.backpack[pid].split(";");
  bpck[31] -= 1;
  if(bpck[31] == 0)
  {
    bpck[30] = 41;
    bpck[31] += 1;
  }
  plr.backpack[pid] = bpck.join(";");

  if(plr.players[pid]=="0" || plr.players[pid]=="1") return;

  var xx = plr.players[pid].split(";")[0];
  var yy = plr.players[pid].split(";")[1];
  sendToAllPlayers("/RetEmitParticles "+pid+" 5 "+xx+" "+yy+" X X");
}

//Asteroid functions
function sazeConvert(saze) {
  return func.parseIntU(saze.split("b")[1]) * 7 + func.parseIntU(saze.split("b")[0]) - 4;
}
function generateAsteroid(saze) {
  var typeDatas;

  if (saze.split("").includes("b")) {
    typeDatas = typeSet[sazeConvert(saze)];
    if (typeDatas != "") typeDatas = typeDatas.split(";");
    else typeDatas = "0;0;999".split(";");
  } else if (saze.split("").includes("t")) {
    typeDatas = (saze.split("t")[1] + ";0;999").split(";");
  } else typeDatas = "0;0;999".split(";");

  var rand = func.randomInteger(0, 999);
  var i = 0,
    j,
    k;
  while (!(rand >= typeDatas[i + 1] && rand <= typeDatas[i + 2]) && i < 1000)
    i += 3;
  if (i >= 1000) td = "0";
  else td = typeDatas[i];

  var strobj = "";
  var typeDatas2 = fobGenerate[td].split(";");
  var how_many = 2 * func.parseIntU(saze); //parse all before first letter

  for (j = 0; j < how_many; j++) {
    k = 0;
    rand = func.randomInteger(0, 999);
    while (
      !(rand >= typeDatas2[k + 1] && rand <= typeDatas2[k + 2]) &&
      k < 1000
    )
      k += 3;
    if (saze.split("").includes("t") && saze.split("t")[2].split(";")[j] != "")
      strobj = strobj + ";" + func.parseIntU(saze.split("t")[2].split(";")[j]);
    else if (k >= 1000) strobj = strobj + ";0";
    else strobj = strobj + ";" + typeDatas2[k];
  }
  return td + strobj;
}

//Get player commands
function getPlayerPosition(pid)
{
  var spl = plr.data[pid].split(";");
  return [spl[0],spl[1]];
}
function getPlayerBulletDamage(pid,type)
{
  var spl = plr.upgrades[pid].split(";");
  var bulUpg = func.parseFloatU(spl[3]);
  var mult = 1.08 ** bulUpg;
  switch(type)
  {
    case "1": return mult * func.parseFloatU(gameplay[3]);
    case "2": return mult * func.parseFloatU(gameplay[27]);
    case "3": return mult * func.parseFloatU(gameplay[28]);
    case "14": return mult * func.parseFloatU(gameplay[33]);
    case "15": return mult * func.parseFloatU(gameplay[34]);
    default: return 0;
  }
}
function getPlayerArtefact(pid)
{
  var splitted = plr.backpack[pid].split(";");
  var item = func.parseIntU(splitted[30]);
  var count = func.parseIntU(splitted[31]);
  item -= 41;
  if(item < 1 || item > 6) item=0;
  if(count > 0) return item;
  else return 0;
}

function inHeaven(pid)
{
  return (plr.players[pid]=="0" || plr.players[pid]=="1");
}

function Censure(pldata,pid,livID)
{
  if(pldata=="1" || plr.livID[pid]!=livID) return "1";
  
  var pldata = pldata.split(";");
  pldata[6] = (plr.pclass[pid].respawn_x+"").replaceAll(".",",");
  pldata[7] = (plr.pclass[pid].respawn_y+"").replaceAll(".",",");
  pldata[8] = (plr.sHealth[pid]+"").replaceAll(".",",");
  pldata[10] = plr.sRegTimer[pid];

  var artid = plr.backpack[pid].split(";")[30] - 41;
  if(plr.backpack[pid].split(";")[31]=="0") artid = -41;
  if(artid<1 || artid>6) artid=0;
  var cl_martid = func.parseIntU(pldata[5].split("&")[1]) % 100;
  artid = (100*artid + cl_martid);
  pldata[5] = pldata[5].split("&")[0] + "&" + artid;

  var cens = pldata.join(";");
  return cens;
}

function updateHasSense(before,after,pid,flags)
{
  if((before=="0" && after.length>1) || (before=="1" && after.length>1)) //player joins or respawns
  {
    //check if at spawn/respawn
    var tab_aft = after.split(";");
    var pos_aft_x = func.parseFloatU(tab_aft[0]);
    var pos_aft_y = func.parseFloatU(tab_aft[1]);
    var tab_dat = plr.data[pid].split(";");
    var pos_dat_x = func.parseFloatU(tab_dat[0]);
    var pos_dat_y = func.parseFloatU(tab_dat[1]);
    var spawn_deviation = Math.sqrt((pos_aft_x-pos_dat_x)**2 + (pos_aft_y-pos_dat_y)**2);
    if(spawn_deviation > 1) return false;
  }

  else if(before.length>1 && after.length>1) //player normally updates
  {
    //check if good movement
    var tab_bef = before.split(";");
    var pos_bef_x = func.parseFloatU(tab_bef[0]);
    var pos_bef_y = func.parseFloatU(tab_bef[1]);
    var tab_aft = after.split(";");
    var pos_aft_x = func.parseFloatU(tab_aft[0]);
    var pos_aft_y = func.parseFloatU(tab_aft[1]);
    var position_change = Math.sqrt((pos_aft_x-pos_bef_x)**2 + (pos_aft_y-pos_bef_y)**2);
    
    if(flags[3]!="T") {
      plr.pclass[pid].PosChangeAdd(position_change);
      if(plr.pclass[pid].PosChangeSum() > config.anti_cheat.max_movement_per_period) return false;
    }
    else {
      if(position_change > 150 || !plr.pclass[pid].allowed_teleport_small) return false;
    }
  }

  //player dies -> can't happen in PlayerUpdate

  else if(before=="1" && after=="1") //player is in heaven
  { /* ALWAYS GOOD */ }

  else if(before=="1" && after.length>1) //player respawns
  {
    //check if can respawn
    if(plr.pclass[pid].force_teleport_respawn) plr.pclass[pid].force_teleport_respawn = false;
    else return false;
  }

  else return false;
  return true; //no problem noticed
}

//Connection limiter variables
const con_map = new Map();
const max_cons = config.max_connections_per_ip;

//Connection limiter functions
function coMinuteFunction() {
  
}
function coSekundeFunction() {
  
}
setInterval(coMinuteFunction, 60 * 1000);
setInterval(coSekundeFunction, 1 * 1000);

//Websocket brain
wss.on("connection", function connection(ws,req)
{
  //get IP
  const client_ip = req.socket.remoteAddress;

  //IP bans execute
  config.banned_ips.forEach(function(ip){
    if(client_ip.endsWith(ip)) { ws.close(); return; };
  });

  //Connection limiter (+)
  if(!con_map.has(client_ip)) con_map.set(client_ip,0);
  var cur_cons = con_map.get(client_ip);
  if(cur_cons >= max_cons) { ws.close(); return; }
  else con_map.set(client_ip, cur_cons+1);

  ws.on("close", (code, reason) => {
    var i;
    for(i=0;i<max_players;i++){
      if(ws==se3_ws[i])
      {
        if(se3_wsS[i]=="joining") se3_ws[i]="";
        if(se3_wsS[i]=="game" || se3_wsS[i]=="menu") kick(i);
      }
    }

    //Connection limiter (-)
    var cur_cons = con_map.get(client_ip);
    con_map.set(client_ip, cur_cons-1);
  });

  ws.on("message", (msg) => {
    var i,
      arg = (msg+"").split(" ");
    var msl = arg.length;

    size_download += (msg+"").length;

    if (arg[0] == "/AllowConnection") // 1[nick] 2[RedVersion] 3[ConID]
    {
      if(!VerifyCommand(arg,["nick","short","EndID"])) return;

      var bV = arg[2] != serverRedVersion;
      var bN = nickWrong(arg[1]);
      var bJ = plr.nicks.includes(arg[1]);
      var bA = !((!config.whitelist_enabled || config.whitelist.includes(arg[1])) && (!config.banned_players.includes(arg[1])));
      
      var bT = true;
      if(config.require_se3_account) {
        var wa_lngt = waiting_authorized.length;
        for(i=0;i<wa_lngt;i++) {
          if(waiting_authorized[i][0]==arg[1] && waiting_authorized[i][1]==arg[3]) {
            waiting_authorized.splice(i,1);
            bT = false;
            break;
          }
        }
      }
      else bT = false;
      var conInsert = ""; if(config.require_se3_account) conInsert = " "+connectionAddress;

      if(bN || bJ || bV || bA || bT) {
        if(bV) sendTo(ws,"/RetAllowConnection -1 X X"); //incompatible version
        else if(bT) sendTo(ws,"/RetAllowConnection -7"+conInsert+" X X"); //this server requires se3 account to verify users
        else if(bA) sendTo(ws,"/RetAllowConnection -5 X X"); //you are banned or not on a whitelist
        else if(bN) sendTo(ws,"/RetAllowConnection -2 X X"); //wrong nick format
        else if(bJ) sendTo(ws,"/RetAllowConnection -3 X X"); //player already joined
        return;
      }
      for(i=0;i<max_players;i++)
      {
        if (plr.waiter[i] == 0) {
          plr.waiter[i] = 30*50; //30 seconds for idling in menu
          plr.nicks[i] = arg[1];

          readPlayer(i);
          plr.pushInventory[i] = "1;2;3;4;5;6;7;8;9";
          if (plr.data[i] == "0") plr.data[i] = "0;0;0;0;0;0;0;0;1;0;0;0";
          if (plr.inventory[i] == "0") plr.inventory[i] = "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
          if (plr.backpack[i] == "0") plr.backpack[i] = "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
          if (plr.upgrades[i] == "0") plr.upgrades[i] = "0;0;0;0;0";
          plr.sHealth[i] = func.parseFloatU(plr.data[i].split(";")[8]);
          plr.sRegTimer[i] = func.parseFloatU(plr.data[i].split(";")[10]);
          plr.pclass[i].DataImport(plr.data[i].split(";")[6],plr.data[i].split(";")[7]);
          SaveAllNow();

          plr.conID[i] = arg[3];
          sendTo(ws,
            "/RetAllowConnection " + i + " " +
              plr.data[i] + " " +
              plr.inventory[i] + " " +
              max_players + " " +
              clientDatapacksVar + " " +
              plr.upgrades[i] + " " +
              plr.backpack[i] + " " +
              seed+"&"+(biome_memories.join("?")) +
              " X X"
          );
          se3_ws[i] = ws;
          se3_wsS[i] = "menu";
          console.log(hourHeader + plr.nicks[i] + " connected: " + i + "");
          return;
        }
      }
      sendTo(ws,"/RetAllowConnection -4 X X"); //server is full
      return;
    }
    if (arg[0] == "/ImJoining") // 1[PlayerID]
    {
      if(!VerifyCommand(arg,["PlaID"])) return;
      if(!checkPlayerM(arg[1],ws)) return;

      plr.waiter[arg[1]] = 500;
      se3_wsS[arg[1]] = "joining";
      return;
    }
    if (arg[0] == "/ImJoined") // 1[PlayerID] 2[ConID]
    {
      if(!VerifyCommand(arg,["PlaID","EndID"])) return;
      if(!checkPlayerJ(arg[1],arg[2])) {
        try {
          ws.close();
        } catch{}
        return;
      }

      se3_ws[arg[1]] = ws;
      se3_wsS[arg[1]] = "game";

      plr.connectionTime[arg[1]] = 0;

      sendToAllPlayers(
        "/RetInfoClient " +
          (plr.nicks[arg[1]] + " joined the game").replaceAll(" ", "`") +
          " " +
          arg[1] +
          " X X"
      );

      //RPC send everything to new player
      var eff,lngt = plr.players.length;
      var gtt = GetRPC(plr.players,lngt,true);
      eff = "/RPC " + max_players + " " + gtt + " X X";
      sendTo(ws,eff);

      //New player immune to all existing bullets
      lngt = bulletsT.length;
      var tpl;
      for(i=0;i<lngt;i++)
        bulletsT[i].immune.push(arg[1]+"");

      //Treasure data sending
      sendTo(ws,"/RetTreasureLoot "+plr.pclass[arg[1]].TreasureNextDrops[0]+" 0 X X");
      sendTo(ws,"/RetTreasureLoot "+plr.pclass[arg[1]].TreasureNextDrops[1]+" 0 X X");
      sendTo(ws,"/RetTreasureLoot "+plr.pclass[arg[1]].TreasureNextDrops[2]+" 0 X X");
      sendTo(ws,"/RetTreasureLoot "+plr.pclass[arg[1]].TreasureNextDrops[3]+" 0 X X");
      sendTo(ws,"/RetTreasureLoot "+plr.pclass[arg[1]].DarkTreasureNextDrops[0]+" 1 X X");
      sendTo(ws,"/RetTreasureLoot "+plr.pclass[arg[1]].DarkTreasureNextDrops[1]+" 1 X X");
      sendTo(ws,"/RetTreasureLoot "+plr.pclass[arg[1]].DarkTreasureNextDrops[2]+" 1 X X");
      sendTo(ws,"/RetTreasureLoot "+plr.pclass[arg[1]].DarkTreasureNextDrops[3]+" 1 X X");

      //Join info
      console.log(hourHeader + plr.nicks[arg[1]] + " joined the game");
      return;
    }

    // ----- GAMEPLAY COMMANDS ----- \\

    if (arg[0] == "/PlayerUpdate") // 1[PlayerID] 2<PlayerData> 3[pingTemp] 4[flags]
    {
      if(!VerifyCommand(arg,["PlaID","UpdateData","short","Flags"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      var censured = Censure(arg[2],arg[1],arg[msl-1]);
      if(!updateHasSense(plr.players[arg[1]],censured,arg[1],arg[4])) {kick(arg[1]); return;}

      if(arg[4][3]=="T") plr.pclass[arg[1]].allowed_teleport_small = false;

      if(censured=="1") arg[4]="FFF"+arg[4][3];

      var memX = func.parseFloatU(plr.data[arg[1]].split(";")[0]);
      var memY = func.parseFloatU(plr.data[arg[1]].split(";")[1]);

      plr.players[arg[1]] = censured;
      if(censured!="1") plr.data[arg[1]] = censured;

      if (plr.waiter[arg[1]] > 1) plr.waiter[arg[1]] = 250;

      sendTo(ws,"P"+arg[3]); //Short type command

      //Flags explained
      // [0] - impulseEnabled
      // [1] - impulseStarted
      // [2] - invisibilityPulse
      // [3] - doTeleport

      //flags questioning
      var artef = getPlayerArtefact(arg[1]);
      if(artef!=2) { arg[4][0]="F"; arg[4][1]="F"; };
      if(artef!=3) { arg[4][2]="F"; };

      if(arg[4][1]=="T") plr.impulsed[arg[1]] = [];

      if(arg[4][2]=="T") sendToAllPlayers(
        "/RetInvisibilityPulse " + arg[1] + " none X X"
      );

      //impulse damage
      var j, caray = censured.split(";");
      if(caray.length>1 && arg[4][0]=="T")
      {
        var xa = func.parseFloatU(caray[0]);
        var ya = func.parseFloatU(caray[1]);
        var xb = func.parseFloatU(memX);
        var yb = func.parseFloatU(memY);
        func.CollisionLinearBulletSet(xa,ya,xb,yb,1.2);

        if(pvp)
        for(j=0;j<max_players;j++)
        {
          if(plr.players[j]!="0" && plr.players[j]!="1" && arg[1]!=j && !plr.impulsed[arg[1]].includes(j))
          {
            var plas = plr.players[j].split(";");
            var xc = func.parseFloatU(plas[0]);
            var yc = func.parseFloatU(plas[1]);
            if(func.CollisionLinearCheck(xc,yc,0.92))
            {
              DamageFLOAT(j,func.parseFloatU(gameplay[29]))
              plr.impulsed[arg[1]].push(j);
            }
          }
        }

        var lngts = scrs.length;
        for(j=0;j<lngts;j++)
        {
          var l = scrs[j].bID;
          if(scrs[j].dataY[2-2]==2 && !plr.impulsed[arg[1]].includes(-l))
          {
            var xc = scrs[j].posCX + func.ScrdToFloat(scrs[j].dataY[8-2]);
            var yc = scrs[j].posCY + func.ScrdToFloat(scrs[j].dataY[9-2]);
            if(func.CollisionLinearCheck(xc,yc,7.5))
            {
              DamageBoss(j,func.parseFloatU(gameplay[29]))
              plr.impulsed[arg[1]].push(-l);
            }
          }
        }
      }
      return;
    }
    if (arg[0] == "/ChatMessage") // 1[PlayerID] 2[Message]
    {
      if(!VerifyCommand(arg,["PlaID","Msg256"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      console.log(hourHeader + "<" + plr.nicks[func.parseFloatU(arg[1])] + "> " + arg[2].replaceAll("\t"," "));
      sendToAllPlayers("/RetChatMessage " + plr.nicks[func.parseFloatU(arg[1])] + " " + arg[2] + " X X");
    }
    if (arg[0] == "/InventoryPush") // 1[PlayerID] 2[PushID]
    {
      if(!VerifyCommand(arg,["PlaID","PushID"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      var locPlaID = arg[1];
      var locPushID = arg[2];
      var pom, l;

      var pushTab = plr.pushInventory[locPlaID].split(";");
      pom = pushTab[locPushID - 1];
      pushTab[locPushID - 1] = pushTab[locPushID];
      pushTab[locPushID] = pom;

      var newPush = "" + pushTab[0];
      for (l = 1; l < 9; l++) newPush = newPush + ";" + pushTab[l];
      plr.pushInventory[locPlaID] = newPush;
    }
    if (arg[0] == "/GeyzerTurnTry") // 1[PlayerID] 2[ulam] 3[place] 4[turnID]
    {
      if(!VerifyCommand(arg,["PlaID","ulam","place","turnID"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      if(arg[4]=="40") //dead alien turn
        if ( checkFobChange(arg[2], arg[3], "13", "23") || checkFobChange(arg[2], arg[3], "25", "27") ) {
          fobChange(arg[2], arg[3], "40");
          sendToAllPlayers("/RetGeyzerTurn " + arg[2] + " " + arg[3] + " X X");
        }

      if(arg[4]=="45") //immortality artefact turn
        if ( checkFobChange(arg[2], arg[3], "41", "-1") ) {
          fobChange(arg[2], arg[3], "45");
          sendToAllPlayers("/RetGeyzerTurn " + arg[2] + " " + arg[3] + " X X");
        }
    }
    if (arg[0] == "/EmitParticles") // 1[PlayerID] 2[particles] 3[posX/dat1] 4[posY/dat2]
    {
      if(!VerifyCommand(arg,["PlaID","ID128","Float","Float"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      sendToAllPlayers(
        "/RetEmitParticles " +
          arg[1] + " " +
          arg[2] + " " +
          arg[3] + " " +
          arg[4] +
          " X X"
      );
    }
    if(arg[0] == "/TryBattleStart") // 1[PlayerID] 2[bID] 3[storageUlam] 4[storagePlace]
    {
      if(!VerifyCommand(arg,["PlaID","ulam","ulam","place"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      var bID = arg[2];
      var fobID = arg[3];
      var fobIndex = arg[4];

      var lngts = scrs.length;
      for(i=0;i<lngts;i++)
      {
        if(scrs[i].bID==bID)
        {
          if(scrs[i].dataY[2-2]==0 && checkFobDataChange(fobID, fobIndex, "5", "-10", "52"))
          {
            var gCountEnd = fobDataChange(fobID, fobIndex, "5", "-10");
            scrs[i].dataY[2-2] = 1;
            resetScr(i);
            sendToAllPlayers(
              "/RetFobsDataChange " +
                fobID + " " +
                fobIndex + " 5 -10 -1 " + //itemID, deltaCount, playerID
                gCountEnd + " 52 X X" //fob21ID
            );
          }
          return;
        }
      }
    }
    if(arg[0] == "/GiveUpTry") // 1[PlayerID] 2[bID]
    {
      if(!VerifyCommand(arg,["PlaID","ulam"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      var bID = arg[2];
      var blivID = arg[msl-1];

      var j,lngts = scrs.length;
      for(i=0;i<lngts;i++)
      {
        if(scrs[i].bID==bID) {
          if(scrs[i].dataY[2-2]!=2) break;

          var players_inside = 0;
          var found_me = false;
          for(j=0;j<max_players;j++) {
            if(plr.players[j]!="0" && plr.players[j]!="1")
            {
              var plas = plr.players[j].split(";");
              var dx = func.parseFloatU(plas[0]) - scrs[i].posCX;
              var dy = func.parseFloatU(plas[1]) - scrs[i].posCY;
              if(dx**2 + dy**2 <= (in_arena_range)**2) {
                players_inside++;
                if(arg[1]==j+"") found_me = true;
              }
            }
          }
          if(found_me) {
            if(players_inside==1) scrs[i].dataY[4-2] = 1;
            else {
              sendTo(ws,"/RetGiveUpTeleport "+arg[1]+" "+bID+" 1024 X "+blivID);
              plr.pclass[arg[1]].allowed_teleport_small = true; //might turn "true" for a longer time when player dies while trying to teleport, but it only allows for one short-distance teleportation with cheats
            }
          }
          break;
        }
      }
    }
    if (arg[0] == "/ClientDamage") // 1[PlayerID] 2[dmg] 3[ImmID] 4[info] 5[ifRinged]
    {
      if(!VerifyCommand(arg,["PlaID","fraction01","EndID","short","short"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      var serLivID = plr.livID[arg[1]];
      var serImmID = plr.immID[arg[1]];
      var cliLivID = arg[msl-1];
      var cliImmID = arg[3];

      var cis="";
      if(serLivID==cliLivID) cis+="T"; else cis+="F";
      if(serImmID==cliImmID) cis+="T"; else cis+="F";

      if(cis=="TT")
        CookedDamage(arg[1],arg[2],arg[5]=="T");

      if(arg[4]=="K") //client kill
      {
        switch(cis)
        {
          case "TT": kill(arg[1]); break;
          case "TF": kick(arg[1]); return;
          case "FT": break;
          case "FF": kick(arg[1]); return;
        }
      }
      if(arg[4]=="I") //client immortal
      {
        switch(cis)
        {
          case "TT": immortal(arg[1]); break;
          case "TF": break;
          case "FT": kick(arg[1]); return;
          case "FF": kick(arg[1]); return;
        }
      }

      var censured = Censure(plr.players[arg[1]],arg[1],cliLivID);
      plr.players[arg[1]] = censured;
      if(censured!="1") plr.data[arg[1]] = censured;
      sendTo(ws,"/RetDamageBalance " + arg[2] + " " + cliLivID + " " + cliImmID + " X X");
    }
    if (arg[0] == "/Upgrade") // 1[PlayerID] 2[upgID] 3[slot]
    {
      if(!VerifyCommand(arg,["PlaID","UpgID","Slot"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var ljPlaID = arg[1];
      var ljUpgID = arg[2];
      var ljSlot = arg[3];

      var ljTab = plr.upgrades[ljPlaID].split(";");
      var current_level = func.parseIntU(ljTab[ljUpgID]);

      var upg_costs = [
        [func.parseIntU(gameplay[112]),func.parseIntU(gameplay[113])],
        [func.parseIntU(gameplay[114]),func.parseIntU(gameplay[115])],
        [func.parseIntU(gameplay[116]),func.parseIntU(gameplay[117])],
        [func.parseIntU(gameplay[118]),func.parseIntU(gameplay[119])],
        [func.parseIntU(gameplay[120]),func.parseIntU(gameplay[121])]
      ];
      var ljItem = upg_costs[current_level][0];
      var ljCount = upg_costs[current_level][1];

      if(current_level < 0 || current_level > 4) return;

      if(invChangeTry(ljPlaID, ljItem, -ljCount, ljSlot))
      {
        ljTab[ljUpgID] = current_level + 1;
        var ij, eff = ljTab[0];
        for (ij = 1; ij < 5; ij++) eff += ";" + ljTab[ij];
        plr.upgrades[ljPlaID] = eff;
        sendTo(ws,"/RetUpgrade " + ljPlaID + " " + ljUpgID + " X " + plr.livID[ljPlaID]);
      }
      else kick(ljPlaID);
    }
    if (arg[0] == "/FobsDataChange") // 1[PlayerID] 2[UlamID] 3[PlaceID] 4[Item] 5[DeltaCount] 6[Slot] 7[StorageID]
    {
      if(!VerifyCommand(arg,["PlaID","ulam","place","item","1,-1","Slot","StorageID"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[5]=="1" && arg[7]=="2") return; //Trying to insert item into driller
      var overolded = (arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1]));

      var gPlayerID = arg[1];
      var gUlamID = arg[2];
      var gPlaceID = arg[3];
      var gItem = arg[4];
      var gDeltaCount = arg[5];
      var gSlot = arg[6];
      var gID21 = arg[7];
      
      if (checkFobDataChange(gUlamID, gPlaceID, gItem, gDeltaCount, gID21) && !overolded)
      {
        if (invChangeTry(gPlayerID, gItem, -gDeltaCount, gSlot))
        {
          var gCountEnd = fobDataChange(gUlamID, gPlaceID, gItem, gDeltaCount);
          sendToAllPlayers(
            "/RetFobsDataChange " +
              gUlamID + " " +
              gPlaceID + " " +
              gItem + " " +
              gDeltaCount + " " +
              gPlayerID + " " +
              gCountEnd + " " +
              gID21 +
              " X X"
          );
          sendTo(ws,
            "/RetInventory " +
              gPlayerID + " " +
              gItem + " 0 " +
              gSlot + " " +
              gDeltaCount +
              " X " + plr.livID[gPlayerID]
          );
          return;
        }
        else {kick(gPlayerID); return;}
      }

      //If failied
      sendTo(ws,
        "/RetFobsDataCorrection " +
          gUlamID + " " +
          gPlaceID + " " +
          nbt(gUlamID, gPlaceID, "n", "0;0") + ";" + gDeltaCount + " " +
          gPlayerID + " " +
          gID21 +
          " X X"
      );
      sendTo(ws,
        "/RetInventory " +
          gPlayerID + " " +
          gItem + " " +
          gDeltaCount + " " +
          gSlot + " " +
          -gDeltaCount +
          " X " + plr.livID[gPlayerID]
      );
    }
    if (arg[0] == "/Backpack") // 1[PlayerID] 2[Item] 3[Count] 4[SlotI] 5[SlotB]
    {
      if(!VerifyCommand(arg,["PlaID","item","count","SlotI","SlotB"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var bpPlaID = arg[1];
      var bpItem = arg[2];
      var bpCount = arg[3];
      var bpSlotI = arg[4];
      var bpSlotB = arg[5];

      var safeCopyI = plr.inventory[bpPlaID];
      var safeCopyB = plr.backpack[bpPlaID];

      if (invChangeTry(bpPlaID, bpItem, bpCount, bpSlotI))
        if (invChangeTry(bpPlaID, bpItem, -bpCount, bpSlotB)) return;

      plr.inventory[bpPlaID] = safeCopyI;
      plr.backpack[bpPlaID] = safeCopyB;
      kick(bpPlaID);
    }
    if(arg[0] == "/ScrRefresh") // 1[PlayerID] 2[bID] 3[inArena]
    {
      if(!VerifyCommand(arg,["PlaID","ulam","short"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      var bID = arg[2];
      var inArena = (arg[3]=="T");

      var lngts = scrs.length;
      for(i=0;i<lngts;i++)
      {
        if(scrs[i].bID==bID)
        {
          //Boss found
          scrs[i].timeToDisappear = 1000;
          if(inArena) scrs[i].timeToLose = 1000;

          if(!plr.bossMemories[arg[1]].includes(bID))
            plr.bossMemories[arg[1]].push(bID);

          break;
        }
      }
    }
    if(arg[0] == "/ScrForget") // 1[PlayerID] 2[bID]
    {
      if(!VerifyCommand(arg,["PlaID","ulam"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      var remindex = plr.bossMemories[arg[1]].indexOf(arg[2]);
      if(remindex!=-1)
        plr.bossMemories[arg[1]].remove(remindex);
    }
    if (arg[0] == "/GrowLoaded") // 1[PlayerID] 2[UlamList]
    {
      if(!VerifyCommand(arg,["PlaID","ulamList"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      var tab = arg[2].split(";");
      var lngt = tab.length - 1;
      for(i=1;i<lngt;i++) growActive(tab[i]);
    }
    if (arg[0] == "/Crafting") // 1[PlaID] 2[CraftID] 3[Slot1] 4[Slot2] 5[SlotE]
    {
      if(!VerifyCommand(arg,["PlaID","CraftID","Slot","SlotIf","Slot"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var cPlaID = arg[1];
      var tab = craftings.split(";");

      //first ingredient
      var cId1 = tab[func.parseIntU(arg[2])*6 + 0];
      var cCo1 = tab[func.parseIntU(arg[2])*6 + 1];
      var cSl1 = arg[3];

      //second ingredient
      var cId2 = tab[func.parseIntU(arg[2])*6 + 2];
      var cCo2 = tab[func.parseIntU(arg[2])*6 + 3];
      var cSl2 = arg[4];
      if(cSl2=="-1" && cCo2!="0") return;

      //crafted item
      var cIdE = tab[func.parseIntU(arg[2])*6 + 4];
      var cCoE = tab[func.parseIntU(arg[2])*6 + 5];
      var cSlE = arg[5];

      var safeCopyI = plr.inventory[cPlaID];
      var safeCopyB = plr.backpack[cPlaID];

      if (invChangeTry(cPlaID, cId1, -cCo1, cSl1))
        if (invChangeTry(cPlaID, cId2, -cCo2, cSl2))
          if (invChangeTry(cPlaID, cIdE, cCoE, cSlE)) return;

      plr.inventory[cPlaID] = safeCopyI;
      plr.backpack[cPlaID] = safeCopyB;
      kick(cPlaID);
    }
    if (arg[0] == "/Potion") // 1[PlaID] 2[PotionID] 3[SlotID]
    {
      if(!VerifyCommand(arg,["PlaID","PotionID","SlotI"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) {
        sendTo(ws,"/RetHeal "+arg[1]+" "+arg[2]+" X X");
        return;
      }

      var pid=arg[1];
      var tab = [0,55,61,71,57,59,63]; //special potion ID

      if(!invChangeTry(arg[1],tab[arg[2]],-1,arg[3])) {
        kick(arg[i]);
        return;
      }

      if(arg[2]=="1" || arg[2]=="2" || arg[2]=="3")
      {
        var artid = plr.backpack[pid].split(";")[30] - 41;
        if(plr.backpack[pid].split(";")[31]=="0") artid = -41;

        var sth1 = plr.sHealth[pid];

        var heal_size;
        if(arg[2]=="1") heal_size = gameplay[31];
        if(arg[2]=="2") heal_size = gameplay[39];
        if(arg[2]=="3") heal_size = "10000";

        var potHHH = func.parseFloatU(plr.upgrades[pid].split(";")[0]) + getProtLevelAdd(artid) + func.parseFloatU(gameplay[26]);
		    if(potHHH<-50) potHHH = -50; if(potHHH>56.397) potHHH = 56.397;
		    var heal=0.02*func.parseFloatU(heal_size)/(Math.ceil(50*Math.pow(health_base,potHHH))/50);
        if(heal<0) heal=0;
        
        plr.sHealth[pid] += heal;
        if(plr.sHealth[pid]>1) plr.sHealth[pid]=1;

        var sth2 = plr.sHealth[pid];

        var del = ((sth2-sth1)+"").replaceAll(".",",");
        var abl = ((sth2)+"").replaceAll(".",",");
        sendTo(ws,"R "+del+" "+plr.immID[pid]+" "+plr.livID[pid]+" "+abl); //Medium type message
        sendTo(ws,"/RetHeal "+arg[1]+" "+arg[2]+" X X");
      }
      else return;
    }
    if (arg[0] == "/JunkDiscard") // 1[PlayerID] 2[Item] 3[Count]
    {
      if(!VerifyCommand(arg,["PlaID","item","count-"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      if(!invChangeTry(arg[1], arg[2], arg[3], "25")) kick(arg[1]);
    }
    if (arg[0] == "/SetRespawn") // 1[PlayerID] 2[Slot] 3,4[newpos(0 0/1 1)]
    {
      if(!VerifyCommand(arg,["PlaID","Slot","short","short"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) {kick(arg[1]); return;} //Kick if modified respawn while not living
      
      if(arg[3]+" "+arg[4]!="0 0") { //create
        var pla_pos = getPlayerPosition(arg[1]);
        arg[3] = pla_pos[0];
        arg[4] = pla_pos[1];
        if(!invChangeTry(arg[1], "20", "-1", arg[2])) {kick(arg[1]); return;}
        plr.pclass[arg[1]].ModifyRespawn(arg[3],arg[4]);
      }
      else { //pickup
        if(plr.pclass[arg[1]].respawn_x+" "+plr.pclass[arg[1]].respawn_y=="0 0") {kick(arg[1]); return;} //Kick when trying to remove not existing respawn
        if(!invChangeTry(arg[1], "10", "3", arg[2])) {kick(arg[1]); return;}
        plr.pclass[arg[1]].ModifyRespawn(0,0);
      }
       var cens = Censure(plr.players[arg[1]],arg[1],arg[msl-1]);
       plr.players[arg[1]] = cens;
       if(cens!="1") plr.data[arg[1]] = cens;
    }
    if (arg[0] == "/DrillAsk") // 1[PlayerID] 2[DrillID] 3[DrillGroup]
    {
      if(!VerifyCommand(arg,["PlaID","DrillID","EndID"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      plr.pclass[arg[1]].DrillAsk(arg[2],arg[1],arg[3]);
    }
    if (arg[0] == "/DrillGet") // 1[PlayerID] 2[Item] 3[Slot]
    {
      if(!VerifyCommand(arg,["PlaID","item","Slot"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) arg[3]="25";

      if(arg[3]!="25") {
        if(!plr.pclass[arg[1]].DrillGet(arg[1],arg[2],arg[3])) kick(pid);
      }
      else plr.pclass[arg[1]].DrillDiscard(arg[2]);
    }
    if (arg[0] == "/FobPlace") // 1[PlayerID] 2[UlamID] 3[PlaceID] 4[EndFob] 5[Slot]
    {
      if(!VerifyCommand(arg,["PlaID","ulam","place","fob-interactable","Slot"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      var overolded = (arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1]));

      var fPlayerID = arg[1];
      var fUlamID = arg[2];
      var fPlaceID = arg[3];
      var fEndFob = arg[4];
      var fDropID = fEndFob;
      var fCount = "-1";
      var fSlot = arg[5];

      var fFob21TT = nbt(fUlamID, fPlaceID, "n", "0;0");

      if (checkFobChange(fUlamID, fPlaceID, "0", "-1") && !overolded)
      {
        if (invChangeTry(fPlayerID, fDropID, fCount, fSlot))
        {
          fobChange(fUlamID, fPlaceID, fEndFob);
          fFob21TT = nbt(fUlamID, fPlaceID, "n", "0;0");
          sendToAllPlayers(
            "/RetFobsChange " +
              fUlamID + " " +
              fPlaceID + " " +
              fEndFob + " " +
              fFob21TT +
              " X X"
          );
          sendTo(ws,
            "/RetInventory " +
              fPlayerID + " " +
              fDropID + " 0 " +
              fSlot + " " +
              -fCount +
              " X " + plr.livID[fPlayerID]
          );
          sendTo(ws,"/RetFobsPing "+arg[1]+";"+arg[2]+";"+arg[3]+" X X");
          return;
        }
        else {kick(fPlayerID); return;}
      }

      //If failied
      sendTo(ws,
        "/RetFobsChange " +
          fUlamID + " " +
          fPlaceID + " " +
          getBlockAt(fUlamID, fPlaceID) + " " +
          fFob21TT +
          " X X"
      );
      sendTo(ws,
        "/RetInventory " +
          fPlayerID + " " +
          fDropID + " " +
          -fCount + " " +
          fSlot + " " +
          fCount +
          " X " + plr.livID[fPlayerID]
      );
      sendTo(ws,"/RetFobsPing "+arg[1]+";"+arg[2]+";"+arg[3]+" X X");
    }
    if (arg[0] == "/FobBreak") // 1[PlayerID] 2[UlamID] 3[PlaceID] 4[StartFob] 5[Slot]
    {
      if(!VerifyCommand(arg,["PlaID","ulam","place","fob-interactable","Slot"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      var overolded = (arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1]));

      var fPlayerID = arg[1];
      var fUlamID = arg[2];
      var fPlaceID = arg[3];
      var fStartFob1 = arg[4];
      var fSlot = arg[5];

      //fStartFob2 set
      var fStartFob2 = fStartFob1;
      if(fStartFob1=="5") fStartFob2 = "6";
      if(fStartFob1=="6") fStartFob2 = "7";
      if(fStartFob1=="23") fStartFob2 = "25";
      if(fStartFob1=="25") fStartFob2 = "23";
      if(fStartFob1=="41") fStartFob2 = "45";

      //Drop set
      var fDropID = fStartFob1;
      var fCount = "1";
      if(modifiedDrops[fStartFob1]!="") {
        var modTab = modifiedDrops[fStartFob1].split(";");
        fDropID = modTab[0];
        fCount = modTab[1];
      }

      //Treasure drop set
      var treasure_type = -1;
      if(fStartFob1=="37") {
        var treTab = plr.pclass[fPlayerID].TreasureNextDrops[0].split(";");
        fDropID = treTab[0];
        fCount = treTab[1];
        treasure_type = 0;
      }
      if(fStartFob1=="68") {
        var treTab = plr.pclass[fPlayerID].DarkTreasureNextDrops[0].split(";");
        fDropID = treTab[0];
        fCount = treTab[1];
        treasure_type = 1;
      }

      var fFob21TT = nbt(fUlamID, fPlaceID, "n", "0;0");

      if ((checkFobChange(fUlamID, fPlaceID, fStartFob1, fStartFob2) ||
         (["13", "23", "25", "27"].includes(fStartFob1) &&
         checkFobChange(fUlamID, fPlaceID, "40", "-1"))) && !overolded)
      {
        if (invChangeTry(fPlayerID, fDropID, fCount, fSlot))
        {
          if(treasure_type!=-1) plr.pclass[fPlayerID].TreasureArrayUpdate(treasure_type,true);
          fobChange(fUlamID, fPlaceID, "0");
          fFob21TT = nbt(fUlamID, fPlaceID, "n", "0;0");
          sendToAllPlayers(
            "/RetFobsChange " +
              fUlamID + " " +
              fPlaceID + " " +
              "0" + " " +
              fFob21TT +
              " X X"
          );
          sendTo(ws,
            "/RetInventory " +
              fPlayerID + " " +
              fDropID + " 0 " +
              fSlot + " " +
              -fCount +
              " X " + plr.livID[fPlayerID]
          );
          sendTo(ws,"/RetFobsPing "+arg[1]+";"+arg[2]+";"+arg[3]+" X X");
          return;
        }
        else {kick(fPlayerID); return;}
      }

      //If failied
      if(treasure_type!=-1) plr.pclass[fPlayerID].TreasureArrayUpdate(treasure_type,false);
      sendTo(ws,
        "/RetFobsChange " +
          fUlamID + " " +
          fPlaceID + " " +
          getBlockAt(fUlamID, fPlaceID) + " " +
          fFob21TT +
          " X X"
      );
      sendTo(ws,
        "/RetInventory " +
          fPlayerID + " " +
          fDropID + " " +
          -fCount + " " +
          fSlot + " " +
          fCount +
          " X " + plr.livID[fPlayerID]
      );
      sendTo(ws,"/RetFobsPing "+arg[1]+";"+arg[2]+";"+arg[3]+" X X");
    }
    if (arg[0] == "/BulletSend") // 1[PlayerID] 2[type] 3,4[vector] 5[ID] 6[BulletSource] 7[Slot] 8,9[position]
    {
      if(!VerifyCommand(arg,["PlaID","BulletType","Float","Float","EndID","short","Slot","Float","Float"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      var revert_string = "/RetNewBulletDestroy "+arg[1]+" "+arg[5]+" 0 "+false+"  X X";
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) { sendTo(ws,revert_string); return; } //not living
      for(i=0;i<bulletsT.length;i++) if(bulletsT[i].ID+""==arg[5]) { sendTo(ws,revert_string); return; } //existing ID (veeery small chance)

      //Command data
      var bPlaID = arg[1];
      var bType = arg[2];
      var bVectX = arg[3];
      var bVectY = arg[4];
      var bRandID = arg[5];
      var bBulSrc = arg[6];
      var bSlot = arg[7];
      var bPosX = arg[8];
      var bPosY = arg[9];
      var bDmg = getPlayerBulletDamage(bPlaID,bType);

      //Bullet paywall
      if(bBulSrc=="I") //inventory
      {
        var bulItem;
        switch(bType)
        {
          case "1": bulItem=24; break;
          case "2": bulItem=39; break;
          case "3": bulItem=48; break;
          case "14": bulItem=64; break;
          case "15": bulItem=65; break;
          default: return;
        }
        if(!invChangeTry(bPlaID,bulItem+"","-1",bSlot)) {kick(bPlaID); return;}
      }
      else if(bBulSrc=="U") //unstable
      {
        if(bType!="3") return;
        if(plr.pclass[bPlaID].unstable_pulses_available > 0) plr.pclass[bPlaID].unstable_pulses_available--;
        else { sendTo(ws,revert_string); return; }
      }
      else if(bBulSrc=="A") //unstable artefact unstabling
      {
        if(bType!="3") return;
        if(!(plr.backpack[bPlaID].split(";")[30]=="47" && plr.backpack[bPlaID].split(";")[31]!="0")) { sendTo(ws,revert_string); return; } //unstabling without artefact
      }
      else return;

      //Speed anti-cheat
      if((func.parseFloatU(bVectX)**2 + func.parseFloatU(bVectY)**2) ** 0.5 > 1.2) { sendTo(ws,revert_string); return; } //bullet too fast

      //Start position anti-cheat
      var ppos = getPlayerPosition(arg[1]);
      var ddx = func.parseFloatU(ppos[0]) - func.parseFloatU(bPosX);
      var ddy = func.parseFloatU(ppos[1]) - func.parseFloatU(bPosY);
      var distance_to_center = Math.sqrt(ddx**2 + ddy**2);
      if(distance_to_center > config.anti_cheat.bullet_spawn_allow_radius) { sendTo(ws,revert_string); return; } //bullet wrong spawn position

      //bullet summon
      var tpl = Object.assign({},bulletTemplate);
      tpl.start = Object.assign({},bulletTemplate.start);
      tpl.vector = Object.assign({},bulletTemplate.vector);
      tpl.pos = Object.assign({},bulletTemplate.pos);
      tpl.damaged = [];
      tpl.immune = [];

      tpl.ID = func.parseIntU(bRandID);
      tpl.owner = func.parseIntU(bPlaID);
      tpl.type = func.parseIntU(bType);
      tpl.start.x = func.parseFloatU(bPosX);
      tpl.start.y = func.parseFloatU(bPosY);
      tpl.vector.x = func.parseFloatU(bVectX);
      tpl.vector.y = func.parseFloatU(bVectY);
      tpl.pos.x = tpl.start.x;
      tpl.pos.y = tpl.start.y;

      tpl.normal_damage = func.parseFloatU(bDmg);
      tpl.is_unstable = (bType=="3");
      if(bBulSrc=="A") tpl.unstable_virtual = true;

      if(tpl.vector.x==0) tpl.vector.x = 0.00001;
      if(tpl.vector.y==0) tpl.vector.y = 0.00001;

      //1[PlayerID] 2[type] 3,4[position] 5,6[vector] 7[ID] 8["0"]
      spawnBullet(tpl,["/NewBulletSend",bPlaID,bType,bPosX,bPosY,bVectX,bVectY,bRandID,"0"],0);
    }
    if (arg[0] == "/BulletRemove") // 1[PlayerID] 2[BulletID] 3[age]
    {
      if(!VerifyCommand(arg,["PlaID","EndID","Integer"])) return;
      if(!checkPlayerG(arg[1],ws)) return;
      
      var lngt = bulletsT.length;
      for(i=0;i<lngt;i++)
        if(bulletsT[i].owner==arg[1] && bulletsT[i].ID==arg[2])
          destroyBullet(i,arg,true);
    }
    if (arg[0] == "/ShotTurn") // 1[PlayerID] 2[ulam] 3[place] 4[FobStart] 5[BulletID]
    {
      if(!VerifyCommand(arg,["PlaID","ulam","place","ShotTurnFob","EndID"])) return;
      if(!checkPlayerG(arg[1],ws)) return;

      var lngt = bulletsT.length;
      for(i=0;i<=lngt;i++) {
        if(i==lngt) return;
        if(bulletsT[i].owner==arg[1] && bulletsT[i].ID==arg[5] && !bulletsT[i].turn_used && (arg[4]!="0" || (bulletsT[i].type=="3" && !bulletsT[i].unstable_virtual))) {
          bulletsT[i].turn_used = true;
          break;
        }
      }

      var arg4b = "-1", argend = "48";
      if(arg[4]=="23") {arg4b = "25"; argend = "25";}
      if(arg[4]=="25") {arg4b = "23"; argend = "25";}

      if (checkFobChange(arg[2],arg[3],arg[4],arg4b)) {
        fobChange(arg[2], arg[3], argend);
        sendToAllPlayers(
          "/RetFobsTurn " +
            arg[1] + " " +
            arg[2] + " " +
            arg[3] + " " +
            argend +
            " X X"
        );
      }
    }


    //---------------//
    // WILD COMMANDS //
    //---------------//


    if (arg[0] == "/AsteroidData") {
      //AsteroidData 1[UlamID] 2[generation_code] 3[PlayerID]
      if (!checkPlayerG(arg[3],ws)) return;

      var ulamID = arg[1];
      var localSize = arg[2];

      var lc3T, lc3;
      var det = asteroidIndex(ulamID);
      if (chunk_data[det[0]][det[1]][0] == "" || func.parseIntU(chunk_data[det[0]][det[1]][0]) >= 1024) {
        lc3 = generateAsteroid(localSize);
        lc3T = lc3.split(";");
        for (i = 0; i <= 20; i++) chunk_data[det[0]][det[1]][i] = lc3T[i];
      } else {
        lc3 = chunk_data[det[0]][det[1]][0];
        for (i = 1; i <= 20; i++) lc3 += ";" + chunk_data[det[0]][det[1]][i];
      }
      growActive(ulamID);

      sendTo(ws,
        "/RetAsteroidData " +
          ulamID + " " +
          lc3 + " " +
          nbts(ulamID, "n", "0;0") +
          " X X"
      );
    }
    if (arg[0] == "/ScrData") {
      //ScrData 1[PlayerID] 2[bID] 3[1024] 4[bossType] 5[bossPosX] 6[bossPosY]
      if (!checkPlayerG(arg[1],ws)) return;

      var bID = arg[2];
      var scrID = arg[3];
      var bossType = arg[4];
      var bossPosX = arg[5];
      var bossPosY = arg[6];

      var lc3T, lc3, inds=-1, lngts=scrs.length;

      for(i=0;i<lngts;i++)
      {
        if(scrs[i].bID==bID) {
          inds = i;
          break;
        }
      }

      if(inds==-1)
      {
        if(scrID=="1024")
        {
          var det = asteroidIndex(bID);
          if (chunk_data[det[0]][det[1]][0]!=scrID)
          {
            //Not exists
            lc3 = scrID;
            for(i=1;i<=60;i++) lc3 += ";0";
          }
          else
          {
            //Exists
            lc3 = chunk_data[det[0]][det[1]][0]+";"+chunk_data[det[0]][det[1]][1];
            for(i=2;i<=60;i++) lc3 += ";0";
          }
          
          lc3T = lc3.split(";");
          for(i=0;i<=60;i++) chunk_data[det[0]][det[1]][i] = lc3T[i];

          var tpl = Object.assign({},scrTemplate);
          tpl.bID = bID;
          tpl.type = bossType;
          tpl.posCX = func.parseFloatU(bossPosX);
          tpl.posCY = func.parseFloatU(bossPosY);
          tpl.dataX = [];
          tpl.dataY = [];
          tpl.bulCols = [];
          tpl.dataX = [func.parseIntU(lc3T[0]),func.parseIntU(lc3T[1])];
          for(i=2;i<=60;i++) tpl.dataY[i-2] = 0;
          var tpl2 = {x:0,y:0}; tpl2.x = tpl.posCX; tpl2.y = tpl.posCY;
          tpl.behaviour = new CBoss(bossType,tpl2,tpl.dataX,tpl.dataY,visionInfo);
          scrs.push(tpl);

          //Bosbul static commands
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,36.39999,0,0,171]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,0,36.39999,90,172]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,-36.39999,0,180,173]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,0,-36.39999,270,174]);
          ScrShapeAdd(["",0,bID,"sphere",4,0,36.42503,8.337769,11.25,1025]);
          ScrShapeAdd(["",0,bID,"sphere",4,0,-8.337723,36.42503,101.25,1026]);
          ScrShapeAdd(["",0,bID,"sphere",4,0,-36.42505,-8.337677,191.25,1027]);
          ScrShapeAdd(["",0,bID,"sphere",4,0,8.337646,-36.42505,281.25,1028]);
          ScrShapeAdd(["",0,bID,"sphere",4,0,36.425,-8.337936,348.75,1029]);
          ScrShapeAdd(["",0,bID,"sphere",4,0,-8.337891,-36.425,258.75,1030]);
          ScrShapeAdd(["",0,bID,"sphere",4,0,-36.42502,8.337845,168.75,1031]);
          ScrShapeAdd(["",0,bID,"sphere",4,0,8.337799,36.42502,78.75,1032]);
          ScrShapeAdd(["",0,bID,"sphere",3.5,0,31.10144,21.44495,33.75,1033]);
          ScrShapeAdd(["",0,bID,"sphere",3.5,0,21.44498,31.10143,56.25,1034]);
          ScrShapeAdd(["",0,bID,"sphere",3.5,0,-21.4449,31.10147,123.75,1035]);
          ScrShapeAdd(["",0,bID,"sphere",3.5,0,-31.10139,21.44501,146.25,1036]);
          ScrShapeAdd(["",0,bID,"sphere",3.5,0,-31.10149,-21.44489,213.75,1037]);
          ScrShapeAdd(["",0,bID,"sphere",3.5,0,-21.44505,-31.10138,236.25,1038]);
          ScrShapeAdd(["",0,bID,"sphere",3.5,0,21.44484,-31.10152,303.75,1039]);
          ScrShapeAdd(["",0,bID,"sphere",3.5,0,31.10135,-21.44508,326.25,1040]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,33.64288,15.1069,22.5,1083]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,26.21841,26.2184,45,1084]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,15.10693,33.64287,67.5,1085]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,-15.10686,33.6429,112.5,1086]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,-26.21837,26.21844,135,1087]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,-33.64285,15.10696,157.5,1088]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,-33.64291,-15.10683,202.5,1089]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,-26.21849,-26.21832,225,1090]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,-15.10703,-33.64282,247.5,1091]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,15.1068,-33.64291,292.5,1092]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,26.21831,-26.21851,315,1093]);
          ScrShapeAdd(["",0,bID,"cylinder",1.5,5,33.64282,-15.10704,337.5,1094]);
        }
        else return;
      }
    }
    if (arg[0] == "/TryInsertBiome") {
      //TryInsertBiome 1[PlayerID] 2[Ulam] 3[Biome]
      if (!checkPlayerG(arg[1],ws)) return;
      
      var ulam = func.parseIntU(arg[2]);
      var biome = func.parseIntU(arg[3]);

      var ln = Math.floor(ulam / 1000);
			var id = ulam % 1000;
			if(ln>=16000)
			{
				id += (ln-15999) * 1000;
				ln = 15999;
			}
			while(biome_memories[ln].length <= id)
				biome_memories[ln] += '-';
			if(biome_memories[ln][id]=="-") biome_memories[ln] = replaceCharAtIndex(biome_memories[ln], id, Num31ToChar(biome));

    }
  });
});

//Command verifier function
function VerifyCommand(args,formats)
{
  formats.push("EndID");
  if(args.length != formats.length+1) return false;
  var i,lngt = args.length;
  for(i=1;i<lngt;i++)
  {
    var sw = formats[i-1];
    var test = args[i];
    if(sw!="s")
    {
      if(sw=="EndID") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 999999999) return false;
      }
      else if(sw=="PlaID") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p >= max_players) return false;
      }
      else if(sw=="nick") {
        if(nickWrong(test)) return false;
      }
      else if(sw=="Float") {
        var p = func.parseFloatP(test);
        if(isNaN(p)) return false;
      }
      else if(sw=="short") {
        if(test.length >= 32) return false;
      }
      else if(sw=="UpdateData") {
        if(test!="1")
          if(!VerifyCommand((";"+test+";0").split(";"),["Float","Float","NULL","NULL","Angle","RocketInfo","NULL","NULL","NULL","Bar","NULL","Bar"])) return false;
      }
      else if(sw=="Flags") {
        if(test.length!=4) return false;
      }
      else if(sw=="NULL") {
        if(test!="") return false;
      }
      else if(sw=="Angle") {
        var p = func.parseFloatP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 360) return false;
      }
      else if(sw=="Bar") {
        var p = func.parseFloatP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 1) return false;
      }
      else if(sw=="RocketInfo") {
        if(!VerifyCommand(("&"+test+"&0").split("&"),["OldIntInfo","NewIntInfo"])) return false;
      }
      else if(sw=="OldIntInfo") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 63) return false;
      }
      else if(sw=="NewIntInfo") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p >= 700) return false;
      }
      else if(sw=="Msg256") {
        if(test.length > 256 && !test.includes("\r") && !test.includes("\n")) return false;
      }
      else if(sw=="PushID") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 1 || p > 8) return false;
      }
      else if(sw=="ulam") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p <= 1 || p > 1700000000) return false;
      }
      else if(sw=="place") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 19) return false;
      }
      else if(sw=="turnID") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p!=45 && p!=40) return false;
      }
      else if(sw=="ID128") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 127) return false;
      }
      else if(sw=="fraction01") {
        var p = func.parseFloatP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 1) return false;
      }
      else if(sw=="UpgID") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 4) return false;
      }
      else if(sw=="Slot") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 25) return false;
      }
      else if(sw=="SlotIf") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < -1 || p > 25) return false;
      }
      else if(sw=="SlotI") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 8) return false;
      }
      else if(sw=="SlotB") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 9 || p > 25) return false;
      }
      else if(sw=="item") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 1 || p > 127) return false;
      }
      else if(sw=="count") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < -9999999 || p > 9999999) return false;
      }
      else if(sw=="count-") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < -9999999 || p > -1) return false;
      }
      else if(sw=="1,-1") {
        if(!["1","-1"].includes(test)) return false;
      }
      else if(sw=="StorageID") {
        if(!["2","52","21"].includes(test)) return false;
      }
      else if(sw=="ulamList") {
        var splitTab = test.split(";");
        var j,jngt = splitTab.length - 1;
        if(jngt >= 512) return false;
        for(j=1;j<jngt;j++)
        {
          var p = func.parseIntP(splitTab[j]);
          if(isNaN(p)) return false;
          if(p <= 1 || p > 1700000000) return false;
        }
      }
      else if(sw=="CraftID") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p >= craftings.split(";").length/6) return false;
        if(p % 7 >= 5) return false;
      }
      else if(sw=="PotionID") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 1 || p > 6) return false;
      }
      else if(sw=="DrillID") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(p < 0 || p > 15) return false;
      }
      else if(sw=="fob-interactable") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(!(
          (p>=1 && p<=19) ||
          (p>=21 && p<=23) ||
          (p>=25 && p<=38) ||
          (p>=40 && p<=48) ||
          (p==51) ||
          (p>=54 && p<=62 && p%2==0) ||
          (p>=65 && p<=70)
        )) return false;
      }
      else if(sw=="BulletType") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(![1,2,3,14,15].includes(p)) return false;
      }
      else if(sw=="Integer") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
      }
      else if(sw=="ShotTurnFob") {
        var p = func.parseIntP(test);
        if(isNaN(p)) return false;
        if(![0,23,25].includes(p)) return false;
      }
      else console.log("Error: Unknown format: "+sw);
    }
  }
  return true;
}

//Jse3 datapack converter
function crash(str) {
  console.error(str);
  process.exit(-1);
}
function constructPsPath(tab, val, n) {
  var effect = "";
  var i;
  for (i = 0; i < n; i++) effect = effect + tab[i] + ";";
  return effect + val;
}
function datapackError(str) {
  crash("Invalid datapack: " + str);
}
function translate(str, mod) {
  if (str == "") return "ERROR";

  var i;
  if (mod == 1) {
    for (i = 0; i < 16; i++) {
      if (translateAsteroid[i] == str) return i + "";
      if (translateAsteroid[i]+"A" == str) return (16+i) + "";
      if (translateAsteroid[i]+"B" == str) return (32+i) + "";
      if (translateAsteroid[i]+"C" == str) return (48+i) + "";
    }
  } else if (mod == 2) {
    for (i = 0; i < 128; i++) {
      if (translateFob[i] == str) return i + "";
    }
  }

  //If just translated
  try {
    var ipr = func.parseIntE(str) + "";
    return ipr;
  } catch {
    return "ERROR";
  }
}
function translateAll(str, mod) {
  var i,
    lngt = str.length,
    builds = 0;
  var effect = "",
    build = "",
    pom;
  var reading = true;
  var c;

  for (i = 0; i <= lngt; i++) {
    if (i != lngt) c = str[i];
    else c = ";";

    if (reading) {
      if (c != "(" && c != "+" && c != "-" && c != ";") {
        build = build + c;
      } else {
        pom = translate(build, mod);
        build = "";
        if (pom == "ERROR") return "ERROR";
        effect = effect + pom;
        builds++;
        if (c == "(") {
          effect = effect + ";";
          reading = false;
        } else effect = effect + ";1";
        if (c == "+" || c == "-") effect = effect + ";";
        if (c == "-") i++;
      }
    } else {
      if (c != ")" && c != "+" && c != "-" && c != ">" && c != ";")
        effect = effect + c;
      if (c == "+" || c == "-") effect = effect + ";";
      if (c == ">" || c == "+") reading = true;
    }
    if (c == "-" && builds == 1) effect = effect + "0;0;";
  }
  return effect;
}
function percentRemove(str) {
  var i,
    lngt = str.length;
  var effect = "";
  for (i = 0; i < lngt - 1; i++) effect = effect + str[i];
  return effect;
}
function allPercentRemove(str, must_be_1000) {
  if (str == "ERROR") return "ERROR";

  var tab = str.split(";");
  var i,
    lngt = tab.length,
    lng;
  var pom;
  var totalChance = 0,
    pre;

  for (i = 0; i < lngt; i++) {
    pom = tab[i];
    lng = pom.length;
    if (pom[lng - 1] == "%") {
      lng--;
      pom = percentRemove(pom);
      pre = totalChance;
      totalChance += func.parseIntE(func.parseFloatE(pom) * 10 + "");
      tab[i] = pre + ";" + (totalChance - 1);
    }
  }

  if (must_be_1000 && totalChance != 1000) return "ERROR";
  if (totalChance > 1000) return "ERROR";

  var effect = tab[0];
  for (i = 1; i < lngt; i++) effect = effect + ";" + tab[i];
  return effect;
}
function datapackTranslate(dataSource) {
  var dsr = dataSource.split("~");
  dataSource = "";
  var y;
  for (y = 1; y < dsr.length; y++) dataSource += dsr[y];

  var raw = "";
  var c;
  var comment = false;
  var catch_all = false;

  var i,
    lngt = dataSource.length;
  for (i = 0; i < lngt; i++) {
    c = dataSource[i];
    if (c == "<") comment = true;
    if (c == "'" && !comment) { catch_all = !catch_all; continue; }
    if((catch_all || (c!=" " && c!="\r" && c!="\n" && c!="\t")) && !comment)
    {
      if(c=="\r" || c=="\n") raw = raw + " ";
      else raw = raw + c;
    }
    if (c == ">") comment = false;
  }

  lngt = raw.length;
  if (lngt == 0) datapackError("Empty file");

  var build_path = [];
  var build = "";
  var varB = false;
  var clam_level = 0,
    varN = 0;

  for (i = 0; i < lngt; i++) {
    c = raw[i];
    if (c == "~") datapackError("Illegal symbol: " + c);
    if (!varB) {
      if (c == "{") {
        build_path[clam_level] = build;
        clam_level++;
        build = "";
      } else if (c == "}") {
        if (clam_level == 0 && build == "")
          datapackError("Unexpected symbol '}'");
        clam_level--;
        build_path[clam_level] = "";
      } else if (c == ":") {
        if (build == "") datapackError("Variable name can't be empty.");
        jse3Var[varN] = constructPsPath(build_path, build, clam_level);
        build = "";
        varB = true;
      } else if (c == ";") {
        datapackError("Unexpected symbol ';'");
      } else build = build + c;
    } else {
      if (c == "{" || c == "}" || c == ":") {
        datapackError("Unexpected symbol '" + c + "'");
      } else if (c == ";") {
        jse3Dat[varN] = build;
        build = "";
        varN++;
        varB = false;
      } else build = build + c;
    }
  }
  if (clam_level != 0)
    datapackError("Number of '{' is not equal to number of '}'");
  if (raw[lngt - 1] != ";" && raw[lngt - 1] != "}")
    datapackError("Unexpected ending");
  if (varN == 0) datapackError("No variables");

  finalTranslate(varN);
}
function finalTranslate(varN) {
  //Final translate
  var i, mID, lg;
  var mSTR;
  var cur1000biome = 0;

  //Starting actions
  for (i = 0; i < varN; i++) {
    var psPath = jse3Var[i].split(";");
    var lgt = psPath.length;
    if (lgt == 1) {
      if (psPath[0] == "version") version = jse3Dat[i];
      if (psPath[0] == "name") datName = jse3Dat[i];
    } else if (lgt == 2) {
      if (psPath[0] == "gameplay") {
        try {

          //Gameplay variable set
          var gp_num = func.VarNumber(psPath[1],gpl_number);
          if(gp_num!=-1) {
            gameplay[gp_num] = func.FilterValue(gp_num,jse3Dat[i]);
          }

        } catch {
          datapackError("Error in variable: " + jse3Var[i]);
        }
      }
      else if(psPath[0]=="custom_structures")
      {
          try{

          mID=func.parseIntE(psPath[1]);
          customStructures[mID]=jse3Dat[i].replaceAll('^',' ');

          }catch{datapackError("Error in variable: " + jse3Var[i]);}
      }
    } else if (lgt == 3) {
      if (psPath[0] == "game_translate") {
        if (psPath[1] == "Asteroids") {
          try {
            mID = func.parseIntE(psPath[2]);
            translateAsteroid[mID] = jse3Dat[i];
          } catch {
            datapackError("Error in variable: " + jse3Var[i]);
          }
        } else if (psPath[1] == "Items_and_objects") {
          try {
            mID = func.parseIntE(psPath[2]);
            translateFob[mID] = jse3Dat[i];
          } catch {
            datapackError("Error in variable: " + jse3Var[i]);
          }
        }
      }
    }
  }

  //Dictionary required actions
  var pom,
    mID2,
    crMax = 0;
  var crafts = []; //512*7
  for (i = 0; i < 3584; i++) crafts[i] = "0;0;0;0;0;0";

  for (i = 0; i < varN; i++) {
    var raw_name = jse3Var[i];
    var psPath = jse3Var[i].split(";");
    var lgt = psPath.length;

    if (lgt == 2) {
      if (
        psPath[0] == "drill_loot" ||
        psPath[0] == "objects_generate" ||
        psPath[0] == "modified_drops"
      ) {
        try {
          if (psPath[0] == "modified_drops")
            mID = func.parseIntE(translate(psPath[1], 2));
          else mID = func.parseIntE(translate(psPath[1], 1));
          jse3Var[i] = psPath[0] + ";" + mID;

          jse3Dat[i] = translateAll(jse3Dat[i], 2);
          jse3Dat[i] = allPercentRemove(jse3Dat[i], false);

          var vsl = jse3Dat[i].split(";").length;
          if (
            (vsl % 2 != 0 && psPath[0] == "modified_drops") ||
            (vsl % 3 != 0 && psPath[0] != "modified_drops")
          ) {
            datapackError("Error in variable: " + raw_name);
          }

          if (psPath[0] == "drill_loot") drillLoot[mID] = jse3Dat[i];
          if (psPath[0] == "objects_generate") fobGenerate[mID] = jse3Dat[i];
          if (psPath[0] == "modified_drops") modifiedDrops[mID] = jse3Dat[i];
        } catch {
          datapackError("Error in variable: " + raw_name);
        }
      }
    } else if (lgt == 3) {
      if (psPath[0] == "craftings") {
        try {
          mID = func.parseIntE(psPath[1]);
          if (psPath[2] == "title_image") {
            mID2 = 7;
            mID = 7 * (mID - 1) + 6;
            jse3Dat[i] = translate(jse3Dat[i], 2) + ";1;0;0;-1;1";
          } else {
            mID2 = func.parseIntE(psPath[2]);
            mID = 7 * (mID - 1) + mID2 - 1;
            jse3Dat[i] = translateAll(jse3Dat[i], 2);
          }
          jse3Var[i] = psPath[0] + ";" + mID;

          if (jse3Dat[i].split(";")[0] == "ERROR") {
            datapackError("Error in variable: " + raw_name);
          }
          if (
            jse3Dat[i].split(";").length != 6 ||
            !((mID2 >= 1 && mID2 <= 5) || psPath[2] == "title_image")
          ) {
            datapackError("Error in variable: " + raw_name);
          }
          if (jse3Dat[i].split(";")[0] == jse3Dat[i].split(";")[2]) {
            datapackError("Error in variable: " + raw_name);
          }

          crafts[mID] = jse3Dat[i];
          if (mID > crMax) crMax = mID;
        } catch {
          datapackError("Error in variable: " + raw_name);
        }
      } else if (psPath[0] == "generator_settings") {
        try {
          mID = func.parseIntE(psPath[1]);
          if (mID < 0 || mID > 31) throw "error";

          if (psPath[2] == "settings") {
            biomeTags[mID] = jse3Dat[i].replaceAll(" ", "_");
          } else if (psPath[2] != "chance") {
            if (psPath[2] == "all_sizes") mID2 = -4;
            else mID2 = func.parseIntE(psPath[2]) - 4;
            if ((mID2 < 0 || mID2 > 6) && mID2 != -4) throw "error";

            if (mID2 != -4) mID = 7 * mID + mID2;
            else mID = 7 * mID;
            jse3Var[i] = psPath[0] + ";" + mID;

            jse3Dat[i] = translateAll(jse3Dat[i], 1);
            jse3Dat[i] = allPercentRemove(jse3Dat[i], true);

            if (jse3Dat[i].split(";").length % 3 != 0) {
              datapackError("Error in variable: " + raw_name);
            }

            if (mID2 != -4) typeSet[mID] = jse3Dat[i];
            else {
              var uu;
              for (uu = 0; uu < 7; uu++) typeSet[mID + uu] = jse3Dat[i];
            }
          }
        } catch {
          datapackError("Error in variable: " + raw_name);
        }
      }
    }
  }

  //Late actions
  for (i = 0; i < varN; i++) {
    var raw_name = jse3Var[i];
    var psPath = jse3Var[i].split(";");
    var lgt = psPath.length;

    if (lgt == 3) {
      if (psPath[0] == "generator_settings") {
        try {
          mID = func.parseIntE(psPath[1]);
          if (mID < 0 || mID > 31) throw "error";

          if (psPath[2] == "chance") {
            if (mID == 0) throw "error";
            var efe = mID + ";" + cur1000biome + ";";

            var le = jse3Dat[i].length;
            if (jse3Dat[i][le - 1] == "%")
              jse3Dat[i] = percentRemove(jse3Dat[i]);
            else throw "error";

            var mno;
            if (tagContains(biomeTags[mID], "structural")) mno = 2;
            else mno = 1;

            cur1000biome += mno * func.parseIntE(func.parseFloatE(jse3Dat[i]) * 10 + "");
            efe += cur1000biome - 1 + ";";
            biomeChances += efe;
          }
        } catch {
          datapackError("Error in variable: " + raw_name);
        }
      }
    }
  }

  craftings = crafts[0];
  for (i = 1; i <= crMax; i++) {
    craftings = craftings + ";" + crafts[i];
  }
  craftMaxPage = ~~(crMax / 7) + 1 + "";

  //last biome chance correction
  if (biomeChances != "") biomeChances = percentRemove(biomeChances);

  //Check if all is good
  for (i = 0; i < gpl_number; i++)
    if (gameplay[i] == "") {
      datapackError("Required variable not found: gameplay_" + i);
    }
  if (cur1000biome > 1000) {
    datapackError(
      "Total biome chance can't be over 1000p. Current: " +
        cur1000biome +
        "p. 1p = 0,1%.\r\nNote: structural option doubles 'p' ussage, but the chance is still multiplied by 1."
    );
  }

  if (version != serverVersion) {
    datapackError("Wrong version or empty version variable");
  }
  if (datName == "") {
    datapackError("Datapack name can't be empty");
  }

  try {
    checkDatapackGoodE();
  } catch {
    datapackError("Unknown error detected");
  }
}

function tagContains(tags, tag) {
  return (
    tags
      .replaceAll("[", "_")
      .replaceAll("]", "_")
      .replaceAll("_", ",")
      .split(",")
      .indexOf(tag) > -1
  );
}

function intsAll(str, div) {
  var strs = str.split(";");
  var i = 0,
    lngt = strs.length;
  var pom;

  try {
    if (str != "")
      for (i = 0; i < lngt; i++) {
        pom = func.parseIntE(strs[i]);
      }
  } catch {
    return false;
  }

  if (i % div == 0) return true;
  else return false;
}

function in1000(str, must_be_1000) {
  var strT = str.split(";");
  var i,
    lngt = strT.length;
  if (str == "") lngt = 0;

  var ended = true;
  var actual = -1;
  for (i = 1; i < lngt; i++) {
    if (ended) {
      if (actual + 1 != func.parseIntE(strT[i])) return false;
      actual++;
      ended = false;
    } else {
      if (actual > func.parseIntE(strT[i]) + 1) return false;
      actual = func.parseIntE(strT[i]);
      ended = true;
      i++;
    }
  }
  if (!must_be_1000 && actual <= 999) return true;
  if (must_be_1000 && actual == 999) return true;
  return false;
}

function goodItems(str, craft_mode) {
  var strT = str.split(";");
  var i,
    lngt = strT.length;
  if (str == "") lngt = 0;

  for (i = 0; i < lngt; i += 2) {
    if (func.parseIntE(strT[i + 1]) < 0) return false;
    if (strT[i] == "0" && strT[i + 1] != "0") return false;
    if (strT[i + 1] == "0" && strT[i] != "0") return false;
  }
  if (craft_mode)
    for (i = 0; i < lngt; i += 6) {
      if (
        strT[i] != "0" &&
        strT[i + 1] != "0" &&
        strT[i + 2] != "0" &&
        strT[i + 3] != "0" &&
        strT[i + 4] != "0" &&
        strT[i + 5] != "0"
      )
        if (
          strT[i + 4] == "0" ||
          strT[i] == "0" ||
          strT[i] == strT[i + 2] //||
          //strT[i + 2] == strT[i + 4] ||
          //strT[i + 4] == strT[i]
        )
          return false;
    }
  return true;
}

function drillGoodItem(str) {
  var strT = str.split(";");
  var i,
    lngt = strT.length;
  if (str == "") lngt = 0;

  for (i = 0; i < lngt; i += 3) {
    if (strT[i + 2] == "0") return false;
  }
  return true;
}

//MUST BE INSIDE try{} catch{}
function checkDatapackGoodE() {
  var i;

  //Check int arrays
  if (!intsAll(craftings, 6) || !goodItems(craftings, true)) throw "error";
  if (!intsAll(biomeChances, 3) || !in1000(biomeChances, false)) throw "error";
  for (i = 0; i < 16; i++) {
    if (
      !intsAll(drillLoot[i], 3) ||
      !in1000(drillLoot[i], false) ||
      !drillGoodItem(drillLoot[i])
    )
      throw "error";
  }
  for (i = 0; i < 64; i++) {
    if (!intsAll(fobGenerate[i], 3) || !in1000(fobGenerate[i], false)) throw "error";
  }
  for (i = 0; i < 224; i++) {
    if (typeSet[i] != "")
      if (!intsAll(typeSet[i], 3) || !in1000(typeSet[i], true)) throw "error";
  }
  for (i = 0; i < 128; i++) {
    if (!intsAll(modifiedDrops[i], 2) || !goodItems(modifiedDrops[i], false))
      throw "error";
  }
}

function datapackPaste(splitTab) {
  var dsr = splitTab.split("~");
  splitTab = "";
  var y;
  for (y = 1; y < dsr.length; y++) splitTab += dsr[y] + "~";

  var i;
  var raws = splitTab.split("~");

  try {
    //Load data
    craftings = raws[0];
    biomeChances = raws[8];
    craftMaxPage = func.parseIntE(raws[1]) + "";

    for (i = 0; i < 16; i++) drillLoot[i] = raws[2].split("'")[i];
    for (i = 0; i < 64; i++) fobGenerate[i] = raws[3].split("'")[i];
    for (i = 0; i < 224; i++) typeSet[i] = raws[4].split("'")[i];
    for (i = 0; i < gpl_number; i++) {
      if (i==105||i==106) gameplay[i] = raws[5].split("'")[i];
      else gameplay[i] = func.parseFloatE(raws[5].split("'")[i]) + "";
    }
    for (i = 0; i < 128; i++) modifiedDrops[i] = raws[6].split("'")[i];
    for (i = 0; i < 32; i++) biomeTags[i] = raws[7].replaceAll(" ", "_").split("'")[i];
    for (i = 0; i < 32; i++) customStructures[i] = raws[9].replaceAll("^"," ").split("'")[i];

    checkDatapackGoodE();
  } catch {
    crash(
      "Failied loading imported datapack\r\nDelete " + universe_name + "/UniverseInfo.se3 file and try again"
    );
  }
}

function GplGet(str)
{
    return func.parseFloatU(gameplay[func.VarNumber(str,gpl_number)]);
}

//Start functions
console.log("-------------------------------");

if (!existsF(universe_name + "/UniverseInfo.se3"))
{
  var datapackjse3, defaultjse3 = readF("server_technicals/DefaultDatapack.jse3");
  if(existsF("Datapack.jse3"))
  {
    datapackjse3 = readF("Datapack.jse3");
    datapackTranslate("NoName~" + datapackjse3);
  }
  else crash("File Datapack.se3 doesn't exist");

  if(datapackjse3==defaultjse3) verF = "DEFAULT";
  else verF = "Custom Data";

  clientDatapacksVar = clientDatapacks();
  uniTime = 0;
  uniMiddle = verF + "~" + clientDatapacksVar;
  uniVersion = serverVersion;
  writeF(
    universe_name + "/UniverseInfo.se3",
    [uniTime, uniMiddle, uniVersion, ""].join("\r\n")
  );

  if(verF=="Custom Data" && datName=="DEFAULT") console.log("Datapack imported: CUSTOM");
  else console.log("Datapack imported: " + datName + "");
}
else
{
  var uiSource = readF(universe_name + "/UniverseInfo.se3").split("\r\n");
  if (uiSource.length < 3) crash("Error in " + universe_name + "/UniverseInfo.se3");
  if (uiSource[2] != serverVersion)
    crash(
      "Loaded universe has a wrong version: " +
        uiSource[2] +
        " != " +
        serverVersion +
        "\r\nYou can update your universe by removing file " + universe_name + "/UniverseInfo.se3" +
        "\r\nBe sure that file Datapack.jse3 contains a default datapack before updating." +
        "\r\nNote that universe updating is supported only when updating Beta 2.1 or newer universes\r\nand only when they use a default datapack."
    );

  var dataGet = uiSource[1].split("~");
  if (dataGet.length < 2) crash("Error in " + universe_name + "/UniverseInfo.se3");
  datapackPaste(uiSource[1]);

  clientDatapacksVar = clientDatapacks();
  uniTime = 0;
  uniMiddle = uiSource[1];
  uniVersion = serverVersion;
  writeF(
    universe_name + "/UniverseInfo.se3",
    [uniTime, uniMiddle, uniVersion, ""].join("\r\n")
  );

  console.log("Datapack loaded");
}

//Seed read
if (!existsF(universe_name + "/Seed.se3")) {
  seed = func.randomInteger(0, 10000000);
  writeF(universe_name + "/Seed.se3", seed + "\r\n");
  console.log("New seed generated: " + seed + "");
}
else {
  seed = func.parseIntU(readF(universe_name + "/Seed.se3").split("\r\n")[0]);
  writeF(universe_name + "/Seed.se3", seed + "\r\n");
}

//Biome memories read
var dii;
for(dii=0;dii<16000;dii++) biome_memories[dii] = "";

if (existsF(universe_name + "/Biomes.se3")) {
  var rtr = readF(universe_name + "/Biomes.se3").split("\r\n");
  for(dii=0;(dii<16000 && dii<rtr.length);dii++)
    biome_memories[dii] = rtr[dii];
}

function laggy_comment(nn)
{
  if(nn<=0) return "\r\nWarning: Can't join to server with "+max_players+" max players.";
  if(nn>=1 && nn<=25) return "";
  if(nn>=26 && nn<=150) return "\r\nWarning: Over 25 max players can cause lags on some weaker devices";
  if(nn>=151 && nn<=500) return "\r\nWarning: Over 150 max players can cause lags on most devices";
  if(nn>=501) return "\r\nWarning: Too many players! Shut it down! Your device might explode, but of course you can try joining ;)";
}

//Awake gameplay set
boss_damages[4] = GplGet("boss_bullet_electron_damage");
boss_damages[5] = GplGet("boss_bullet_fire_damage");
boss_damages[6] = GplGet("boss_bullet_spike_damage");
boss_damages[7] = GplGet("boss_bullet_brainwave_damage");
boss_damages[9] = GplGet("boss_bullet_rocket_damage");
boss_damages[10] = GplGet("boss_bullet_spikeball_damage");
boss_damages[11] = GplGet("boss_bullet_copper_damage");
boss_damages[12] = GplGet("boss_bullet_red_damage");
boss_damages[13] = GplGet("boss_bullet_unstable_damage");
boss_damages[16] = GplGet("boss_bullet_graviton_damage");
boss_damages[17] = GplGet("boss_bullet_neutronium_damage");

var gsol5a = 50 * Math.floor(GplGet("amethyst_grow_time_min"));
var gsol5b = 50 * Math.floor(GplGet("amethyst_grow_time_max"));
var gsol25 = 50 * Math.floor(GplGet("magnetic_alien_grow_time"));

//plr class creation using datapack
for(ki=0;ki<max_players;ki++)
  plr.pclass.push(new CPlayer(ki));

if(gsol5a<=0) gsol5a = 50;
if(gsol5b<=0) gsol5b = 50;
if(gsol25<=0) gsol25 = 50;

if(gsol5a > gsol5b)
{
  var gsolpom = gsol5a;
  gsol5a = gsol5b;
  gsol5b = gsolpom;
}

unstable_sprobability = Math.floor(func.parseFloatU(gameplay[24])*50+1);
if(unstable_sprobability < 1) unstable_sprobability = 1;

growSolid[5] = gsol5a +";"+ gsol5b +";6";
growSolid[6] = gsol5a +";"+ gsol5b +";7";
growSolid[25] = gsol25 +";"+ gsol25 +";23";

//Starting ending
console.log("Server started on version: " + serverVersion + "");
console.log("Universe directory: " + universe_name + "");
console.log("Max players: " + max_players + "");
console.log("Port: " + connectionOptions.port + "");
console.log("Server address: " + connectionAddress + "" + laggy_comment(max_players));
console.log("-------------------------------");

updateHourHeader();
setTerminalTitle("SE3 server | "+serverVersion+" | "+getRandomFunnyText());

//END OF MAIN SEGMENT

})
.catch((error) => {
  console.log("\nClosing the server: "+error);
});
