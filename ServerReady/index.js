const WebSocket = require("ws");
const fs = require("fs");
const path = require("path");
const { runInNewContext } = require("vm");
const { parse } = require("path");
const { func } = require("./functions");
const { Parsing } = require("./functions");
const { CBoss } = require("./behaviour");
const readline = require('readline');
const readline_sync = require('readline-sync');

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

//File functions
function crash(str) {
  console.error(str);
  process.exit(-1);
}
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

console.log("Starting procedures...");

//Config generator
const default_config = {
	"universe_name": "ServerUniverse",
	"max_players": 10,
	"max_connections_per_ip": 10,
  "trust_proxy": false,
	"port": 27683,
	"pvp": true,
  "difficulty": 2,
  "keep_inventory": false,
	"show_positions": true,
	"require_se3_account": false,
  "authorization_waiting_time": 15,
  "max_dict_size": 128,
  "max_active_bosses": 16,
	"whitelist_enabled": false,
	"whitelist": [],
	"banned_players": [],
	"banned_ips": [],
	"anti_cheat": {
    "max_movement_speed": 100,
		"bullet_spawn_allow_radius": 3,
    "power_speculative_minimum_value": -0.2,
    "max_interaction_range": 150,
    "max_worldgen_range": 250,
    "max_player_updates_per_3_seconds": 180,
    "max_messages_per_second": 1200,
    "allow_everywhere_spawn": false
	}
};
if(!existsF("./config.json")) {
  writeF("config.json",JSON.stringify(default_config,null,2));
  console.log("File config.json generated.");
}
const config = require("./config.json");
if(config.difficulty<0 || config.difficulty>5) config.difficulty=2;

//AuthConfig generator
const default_authConfig = {
	"is_configured": false,
	"auth_server_url": "wss://comp.se3.page:27684",
	"host_nickname": "SE3_nickname",
	"host_password": "SE3_password",
	"server_name": "connect.through.this.name",
	"server_redirect_address": "ws://0.0.0.0:27683"
};
if(!existsF("./authConfig.json")) {
  writeF("authConfig.json",JSON.stringify(default_authConfig,null,2));
  console.log("File authConfig.json generated.");
}
const authConfig = require("./authConfig.json");
const { randomFillSync } = require("crypto");

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
          //Add nick with conID for X seconds
          waiting_authorized.push([arg[1],arg[2],config.authorization_waiting_time]);
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
var biome_memories = new Array(16000).fill("");
var biome_memories_state = new Array(16000).fill(0);
var hourHeader = "";
var gpl_number = 125;
var max_players = 10;
var verF;
var connectionAddress = "IP or DNS + port";
if(config.require_se3_account) connectionAddress = "se3://" + authConfig.server_name;

var IPv4s = [];

var boss_damages = [0,0,0,0,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,0,0 ,-1,-1,0,0,0,0,0,0,0,0,0,0,0,0,0,0];
var other_bullets_colliders = [0,0.14,0.14,0.12,1,0.25,0.25,1.2,1.68,0.92,0.92,0.25,0.25,0.25,0.08,0.08 ,1.68,0.25,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.08];
var bullet_air_consistence = [0,0,0,1,0,1,0,1,0,0,0,0,0,1,1,1 ,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0];

if(Number.isInteger(config.max_players))
  max_players = config.max_players;

var tab_difficulty = [0,0.7,1,1.43,2,10000];
var difficulty = tab_difficulty[config.difficulty];
var difficulty_name_tab = ["NONE","EASY","NORMAL","HARD","DIABOLIC","IMPOSSIBLE"];

var universe_name = "ServerUniverse";
if(config.universe_name) universe_name = config.universe_name;

var chunk_data = [];
var chunk_names = [];
var chunk_waiter = [];

var se3_ws = new Array(max_players);
var se3_wsS = new Array(max_players);
se3_ws.fill(""); Object.seal(se3_ws);
se3_wsS.fill(""); Object.seal(se3_wsS);

const MsgMap = new Map();

var memTemplate = {
  nicks: "0",
  health: "1",
  others1: "0",
  others2: "0",
  rposX: "0",
  rposY: "0",
};

var unstable_sprobability = 1;

const give_array = [];

function TreasureDrop(str)
{
  try {
      var marray = [];
      var i,j,rander=func.randomInteger(0,9999);
      var arra = str.split("-");
      var lngt=Math.floor(arra.length/5);
      for(i=0;i<lngt;i++)
      {
        for(j=0;j<5;j++) marray[j]=Parsing.IntE(arra[5*i+j]);
        if(rander>=marray[3]&&rander<=marray[4]) return marray[0]+";"+func.randomInteger(marray[1],marray[2]);
      }
    } catch { return "8;1"; }
    return "8;1";
}

function getAllSuchFiles(folderPath,ext) {
  try{
      const files = fs.readdirSync(folderPath);
      const textFiles = files.filter(file => {
          const filePath = path.join(folderPath, file);
          const fileStats = fs.statSync(filePath);
          return fileStats.isFile() && file.endsWith(ext);
      });
      return textFiles;
  }catch{ return []; }
}

class Vector3
{
    constructor(x, y, z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    Length()
    {
        return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
    }

    static Add(v1, v2) {
      return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }
    static Subtract(v1, v2) {
      return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }
    static Multiply(v, scalar) {
      return new Vector3(v.x * scalar, v.y * scalar, v.z * scalar);
    }
    static Divide(v, scalar) {
      return new Vector3(v.x / scalar, v.y / scalar, v.z / scalar);
    }
}

// ------------------------------------- \\
// ------------- GENERATOR ------------- \\
// ------------------------------------- \\


// RANDOMIZATION CLASS

const long1 = readF("./technical_data/AsteroidBase.se3");
const long2 = readF("./technical_data/BiomeNewBase.se3");
const long3 = readF("./technical_data/AsteroidSizeBase.se3");
class Deterministics
{
    //Fully deterministic functions
    static Random10e2(sour)
    {
		    sour = sour % 15000;
		    let psInt = (long2[2*sour+0]+"") + (long2[2*sour+1]+"");
		    return Parsing.IntU(psInt);
	  }
	  static Random10e3(sour) //long2 works best for 10e3
    {
		    sour = sour % 10000;
		    let psInt = (long2[3*sour+0]+"") + (long2[3*sour+1]+"") + (long2[3*sour+2]+"");
		    return Parsing.IntU(psInt);
	  }
	  static Random10e4(sour)
    {
		    sour = sour % 7500;
		    let psInt = (long2[4*sour+0]+"") + (long2[4*sour+1]+"") + (long2[4*sour+2]+"") + (long2[4*sour+3]+"");
		    return Parsing.IntU(psInt);
	  }
	  static Random10e5(sour)
    {
		    sour = sour % 6000;
		    let psInt = (long2[5*sour+0]+"") + (long2[5*sour+1]+"") + (long2[5*sour+2]+"") + (long2[5*sour+3]+"") + (long2[5*sour+4]+"");
		    return Parsing.IntU(psInt);
	  }
    static AnyRandom(div,sour)
    {
        return Deterministics.Random10e5(sour) % div;
    }
    static CalculateFromString(chance_string,sour)
    {
        let decider = Deterministics.Random10e3(sour);
        const s_nums = chance_string.split(';');
        let i,lngt = Math.floor(s_nums.length/3), V,A,B;
        for(i=0;i<lngt;i++)
        {
            V = Parsing.IntU(s_nums[3*i + 0]);
            A = Parsing.IntU(s_nums[3*i + 1]);
            B = Parsing.IntU(s_nums[3*i + 2]);
            if(decider>=A && decider<=B) return V;
        }
        return 0;
    }
}


// GENERATION LAYER 1 (outer) -> Biome and structure specify

class CBiomeInfo
{
    constructor(ulam)
    {
        this.biome = Generator.GetBiome(ulam);
        this.size = Generator.GetBiomeSize(ulam,this.biome);
        this.move = Generator.GetBiomeMove(ulam,this.biome,this.size);
    }
}

class Generator
{
    static seed;

    static tag_min = new Array(32).fill();
    static tag_max = new Array(32).fill();
    static tag_density = new Array(32).fill();
    static tag_priority = new Array(32).fill();
    static tag_struct = new Array(32).fill();
    static tag_gradient = new Array(32).fill();
    static tag_grid = new Array(32).fill();
    static tag_spawn = new Array(32).fill();
    static tag_centred = new Array(32).fill();
    static tag_odd = new Array(32).fill();
    static tag_even = new Array(32).fill();
    static tag_structural = new Array(32).fill();

    static max_dict_size = config.max_dict_size;
    static WorldMap = new Map();

    //Initializes seed in generator, returns the actual seed string
    static SetSeed(s_seed)
    {
        if(Parsing.IntC(s_seed))
        {
            Generator.seed = Parsing.IntU(s_seed);
            return s_seed;
        }
        else
        {
            Generator.seed = func.randomInteger(0,9999999);
            return Generator.seed+"";
        }
    }

    //Converts all biome tags into arrays
    static TagNumbersInitialize()
    {
        let i,j;
        for(i=0;i<32;i++)
        {
            Generator.tag_min[i] = 65;
            Generator.tag_max[i] = 80;
            Generator.tag_density[i] = 60;
            Generator.tag_priority[i] = 16;
            Generator.tag_struct[i] = 0;
            Generator.tag_gradient[i] = 80;
            Generator.tag_grid[i] = false;
            Generator.tag_spawn[i] = false;
            Generator.tag_centred[i] = false;
            Generator.tag_odd[i] = false;
            Generator.tag_even[i] = false;
            Generator.tag_structural[i] = false;

            let tags = biomeTags[i];

            for(j=0;j<=80;j++)
                if(tagContains(tags,"min="+j)) Generator.tag_min[i]=j;
            for(j=0;j<=80;j++)
                if(tagContains(tags,"max="+j)) Generator.tag_max[i]=j;
            for(j=0;j<=80;j++)
                if(tagContains(tags,"radius="+j)) { Generator.tag_min[i]=j; Generator.tag_max[i]=j; }
            
            for(j=0;j<=100;j++)
                if(tagContains(tags,"density="+j+"%")) Generator.tag_density[i]=j;
            for(j=1;j<=31;j++)
                if(tagContains(tags,"priority="+j)) Generator.tag_priority[i]=j;
            for(j=1;j<=31;j++)
                if(tagContains(tags,"struct="+j)) Generator.tag_struct[i]=j;

            for(j=0;j<=80;j++)
                if(tagContains(tags,"gradient="+j)) Generator.tag_gradient[i]=j;

            if(tagContains(tags,"grid")) Generator.tag_grid[i] = true;
            if(tagContains(tags,"spawn")) Generator.tag_spawn[i] = true;
            if(tagContains(tags,"centred")) Generator.tag_centred[i] = true;
            if(tagContains(tags,"odd")) Generator.tag_odd[i] = true;
            if(tagContains(tags,"even")) Generator.tag_even[i] = true;
            if(tagContains(tags,"structural")) Generator.tag_structural[i] = true;

            if(Generator.tag_min[i] > Generator.tag_max[i])
            {
                let pom = Generator.tag_min[i];
                Generator.tag_min[i] = Generator.tag_max[i];
                Generator.tag_max[i] = pom;
            }

            if(Generator.tag_structural[i]) Generator.tag_priority[i] = 32;
            else Generator.tag_struct[i] = 0;
        }

        Generator.tag_priority[0] = 0;
        Generator.tag_struct[0] = 0;
        Generator.tag_structural[0] = false;
    }

    //Generator partly independent methods
    static MixID(ID,sed)
    {
        return ID+sed*2;
    }
	  static MoveVariant(size)
	  {
      if(size<=80 && size>=71) return 1;
      if(size<=70 && size>=61) return 2;
      if(size<=60 && size>=40) return 3;
      if(size<=39 && size>=30) return 2;
      if(size<=29 && size>=20) return 1;
      return 0;
	  }
	  static IsBiggerPriority(ulam1, ulam2, prio1, prio2)
	  {
		    if(prio1 > prio2) return true;
		    else if(prio1 == prio2) {
			      let rnd1 = Deterministics.Random10e2(ulam1+Generator.seed);
			      let rnd2 = Deterministics.Random10e2(ulam2+Generator.seed);
			      if(rnd1 > rnd2) return true;
			      else if(rnd1 == rnd2) {
				        if(ulam1 > ulam2) return true;
			      }
		    }
        return false;
	  }

    //Dictionary access
    static GetBiomeData(ulam)
    {
        if(!Generator.WorldMap.has(ulam))
        {
            let lngt = Generator.WorldMap.size;
            if(lngt >= Generator.max_dict_size) Generator.WorldMap.clear();
            Generator.WorldMap.set(ulam,new CBiomeInfo(ulam));
            return Generator.WorldMap.get(ulam);
        }
        else return Generator.WorldMap.get(ulam);
    }

    //Basement functions
    static GetBiome(ulam)
    {
        //Unconditional biome 0
        if((ulam>=2 && ulam<=9) || ulam%2==0) return 0;
        let XY = ulamToXY(ulam);
        if(XY[0] < -2000 || XY[0] >= 2000) return 0;
        if(XY[1] < -2000 || XY[1] >= 2000) return 0;

        //Memories check and generate
        let biome = FindBiome(ulam);
        if(biome==-1)
        {
            let i;
            if(ulam==1)
            {
                biome = 0;
                for(i=1;i<=31;i++)
                    if(Generator.tag_spawn[i]) biome = i;
            }
            else
            {
                biome = Deterministics.CalculateFromString(biomeChances,ulam+Generator.seed);
            }
            InsertBiome(ulam,biome);
        }

        //Structures erase
        if(!Generator.tag_structural[biome]) return biome;
        else
        {
            const XY = ulamToXY(ulam);
            if(!(XY[0]%2==0 || XY[1]%2==0)) return 0;
            else return biome;
        }
    }
    static GetBiomeSize(ulam,biome)
    {
        if(biome==0) return -1;
		    let ps_rand = Deterministics.Random10e4(ulam+Generator.seed) % ((Generator.tag_max[biome]-Generator.tag_min[biome])+1);
		    return Generator.tag_min[biome] + ps_rand;
    }
    static GetBiomeMove(ulam,biome,size)
    {
        let move_variant = Generator.MoveVariant(size);
        let table_size = move_variant*2 + 1;
        let table_square = table_size * table_size;
        if(Generator.tag_centred[biome] || move_variant==0) return [0,0];

        let field_num = Deterministics.Random10e3(ulam+Generator.seed) % table_square + 1;
        if(Generator.tag_even[biome] && field_num % 2 != 0)
        {
            if(field_num != table_square) field_num++;
            else field_num--;
        }
        if(Generator.tag_odd[biome] && field_num % 2 == 0)
        {
            field_num++;
        }
        let xy = ulamToXY(field_num);
        return [xy[0]*10,xy[1]*10];
    }
}


// GENERATION LAYER 2 (middle) -> Gameplay objects create

class CObjectInfo
{
    constructor(p_ulam,start_pos)
    {
        //Indicators
        this.ulam = p_ulam;
        this.obj = "unknown";
        this.animator = -1;
        this.animator_reference = null;

        //Transform
        this.default_position = start_pos;
        this.position = start_pos;
        this.rotation = 0;
        this.fob_positions = new Array(20).fill(null);
        this.fob_rotations = new Array(20).fill(0);

        //Properties
        this.type = -1;
        this.size = -1;
        this.biome = -1;
        this.range = 0;
        this.size1 = 0;
        this.size2 = 0;
        this.hidden = false;
        this.fobcode = "";

        //Animations
        this.animation_type = 0;
        this.animation_size = new Vector3(0,0,0);
        this.animation_when_doing = "";
        this.animation_when_done = "";
        this.animation_when_undoing = "";
    }
    GetGencode()
    {
        if(this.biome==-1) return this.size + "t" + this.type + "t" + this.fobcode;
        else return this.size + "b" + this.biome + "b" + this.fobcode;
    }

    //Summoners
    Asteroid(p_size, p_type, p_fobcode)
    {
        this.size = Universe.RangedIntParse(p_size,4,10);
        this.type = Universe.RangedIntParse(p_type,0,63);
        let fob_array = (p_fobcode+",,,,,,,,,,,,,,,,,,,").split(',');

        let i;
        for(i=0;i<20;i++)
        {
            let t = Universe.RangedIntParse(fob_array[i],-1,127);
            if(t!=-1) this.fobcode += t;
            if(i!=19) this.fobcode += ";";

            this.fob_rotations[i] = -(360/(this.size*2))*i;
            this.fob_positions[i] = Vector3.Add(this.position,Vector3.Multiply(new Vector3(
                Math.sin(-this.fob_rotations[i]*Math.PI/180),
                Math.cos(-this.fob_rotations[i]*Math.PI/180),
            0),this.size/2));
        }
        this.obj = "asteroid";
    }
    Wall(p_size1, p_size2, p_type)
    {
        this.size1 = Universe.PositiveFloatParse(p_size1,false);
        this.size2 = Universe.PositiveFloatParse(p_size2,false);
        this.type = Universe.RangedIntParse(p_type,0,15);

        this.obj = "wall";
    }
    Sphere(p_size1, p_type)
    {
        this.size1 = Universe.PositiveFloatParse(p_size1,false);
        this.type = Universe.RangedIntParse(p_type,0,15);

        this.obj = "sphere";
    }
    Piston(p_size1, p_size2, p_type)
    {
        this.size1 = Universe.PositiveFloatParse(p_size1,false);
        this.size2 = Universe.PositiveFloatParse(p_size2,false);
        this.type = Universe.RangedIntParse(p_type,0,15);

        this.obj = "piston";
    }
    Ranger(p_range, p_obj)
    {
        this.range = Universe.PositiveFloatParse(p_range,false);

        this.obj = p_obj;
    }
    Spherical(p_obj)
    {
        this.size1 = 1;

        this.obj = p_obj;
    }
    Boss(p_type)
    {
        this.type = Universe.RangedIntParse(p_type,0,6);

        this.animation_type = 1;
        this.animation_when_doing = "b1a1;b2a2;b3a3;";
        this.animation_when_done = "default;A1;A2;A3;R;b1a2;b2a3;b3r;";
        this.animation_when_undoing = "a1b1;a2b2;a3b3;";

        this.obj = "boss";
    }

    //Modifiers
    SetBiomeBase(p_biome)
    {
        if(this.obj!="asteroid") return;

        this.biome = Universe.RangedIntParse(p_biome,0,31);
        this.type=-1;
    }
    Move(p_x, p_y)
    {
        if(this.obj=="boss") return;

        let x = Universe.PositiveFloatParse(p_x,true);
        let y = Universe.PositiveFloatParse(p_y,true);
        let get = func.RotatePoint([x,y],this.rotation);
        this.position = Vector3.Add(this.position, new Vector3(get[0],get[1],0));

        if(this.obj=="asteroid")
        {
            let i;
            for(i=0;i<20;i++)
            {
                this.fob_positions[i] = Vector3.Add(this.fob_positions[i], new Vector3(get[0],get[1],0));
            }
        }
    }
    Rotate(p_rot)
    {
        if(this.obj=="boss") return;

        let rot = Universe.PositiveFloatParse(p_rot,true);
        this.rotation += rot;
        
        if(this.obj=="asteroid")
        {
            let i;
            for(i=0;i<20;i++)
            {
                let raw_delta_pos = Vector3.Subtract(this.fob_positions[i], this.position);
                let delta_pos = func.RotatePoint([raw_delta_pos.x,raw_delta_pos.y],-this.fob_rotations[i]);
                this.ResetS(i,"position");
                this.RotateS(i,rot+"");
                this.MoveS(i,delta_pos[0]+"",delta_pos[1]+"");
            }
        }
    }
    Reset(p_what)
    {
        if(this.obj=="boss") return;

        if(p_what=="position")
        {
            let delta_pos = Vector3.Subtract(this.position, this.default_position);
            this.position = Vector3.Subtract(this.position, delta_pos);

            if(this.obj=="asteroid")
            {
                let i;
                for(i=0;i<20;i++)
                    this.fob_positions[i] = Vector3.Subtract(this.fob_positions[i], delta_pos);
            }
        }
        if(p_what=="rotation") this.Rotate((-this.rotation)+"");
    }

    //Child modifiers
    MoveS(S, p_x, p_y)
    {
        if(this.obj!="asteroid") return;

        let x = Universe.PositiveFloatParse(p_x,true);
        let y = Universe.PositiveFloatParse(p_y,true);
        let get = func.RotatePoint([x,y],this.fob_rotations[S]);
        this.fob_positions[S] = Vector3.Add(this.fob_positions[S], new Vector3(get[0],get[1],0));
    }
    RotateS(S, p_rot)
    {
        if(this.obj!="asteroid") return;

        let rot = Universe.PositiveFloatParse(p_rot,true);
        this.fob_rotations[S] = this.fob_rotations[S] + rot;
    }
    ResetS(S, p_what)
    {
        if(this.obj!="asteroid") return;

        if(p_what=="position") this.fob_positions[S] = this.position;
        if(p_what=="rotation") this.fob_rotations[S] = this.rotation;
    }

    //Animation initializer
    AnimationCreate(p_type, p_when, p_dx, p_dy)
    {
        if(this.obj!="animator") return;

        this.animation_when_doing = "";
        this.animation_when_done = "";
        this.animation_when_undoing = "";

        let i;
        let given_states = p_when.split(',');
        let st_array = ["R","A1","A2","A3","B1","B2","B3","default"];
        let have = new Array(8);

        //Static states
        for(i=0;i<8;i++)
        {
            have[i] = given_states.includes(st_array[i]);
            if(have[i]) this.animation_when_done += st_array[i]+";";
        }

        //Activating & losing
        for(i=1;i<=3;i++)
        {
            if(have[i] && have[i+3]) {
                this.animation_when_done += "a"+i+"b"+i+";";
                this.animation_when_done += "b"+i+"a"+i+";";
            }
            if(have[i] && !have[i+3]) {
                this.animation_when_undoing += "a"+i+"b"+i+";";
                this.animation_when_doing += "b"+i+"a"+i+";";
            }
            if(!have[i] && have[i+3]) {
                this.animation_when_doing += "a"+i+"b"+i+";";
                this.animation_when_undoing += "b"+i+"a"+i+";";
            }
        }

        //Winning
        for(i=1;i<=2;i++)
        {
            if(have[i+1] && have[i+3]) this.animation_when_done += "b"+i+"a"+(i+1)+";";
            if(have[i+1] && !have[i+3]) this.animation_when_doing += "b"+i+"a"+(i+1)+";";
            if(!have[i+1] && have[i+3]) this.animation_when_undoing += "b"+i+"a"+(i+1)+";";
        }

        //Completing
        if(have[0] && have[6]) this.animation_when_done += "b3r;";
        if(have[0] && !have[6]) this.animation_when_doing += "b3r;";
        if(!have[0] && have[6]) this.animation_when_undoing += "b3r;";
        
        //Animation
        if(p_type=="hiding")
        {
            this.animation_type = 1;
        }
        if(p_type=="extending")
        {
            this.animation_type = 2;
            let dx = Universe.PositiveFloatParse(p_dx,true);
            let dy = Universe.PositiveFloatParse(p_dy,true);
            this.animation_size = new Vector3(dx,dy,0);
        }
    }
}

class Universe
{
    static max_dict_size = config.max_dict_size;
    static Sectors = new Map();

    //Public methods
    static GetObject(ulam)
    {
        let sector_name = this.GetSectorNameByUlam(ulam);
        let sector = this.GetSector(sector_name);
        for(let i = 0 ; i < sector.length ; i++)
        {
            let obj = sector[i];
            if(obj!=null) if(obj.ulam==ulam)
                return obj;
        }
        return null;
    }
    static GetSector(sector_name)
    {
        if(this.Sectors.has(sector_name)) return this.Sectors.get(sector_name);

        let spl = sector_name.split('_');
        let X = Parsing.IntU(spl[1]);
        let Y = Parsing.IntU(spl[2]);
        let sX = X; if(X%2!=0) sX++;
        let sY = Y; if(Y%2!=0) sY++;

        if(X < -2000 || X >= 2000) return [];
        if(Y < -2000 || Y >= 2000) return [];

        let i;
        let Build;
        if(spl[0]=="B")
        {
            Build = new Array(50).fill(null);
            let LocalStructure = this.GetSector("S_"+sX+"_"+sY);
            let Holes = [];
            for(i = 0 ; i < LocalStructure.length ; i++)
            {
                let obj = LocalStructure[i];
                if(obj!=null) if(obj.obj=="hole")
                    Holes.push(obj);
            }
            for(i=0;i<50;i++)
            {
                let x = 10*X + Math.floor(i/5);
                let y = 10*Y + 2*(i%5);
                if(x%2!=0) y++;
                let ulam = makeUlam(x,y);
                Build[i] = this.AsteroidBuild(ulam,Holes);
            }
        }
        else Build = this.StructureBuild(makeUlam(sX,sY));

        if(this.Sectors.size >= this.max_dict_size) this.Sectors.clear();
        this.Sectors.set(sector_name,Build);
        return this.Sectors.get(sector_name);
    }

    //Conversion methods
    static GetSectorNameByUlam(ulam)
    {
        let xy = ulamToXY(ulam);
        let X,Y;

        if(xy[0]>=0) X = Math.floor(xy[0]/10);
        else X = -(Math.floor((-xy[0]-1)/10)+1);

        if(xy[1]>=0) Y = Math.floor(xy[1]/10);
        else Y = -(Math.floor((-xy[1]-1)/10)+1);

        if(ulam%2==0)
        {
            if(X%2!=0) X--;
            if(Y%2!=0) Y--;
            return "S_"+X+"_"+Y;
        }
        else return "B_"+X+"_"+Y;
    }
    static GetAsteroidMove(ulam, size, biome)
    {
		    if(Generator.tag_grid[biome]) return new Vector3(0,0,0);
		    let r121 = Deterministics.Random10e3(ulam+Generator.seed) % 121;
		    let dE = 5 - (size+2)*0.35;
		    let dZ = (Math.floor(r121/11)-5) * dE * 0.2;
		    let dW = (r121%11-5) * dE * 0.2;
		    return new Vector3(dW-dZ,dW+dZ,0);
    }

    static RangedIntParse(str, min, max)
    {
        let n = Parsing.IntU(str);
        if(!Parsing.IntC(str)) return min;
        if(n < min || n > max) return min;
        return n;
    }
    static PositiveFloatParse(str,allow_negative)
    {
        let n = Parsing.FloatU(str);
        if(!Parsing.FloatC(str)) return 0;
        if(!allow_negative && n < 0) return 0;
        return n;
    }

    //Generator methods
    static AsteroidBuild(ulam, Holes)
    {
        if(ulam==1 || ulam%2==0) return null;
        let ulam_mixed = Generator.MixID(ulam,Generator.seed);
        let r100 = long1.charCodeAt((ulam_mixed%65536-1)/2) - 28;
        let s100 = long3.charCodeAt((ulam_mixed%65536-1)/2) - 48;

        let xy = ulamToXY(ulam);
        let ast_pos = Vector3.Multiply(new Vector3(xy[0],xy[1],0),10);
        let Ulam = this.LeadingBiomeUlam(ast_pos);
        let Biome = Generator.GetBiomeData(Ulam);
        let XY = ulamToXY(Ulam);
        let biome_pos = Vector3.Add(Vector3.Multiply(new Vector3(XY[0],XY[1],0),100), new Vector3(Biome.move[0],Biome.move[1],0));
        let is_in = (Vector3.Subtract(ast_pos,biome_pos).Length() < Biome.size);

        let gen_biome = 0, gen_size = s100;
        if(is_in) gen_biome = Biome.biome;
        if(r100 >= Generator.tag_density[gen_biome]) return null;

        for(let i = 0 ; i < Holes.length ; i++)
        {
            let obj = Holes[i];
            if(Vector3.Subtract(ast_pos,obj.position).Length() < obj.range)
                return null;
        }

        let Asteroid = new CObjectInfo(ulam, ast_pos);
        Asteroid.Asteroid(gen_size+"","0","");
        Asteroid.SetBiomeBase(gen_biome+"");
        let loc_mov = this.GetAsteroidMove(ulam,gen_size,gen_biome);
        Asteroid.Move(loc_mov.x+"",loc_mov.y+"");
        return Asteroid;
    }
    static LeadingBiomeUlam(ast_pos)
    {
        let cen_pos = Vector3.Multiply(new Vector3(Math.round(ast_pos.x/100),Math.round(ast_pos.y/100),0),100);
        let X = Math.floor(cen_pos.x/100);
		    let Y = Math.floor(cen_pos.y/100);
		
        let i;
		    let udels = [
            new Vector3(-1,-1,0), new Vector3(0,-1,0), new Vector3(1,-1,0),
            new Vector3(-1,0,0), new Vector3(0,0,0), new Vector3(1,0,0),
            new Vector3(-1,1,0), new Vector3(0,1,0), new Vector3(1,1,0)
        ];
		    let ulams = new Array(9);
		    let insp = new Array(9);
        let biomes = new Array(9);
		    for(i=0;i<9;i++)
        {
            ulams[i] = makeUlam(X+udels[i].x, Y+udels[i].y);
            biomes[i] = Generator.GetBiomeData(ulams[i]);
            insp[i] = (Vector3.Subtract(
                Vector3.Add(Vector3.Add(cen_pos, Vector3.Multiply(udels[i],100)), new Vector3(biomes[i].move[0],biomes[i].move[1],0)),
                ast_pos
            ).Length() < biomes[i].size);
        }
	
		    let proper=0, prr=0;
		    for(i=0;i<9;i++)
		    {
			      if(insp[i])
			      {
				        let locP = Generator.tag_priority[biomes[i].biome];
				        if(Generator.IsBiggerPriority(ulams[i],ulams[proper],locP,prr))
				        {
					          proper = i;
					          prr = locP;
				        }
			      }
		    }
		    if(proper==0 && !insp[0]) return 1;
		    return ulams[proper];
    }
    static StructureBuild(Ulam)
    {
        let XY = ulamToXY(Ulam);
        let X = XY[0];
        let Y = XY[1];
        
        let Build = new Array(1000).fill(null);
        let biomeInfo = Generator.GetBiomeData(Ulam);
        let struct_id = Generator.tag_struct[biomeInfo.biome];
        if(struct_id==0) return Build;

        let SeonArgs = this.TxtToSeonArray(customStructures[struct_id]);
        let base_position = Vector3.Add(Vector3.Multiply(new Vector3(X,Y,0),100), new Vector3(biomeInfo.move[0],biomeInfo.move[1],0));
        let setrand=0, ifrand1=-1, ifrand2=-1, setrand_initializer = Ulam + Generator.seed;

        //Random processing
        let args = [];
        let i,lngt = SeonArgs.length;
        for(i=0;i<lngt;i++)
        {
            if(i<=lngt-2)
            {
                if(SeonArgs[i]=="setrand")
                {
                    let a = this.RangedIntParse(SeonArgs[i+1],1,1000);
                    setrand_initializer = Deterministics.Random10e5(setrand_initializer);
                    setrand = setrand_initializer % a;
                    i++; continue;
                }
                if(SeonArgs[i]=="ifrand")
                {
                    let ifrand_str = (SeonArgs[i+1]+"-").split("-");
                    ifrand1 = this.RangedIntParse(ifrand_str[0],-1,999);
                    ifrand2 = this.RangedIntParse(ifrand_str[1],ifrand1,999);
                    i++; continue;
                }
            }
            if((ifrand1<=setrand && setrand<=ifrand2) || (ifrand1<=-1 && -1<=ifrand2))
                args.push(SeonArgs[i]);
        }

        lngt = args.length;
        let started = false;
        let cmd = "";
        let cmds = [];
        let key_words = [
            "summon",
            "move","rotate","reset",
            "setbiome","hide","steal",
            "setanimator","animate",
            "move$","rotate$","reset$"
        ];
        let H1=0, H2=0, S1=0, S2=0;
        for(i=0;i<lngt;i++)
        {
            if(i<=lngt-3)
            {
                if(args[i]=="catch")
                {
                    if(args[i+1]=="#")
                    {
                        let m_spl = (args[i+2]+"-").split('-');
                        H1 = this.RangedIntParse(m_spl[0],0,999);
                        H2 = this.RangedIntParse(m_spl[1],H1,999);
                        i+=2; continue;
                    }
                    if(args[i+1]=="$")
                    {
                        let m_spl = (args[i+2]+"-").split('-');
                        S1 = this.RangedIntParse(m_spl[0],0,19);
                        S2 = this.RangedIntParse(m_spl[1],S1,19);
                        i+=2; continue;
                    }
                }
            }
            if(key_words.includes(args[i]))
            {
                if(started) cmds.push(cmd+"          ");
                started = true;
                cmd = H1+" "+H2+" "+S1+" "+S2+" "+args[i];
            }
            else cmd += " "+args[i];
        }
        if(started) cmds.push(cmd+"          ");

        lngt = cmds.length;
        for(i=0;i<lngt;i++)
        {
            let line = cmds[i];
            let arg = line.split(' ');
            H1 = Parsing.IntU(arg[0]);
            H2 = Parsing.IntU(arg[1]);
            S1 = Parsing.IntU(arg[2]);
            S2 = Parsing.IntU(arg[3]);
            let H,S;
            for(H=H1;H<=H2;H++)
            {
                if(arg[4]=="summon")
                {
                    Build[H] = new CObjectInfo(this.BuildUlam(X,Y,H),base_position);

                    if(arg[5]=="wall") Build[H].Wall(arg[6],arg[7],arg[8]);
                    if(arg[5]=="piston") Build[H].Piston(arg[6],arg[7],arg[8]);
                    if(arg[5]=="sphere") Build[H].Sphere(arg[6],arg[7]);
                    if(arg[5]=="respblock") Build[H].Ranger(arg[6],arg[5]);
                    if(arg[5]=="hole") Build[H].Ranger(arg[6],arg[5]);
                    if(arg[5]=="animator") Build[H].obj = "animator";
                    if(arg[5]=="star") Build[H].Spherical(arg[5]);
                    if(arg[5]=="monster") Build[H].Spherical(arg[5]);

                    if(H>199) continue;
                    if(arg[5]=="asteroid") Build[H].Asteroid(arg[6],arg[7],arg[8]);

                    if(H>0) continue;
                    if(arg[5]=="boss") Build[H].Boss(arg[6]);
                }
                if(Build[H]==null) continue;
                if(arg[4]=="move")
                {
                    let a,b,c=0,d=0;
                    a = this.PositiveFloatParse(arg[5],true);
                    b = this.PositiveFloatParse(arg[6],true);
                    if(arg[7]=="mod") {
                      c = this.PositiveFloatParse(arg[8],true);
                      d = this.PositiveFloatParse(arg[9],true);
                    }
                    if(arg[7]=="offset") {
                      setrand_initializer = Deterministics.Random10e5(setrand_initializer);
                      let vrn = setrand_initializer % 9;
                      let x_vrn = Math.floor(vrn/3)-1, y_vrn = vrn%3-1;
                      a*= x_vrn; b*= y_vrn;
                    }
                    let n = H-H1;
                    Build[H].Move((a+n*c)+"",(b+n*d)+"");
                }
                if(arg[4]=="rotate")
                {
                    let a,c=0;
                    a = this.PositiveFloatParse(arg[5],true);
                    if(arg[6]=="mod") {
                      c = this.PositiveFloatParse(arg[7],true);
                    }
                    let n = H-H1;
                    Build[H].Rotate((a+n*c)+"");
                }
                if(arg[4]=="reset") Build[H].Reset(arg[5]);
                if(arg[4]=="setbiome") Build[H].SetBiomeBase(arg[5]);
                if(arg[4]=="hide")
                {
                    if(Build[H].obj!="asteroid") continue;
                    Build[H].hidden = true;
                }
                if(arg[4]=="steal")
                {
                    if(Build[H].obj!="wall") continue;
                    let b = 0;
                    if(arg[5]=="fromhash") b = this.RangedIntParse(arg[6],0,999);
                    else if(arg[5]=="fromdelta")
                    {
                        b = Parsing.IntU(arg[6]);
                        b += H; b = b+"";
                        b = this.RangedIntParse(b,0,999);
                    }
                    if(Build[b]==null) continue;
                    if(Build[b].obj!="asteroid") continue;
                    
                    let a = Build[b].size;
                    let rel_pos = Vector3.Subtract(Build[H].position, Build[b].position);
                    Build[b].Reset("rotation");
                    Build[b].Move(rel_pos.x+"",rel_pos.y+"");
                    Build[b].hidden = true;

                    let j,start_dx = -1.7*(a-1)/2;
                    for(j=0;j<2*a;j++)
                    {
                        Build[b].ResetS(j,"position");
                        Build[b].ResetS(j,"rotation");
                    }
                    for(j=0;j<a;j++)
                    {
                        Build[b].RotateS(2*j,(Build[H].rotation-90)+"");
                        Build[b].RotateS(2*j+1,(Build[H].rotation+90)+"");
                        Build[b].MoveS(2*j,(start_dx + j*1.7)+"",(1.5*Build[H].size1)+"");
                        Build[b].MoveS(2*j+1,(start_dx + j*1.7)+"",(1.5*Build[H].size1)+"");
                    }
                }
                if(arg[4]=="setanimator")
                {
                    let a = this.RangedIntParse(arg[5],0,999);
                    if(Build[a]==null) continue;
                    if(Build[a].obj!="animator") continue;
                    if(Build[H].obj=="animator" || Build[H].obj=="boss") continue;
                    Build[H].animator = a;
                    Build[H].animator_reference = Build[a];
                }
                if(arg[4]=="animate")
                {
                    if(arg[6]=="when")
                        Build[H].AnimationCreate(arg[5],arg[7],arg[8],arg[9]);
                }
                if(Build[H].obj!="asteroid") continue;
                for(S=S1;S<=S2;S++)
                {
                    if(arg[4]=="move$")
                    {
                        let a,b,c=0,d=0;
                        a = this.PositiveFloatParse(arg[5],true);
                        b = this.PositiveFloatParse(arg[6],true);
                        if(arg[7]=="mod") {
                          c = this.PositiveFloatParse(arg[8],true);
                          d = this.PositiveFloatParse(arg[9],true);
                        }
                        let n = S-S1;
                        Build[H].MoveS(S,(a+n*c)+"",(b+n*d)+"");
                    }
                    if(arg[4]=="rotate$")
                    {
                        let a,c=0;
                        a = this.PositiveFloatParse(arg[5],true);
                        if(arg[6]=="mod") {
                          c = this.PositiveFloatParse(arg[7],true);
                        }
                        let n = S-S1;
                        Build[H].RotateS(S,(a+n*c)+"");
                    }
                    if(arg[4]=="reset$") Build[H].ResetS(S,arg[5]);
                }
            }
        }

        return Build;
    }
    static TxtToSeonArray(str)
	  {
		    str = str.replaceAll("\t", " ");
    	  str = str.replaceAll("\r", " ");
    	  str = str.replaceAll("\n", " ");
    	  str = str.replaceAll("[", " ");
    	  str = str.replaceAll("]", " ");
		    str = str.trim();

		    while(str.includes("  "))
        	  str = str.replaceAll("  ", " ");

    	  str = str.replaceAll(".",",");
        return str.split(' ');
	  }
    static BuildUlam(X, Y, id) // <0;199>
	  {
        if(id>=50 && id<100) { X++; id-=50; }
        if(id>=100 && id<150) { Y++; id-=100; }
        if(id>=150 && id<200) { X++; Y++; id-=150; }
        if(id>=200) return 0;

		    let sX = 2*(id%5);
		    let sY = Math.floor(id/5);
		    if(sY%2==0) sX++;
		    return makeUlam(10*X + sX, 10*Y + sY);
	  }
}


// GENERATION LAYER 3 (inner) -> Fobs and files communication

let d0,d1,d_ulam,d_dont_update_bosbul=false;
class WorldData
{
    //Technical methods
    static Load(ulam) //Loads X;Y data to memory
    {
        const det = asteroidIndex(ulam);
        d0 = det[0];
        d1 = det[1];
        d_ulam = Parsing.IntU(ulam);
    }
    static DataGenerate(gencode) //Generates data from gencode
    {
        let i;
        for(i=0;i<=60;i++) WorldData.UpdateData(i,0);
        if(gencode=="BOSS")
        {
            WorldData.UpdateType(1024);
            for(i=1;i<=60;i++) WorldData.UpdateData(i,0);
        }
        else
        {
            /*
                Gencode:
                [size]b[biome]b[fobCode] -> biome based code, calculate type from biome
                [size]t[type]t[fobCode] -> type based code, type is given
            */

            //Gencode parse
            const sep = gencode.includes('t') ? 't' : 'b';
            const elements = gencode.split(sep);

            //Size parse
            const size = Parsing.IntU(elements[0]);

            //Type parse
            const type = (sep=='t') ? Parsing.IntU(elements[1]) : Deterministics.CalculateFromString(typeSet[Parsing.IntU(elements[1])*7 + size-4], d_ulam + Generator.seed);

            //Gens parse
            const gens = Array(20).fill(-1);
            if(elements.length>2) {
                const s_gens = elements[2].split(';');
                for(i=0;(i<s_gens.length && i<20);i++) {
                    if(Parsing.IntC(s_gens[i])) gens[i] = Parsing.IntU(s_gens[i]);
                }
            }
            
            //Generate type and fobs
            WorldData.UpdateType(type);
            for(i=1;i<=size*2;i++)
            {
                let gen = gens[i-1];
                if(gen==-1) gen = Deterministics.CalculateFromString(fobGenerate[type], 20*((d_ulam + Generator.seed) % 1000000)+i);
                d_dont_update_bosbul = true;
                WorldData.UpdateFob(i,gen);
            }
        }
    }
    
    //Read methods
    static GetData(place) //Returns the place data ("" -> 0)
    {
        let got = chunk_data[d0][d1][place];
        return Parsing.IntU(got);
    }
    static GetNbt(place,index) //Returns the fob nbt data ("" -> 0)
    {
        return WorldData.GetData(21+index+2*(place-1));
    }
    static GetFob(place) //Returns the fob (0-127)
    {
        let got = chunk_data[d0][d1][place];
        if(Parsing.IntC(got))
        {
            let num = Parsing.IntU(got);
            if(num>=0 && num<=127) return num;
            else return -1;
        }
        else return -1;
    }
    static GetType() //Returns data type (0-63 or 1024)
    {
        let got = chunk_data[d0][d1][0];
        if(Parsing.IntC(got))
        {
            let num = Parsing.IntU(got);
            if((num>=0 && num<=63) || num==1024) return num;
            else return -1;
        }
        else return -1;
    }

    //Write methods
    static UpdateData(place,data) //Updates data (0 -> "")
    {
        if(data!=0) chunk_data[d0][d1][place] = data+"";
        else chunk_data[d0][d1][place] = "";
    }
    static UpdateNbt(place,index,data) //Updates nbt data (0 -> "")
    {
        WorldData.UpdateData(21+index+2*(place-1),data);
    }
    static UpdateFob(place,data) //Updates the fob (0-127)
    {
        WorldData.ResetNbt(place);
        if(data>=0 && data<=127) chunk_data[d0][d1][place] = data+"";
        else chunk_data[d0][d1][place] = "0";

        if(!d_dont_update_bosbul) Bosbul.UpdateFobColliders(d_ulam);
        d_dont_update_bosbul = false;
    }
    static UpdateType(data) //Updates data type (0-63 or 1024)
    {
        if((data>=0 && data<=63) || data==1024) chunk_data[d0][d1][0] = data+"";
        else chunk_data[d0][d1][0] = "";
    }

    //Private methods
    static ResetNbt(place) //Resets fob nbt data ("")
    {
        WorldData.UpdateNbt(place,0,0);
        WorldData.UpdateNbt(place,1,0);
    }
}
// ----------------------------------------- \\
// ------------- BOSBUL SYSTEM ------------- \\
// ----------------------------------------- \\

class CExactCollider
{
  constructor()
  {
      //This class was translated from C# by ChatGPT

      this.type = '';
      this.x = 0;
      this.y = 0;
      this.r = 0;
      this.rx = 0;
      this.ry = 0;
      this.angle = 0;
  }

  // Initializers
  Circle(_x, _y, _r)
  {
      this.type = "circle";
      this.x = _x;
      this.y = _y;
      this.r = _r;
  }

  Rectangle(_x, _y, _rx, _ry, _angle)
  {
      this.type = "rectangle";
      this.x = _x;
      this.y = _y;
      this.rx = _rx;
      this.ry = _ry;
      this.angle = _angle;
  }

  // Checkers
  static CheckCollision(C1, C2)
  {
      if (C1 === null || C2 === null) return false;
      if (C1.type === "circle" && C2.type === "circle") return this.CollideCC(C1, C2);
      if (C1.type === "rectangle" && C2.type === "rectangle") return false; // such collisions are not needed nor implemented
      if (C1.type === "circle" && C2.type === "rectangle") return this.CollideCR(C1, C2);
      if (C1.type === "rectangle" && C2.type === "circle") return this.CollideCR(C2, C1);
      return false;
  }

  static CollideCC(C1, C2)
  {
      // Primitive circle collision
      const dx = C2.x - C1.x;
      const dy = C2.y - C1.y;
      const rs = C1.r + C2.r;
      return (rs * rs > dx * dx + dy * dy);
  }

  static CollideCR(C1, C2)
  {
      // Performance boost
      const mcd = C1.r + C2.rx + C2.ry;
      const dx = C2.x - C1.x;
      const dy = C2.y - C1.y;
      if (mcd * mcd < dx * dx + dy * dy) return false;

      // Exclude null rectangles
      if (C2.rx === 0 && C2.ry === 0) return false;

      // Initialize temporary objects
      const B1 = new CExactCollider();
      const B2 = new CExactCollider();

      // Move to center
      B1.Circle(C1.x - C2.x, C1.y - C2.y, C1.r);
      B2.Rectangle(0, 0, C2.rx, C2.ry, C2.angle);

      // Rotate objects
      const sph_rot = func.RotatePoint([B1.x, B1.y], -B2.angle);
      B1.Circle(sph_rot[0], sph_rot[1], B1.r);
      B2.Rectangle(0, 0, B2.rx, B2.ry, 0);

      // Detect sector
      let sec_x = 0, sec_y = 0;
      if (B1.x > B2.rx) sec_x = 1; if (B1.x < -B2.rx) sec_x = -1;
      if (B1.y > B2.ry) sec_y = 1; if (B1.y < -B2.ry) sec_y = -1;

      // Check collision in trivial sectors
      if (sec_x === 0) return (Math.abs(B1.y) < B1.r + B2.ry);
      if (sec_y === 0) return (Math.abs(B1.x) < B1.r + B2.rx);

      // Check collision in corner sectors
      const W1 = new CExactCollider(); W1.Circle(sec_x * B2.rx, sec_y * B2.ry, 0);
      return this.CollideCC(B2, W1);
  }

  // Clone
  Clone()
  {
      return { ...this };
  }
}

class CBosbulCollider
{
  constructor(obj, fob)
  {
      //This class was translated from C# by ChatGPT

      this.WhenAnimated = [];
      this.ColliderDefault = null;
      this.ColliderAnimated = null;
      this.x = 0;
      this.y = 0;
      this.ax = 0;
      this.ay = 0;

      if (fob === -1) {
          this.x = obj.position.x;
          this.y = obj.position.y;
      } else
      {
          this.x = obj.fob_positions[fob].x;
          this.y = obj.fob_positions[fob].y;
      }
      this.ax = this.x;
      this.ay = this.y;

      // Create base colliders
      this.ColliderDefault = new CExactCollider();
      if (obj.obj === "asteroid")
      {
          if (fob === -1) this.ColliderDefault.Circle(this.x, this.y, obj.size / 2);
          else this.ColliderDefault.Rectangle(this.x, this.y, 0, 0, obj.fob_rotations[fob]);
      }
      else if (obj.obj === "sphere") this.ColliderDefault.Circle(this.x, this.y, obj.size1 / 2);
      else if (obj.obj === "star") this.ColliderDefault.Circle(this.x, this.y, 9.6 * obj.size1);
      else if (obj.obj === "monster") this.ColliderDefault.Circle(this.x, this.y, 9.6 * obj.size1);
      else if (obj.obj === "wall") this.ColliderDefault.Rectangle(this.x, this.y, 1.5 * obj.size1, 5 * obj.size2, obj.rotation);
      else if (obj.obj === "piston") this.ColliderDefault.Rectangle(this.x, this.y, 1.5 * obj.size1, 3 * obj.size2, obj.rotation);
      else this.ColliderDefault = null;

      // Update to state colliders
      if (obj.animator_reference === null)
      {
          this.WhenAnimated = [];
          this.ColliderAnimated = null;
      }
      else if (obj.animator_reference.animation_type !== 0)
      {
          this.WhenAnimated = obj.animator_reference.animation_when_done.split(";");
          if (obj.animator_reference.animation_type === 2)
          {
              this.ColliderAnimated = this.ColliderDefault.Clone();
              this.ColliderAnimated.x += obj.animator_reference.animation_size.x;
              this.ColliderAnimated.y += obj.animator_reference.animation_size.y;
              this.ax = this.ColliderAnimated.x;
              this.ay = this.ColliderAnimated.y;
          }
          else this.ColliderAnimated = null;
      }
  }

  CheckCollision(C1, reduced_state)
  {
      if (!this.WhenAnimated.includes(reduced_state)) return CExactCollider.CheckCollision(this.ColliderDefault, C1);
      else return CExactCollider.CheckCollision(this.ColliderAnimated, C1);
  }

  UpdateFobCollider(num)
  {
      let RX = 0;
      let RY = 0;
      let OF = 0;

      // More general
      if ([8, 9, 10, 11, 16, 30, 50, 56, 58, 60, 62, 66].includes(num))  { // stones & elements
          RX = 0.9;
          RY = 1;
          OF = 0;
      }
      if ([17, 18, 19, 22, 26, 31, 49, 67, 32].includes(num)) { // packeds & big diamond
          RX = 1.4;
          RY = 1.6;
          OF = 0.5;
      }
      if ([41, 42, 43, 44, 45, 46, 47].includes(num)) { // artefacts
          RX = 1.15;
          RY = 1.2;
          OF = 0;
      }
      if ([13, 25, 27, 40].includes(num)) { // smaller aliens
          RX = 1.4;
          RY = 2.6;
          OF = 0.5;
      }

      // More individual
      if ([23, 53].includes(num)) { RX = 1.4; RY = 3; OF = 0.5; } // bigger aliens
      if ([1].includes(num)) { RX = 1.15; RY = 1.5; OF = 0; } // stone with crystals
      if ([37, 68].includes(num)) { RX = 1.4; RY = 2.2; OF = 0.5; } // treasures
      if ([34, 36].includes(num)) { RX = 0.4; RY = 2.3; OF = 0.5; } // drills
      if ([35].includes(num)) { RX = 0.4; RY = 3; OF = 0.5; } // magnetic lamp
      if ([54].includes(num)) { RX = 1; RY = 2.2; OF = 0.5; } // bone
      if ([51].includes(num)) { RX = 1.4; RY = 2; OF = 0.5; } // metal piece
      if ([29, 69].includes(num)) { RX = 1.4; RY = 3.6; OF = 1; } // tombs
      if ([28].includes(num)) { RX = 1.4; RY = 1.2; OF = 0; } // red spikes
      if ([33].includes(num)) { RX = 0.9; RY = 1.1; OF = 0; } // small diamond
      if ([38, 70].includes(num)) { RX = 1.5; RY = 1.6; OF = 0; } // normal and lava geyzer
      if ([3, 4].includes(num)) { RX = 1.4; RY = 2.2; OF = 0; } // pumpkin & mega geyzer
      if ([5].includes(num)) { RX = 0.6; RY = 1; OF = 0; } // small amethyst
      if ([6].includes(num)) { RX = 1; RY = 1.6; OF = 0; } // medium amethyst
      if ([7].includes(num)) { RX = 1.4; RY = 2.2; OF = 0; } // big amethyst
      if ([2].includes(num)) { RX = 1.4; RY = 3.2; OF = 1; } // driller
      if ([21, 52].includes(num)) { RX = 1.4; RY = 2.4; OF = 1; } // storages
      if ([15].includes(num)) { RX = 1.8; RY = 8; OF = 3.7; } // copper chimney

      const dXY = func.RotatePoint([0, OF], this.ColliderDefault.angle);

      this.ColliderDefault.x = this.x + dXY[0];
      this.ColliderDefault.y = this.y + dXY[1];
      this.ColliderDefault.rx = RX / 2;
      this.ColliderDefault.ry = RY / 2;

      if (this.ColliderAnimated !== null)
      {
          this.ColliderAnimated.x = this.ax + dXY[0];
          this.ColliderAnimated.y = this.ay + dXY[1];
          this.ColliderAnimated.rx = RX / 2;
          this.ColliderAnimated.ry = RY / 2;
      }
  }
}

class Bosbul
{
  //This class was translated from C# with assistance of ChatGPT

  static BosbulSectors = {};
  static max_dict_size = config.max_dict_size;

  static GetBosbuls(X, Y)
  {
      const key = makeUlam(X, Y);
      if (this.BosbulSectors.hasOwnProperty(key)) return this.BosbulSectors[key];
      else
      {
          const Surroundings = [];
          Universe.GetSector("S_" + X + "_" + Y).forEach(elm => Surroundings.push(elm));
          Universe.GetSector("B_" + X + "_" + Y).forEach(elm => Surroundings.push(elm));
          Universe.GetSector("B_" + (X - 1) + "_" + Y).forEach(elm => Surroundings.push(elm));
          Universe.GetSector("B_" + X + "_" + (Y - 1)).forEach(elm => Surroundings.push(elm));
          Universe.GetSector("B_" + (X - 1) + "_" + (Y - 1)).forEach(elm => Surroundings.push(elm));

          const Build = new Map();
          for(const obj of Surroundings)
              if(obj !== null)
              {
                  if(obj.obj === "asteroid")
                  {
                      if (!obj.hidden) Build.set("ast_" + obj.ulam, new CBosbulCollider(obj, -1));
                      for (let i = 0; i < obj.size * 2; i++)
                          Build.set("fob_" + obj.ulam + "_" + i, new CBosbulCollider(obj, i));
                      this.UpdateFobCollidersInDictionary(Build, obj.ulam);
                  }
                  else if (["sphere", "star", "monster", "wall", "piston"].includes(obj.obj))
                  {
                      const random_key = Math.floor(Math.random() * 1000000000);
                      Build.set(random_key, new CBosbulCollider(obj, -1));
                  }
              }

          if (Object.keys(this.BosbulSectors).length >= this.max_dict_size) this.BosbulSectors = {};
          this.BosbulSectors[key] = Build;
          return this.BosbulSectors[key];
      }
  }

  static CollidesWithBosbul(C1, reduced_state)
  {
      const X = Math.round(C1.x / 200) * 2;
      const Y = Math.round(C1.y / 200) * 2;
      const LocalBosbuls = this.GetBosbuls(X, Y).values();
      for(const bbc of LocalBosbuls) {
          if (bbc.CheckCollision(C1, reduced_state)) {
              return true;
          }
      }
      return false;
  }

  static UpdateFobColliders(ulam)
  {
      const nums = Universe.GetSectorNameByUlam(ulam).split('_');
      let X = Parsing.IntU(nums[1]); if (X % 2 !== 0) X++;
      let Y = Parsing.IntU(nums[2]); if (Y % 2 !== 0) Y++;
      const LocalDictionary = this.GetBosbuls(X, Y);
      this.UpdateFobCollidersInDictionary(LocalDictionary, ulam);
  }

  static UpdateFobCollidersInDictionary(LocalDictionary, ulam)
  {
      WorldData.Load(ulam);
      for (let i = 0; i < 20; i++)
      {
          const key = "fob_" + ulam + "_" + i;
          if (LocalDictionary.has(key))
              LocalDictionary.get(key).UpdateFobCollider(WorldData.GetFob(i + 1));
      }
  }
}

// ----------------------------------------- \\
// -------------- CLASSES END -------------- \\
// ----------------------------------------- \\

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
    this.ctrlPower = 0;
    this.powerRegenBlocked = false;
    this.periodic = {};
  }
  DataImport(rsp_x,rsp_y,ctrl_power) {
    this.Reset();
    this.ModifyRespawn(rsp_x,rsp_y);
    this.ctrlPower = ctrl_power;
    this.periodic = {
      player_update: [null,0, 0,config.anti_cheat.max_player_updates_per_3_seconds, 3*1000], // [0nextdate,1current, 2min,3max, 4period] note: it has to be imported at least once in player lifetime for system to work
      chat: [null,0, 0,5, 2*1000],
      particles: [null,0, 0,20, 1*1000],
      grow_loaded: [null,0, 0,1, 1*1000],
      drill_ask: [null,0, 0,10, 1*1000],
      request_memories: [null,0, 0,200, 1*1000]
    };
    this.bullet_wait = Date.now();
    this.impulse_wait = Date.now();
  }
  ModifyRespawn(rsp_x,rsp_y) {
    this.respawn_x = Parsing.FloatU(rsp_x);
    this.respawn_y = Parsing.FloatU(rsp_y);
  }
  DrillAsk(drillID,pid,drillGroup) {
    this.drill_asteroid = drillID;
    this.drill_group = drillGroup;
    this.drill_counter = handDrillTimeGet(pid);
  }
  DrillReady(pid) {
    var mined = drillItemGet(this.drill_asteroid,0);
    if(mined!=0) this.drill_list.push(mined);
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
    if(this.last_pos_changes.length >= 10) this.last_pos_changes.shift();
    this.last_pos_changes.push(distance);
  }
  PosChangeSum() {
    var sum=0,i,lngt = this.last_pos_changes.length;
    for(i=0;i<lngt;i++) sum += this.last_pos_changes[i];
    return sum;
  }
  PeriodicInsert(per_type)
  {
    var lv = this.periodic[per_type];
    if(lv[0]==null) lv[0] = Date.now() + lv[4];
    lv[1]++;
    return (this.periodic[per_type][1] <= this.periodic[per_type][3]);
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

var AlienDatabase = {};

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
  upgrade_boost: 0,

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
  givenUpPlayers: [],
  bID: -1,
  sID: -1,
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

function Num31ToChar(num)
{
    if (num < 10) return String.fromCharCode(48 + num)+"";
    return String.fromCharCode(55 + num)+"";
}
function CharToNum31(ch)
{
		if(ch=='-') return -1;
		var num = ch.charCodeAt(0);
		if(num < 65) return num-48;
		else return num-55;
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
        tpl.x = Parsing.FloatU(splitted[0]) - deltapos.x;
        tpl.y = Parsing.FloatU(splitted[1]) - deltapos.y;
        var px = tpl.x;
        var py = tpl.y;
        if(Parsing.IntU(splitted[5].split("&")[1]) % 100 != 1) //not invisible (must be %100)
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

      tpl.normal_damage = boss_damages[type] * Parsing.FloatU(gameplay[32]);
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
  for (i = 0; i < 6; i++) if (!Parsing.FloatC(elem0[i])) return false;
  if (!Parsing.IntC(elem0[6])) return false;
  if (!Parsing.FloatC(elem0[7])) return false;
  for (i = 0; i < 18; i++) if (!Parsing.IntC(elem1[i])) return false;
  for (i = 0; i < 42; i++) if (!Parsing.IntC(elem2[i])) return false;
  for (i = 0; i < 5; i++) if (!Parsing.IntC(elem3[i])) return false;

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
function makeUlam(X,Y)
{
    let ID=0,P;
    if(Math.abs(X)>Math.abs(Y)) P=Math.abs(X);
		else P=Math.abs(Y);

		ID=4*(P*P-P)+1;

		X=X+P+1;
		Y=Y+P+1;

		if(X==(2*P+1)&&Y!=1) //first
			  ID=ID+Y-1;
		else if(Y==(2*P+1)) //second
			  ID=ID+4*P+1-X;
		else if(X==1) //third
			  ID=ID+6*P+1-Y;
		else if(Y==1) //fourth
			  ID=ID+6*P+X-1;
    
    return ID;
}
function ulamToXY(ulam)
{
  let sqrt = intSqrt(ulam);
  if (sqrt % 2 === 0) sqrt--;

  let x = Math.floor(sqrt / 2) + 1;
  let y = -Math.floor(sqrt / 2) - 1;
  let pot = sqrt * sqrt;
  let delta = ulam - pot;
  let cwr = Math.floor(delta / (sqrt + 1));
  let dlt = delta % (sqrt + 1);

  if (cwr === 0 && dlt === 0) return [x - 1, y + 1];
  if (cwr > 0) y += (sqrt + 1);
  if (cwr > 1) x -= (sqrt + 1);
  if (cwr > 2) y -= (sqrt + 1);

  if (cwr === 0) y += dlt;
  if (cwr === 1) x -= dlt;
  if (cwr === 2) y -= dlt;
  if (cwr === 3) x += dlt;

  return [x, y];
}
function intSqrt(n)
{
  let a = 0, b = n;
  if (b > 46340) b = 46340; // overflow protection
  while (a <= b) {
      let piv = Math.floor((a + b) / 2);
      let sqpiv = piv * piv;
      if (sqpiv > n) {
          b = piv - 1;
          continue;
      } else if (sqpiv < n) {
          a = piv + 1;
          continue;
      } else return piv;
  }
  return b;
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
          for (j = 0; j < lnt; j++) if (datM[j] != "") pom = Parsing.IntE(datM[j]);
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
function removeEnds(str)
{
    var i, lngt = str.length, eff = "";
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
  console.log("Data saved on exit");
  if (options.exit) process.exit();
}

process.on("SIGINT", exitHandler.bind(null, { exit: true })); //On ctrl+C
process.on("SIGHUP", exitHandler.bind(null, { exit: true })); //On cmd closed
process.on("SIGUSR1", exitHandler.bind(null, { exit: true })); //???
process.on("SIGUSR2", exitHandler.bind(null, { exit: true })); //???

//process.on('uncaughtException', exitHandler.bind(null, {exit:true})); //On error
//process.on('exit', exitHandler.bind(null,{cleanup:true})); //???

//Save optimalize functions
function SaveAllNow()
{
    var i,lngt = chunk_data.length;
    for(i=0;i<max_players;i++) if (checkPlayerCn(i, plr.conID[i])) savePlayer(i);
    for(i=0;i<lngt;i++) chunkSave(i);
    for(i=0;i<16000;i++)
    {
        if(biome_memories_state[i]==3 && biome_memories[i]!="")
        {
            writeF(universe_name + "/Biomes/Memory_"+i+".se3", biome_memories[i]+"\r\n");
            biome_memories_state[i] = 2;
        }
    }
    var natete = universe_name + "/Biomes.se3";
    if(existsF(natete)) removeF(natete);
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
	else return Parsing.FloatU(gameplay[16]);
}
function getProtRegenMultiplier(art)
{
	if(art!=1) return 1;
	else return Parsing.FloatU(gameplay[17]);
}

function HealFLOAT(pid,hp)
{
  var artid = plr.backpack[pid].split(";")[30] - 41;
  if(plr.backpack[pid].split(";")[31]=="0") artid = -41;

  var sth1 = plr.sHealth[pid];

  var heal_size = hp+"";
  var potHHH = Parsing.FloatU(plr.upgrades[pid].split(";")[0]) + getProtLevelAdd(artid) + Parsing.FloatU(gameplay[26]);
	if(potHHH<-50) potHHH = -50; if(potHHH>56.397) potHHH = 56.397;
	var heal=0.02*Parsing.FloatU(heal_size)/(Math.ceil(50*Math.pow(health_base,potHHH))/50);
  if(heal<0) heal=0;
        
  plr.sHealth[pid] += heal;
  if(plr.sHealth[pid]>1) plr.sHealth[pid]=1;

  var sth2 = plr.sHealth[pid];

  var del = ((sth2-sth1)+"").replaceAll(".",",");
  var abl = ((sth2)+"").replaceAll(".",",");
  sendTo(se3_ws[pid],"R "+del+" "+plr.immID[pid]+" "+plr.livID[pid]+" "+abl); //Medium type message
}
function DamageFLOAT(pid,dmg)
{
  dmg = Parsing.FloatU(dmg);
  if(dmg>0 && plr.players[pid].split(";").length!=1 && plr.connectionTime[pid]>=50)
  {
    var artid = plr.backpack[pid].split(";")[30] - 41;
    if(plr.backpack[pid].split(";")[31]=="0") artid = -41;
    if(Parsing.FloatU(plr.players[pid].split(";")[5].split("&")[1])%25==2) return;
    var potHHH = Parsing.FloatU(plr.upgrades[pid].split(";")[0]) + getProtLevelAdd(artid) + Parsing.FloatU(gameplay[26]);
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
  dmg = Parsing.FloatU(dmg);
  plr.sHealth[pid] -= dmg;
  if(Math.round(plr.sHealth[pid]*10000) == 0) plr.sHealth[pid] = 0;
  plr.sRegTimer[pid] = Math.floor(50*Parsing.FloatU(gameplay[4]));

  if(plr.players[pid]=="0" || plr.players[pid]=="1") return;

  var xx = plr.players[pid].split(";")[0];
  var yy = plr.players[pid].split(";")[1];
  if(!if_ringed) sendToAllPlayers("/RetEmitParticles "+pid+" 2 "+xx+" "+yy+" X X");
  else sendToAllPlayers("/RetEmitParticles "+pid+" 18 "+xx+" "+yy+" X X");
  sendToAllPlayers("/RetInvisibilityPulse "+pid+" wait X X");
}
function getBulletDamage(pid,bll)
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
    if(!is_bos) {
      if(bll.owner>=0) return bll.normal_damage;
      else return bll.normal_damage * difficulty;
    }
    else {
      var damage_modifier = 1;
      if(bll.type==15 && pid==-3) damage_modifier = 0;
      return bll.normal_damage * damage_modifier;
    }
  }
  else return 0;
}

function GetState(general,additional)
{
    let combo = general+"_"+additional;
        
    if(combo=="0_0") return "A1";
    if(combo=="0_1") return "a1b1";
    if(combo=="0_2") return "B1";
    if(combo=="0_3") return "b1a1";
    if(combo=="1_4") return "b1a2";

    if(combo=="1_0") return "A2";
    if(combo=="1_1") return "a2b2";
    if(combo=="1_2") return "B2";
    if(combo=="1_3") return "b2a2";
    if(combo=="2_4") return "b2a3";

    if(combo=="2_0") return "A3";
    if(combo=="2_1") return "a3b3";
    if(combo=="2_2") return "B3";
    if(combo=="2_3") return "b3a3";
    if(combo=="3_4") return "b3r";

    if(combo=="3_0") return "R";

    if(general==0) return "A1";
    if(general==1) return "A2";
    if(general==2) return "A3";
    if(general==3) return "R";

    return "default";
}

function TransitionToEffect(s)
{
  let result = "";
  for (let i = 2; i < s.length; i++) {
      if (s[i].toLowerCase() === s[i]) 
          result += s[i].toUpperCase();
      else 
          result += s[i];
  }
  return result;
}

function GetReducedState(scr)
{
    let normal_state = GetState(scr.dataX[1],scr.dataY[2-2]);
    if(normal_state=="default") return "default";
    if(normal_state.length<=2) return normal_state;
    else return TransitionToEffect(normal_state);
}

//HUB INTERVAL <interval #0>
var date_before = Date.now();
var date_start = Date.now();
var time_loan = 0;
setInterval(function () { // <interval #2>
  while(Date.now() > date_before)
  {
    date_before+=20;
    if((date_before-date_start) % 6000 == 0) //precisely 1 time per 6 seconds (never affected)
    {
      GrowActiveDelay.forEach(element => {
        growActive(element);
      });
      GrowActiveDelay.clear();
    }
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

      for (let vrb in AlienDatabase) {
        AlienDatabase[vrb][1]--;
        if (AlienDatabase[vrb][1] == 0)
          delete AlienDatabase[vrb];
      }

      MsgMap.clear();
    }

    //LAG PREVENTING
    if(time_loan>=15) {
      time_loan-=15;
      continue;
    }

    var v1_date_now = Date.now();

    //FRAME UPDATE (50TPS by default):
    //while(Date.now()<v1_date_now+40) { console.log("lagger enabled"); }

    // BULLET UPDATE CODE (move & collision)
    var i, j;
    var lngt=bulletsT.length
    var slngt=scrs.length;

    //Iteration through all bullets
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

        //Check discrete bosbul collisions with the environment
        if(bulletsT[i].owner < 0)
        {
            //Check nearest arena state
            let X = Math.round(bulletsT[i].pos.x/200) * 2;
            let Y = Math.round(bulletsT[i].pos.y/200) * 2;
            let Ulam = makeUlam(X,Y);
            let reduced_state = "default";
            for(j=0;j<slngt;j++) {
                if(Ulam==scrs[j].sID) {
                    reduced_state = GetReducedState(scrs[j]);
                    break;
                }
            }

            //Check collision
            let bulcol = new CExactCollider();
            bulcol.Circle(bulletsT[i].pos.x,bulletsT[i].pos.y,other_bullets_colliders[bulletsT[i].type]);
            if(Bosbul.CollidesWithBosbul(bulcol,reduced_state)) {
                destroyBullet(i, ["", bulletsT[i].owner, bulletsT[i].ID, bulletsT[i].age], true);
            }
        }

        //Initialize non-discrete bullet colliders
        var xv = bulletsT[i].vector.x;
        var yv = bulletsT[i].vector.y;
        var xa = bulletsT[i].pos.x;
        var ya = bulletsT[i].pos.y;
        var xb = xa + bulletsT[i].vector.x;
        var yb = ya + bulletsT[i].vector.y;
        func.CollisionLinearBulletSet(xa,ya,xb,yb,other_bullets_colliders[bulletsT[i].type]);

        // Collision check: player/boss -> player
        var bul_vp = (bulletsT[i].vector.x+" "+bulletsT[i].vector.y).replaceAll(".",",");
        if(config.pvp || bulletsT[i].owner<0)
        for(j=0;j<max_players;j++)
        {
          if(plr.players[j]!="0" && plr.players[j]!="1" && bulletsT[i].owner!=j)
          {
            var plas = plr.players[j].split(";");
            var xc = Parsing.FloatU(plas[0]);
            var yc = Parsing.FloatU(plas[1]);
            if(func.CollisionLinearCheck(xc,yc,0.92))
            {
              if(bullet_air_consistence[bulletsT[i].type]==0) {
                if( DamageFLOAT(j, getBulletDamage(j, bulletsT[i]) ) != "K")
                  sendTo(se3_ws[j],"/RetDamageUsing "+bulletsT[i].type+" "+bul_vp+" X "+plr.livID[j]);
                destroyBullet(i, ["", bulletsT[i].owner, bulletsT[i].ID, bulletsT[i].age], false);
                break;
              }
              else if(!bulletsT[i].damaged.includes(j)) {
                if( DamageFLOAT(j, getBulletDamage(j, bulletsT[i]) ) != "K")
                  sendTo(se3_ws[j],"/RetDamageUsing "+bulletsT[i].type+" "+bul_vp+" X "+plr.livID[j]);
                bulletsT[i].damaged.push(j);
              }
            }
          }
        }

        // Collision check: player -> boss
        var lngts = scrs.length;
        if(bulletsT[i].owner>=0)
        for(j=0;j<lngts;j++)
        {
          if(scrs[j].dataY[2-2]==2)
          {
            var xc = scrs[j].posCX + func.ScrdToFloat(scrs[j].dataY[8-2]);
            var yc = scrs[j].posCY + func.ScrdToFloat(scrs[j].dataY[9-2]);
            if(func.CollisionLinearCheck(xc,yc,7.5))
            {
              if(scrs[j].type==6) DamageBoss(j, getBulletDamage(-2, bulletsT[i]) );
              else if(scrs[j].type==4) DamageBoss(j, getBulletDamage(-3, bulletsT[i]) );
              else DamageBoss(j, getBulletDamage(-1, bulletsT[i]) );
              if(bulletsT[i].type==14)
              {
                  //Calculate boss force
                  var wind_force = Parsing.FloatU(gameplay[123]) / 50;
                  var force_vector = Vector3.Subtract(
                    new Vector3(scrs[j].posCX + func.ScrdToFloat(scrs[j].dataY[8-2]), scrs[j].posCY + func.ScrdToFloat(scrs[j].dataY[9-2]),0),
                    new Vector3(bulletsT[i].pos.x,bulletsT[i].pos.y,0)
                  );
                  var dist = Math.sqrt(force_vector.x**2 + force_vector.y**2);
                  if(dist!=0)
                  {
                      var wx = wind_force * force_vector.x / dist;
                      var wy = wind_force * force_vector.y / dist;
  
                      //Add force to boss
                      var d = func.ScrdToFloat(scrs[j].dataY[13-2]);
                      var ang = func.ScrdToFloat(scrs[j].dataY[14-2]);
                      wx += Math.cos(ang) * d;
                      wy += Math.sin(ang) * d;
                      scrs[j].dataY[13-2] = func.FloatToScrd(Math.sqrt(wx*wx + wy*wy));
                      scrs[j].dataY[14-2] = func.FloatToScrd(Math.atan2(wy,wx));
                  }
              }
              if(bulletsT[i].type==15 && scrs[j].type!=4 && !(scrs[j].behaviour.type==1 && scrs[j].dataY[18-2]==2))
              {
                  //Give fire effect
                  if(bulletsT[i].upgrade_boost > scrs[j].dataY[25-2] || scrs[j].dataY[24-2]==0)
                      scrs[j].dataY[25-2] = bulletsT[i].upgrade_boost;
                  scrs[j].dataY[24-2] = Math.floor(Parsing.FloatU(gameplay[37])+1) * 50;
              }
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
        var loc_artid = Parsing.IntU(bpckArray[30]) - 41;
        if(Parsing.IntU(bpckArray[31]) < 1) loc_artid = -42;
        if(plr.connectionTime[i] > 50 && plr.pclass[i].unstable_pulses_available < 5 && loc_artid==6) {
          if(func.randomInteger(0,unstable_sprobability-1)==0) {
            plr.pclass[i].unstable_pulses_available++;
            sendTo(se3_ws[i],"/RetUnstablePulse X X");
          }
        }
    }

    //Health regeneration & Power speculation
    for(i=0;i<max_players;i++)
    {
        if(plr.data[i]!="0" && plr.data[i]!="1" && se3_wsS[i]=="game")
        {
          var artid = plr.backpack[i].split(";")[30] - 41;
          if(plr.backpack[i].split(";")[31]==0) artid = -41;

          //Health regeneration
          if(plr.sRegTimer[i]==0 && plr.sHealth[i]<1)
            HealFLOAT(i,50 * unit * getProtRegenMultiplier(artid) * Parsing.FloatU(gameplay[5]));
          
          if(plr.sRegTimer[i]>0)
	        {
      			if(artid!=1) plr.sRegTimer[i]--;
			      else plr.sRegTimer[i]-=2;
			      if(plr.sRegTimer[i]<0) plr.sRegTimer[i]=0;
		      }

          //Power speculation
          if(plr.pclass[i].ctrlPower < config.anti_cheat.power_speculative_minimum_value) {
            kick(i); continue;
          }
          if(plr.pclass[i].ctrlPower < 1 && !plr.pclass[i].powerRegenBlocked) {
            if(artid==2) plr.pclass[i].ctrlPower += unit * Parsing.FloatU(gameplay[18]); //IMPULSE
            if(artid==3) plr.pclass[i].ctrlPower += unit * Parsing.FloatU(gameplay[21]); //ILLUSION
            if(plr.pclass[i].ctrlPower > 1) plr.pclass[i].ctrlPower = 1;
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
          if(scrs[i].behaviour.type==1 && scrs[i].dataY[18-2]==2) scrs[i].dataY[24-2] = 0;
          if(scrs[i].dataY[24-2]>0) scrs[i].dataY[24-2]--;
          if(scrs[i].dataY[24-2]%50==0 && scrs[i].dataY[24-2]!=0) {
            DamageBoss(i,Parsing.FloatU(gameplay[38]) * Math.pow(1.08,scrs[i].dataY[25-2]));
          }
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
        //Grow loop
        var i,lngt = growT.length;
        for(i=0;i<lngt;i++)
        {
            growW[i]--;
            if(growW[i] > 0)
            {
                var ulam = growT[i].split("g")[0];
                var place = growT[i].split("g")[1];
                WorldData.Load(ulam);
                var tume = WorldData.GetNbt(Parsing.IntU(place)+1,0);
                
                tume -= 5;
                if(tume > 0) WorldData.UpdateNbt(Parsing.IntU(place)+1,0,tume)
                else
                {
                    serverGrow(ulam, place);
                    growT.remove(i);
                    growW.remove(i);
                    lngt--; i--;
                    growActive(ulam);
                }
            }
            else
            {
                growT.remove(i);
                growW.remove(i);
                lngt--; i--;
            }
      }

      //Driller loop
      lngt = drillT.length;
      for(i=0;i<lngt;i++)
      {
          drillW[i]--;
          if(drillW[i] > 0)
          {
              drillC[i] -= 5;
              if(drillC[i] <= 0)
              {
                  var ulam = drillT[i].split("w")[0];
                  var place = drillT[i].split("w")[1];
                  serverDrill(ulam, place);
                  drillT.remove(i);
                  drillW.remove(i);
                  drillC.remove(i);
                  lngt--; i--;
                  growActive(ulam);
              }
          }
          else
          {
              drillT.remove(i);
              drillW.remove(i);
              drillC.remove(i);
              lngt--; i--;
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

      //Spam interval resets & checking
      for(i=0;i<max_players;i++)
      {
          var pl = plr.pclass[i].periodic;
          for(var per_type in pl)
          {
              var pl2 = pl[per_type];
              if(pl2[0]==null) continue;
              if(Date.now() < pl2[0]) continue;
              if(pl2[1] < pl2[2]) { //if was not done enough times
                  kick(i); break;
              }
              pl2[0] += pl2[4];
              pl2[1] = 0;
          }
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
    WorldData.Load(scrs[i].bID);
    WorldData.UpdateData(1,scrs[i].dataX[1]);

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
  if(scrs[i].dataY[2-2]!=2 || (scrs[i].behaviour.type==1 && scrs[i].dataY[18-2]==2)) return;
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
  if(config.show_positions) eff = "/RPU " + max_players + " ";
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
function kick(i)
{
  //Player saving
  var artid = plr.backpack[i].split(";")[30] - 41;
  if(plr.backpack[i].split(";")[31]==0) artid = -41;

  var pats = plr.data[i].split(";");
  var power_spec = plr.pclass[i].ctrlPower;
  if(power_spec < 0) power_spec = 0; if(power_spec > 1) power_spec = 1;
  if([2,3].includes(artid)) pats[11] = (power_spec+"").replaceAll(".",",");
  plr.data[i] = pats.join(";");
  SaveAllNow();

  //Player cleaning
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
  plr.pclass[i].periodic = {};
}

//Bullet functions
function MaxAgeOfPlayerBullet(n)
{
  var ret = 0;
  if(n==1) ret = Parsing.FloatU(gameplay[45]);
  if(n==2) ret = Parsing.FloatU(gameplay[46]);
  if(n==3) ret = Parsing.FloatU(gameplay[47]);
  if(n==14) ret = Parsing.FloatU(gameplay[49]);
  if(n==15) ret = Parsing.FloatU(gameplay[48]);
  if(ret==0) ret = 0.001;
        
  var ret2 = 100;
  if(n==1) ret2 = Math.floor(35*Parsing.FloatU(gameplay[92])/ret) + 1;
  if(n==2) ret2 = Math.floor(35*Parsing.FloatU(gameplay[93])/ret) + 1;
  if(n==3) ret2 = Math.floor(35*Parsing.FloatU(gameplay[96])/ret) + 1;
  if(n==14) ret2 = Math.floor(35*Parsing.FloatU(gameplay[94])/ret) + 1;
  if(n==15) ret2 = Math.floor(35*Parsing.FloatU(gameplay[95])/ret) + 1;

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
  if(Parsing.IntU(arg[3]) <= bulletsT[n].max_age)
  {
    bulletsT[n].max_age = Parsing.IntU(arg[3]);
    sendToAllPlayers(
      "/RetNewBulletDestroy "+
      arg[1]+" "+
      arg[2]+" "+
      arg[3]+" "+
      noise +" "+
      " X X"
    );
  }
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
      eff = insertFloatToChar4(eff,-9,Parsing.FloatU(splitted[0]));
      eff = insertFloatToChar4(eff,-5,Parsing.FloatU(splitted[1]));
      eff = insertRotToChar1(eff,-1,Parsing.FloatU(splitted[4]));
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
        eff = insertHealthToChar1(eff,-1,Parsing.FloatU(splitted[8]));
        if(!sendAll) plr.mems[i].health = splitted[8];
      }
      if(rbt[1][2]==1) //resp pos
      {
        eff += "XXXXXXXX";
        eff = insertFloatToChar4(eff,-8,Parsing.FloatU(splitted[6]));
        eff = insertFloatToChar4(eff,-4,Parsing.FloatU(splitted[7]));
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
    eff = insertIntToChar3(eff,-3,Parsing.FloatU(str[i]));
  }
  return eff;
}

function serverGrow(ulam, place)
{
    WorldData.Load(ulam);
    var fob_here = WorldData.GetFob(Parsing.IntU(place)+1);
    if([5,6,25].includes(fob_here))
    {
        WorldData.UpdateFob(Parsing.IntU(place)+1,Parsing.IntU(growSolid[fob_here].split(";")[2])); //Mtp counters handling around this function
        sendToAllPlayers("/RetGrowNow " + ulam + " " + place + " X X");
    }
}
function serverDrill(ulam, place)
{
    WorldData.Load(ulam);
    if([2].includes(WorldData.GetFob(Parsing.IntU(place)+1)))
    {
        var stackedItem = WorldData.GetNbt(Parsing.IntU(place)+1,0);
        var type = WorldData.GetType() % 16;
        var gItem = Parsing.IntU(drillItemGet(type,stackedItem));

        if(gItem==0) return;
        var gCountEnd = WorldData.GetNbt(Parsing.IntU(place)+1,1) + 1;

        WorldData.UpdateNbt(Parsing.IntU(place)+1,0,gItem);
        WorldData.UpdateNbt(Parsing.IntU(place)+1,1,gCountEnd);

        sendToAllPlayers("/RetFobsDataChange "+ulam+" "+place+" "+gItem+" 1 -1 "+gCountEnd+" 2 X X");
    }
}
function drillItemGet(ast,stackedItem)
{
  var ltdt = drillLoot[ast].split(";");
  var lngt = ltdt.length;
  var rnd = func.randomInteger(0, 999);
  var i;
  for (i=0;i*3+2<lngt;i++) {
    if (rnd >= ltdt[i*3+1] && rnd <= ltdt[i*3+2]) {
      if(stackedItem==0 || stackedItem==ltdt[i*3]) return ltdt[i*3];
      else return 0;
    }
  }
  return 0;
}
function handDrillTimeGet(pid)
{
  var upg3hugity = 1.12;
  var upg3down = 90, upg3up = 210;
  var matpow = upg3hugity ** (Parsing.FloatU(plr.upgrades[pid].split(";")[2])+Parsing.FloatU(gameplay[2]));
	down = Math.round(upg3down/matpow);
	up = Math.round(upg3up/matpow);
	return func.randomInteger(down,up);
}
let GrowActiveDelay = new Set();
function growActive(ulam)
{
    var i, block, tim;
    WorldData.Load(ulam);
    var type = WorldData.GetType();
    if(!(type>=0 && type<=63)) return;
    type = type % 16;

    for(i=0;i<20;i++)
    {
        block = WorldData.GetFob(i+1);
        if([25].includes(block) || ([5,6].includes(block) && type==6)) //Grow segment
        {
            if(!growT.includes(ulam+"g"+i))
            {
                if(WorldData.GetNbt(i+1,0)==0)
                {
                    var tab = growSolid[block].split(";");
                    tim = func.randomInteger(tab[0], tab[1]);
                    WorldData.UpdateNbt(i+1,0,tim);
                }
                growT.push(ulam+"g"+i);
                growW.push(100);
            }
            else growW[growT.indexOf(ulam+"g"+i)] = 100;
        }
        if([2].includes(block) && WorldData.GetNbt(i+1,1) < 5) //Driller segment
        {
            if(!drillT.includes(ulam+"w"+i))
            {
                tim = func.randomInteger(180, 420);
                drillT.push(ulam+"w"+i);
                drillW.push(100);
                drillC.push(tim);
            }
            else drillW[drillT.indexOf(ulam+"w"+i)] = 100;
        }
    }
}

function mtpCountersReset(ulam, place)
{
  var ind;
  
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
    var bpkUpgrade = Parsing.FloatU(plr.upgrades[invID].split(";")[4]);
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
    if (!(itemS == item && Parsing.IntU(countS) + Parsing.IntU(count) >= 0))
      return false;
  } else return true;

  effTab[slot * 2] = item;
  effTab[slot * 2 + 1] = Parsing.IntU(effTab[slot * 2 + 1]) + Parsing.IntU(count);

  if (mode == "INV") plr.inventory[invID] = effTab.join(";");
  else plr.backpack[invID] = effTab.join(";");

  return true;
}

//Fobs change functions
function checkFobChange(ulam, place, start1, start2)
{  
  WorldData.Load(ulam);
  type_here = WorldData.GetType();

  if(!(type_here>=0 && type_here<=63)) return false; //must be asteroid
  if((start1==21 || start2==21) && WorldData.GetNbt(Parsing.IntU(place)+1,1)!=0) return false; //driller can be broken always

  var fob_here = WorldData.GetFob(Parsing.IntU(place)+1);
  return (fob_here==start1 || fob_here==start2);
}

function fobChange(ulam, place, end)
{
  WorldData.Load(ulam);
  WorldData.UpdateFob(Parsing.IntU(place) + 1, end);
  mtpCountersReset(ulam,place);
  growActive(ulam);
}

//Fob21 change functions
function checkFobDataChange(ulam, place, item, deltaCount, id21)
{
  var max_count;
  if(id21==21) max_count = 35;
  else if(id21==2) max_count = 5;
  else if(id21==52) max_count = 10;
  else return false;

  WorldData.Load(ulam);
  type_here = WorldData.GetType();

  if(!(type_here>=0 && type_here<=63)) return false; //must be asteroid
  if(WorldData.GetFob(Parsing.IntU(place)+1) != id21) return false; //must be good storage

  var item_here = WorldData.GetNbt(Parsing.IntU(place)+1,0);
  var count_here = WorldData.GetNbt(Parsing.IntU(place)+1,1);
  if(item_here==item || count_here==0)
  {
      var countEnd = count_here + Parsing.IntU(deltaCount);
      return (countEnd>=0 && countEnd<=max_count);
  }
  else return false;
}
function fobDataChange(ulam, place, item, deltaCount)
{
  WorldData.Load(ulam);
  var countEnd = WorldData.GetNbt(Parsing.IntU(place)+1,1) + Parsing.IntU(deltaCount);
  if(countEnd != 0) {
    WorldData.UpdateNbt(Parsing.IntU(place)+1,0,Parsing.IntU(item));
    WorldData.UpdateNbt(Parsing.IntU(place)+1,1,Parsing.IntU(countEnd));
  } else {
    WorldData.UpdateNbt(Parsing.IntU(place)+1,0,0);
    WorldData.UpdateNbt(Parsing.IntU(place)+1,1,0);
  }
  return countEnd;
}

//Fobs data functions
function nbts(ulam)
{
    var i,tabR = [];
    WorldData.Load(ulam);
    for(i=1;i<=20;i++) {
      tabR.push(WorldData.GetNbt(i,0) + ";" + WorldData.GetNbt(i,1));
    }
    return tabR.join(" ");
}
function nbt(ulam, place)
{
  WorldData.Load(ulam);
  return WorldData.GetNbt(Parsing.IntU(place)+1,0) + ";" + WorldData.GetNbt(Parsing.IntU(place)+1,1);
}
function getBlockAt(ulam, place)
{
  WorldData.Load(ulam);
  return WorldData.GetFob(Parsing.IntU(place) + 1);
}

//Count boss battles
function CountBossBattles()
{
    var i,lngt=scrs.length,n=0;
    for(i=0;i<lngt;i++)
        if(scrs[i].dataY[2-2]==2) n++;
    return n;
}

//Authority functions
function AsteroidPresency(ulam,pos)
{
    ulam = Parsing.IntU(ulam);
    WorldData.Load(ulam);
    var type = WorldData.GetType();
    if(type<0 || type>63) return false;

    var obj = Universe.GetObject(ulam);
    if(obj==null) return false;
    
    if(new Vector3(pos.x-obj.default_position.x,pos.y-obj.default_position.y,0).Length() >= config.anti_cheat.max_interaction_range)
        return false;

    var anim = obj.animator_reference;
    if(anim==null) return true;
    if(anim.animation_type!=1) return true;
    
    var sec_tab = Universe.GetSectorNameByUlam(ulam).split("_");
    if(sec_tab[0]=="B") return true;

    sec_tab[1] = Parsing.IntU(sec_tab[1]);
    sec_tab[2] = Parsing.IntU(sec_tab[2]);
    var Ulam = makeUlam(sec_tab[1],sec_tab[2]);
    var reduced_state = "default";
        
    var i,lngt = scrs.length;
    for(i=0;i<lngt;i++) {
        if(Ulam==scrs[i].sID) {
            reduced_state = GetReducedState(scrs[i]);
            break;
        }
    }
    return !anim.animation_when_done.split(";").includes(reduced_state);   
}
function BossPresency(ulam,pos)
{
    ulam = Parsing.IntU(ulam);
    WorldData.Load(ulam);
    var type = WorldData.GetType();
    if(type!=1024) return false;

    var obj = Universe.GetObject(ulam);
    if(obj==null) return false;
  
    return (new Vector3(pos.x-obj.default_position.x,pos.y-obj.default_position.y,0).Length() < config.anti_cheat.max_interaction_range);
}
function CheckForLocal(what,pos)
{
    let X = Math.round(pos.x / 200) * 2;
    let Y = Math.round(pos.y / 200) * 2;
    const Surroundings = [];
    Universe.GetSector("S_" + X + "_" + Y).forEach(elm => Surroundings.push(elm));
    for(var x=-2;x<=1;x++) for(var y=-2;y<=1;y++)
        Universe.GetSector("B_" + (X+x) + "_" + (Y+y)).forEach(elm => Surroundings.push(elm));
    var i,lngt = Surroundings.length;
    for(i=0;i<lngt;i++) {
    let obj = Surroundings[i];
    if(obj!=null)
    {
        if(what=="star" && obj.obj=="star") return true;
        if(what.startsWith("drill_"))
        {
            var drill_type = Parsing.IntU(what.split("_")[1]);
            if(["wall","sphere","piston"].includes(obj.obj) && obj.type==drill_type) return true;
            if(obj.obj == "asteroid" && !obj.hidden) {
                WorldData.Load(obj.ulam);
                if(WorldData.GetType()==drill_type) return true;
            }
        }
    }}
    return false;
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
  if(!config.keep_inventory)
  {
      plr.inventory[pid] = "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
      plr.upgrades[pid] = "0;0;0;0;0";
      plr.backpack[pid] = "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
  }
  else
  {
      sendTo(se3_ws[pid],"/RetKeepInventory "+plr.livID[pid]+" "+
        plr.inventory[pid]+" "+
        plr.upgrades[pid]+" "+
        plr.backpack[pid]+
      " X X");
  }

  plr.sHealth[pid] = 1;
  plr.sRegTimer[pid] = 0;

  plr.pclass[pid].force_teleport_respawn = true;
  plr.pclass[pid].last_pos_changes = [];
  plr.pclass[pid].ctrlPower = 0;

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

//Get player commands
function getPlayerPosition(pid)
{
  var spl = plr.data[pid].split(";");
  return [spl[0],spl[1]];
}
function getPlayerBulletDamage(pid,type)
{
  var spl = plr.upgrades[pid].split(";");
  var bulUpg = Parsing.FloatU(spl[3]);
  var mult = 1.08 ** bulUpg;
  switch(type)
  {
    case "1": return mult * Parsing.FloatU(gameplay[3]);
    case "2": return mult * Parsing.FloatU(gameplay[27]);
    case "3": return mult * Parsing.FloatU(gameplay[28]);
    case "14": return mult * Parsing.FloatU(gameplay[33]);
    case "15": return mult * Parsing.FloatU(gameplay[34]);
    default: return 0;
  }
}
function getPlayerArtefact(pid)
{
  var splitted = plr.backpack[pid].split(";");
  var item = Parsing.IntU(splitted[30]);
  var count = Parsing.IntU(splitted[31]);
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
  var cl_martid = Parsing.IntU(pldata[5].split("&")[1]) % 100;
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
    var pos_aft_x = Parsing.FloatU(tab_aft[0]);
    var pos_aft_y = Parsing.FloatU(tab_aft[1]);
    var tab_dat = plr.data[pid].split(";");
    var pos_dat_x = Parsing.FloatU(tab_dat[0]);
    var pos_dat_y = Parsing.FloatU(tab_dat[1]);
    var spawn_deviation = Math.sqrt((pos_aft_x-pos_dat_x)**2 + (pos_aft_y-pos_dat_y)**2);
    if(spawn_deviation > 1 && !config.anti_cheat.allow_everywhere_spawn) return false;
  }

  else if(before.length>1 && after.length>1) //player normally updates
  {
    //check if good movement
    var tab_bef = before.split(";");
    var pos_bef_x = Parsing.FloatU(tab_bef[0]);
    var pos_bef_y = Parsing.FloatU(tab_bef[1]);
    var tab_aft = after.split(";");
    var pos_aft_x = Parsing.FloatU(tab_aft[0]);
    var pos_aft_y = Parsing.FloatU(tab_aft[1]);
    var position_change = Math.sqrt((pos_aft_x-pos_bef_x)**2 + (pos_aft_y-pos_bef_y)**2);
    
    if(flags[3]!="T") {
      plr.pclass[pid].PosChangeAdd(position_change);
      if(plr.pclass[pid].PosChangeSum()*5 > config.anti_cheat.max_movement_speed) return false;
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
  let client_ip;
  if (config.trust_proxy) {
    client_ip = req.headers['x-forwarded-for']?.split(',')[0]?.trim() || req.socket.remoteAddress;
  }
  else client_ip = req.socket.remoteAddress;

  //IP bans execute
  var retbol = false;
  config.banned_ips.forEach(function(ip){
    if(client_ip.endsWith(ip)) { ws.close(); retbol=true; };
  });
  if(retbol) return;

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
        //Bullets remove
        var j,lngt = bulletsT.length;
        for(j=0;j<lngt;j++)
          if(bulletsT[j].owner==i) destroyBullet(j,["",i+"",bulletsT[j].ID,bulletsT[j].age],true);

        //Kicking
        if(se3_wsS[i]=="joining") se3_ws[i]="";
        if(se3_wsS[i]=="game" || se3_wsS[i]=="menu") kick(i);

        break;
      }
    }

    //Connection limiter (-)
    var cur_cons = con_map.get(client_ip);
    con_map.set(client_ip, cur_cons-1);
  });

  ws.on("message", (msg) =>
  {
    var local_string_size = (msg+"").length;
    size_download += local_string_size;
    if(local_string_size > 65536) { ws.close(); return; }

    var i, arg = (msg+"").split(" ");
    var msl = arg.length;

    //IP bans execute 2
    config.banned_ips.forEach(function(ip){
      if(client_ip.endsWith(ip)) { ws.close(); retbol = true; }
    });
    if(retbol) return;

    //messages for connection counter
    if(!MsgMap.has(ws)) MsgMap.set(ws,1);
    else MsgMap.set(ws,MsgMap.get(ws)+1);
    if(MsgMap.get(ws) > config.anti_cheat.max_messages_per_second) { ws.close(); return; }

    if (arg[0] == "/AllowConnection") // 1[nick] 2[RedVersion] 3[ConID]
    {
      if(!FilterArgs(arg,["nick","short","EndID"])) return;

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
          plr.sHealth[i] = Parsing.FloatU(plr.data[i].split(";")[8]);
          plr.sRegTimer[i] = Parsing.FloatU(plr.data[i].split(";")[10]);
          plr.pclass[i].DataImport(plr.data[i].split(";")[6],plr.data[i].split(";")[7],Parsing.FloatU(plr.data[i].split(";")[11]));
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
              seed+"&"+MemoriesForClient(plr.data[i]) +
              " X X"
          );
          se3_ws[i] = ws;
          se3_wsS[i] = "menu";
          IPv4s[i] = client_ip;
          console.log(hourHeader + plr.nicks[i] + " connected: " + i + "");
          return;
        }
      }
      sendTo(ws,"/RetAllowConnection -4 X X"); //server is full
      return;
    }
    if (arg[0] == "/ImJoining") // 1[PlayerID]
    {
      if(!FilterArgs(arg,["PlaID"])) return;
      if(!checkPlayerM(arg[1],ws)) return;

      plr.waiter[arg[1]] = 500;
      se3_wsS[arg[1]] = "joining";
      return;
    }
    if (arg[0] == "/ImJoined") // 1[PlayerID] 2[ConID]
    {
      if(!FilterArgs(arg,["PlaID","EndID"])) return;
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

      sendTo(ws,"/RetDifficultySet "+config.difficulty+" X X");

      //Join info
      console.log(hourHeader + plr.nicks[arg[1]] + " joined the game");
      return;
    }

    // ----- GAMEPLAY COMMANDS ----- \\

    //Gameplay filter
    if(arg[1]==undefined) { ws.close(); return; }
    if(!FilterArgs([arg[1]],["PlaID"],false)) { ws.close(); return; }
    if(!checkPlayerG(arg[1],ws)) { ws.close(); return; }

    //Commands
    if (arg[0] == "/PlayerUpdate") // 1[PlayerID] 2<PlayerData> 3[pingTemp] 4[flags]
    {
      if(!plr.pclass[arg[1]].PeriodicInsert("player_update")) {kick(arg[1]); return;}
      if(!FilterArgs(arg,["PlaID","UpdateData","short","Flags"])) return;
      var censured = Censure(arg[2],arg[1],arg[msl-1]);
      if(!updateHasSense(plr.players[arg[1]],censured,arg[1],arg[4])) {kick(arg[1]); return;}

      if(arg[4][3]=="T") plr.pclass[arg[1]].allowed_teleport_small = false;

      if(censured=="1") arg[4]="FFF"+arg[4][3];

      var memX = Parsing.FloatU(plr.data[arg[1]].split(";")[0]);
      var memY = Parsing.FloatU(plr.data[arg[1]].split(";")[1]);

      var hiddenFlags = "";
      plr.players[arg[1]] = censured;
      if(censured!="1")
      {
        if(Parsing.IntU(censured.split(";")[5].split("&")[1])%25==1) hiddenFlags += "T";
        else hiddenFlags += "F";
        plr.data[arg[1]] = censured;
      }
      else hiddenFlags += "F";

      //Small technicals
      if (plr.waiter[arg[1]] > 1) plr.waiter[arg[1]] = 250;
      sendTo(ws,"P"+arg[3]); //Short type command

      // Flags explained
      // [0] - impulseEnabled
      // [1] - impulseStarted     | POWER -= IMPULSE
      // [2] - invisibilityPulse (relict)
      // [3] - doTeleport

      // Hidden flags
      // [0] - invisible          | POWER_REGEN_BLOCKED, POWER -= ILLUSION_USE

      //Flags questioning
      var artef = getPlayerArtefact(arg[1]);
      if(artef!=2 && (arg[4][0]=="T" || arg[4][1]=="T")) {kick(arg[1]); return;} //IMPULSE
      if(artef!=3 && (hiddenFlags[0]=="T" || arg[4][2]=="T")) {kick(arg[1]); return;} //ILLUSION

      //Flags executing
      if(arg[4][1]=="T") {
        var dtn = Date.now();
        var pl = plr.pclass[arg[1]];
        if(pl.impulse_wait > dtn) {kick(arg[1]); return;} // impulsed too fast
        pl.impulse_wait = dtn + (Math.round(Parsing.FloatU(gameplay[102]))-3) * 20;
        plr.pclass[arg[1]].ctrlPower -= 0.2;
        plr.impulsed[arg[1]] = [];
      }
      if(hiddenFlags[0]=="T") {
        plr.pclass[arg[1]].ctrlPower -= unit * Parsing.FloatU(gameplay[22]);
      }
      plr.pclass[arg[1]].powerRegenBlocked = (hiddenFlags[0]=="T") || (arg[4][0]=="T");

      //Impulse damage
      var j, caray = censured.split(";");
      if(caray.length>1 && arg[4][0]=="T")
      {
        var xa = Parsing.FloatU(caray[0]);
        var ya = Parsing.FloatU(caray[1]);
        var xb = Parsing.FloatU(memX);
        var yb = Parsing.FloatU(memY);
        func.CollisionLinearBulletSet(xa,ya,xb,yb,1.2);

        if(config.pvp)
        for(j=0;j<max_players;j++)
        {
          if(plr.players[j]!="0" && plr.players[j]!="1" && arg[1]!=j && !plr.impulsed[arg[1]].includes(j))
          {
            var plas = plr.players[j].split(";");
            var xc = Parsing.FloatU(plas[0]);
            var yc = Parsing.FloatU(plas[1]);
            if(func.CollisionLinearCheck(xc,yc,0.92))
            {
              DamageFLOAT(j,Parsing.FloatU(gameplay[29]))
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
              DamageBoss(j,Parsing.FloatU(gameplay[29]))
              plr.impulsed[arg[1]].push(-l);
            }
          }
        }
      }
      return;
    }
    if (arg[0] == "/ChatMessage") // 1[PlayerID] 2[Message]
    {
      if(!plr.pclass[arg[1]].PeriodicInsert("chat")) return;
      if(!FilterArgs(arg,["PlaID","Msg256"])) return;
      
      console.log("<" + plr.nicks[Parsing.FloatU(arg[1])] + "> " + arg[2].replaceAll("\t"," "));
      sendToAllPlayers("/RetChatMessage <" + plr.nicks[Parsing.FloatU(arg[1])] + "> " + arg[2] + " X X");
    }
    if (arg[0] == "/InventoryPush") // 1[PlayerID] 2[PushID]
    {
      if(!FilterArgs(arg,["PlaID","PushID"])) return;
      

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
      if(!FilterArgs(arg,["PlaID","ulam","place","turnID"])) return;

      if(arg[4]=="40") //dead alien turn
        if ( checkFobChange(arg[2], arg[3], "13", "23") || checkFobChange(arg[2], arg[3], "25", "27") ) {
          AlienDatabase[arg[2]+";"+arg[3]] = [getBlockAt(arg[2],arg[3]),3];
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
      if(!plr.pclass[arg[1]].PeriodicInsert("particles")) return;
      if(!FilterArgs(arg,["PlaID","ID128","float","float"])) return;

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
      if(!FilterArgs(arg,["PlaID","ulam","ulam","place"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;
      if(CountBossBattles() >= config.max_active_bosses) return;

      var bID = arg[2];
      var fobID = arg[3];
      var fobIndex = arg[4];

      var ppos = getPlayerPosition(arg[1]);
      if(!BossPresency(bID,new Vector3(Parsing.FloatU(ppos[0]),Parsing.FloatU(ppos[1]),0))) return;
      if(!AsteroidPresency(fobID,new Vector3(Parsing.FloatU(ppos[0]),Parsing.FloatU(ppos[1]),0))) return;

      var lngts = scrs.length;
      for(i=0;i<lngts;i++)
      {
        if(scrs[i].bID==bID)
        {
          if(scrs[i].dataY[2-2]==0 && checkFobDataChange(fobID, fobIndex, "5", "-10", "52"))
          {
            var gCountEnd = fobDataChange(fobID, fobIndex, "5", "-10");
            scrs[i].dataY[2-2] = 1;
            scrs[i].givenUpPlayers = [];
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
      if(!FilterArgs(arg,["PlaID","ulam"])) return;

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
              var dx = Parsing.FloatU(plas[0]) - scrs[i].posCX;
              var dy = Parsing.FloatU(plas[1]) - scrs[i].posCY;
              if(dx**2 + dy**2 <= (in_arena_range)**2) {
                players_inside++;
                if(arg[1]==j+"") found_me = true;
              }
            }
          }
          if(found_me) {
            if(players_inside==1) scrs[i].dataY[4-2] = 1;
            else if(!scrs[i].givenUpPlayers.includes(plr.nicks[arg[1]])) {
              sendTo(ws,"/RetGiveUpTeleport "+arg[1]+" "+bID+" 1024 X "+blivID);
              plr.pclass[arg[1]].allowed_teleport_small = true;
              scrs[i].givenUpPlayers.push(plr.nicks[arg[1]]);
            }
          }
          else if(players_inside==0) scrs[i].dataY[4-2] = 1;
          break;
        }
      }
    }
    if (arg[0] == "/ClientDamage") // 1[PlayerID] 2[dmg] 3[ImmID] 4[info] 5[ifRinged]
    {
      if(!plr.pclass[arg[1]].PeriodicInsert("particles")) { kick(arg[1]); return; }
      if(!FilterArgs(arg,["PlaID","fraction01","EndID","short","short"])) return;
      if(inHeaven(arg[1])) return;
      if(arg[4]=="I" && (plr.backpack[arg[1]].split(";")[30]!="45" || Parsing.IntU(plr.backpack[arg[1]].split(";")[31])<=0)) return;

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
      if(!FilterArgs(arg,["PlaID","UpgID","Slot"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var ljPlaID = arg[1];
      var ljUpgID = arg[2];
      var ljSlot = arg[3];

      var ljTab = plr.upgrades[ljPlaID].split(";");
      var current_level = Parsing.IntU(ljTab[ljUpgID]);
      if(current_level < 0 || current_level > 4) return;

      var upg_costs = [
        [Parsing.IntU(gameplay[112]),Parsing.IntU(gameplay[113])],
        [Parsing.IntU(gameplay[114]),Parsing.IntU(gameplay[115])],
        [Parsing.IntU(gameplay[116]),Parsing.IntU(gameplay[117])],
        [Parsing.IntU(gameplay[118]),Parsing.IntU(gameplay[119])],
        [Parsing.IntU(gameplay[120]),Parsing.IntU(gameplay[121])]
      ];

      var ljItem = upg_costs[current_level][0];
      var ljCount = upg_costs[current_level][1];

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
      if(!FilterArgs(arg,["PlaID","ulam","place","item","1,-1","Slot","StorageID"])) return;
      if(arg[5]=="1" && arg[7]=="2") return; //Trying to insert item into driller
      var overolded = (arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1]));

      var gPlayerID = arg[1];
      var gUlamID = arg[2];
      var gPlaceID = arg[3];
      var gItem = arg[4];
      var gDeltaCount = arg[5];
      var gSlot = arg[6];
      var gID21 = arg[7];
      
      var ppos; if(!overolded) ppos = getPlayerPosition(arg[1]);
      if(checkFobDataChange(gUlamID, gPlaceID, gItem, gDeltaCount, gID21) && !overolded)
      if(AsteroidPresency(gUlamID,new Vector3(Parsing.FloatU(ppos[0]),Parsing.FloatU(ppos[1]),0)))
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
          nbt(gUlamID, gPlaceID) + ";" + gDeltaCount + " " +
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
      if(!FilterArgs(arg,["PlaID","item","count","SlotI","SlotB"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var bpPlaID = arg[1];
      var bpItem = arg[2];
      var bpCount = arg[3];
      var bpSlotI = arg[4];
      var bpSlotB = arg[5];

      var safeCopyI = plr.inventory[bpPlaID];
      var safeCopyB = plr.backpack[bpPlaID];

      if (invChangeTry(bpPlaID, bpItem, bpCount, bpSlotI))
        if (invChangeTry(bpPlaID, bpItem, -bpCount, bpSlotB))
        {
          //Success
          if(bpSlotB=="24") { //changed artefact slot
            plr.pclass[bpPlaID].ctrlPower = 0;
          }
          return;
        }

      //Abort
      plr.inventory[bpPlaID] = safeCopyI;
      plr.backpack[bpPlaID] = safeCopyB;
      kick(bpPlaID);
    }
    if(arg[0] == "/ScrRefresh") // 1[PlayerID] 2[bID] 3[inArena]
    {
      if(!FilterArgs(arg,["PlaID","ulam","short"])) return;

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
      if(!FilterArgs(arg,["PlaID","ulam"])) return;

      var remindex = plr.bossMemories[arg[1]].indexOf(arg[2]);
      if(remindex!=-1)
        plr.bossMemories[arg[1]].remove(remindex);
    }
    if (arg[0] == "/GrowLoaded") // 1[PlayerID] 2[UlamList]
    {
      if(!plr.pclass[arg[1]].PeriodicInsert("grow_loaded")) return;
      if(!FilterArgs(arg,["PlaID","UlamList"])) return;

      var tab = arg[2].split(";");
      var lngt = tab.length;
      for(i=0;i<lngt;i++) GrowActiveDelay.add(tab[i]);
    }
    if (arg[0] == "/Crafting") // 1[PlaID] 2[CraftID] 3[Slot1] 4[Slot2] 5[SlotE]
    {
      if(!FilterArgs(arg,["PlaID","CraftID","Slot","SlotIf","Slot"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var cPlaID = arg[1];
      var tab = craftings.split(";");

      //first ingredient
      var cId1 = tab[Parsing.IntU(arg[2])*6 + 0];
      var cCo1 = tab[Parsing.IntU(arg[2])*6 + 1];
      var cSl1 = arg[3];

      //second ingredient
      var cId2 = tab[Parsing.IntU(arg[2])*6 + 2];
      var cCo2 = tab[Parsing.IntU(arg[2])*6 + 3];
      var cSl2 = arg[4];
      if(cSl2=="-1" && cCo2!="0") return;

      //crafted item
      var cIdE = tab[Parsing.IntU(arg[2])*6 + 4];
      var cCoE = tab[Parsing.IntU(arg[2])*6 + 5];
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
      if(!FilterArgs(arg,["PlaID","PotionID","SlotI"])) return;
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
        var heal_size;
        if(arg[2]=="1") heal_size = gameplay[31];
        if(arg[2]=="2") heal_size = gameplay[39];
        if(arg[2]=="3") heal_size = "10000";
        HealFLOAT(arg[1],heal_size);
        sendTo(ws,"/RetHeal "+arg[1]+" "+arg[2]+" X X");
      }

      if(arg[2]=="5" || arg[2]=="3")
      {
        var artid = plr.backpack[arg[1]].split(";")[30] - 41;
        if(plr.backpack[arg[1]].split(";")[31]=="0") artid = -41;
        if(artid==2 || artid==3) plr.pclass[arg[1]].ctrlPower = 1;
      }
    }
    if (arg[0] == "/JunkDiscard") // 1[PlayerID] 2[Item] 3[Count]
    {
      if(!FilterArgs(arg,["PlaID","item","count-"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      if(!invChangeTry(arg[1], arg[2], arg[3], "25")) kick(arg[1]);
    }
    if (arg[0] == "/SetRespawn") // 1[PlayerID] 2[Slot] 3,4[newpos(0 0/1 1)]
    {
      if(!FilterArgs(arg,["PlaID","Slot","short","short"])) return;
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
      if(!plr.pclass[arg[1]].PeriodicInsert("drill_ask")) return;
      if(!FilterArgs(arg,["PlaID","DrillID","EndID"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var ppos = getPlayerPosition(arg[1]);
      if(CheckForLocal("drill_"+arg[2],new Vector3(Parsing.FloatU(ppos[0]),Parsing.FloatU(ppos[1]),0)))
          plr.pclass[arg[1]].DrillAsk(arg[2],arg[1],arg[3]);
    }
    if (arg[0] == "/DrillGet") // 1[PlayerID] 2[Item] 3[Slot]
    {
      if(!FilterArgs(arg,["PlaID","item","Slot"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) arg[3]="25";

      if(arg[3]!="25") {
        if(!plr.pclass[arg[1]].DrillGet(arg[1],arg[2],arg[3])) kick(pid);
      }
      else plr.pclass[arg[1]].DrillDiscard(arg[2]);
    }
    if (arg[0] == "/CommandGive") // 1[PlayerID] 2[Item] 3[Count] 4[Slot]
    {
      if(!FilterArgs(arg,["PlaID","item","count+","Slot"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var indof = give_array.indexOf("give "+plr.nicks[arg[1]]+" "+arg[2]+" "+arg[3]);
      if(indof!=-1) {
        give_array[indof] = "USED";
        if(!invChangeTry(arg[1],arg[2],arg[3],arg[4])) {kick(arg[1]); return;}
        console.log("Given "+arg[3]+"x item("+arg[2]+") to: "+plr.nicks[arg[1]]);
      }
      else {kick(arg[1]); return;}
    }
    if (arg[0] == "/CommandTp") // 1[PlayerID] 2[x] 3[y]
    {
      if(!FilterArgs(arg,["PlaID","length 1 256","length 1 256"])) return;
      if(arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1])) return;

      var indof = give_array.indexOf("tp "+plr.nicks[arg[1]]+" "+arg[2]+" "+arg[3]);
      if(indof!=-1) {
        give_array[indof] = "USED";
        var bef = plr.players[arg[1]].split(";");
        bef[0] = Parsing.FloatU(arg[2]);
        bef[1] = Parsing.FloatU(arg[3]);
        plr.players[arg[1]] = bef.join(";");
        console.log("Teleported player "+plr.nicks[arg[1]]+" to coordinates: "+bef[0]+" "+bef[1]);
      }
    }
    if (arg[0] == "/FobPlace") // 1[PlayerID] 2[UlamID] 3[PlaceID] 4[EndFob] 5[Slot]
    {
      if(!FilterArgs(arg,["PlaID","ulam","place","fob-interactable","Slot"])) return;
      var overolded = (arg[msl-1] != plr.livID[arg[1]] || inHeaven(arg[1]));

      var fPlayerID = arg[1];
      var fUlamID = arg[2];
      var fPlaceID = arg[3];
      var fEndFob = arg[4];
      var fDropID = fEndFob;
      var fCount = "-1";
      var fSlot = arg[5];

      var fFob21TT = nbt(fUlamID, fPlaceID);

      var ppos; if(!overolded) ppos = getPlayerPosition(arg[1]);
      if(checkFobChange(fUlamID, fPlaceID, "0", "48") && !overolded)
      if(AsteroidPresency(fUlamID,new Vector3(Parsing.FloatU(ppos[0]),Parsing.FloatU(ppos[1]),0)))
      {
        if (invChangeTry(fPlayerID, fDropID, fCount, fSlot))
        {
          fobChange(fUlamID, fPlaceID, fEndFob);
          fFob21TT = nbt(fUlamID, fPlaceID);
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
      if(!FilterArgs(arg,["PlaID","ulam","place","fob-interactable","Slot"])) return;
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

      var fFob21TT = nbt(fUlamID, fPlaceID);

      var adb = AlienDatabase[fUlamID+";"+fPlaceID];
      var udb = "NONE"; if(adb!=undefined) udb = adb[0]+"";
      var mTurnable = (checkFobChange(fUlamID, fPlaceID, "40", "-1") && [fStartFob1,fStartFob2].includes(udb));

      var ppos; if(!overolded) ppos = getPlayerPosition(arg[1]);
      if((checkFobChange(fUlamID, fPlaceID, fStartFob1, fStartFob2) || mTurnable) && !overolded)
      if(AsteroidPresency(fUlamID,new Vector3(Parsing.FloatU(ppos[0]),Parsing.FloatU(ppos[1]),0)))
      {
        if (invChangeTry(fPlayerID, fDropID, fCount, fSlot))
        {
          if(mTurnable) delete AlienDatabase[fUlamID+";"+fPlaceID];
          if(treasure_type!=-1) plr.pclass[fPlayerID].TreasureArrayUpdate(treasure_type,true);
          fobChange(fUlamID, fPlaceID, "0");
          fFob21TT = nbt(fUlamID, fPlaceID);
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
      if(!FilterArgs(arg,["PlaID","BulletType","float","float","EndID","short","Slot","float","float"])) return;
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

      //Check cooldowns
      if(bBulSrc=="I" || bBulSrc=="A")
      {
          var dtn = Date.now();
          var pl = plr.pclass[arg[1]];
          if(pl.bullet_wait > dtn) { sendTo(ws,revert_string); return; } //ignored cooldown
          var here_cooldown;
          switch(bType)
          {
            case "1": here_cooldown = Math.floor(Parsing.FloatU(gameplay[97+0])); break;
            case "2": here_cooldown = Math.floor(Parsing.FloatU(gameplay[97+1])); break;
            case "3": here_cooldown = Math.floor(Parsing.FloatU(gameplay[97+4])); break;
            case "14": here_cooldown = Math.floor(Parsing.FloatU(gameplay[97+2])); break;
            case "15": here_cooldown = Math.floor(Parsing.FloatU(gameplay[97+3])); break;
            default: return;
          }
          pl.bullet_wait = dtn + (here_cooldown-3) * 20;
      }

      //Speed anti-cheat
      if((Parsing.FloatU(bVectX)**2 + Parsing.FloatU(bVectY)**2) ** 0.5 > 1.2) { sendTo(ws,revert_string); return; } //bullet too fast

      //Start position anti-cheat
      var ppos = getPlayerPosition(arg[1]);
      var ddx = Parsing.FloatU(ppos[0]) - Parsing.FloatU(bPosX);
      var ddy = Parsing.FloatU(ppos[1]) - Parsing.FloatU(bPosY);
      var distance_to_center = Math.sqrt(ddx**2 + ddy**2);
      if(distance_to_center > config.anti_cheat.bullet_spawn_allow_radius) { sendTo(ws,revert_string); return; } //bullet wrong spawn position

      //bullet summon
      var tpl = Object.assign({},bulletTemplate);
      tpl.start = Object.assign({},bulletTemplate.start);
      tpl.vector = Object.assign({},bulletTemplate.vector);
      tpl.pos = Object.assign({},bulletTemplate.pos);
      tpl.damaged = [];
      tpl.immune = [];

      var spl = plr.upgrades[bPlaID].split(";");
      var bulUpg = Parsing.FloatU(spl[3]);

      tpl.ID = Parsing.IntU(bRandID);
      tpl.owner = Parsing.IntU(bPlaID);
      tpl.type = Parsing.IntU(bType);
      tpl.start.x = Parsing.FloatU(bPosX);
      tpl.start.y = Parsing.FloatU(bPosY);
      tpl.vector.x = Parsing.FloatU(bVectX);
      tpl.vector.y = Parsing.FloatU(bVectY);
      tpl.pos.x = tpl.start.x;
      tpl.pos.y = tpl.start.y;
      tpl.upgrade_boost = bulUpg;

      tpl.normal_damage = Parsing.FloatU(bDmg);
      tpl.is_unstable = (bType=="3");
      if(bBulSrc=="A") tpl.unstable_virtual = true;

      if(tpl.vector.x==0) tpl.vector.x = 0.00001;
      if(tpl.vector.y==0) tpl.vector.y = 0.00001;

      //1[PlayerID] 2[type] 3,4[position] 5,6[vector] 7[ID] 8["0"]
      spawnBullet(tpl,["/NewBulletSend",bPlaID,bType,bPosX,bPosY,bVectX,bVectY,bRandID,"0"],0);
    }
    if (arg[0] == "/BulletRemove") // 1[PlayerID] 2[BulletID] 3[age]
    {
      if(!FilterArgs(arg,["PlaID","EndID","int"])) return;
      
      var lngt = bulletsT.length;
      for(i=0;i<lngt;i++)
        if(bulletsT[i].owner==arg[1] && bulletsT[i].ID==arg[2])
          destroyBullet(i,arg,true);
    }
    if (arg[0] == "/ShotTurn") // 1[PlayerID] 2[ulam] 3[place] 4[FobStart] 5[BulletID]
    {
      if(!FilterArgs(arg,["PlaID","ulam","place","ShotTurnFob","EndID"])) return;

      var lngt = bulletsT.length;
      var bul_ref;
      for(i=0;i<=lngt;i++) {
        if(i==lngt) return;
        if(bulletsT[i].owner==arg[1] && bulletsT[i].ID==arg[5] && !bulletsT[i].turn_used && (arg[4]!="0" || (bulletsT[i].type=="3" && !bulletsT[i].unstable_virtual))) {
          bulletsT[i].turn_used = true;
          bul_ref = bulletsT[i];
          break;
        }
      }

      var arg4b = "-1", argend = "48";
      if(arg[4]=="23") {arg4b = "25"; argend = "25";}
      if(arg[4]=="25") {arg4b = "23"; argend = "25";}

      if(checkFobChange(arg[2],arg[3],arg[4],arg4b))
      if(AsteroidPresency(arg[2],new Vector3(bul_ref.pos.x,bul_ref.pos.y,0)))
      {
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
    if (arg[0] == "/WorldData") // 1[PlayerID] 2[Ulam] 3[AbortReference]
    {
      if(!FilterArgs(arg,["PlaID","ulam","short"])) return;
      
      var ulamID = Parsing.IntU(arg[2]);

      //generation spam anti-flood
      var sect = Universe.GetSectorNameByUlam(ulamID).split("_");
      sect[1] = Parsing.IntU(sect[1]) * 100;
      sect[2] = Parsing.IntU(sect[2]) * 100;
      if(sect[0]=="B") { sect[1]+=50; sect[2]+=50; }

      var ppos = getPlayerPosition(arg[1]); //player or respawn position (plr.data based)
      ppos[0] = Parsing.FloatU(ppos[0]);
      ppos[1] = Parsing.FloatU(ppos[1]);

      var vct = new Vector3(sect[1]-ppos[0],sect[2]-ppos[1],0);
      if(vct.Length() >= config.anti_cheat.max_worldgen_range) {
          sendTo(ws,"/RetWorldDataAbort "+arg[3]+" X X");
          return;
      }
      
      //actual generation
      var obj = Universe.GetObject(ulamID);
      if(obj==null) return;
      if(obj.obj=="asteroid")
      {
          var generation_code = obj.GetGencode();
          WorldData.Load(ulamID);
          var type = WorldData.GetType();
          if(!(type>=0 && type<=63)) //NOT EXISTS
          {
              WorldData.DataGenerate(generation_code);
              type = WorldData.GetType();
          }
          var lc3 = type;
          for(i=1;i<=20;i++) {
              lc3 += ";"+WorldData.GetFob(i);
          }
          growActive(ulamID);
          sendTo(ws,
            "/RetAsteroidData " +
            ulamID + " " +
            lc3 + " " +
            nbts(ulamID) +
            " X X"
          );
      }
      if(obj.obj=="boss")
      {
          var bID = ulamID;
          var bossType = obj.type;
          var bossPosX = obj.position.x;
          var bossPosY = obj.position.y;
  
          var lc3, inds=-1, lngts=scrs.length;
  
          for(i=0;i<lngts;i++)
          {
              if(scrs[i].bID==bID) {
                  inds = i;
                  break;
              }
          }
  
          if(inds==-1)
          {
              WorldData.Load(bID);
              if(WorldData.GetType()!=1024) {
                  WorldData.DataGenerate("BOSS");
              }
              else for(i=2;i<=60;i++) WorldData.UpdateData(i,0);
              var trX1 = WorldData.GetData(1);
  
              //Boss initialization
              var tpl = Object.assign({},scrTemplate);
              tpl.bID = bID;
              tpl.type = bossType;
              tpl.posCX = Parsing.FloatU(bossPosX);
              tpl.posCY = Parsing.FloatU(bossPosY);
              tpl.sID = makeUlam(Math.round(tpl.posCX/200)*2,Math.round(tpl.posCY/200)*2); //all bosses are inside structural squares
              tpl.dataX = [];
              tpl.dataY = [];
              tpl.givenUpPlayers = [];
              tpl.dataX = [1024,trX1];
              for(i=2;i<=60;i++) tpl.dataY[i-2] = 0;
              var tpl2 = {x:0,y:0}; tpl2.x = tpl.posCX; tpl2.y = tpl.posCY;
              tpl.behaviour = new CBoss(bossType,tpl2,tpl.dataX,tpl.dataY,visionInfo);
              scrs.push(tpl);
          }
      }
    }
    if (arg[0] == "/RequestMemories") // 1[PlayerID] 2[MemoryID]
    {
      if(!plr.pclass[arg[1]].PeriodicInsert("request_memories")) {kick(arg[i]); return;}
      if(!FilterArgs(arg,["PlaID","0-16k"])) return;
      
      sendTo(ws,"/RetMemoryData "+arg[2]+"$"+biome_memories[Parsing.IntU(arg[2])]+" X X");
    }
  });
});

//Command verifier function
function FilterArgs(args,formats,include_headers=true)
{
    if(include_headers)
    {
        formats.unshift("short");
        formats.push("EndID");
    }
    if(args.length != formats.length) return false;
    let i, lngt = formats.length;
    for(i=0;i<lngt;i++)
    {
        let format = formats[i];
        if(format=="EndID") format = "int 0 999999999";
        if(format=="PlaID") format = "int 0 "+(max_players-1);
        if(format=="int") format = "int -2147483648 2147483647";
        if(format=="OldIntInfo") format = "int 0 63";
        if(format=="NewIntInfo") format = "int 0 699";
        if(format=="PushID") format = "int 1 8";
        if(format=="place") format = "int 0 19";
        if(format=="ID128") format = "int 0 127";
        if(format=="UpgID") format = "int 0 4";
        if(format=="Slot") format = "int 0 25";
        if(format=="SlotIf") format = "int -1 25";
        if(format=="SlotI") format = "int 0 8";
        if(format=="SlotB") format = "int 9 25";
        if(format=="item") format = "int 1 127";
        if(format=="count") format = "int -9999999 9999999";
        if(format=="count+") format = "int 1 9999999";
        if(format=="count-") format = "int -9999999 -1";
        if(format=="PotionID") format = "int 1 6";
        if(format=="DrillID") format = "int 0 15";
        if(format=="0-16k") format = "int 0 15999";
        if(format=="float") format = "float -1e+32 1e+32";
        if(format=="Angle") format = "float 0 360";
        if(format=="fraction01") format = "float 0 1";
        if(format=="short") format = "length 0 32";
        if(format=="Flags") format = "length 4 4";
        if(format=="NULL") format = "length 0 0";
        if(format=="BulletType") format = "array 1 2 3 14 15";
        if(format=="turnID") format = "array 40 45";
        if(format=="1,-1") format = "array 1 -1";
        if(format=="StorageID") format = "array 2 52 21";
        if(format=="ShotTurnFob") format = "array 0 23 25";
        
        if(format.startsWith("int ")) //corrector
        {
            let spl = format.split(" ");
            spl[0] = Parsing.IntU(args[i]);
            spl[1] = Parsing.IntU(spl[1]);
            spl[2] = Parsing.IntU(spl[2]);
            if(spl[0] < spl[1] || spl[0] > spl[2]) return false;
            args[i] = spl[0]+"";
        }
        else if(format.startsWith("float ")) //corrector
        {
            let spl = format.split(" ");
            spl[0] = Parsing.FloatU(args[i]);
            spl[1] = Parsing.FloatU(spl[1]);
            spl[2] = Parsing.FloatU(spl[2]);
            if(spl[0] < spl[1] || spl[0] > spl[2]) return false;
            args[i] = spl[0]+"";
        }
        else if(format.startsWith("length "))
        {
            let spl = format.split(" ");
            spl[0] = args[i].length;
            spl[1] = Parsing.IntU(spl[1]);
            spl[2] = Parsing.IntU(spl[2]);
            if(spl[0] < spl[1] || spl[0] > spl[2]) return false;
        }
        else if(format.startsWith("array "))
        {
            let spl = format.split(" ");
            spl.shift();
            if(!spl.includes(args[i])) return false;
        }
        else if(format=="nick")
        {
            if(nickWrong(args[i])) return false;
        }
        else if(format=="Msg256")
        {
            if(args[i].length > 256) return false;
            if(args[i].includes("\r")) return false;
            if(args[i].includes("\n")) return false;
        }
        else if(format=="fob-interactable") //corrector
        {
            var p = Parsing.IntU(args[i]);
            if(!(
              (p>=1 && p<=19) ||
              (p>=21 && p<=23) ||
              (p>=25 && p<=38) ||
              (p>=40 && p<=48) ||
              (p==51) ||
              (p>=54 && p<=62 && p%2==0) ||
              (p>=64 && p<=70)
            )) return false;
            args[i] = p+"";
        }
        else if(format=="CraftID") //corrector
        {
            var p = Parsing.IntU(args[i]);
            if(p < 0 || p >= craftings.split(";").length/6) return false;
            if(p % 7 >= 5) return false;
            args[i] = p+"";
        }
        else if(format=="ulam") //corrector
        {
            var p = Parsing.IntU(args[i]);
            if(p<=0) return false;
            var xy = ulamToXY(p);
            if(xy[0] < -200000 || xy[0] >= 200000) return false;
            if(xy[1] < -200000 || xy[1] >= 200000) return false;
            args[i] = p+"";
        }
        else if(format=="UpdateData") //recursive
        {
            if(args[i]!="1") {
              let arg_tab = args[i].split(";");
              if(!FilterArgs(arg_tab,["float","float","NULL","NULL","Angle","RocketInfo","NULL","NULL","NULL","fraction01","NULL","fraction01"],false))
                  return false;
              args[i] = arg_tab.join(";");
            }
        }
        else if(format=="RocketInfo") //recursive
        {
            let arg_tab = args[i].split("&");
            if(!FilterArgs(arg_tab,["OldIntInfo","NewIntInfo"],false))
                return false;
            args[i] = arg_tab.join("&");
        }
        else if(format=="UlamList") //recursive
        {
            var jngt = args[i].split(";").length;
            if(jngt > 1024) return false;
            let arg_tab = args[i].split(";");
            if(!FilterArgs(arg_tab,new Array(jngt).fill("ulam"),false))
                return false;
            args[i] = arg_tab.join(";");
        }
        else {
            console.log("Unknown filtering format: "+format);
            return false;
        }
    }
    return true;
}

//Jse3 datapack converter
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
    var ipr = Parsing.IntE(str) + "";
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
      totalChance += Math.round(Parsing.FloatE(pom) * 10);
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

          mID=Parsing.IntE(psPath[1]);
          customStructures[mID]=jse3Dat[i].replaceAll('^',' ');

          }catch{datapackError("Error in variable: " + jse3Var[i]);}
      }
    } else if (lgt == 3) {
      if (psPath[0] == "game_translate") {
        if (psPath[1] == "Asteroids") {
          try {
            mID = Parsing.IntE(psPath[2]);
            translateAsteroid[mID] = jse3Dat[i];
          } catch {
            datapackError("Error in variable: " + jse3Var[i]);
          }
        } else if (psPath[1] == "Items_and_objects") {
          try {
            mID = Parsing.IntE(psPath[2]);
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
            mID = Parsing.IntE(translate(psPath[1], 2));
          else mID = Parsing.IntE(translate(psPath[1], 1));
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
          mID = Parsing.IntE(psPath[1]);
          if (psPath[2] == "title_image") {
            mID2 = 7;
            mID = 7 * (mID - 1) + 6;
            jse3Dat[i] = translate(jse3Dat[i], 2) + ";1;0;0;-1;1";
          } else {
            mID2 = Parsing.IntE(psPath[2]);
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
          mID = Parsing.IntE(psPath[1]);
          if (mID < 0 || mID > 31) throw "error";

          if (psPath[2] == "settings") {
            biomeTags[mID] = jse3Dat[i].replaceAll(" ", "_");
          } else if (psPath[2] != "chance") {
            if (psPath[2] == "all_sizes") mID2 = -4;
            else mID2 = Parsing.IntE(psPath[2]) - 4;
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
          mID = Parsing.IntE(psPath[1]);
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

            cur1000biome += mno * Math.round(Parsing.FloatE(jse3Dat[i]) * 10);
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

  if(version!=serverVersion) datapackError("Wrong version or empty version variable");
  if(datName=="") datapackError("Datapack name can't be empty");

  try { checkDatapackGoodE(); } catch {
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
        pom = Parsing.IntE(strs[i]);
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
      if (actual + 1 != Parsing.IntE(strT[i])) return false;
      actual++;
      ended = false;
    } else {
      if (actual > Parsing.IntE(strT[i]) + 1) return false;
      actual = Parsing.IntE(strT[i]);
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
    if (Parsing.IntE(strT[i + 1]) < 0) return false;
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
    craftMaxPage = Parsing.IntE(raws[1]) + "";

    for (i = 0; i < 16; i++) drillLoot[i] = raws[2].split("'")[i];
    for (i = 0; i < 64; i++) fobGenerate[i] = raws[3].split("'")[i];
    for (i = 0; i < 224; i++) typeSet[i] = raws[4].split("'")[i];
    for (i = 0; i < gpl_number; i++) {
      if (i==105||i==106) gameplay[i] = raws[5].split("'")[i];
      else gameplay[i] = Parsing.FloatE(raws[5].split("'")[i]) + "";
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
    return Parsing.FloatU(gameplay[func.VarNumber(str,gpl_number)]);
}

//Start functions
console.log("-------------------------------");

if (!existsF(universe_name + "/UniverseInfo.se3"))
{
  var datapackjse3, defaultjse3 = readF("technical_data/DefaultDatapack.jse3");
  var cand_datapacks = getAllSuchFiles("Datapacks",'.jse3');
  var rem_indet = cand_datapacks.indexOf("Default.jse3");
  if(rem_indet!=-1) cand_datapacks.splice(rem_indet,1);
  if(cand_datapacks.length > 0)
  {
    console.log("Custom datapacks were found. Which one do you want to import?");
    console.log("0 - Use DEFAULT anyway");
    for(var cand_ind=1;cand_ind<=cand_datapacks.length;cand_ind++)
      console.log(cand_ind+" - "+cand_datapacks[cand_ind-1]);
    var cand_answ = readline_sync.question("> ");
    if(cand_answ<0 || cand_answ>cand_datapacks.length) cand_answ=0;
    console.log("-------------------------------");
    
    if(cand_answ==0) datapackjse3 = defaultjse3;
    else datapackjse3 = readF("Datapacks/"+cand_datapacks[cand_answ-1]);

    datapackTranslate("NoName~" + datapackjse3);
    if(datapackjse3==defaultjse3) verF = "DEFAULT";
    else verF = "Custom Data";
  }
  else
  {
    datapackTranslate("NoName~" + defaultjse3);
    verF = "DEFAULT";
  }

  clientDatapacksVar = clientDatapacks();
  uniTime = 0;
  uniMiddle = verF + "~" + clientDatapacksVar;
  uniVersion = serverVersion;
  writeF(
    universe_name + "/UniverseInfo.se3",
    [uniTime, uniMiddle, uniVersion, ""].join("\r\n")
  );

  if(verF=="Custom Data" && datName=="DEFAULT") console.log("Datapack imported: CUSTOM (1/2)");
  else console.log("Datapack imported: " + datName+" (1/2)");
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
        "\r\nBe sure that there is no file Datapack.jse3 to import the default datapack." +
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

  var wh_to_call_me;
  if(dataGet[0]=="DEFAULT") wh_to_call_me = "Default Data";
  else wh_to_call_me = "Custom Data";
  console.log("Datapack loaded: "+wh_to_call_me+" (1/2)");
}

//Biome memories functions
function BiomeMemoriesUpdate()
{
		var old_data = new Array(16000).fill("");
		var i,j;
		for(i=0;i<16000;i++)
		{
			  old_data[i] = biome_memories[i];
			  biome_memories[i] = "";
		}
		for(i=0;i<16000;i++)
		{
  			var lngt = old_data[i].length;
			  for(j=1;j<lngt;j+=2) //only odd
			  {
				    var ulam = i*1000 + j;
				    var biome = CharToNum31(old_data[i][j]);
				    if(biome!=-1) InsertBiome(ulam,biome);
			  }
			  biome_memories_state[i] = 3;
		}
}
function FindBiome(ulam)
{
		var LnId = UlamToLnId(ulam);
		var ln = LnId[0];
		var id = LnId[1];

		if(biome_memories[ln].length <= id) return -1;
		var cand = CharToNum31(biome_memories[ln][id]);
		if(cand < 0 || cand > 31) return -1;
		else return cand;
}
function InsertBiome(ulam,biome)
{
		var LnId = UlamToLnId(ulam);
		var ln = LnId[0];
		var id = LnId[1];

		while(biome_memories[ln].length <= id)
			biome_memories[ln] += '-';

    biome_memories[ln] = replaceCharAtIndex(biome_memories[ln],id,Num31ToChar(biome));
		biome_memories_state[ln] = 3;
}
function UlamToLnId(ulam)
{
		var XY = ulamToXY(ulam);
		XY[0] += 2000;
		XY[1] += 2000; //0-3999
		var X = Math.floor(XY[0] / 32);
		var Y = Math.floor(XY[1] / 32);
		var x = XY[0] % 32;
		var y = XY[1] % 32;
		var ln = 125*X + Y;
		var id = 32*x + y;
		return [ln,id];
}
function MemoriesForClient(client_data)
{
  var spl = client_data.split(";");
  return [
    [7812,biome_memories[7812]].join("$"),
    MemoriesOfCoords(Parsing.FloatU(spl[0]),Parsing.FloatU(spl[1])),
    MemoriesOfCoords(Parsing.FloatU(spl[6]),Parsing.FloatU(spl[7]))
  ].join("?");
}
function MemoriesOfCoords(x,y)
{
    x = Math.round(x/3200);
    y = Math.round(y/3200);
    if(x<-61) x=-61; if(x>61) x=61;
    if(y<-61) y=-61; if(y>61) y=61;
    x += 62;
    y += 62;
    var ln = 125*x + y;
    var a = [
      ln-125-1, ln-125, ln-125+1,
      ln-1, ln, ln+1,
      ln+125-1, ln+125, ln+125+1
    ],b=[];
    var i;
    for(i=0;i<9;i++)
      b[i] = a[i] + "$" + biome_memories[a[i]];
    return b.join("?");
}

//Biome memories read
var dii;
if(existsF(universe_name + "/Biomes.se3"))
{
    var rtr = readF(universe_name + "/Biomes.se3").split("\r\n");
    for(dii=0;dii<16000;dii++)
        biome_memories[dii] = rtr[dii];
    BiomeMemoriesUpdate();
}
else
{
    var mem_files = getAllSuchFiles(universe_name + "/Biomes/",'.se3');
    for(dii=0;dii<mem_files.length;dii++)
    {
        const mem_file = mem_files[dii];
        const wyodr = Parsing.IntU(mem_file.match(/\d+/));
        if("Memory_"+wyodr+".se3" == mem_file)
        {
            var natete = universe_name + "/Biomes/Memory_"+wyodr+".se3";
            biome_memories[wyodr] = readF(natete).split("\r\n")[0];
            biome_memories_state[wyodr] = 2;
        }
    }
}

//Generator initialize
if(existsF(universe_name + "/Seed.se3")) seed = readF(universe_name + "/Seed.se3").split("\r\n")[0];
seed = Parsing.IntE(Generator.SetSeed(seed));
writeF(universe_name + "/Seed.se3", seed + "\r\n");
Generator.TagNumbersInitialize();

console.log("Generator initialized (2/2)");

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

unstable_sprobability = Math.floor(Parsing.FloatU(gameplay[24])*50+1);
if(unstable_sprobability < 1) unstable_sprobability = 1;

growSolid[5] = gsol5a +";"+ gsol5b +";6";
growSolid[6] = gsol5a +";"+ gsol5b +";7";
growSolid[25] = gsol25 +";"+ gsol25 +";23";

//Command listener
const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

function give_array_add_temp(element) {
  var ind = give_array.push(element) - 1;
  console.log("Set promise: "+give_array[ind]);
  setTimeout(function() {
      if(give_array[ind]!="USED") console.log("Promise expired: "+give_array[ind]);
      give_array[ind] = undefined;
  }, 3000);
}
function saveConfig() {
  writeF("config.json",JSON.stringify(config,null,2));
}
function listenForMessages()
{
  rl.question('', (message) => {
    ExecuteCommand(message);
    listenForMessages();
  });
}
function ExecuteCommand(message)
{
  if(["stop","quit","exit"].includes(message)) Commands.stop();
  else
  {
    var arg = message.split(" ");

    if(message == "save") Commands.save();
    else if(message == "players") Commands.players();
    else if(message == "connections") Commands.connections();
    else if(message == "difficulty get") Commands.difficultyGet();
    else if(message == "banlist") Commands.banlist();
    else if(message == "baniplist") Commands.baniplist();
    else if(message == "whitelist list") Commands.whitelistList();
    else if(message == "whitelist toggle") Commands.whitelistToggle();

    else if(arg[0]=="say") Commands.say(arg);

    else if(arg.length==2 && arg[0]=="kick") Commands.kick(arg);
    else if(arg.length==3 && arg[0]=="difficulty" && arg[1]=="set") Commands.difficultySet(arg);
    else if(arg.length==2 && arg[0]=="ban") Commands.ban(arg);
    else if(arg.length==2 && arg[0]=="unban") Commands.unban(arg);
    else if(arg.length==2 && arg[0]=="banip") Commands.banip(arg);
    else if(arg.length==2 && arg[0]=="unbanip") Commands.unbanip(arg);
    else if(arg.length==2 && arg[0]=="getip") Commands.getip(arg);
    else if(arg.length==3 && arg[0]=="whitelist" && arg[1]=="add") Commands.whitelistAdd(arg);
    else if(arg.length==3 && arg[0]=="whitelist" && arg[1]=="remove") Commands.whitelistRemove(arg);

    else if(arg.length==3 && arg[0]=="health") Commands.health(arg);
    else if(arg.length==4 && arg[0]=="give") Commands.give(arg);
    else if(arg.length==4 && arg[0]=="tp" && arg[2]=="to") Commands.tpPlayer(arg);
    else if(arg.length==4 && arg[0]=="tp") Commands.tpCoords(arg);

    else if(message=="help")
    {
      console.log("\n------ List of all commands ------\n");

      console.log("'help' - Displays this documentation.");
      console.log("'save' - Saves server data. (happens automatically once per 15 seconds)");
      console.log("'players' - Displays a list of all players.");
      console.log("'connections' - Displays a list of all WebSocket connections.");
      console.log("'getip [nickname]' - Displays player's IPv4 address.")
      console.log("'difficulty get' - Displays the actual server difficulty.");
      console.log("'difficulty set [1-4]' - Changes the server difficulty.");
      console.log("'stop' / 'quit' / 'exit' - Stops the server.\n");

      console.log("'ban [nickname]' - Bans a player.");
      console.log("'unban [nickname]' - Unbans a player.");
      console.log("'banlist' - Displays a list of all banned players.");
      console.log("'banip [IPv4]' - Bans an IPv4 address.");
      console.log("'unbanip [IPv4]' - Unbans an IPv4 address.");
      console.log("'baniplist' - Displays a list of all banned IPv4s.\n");

      console.log("'whitelist add/remove [nickname]' - Adds or removes a player to/from whitelist.");
      console.log("'whitelist list' - Displays all players on whitelist.");
      console.log("'whitelist toggle' - Enables/disables whitelist.\n");
    
      console.log("'say [message]' - Sends a chat message to all players.");
      console.log("'kick [nickname]' - Kicks a player.");
      console.log("'health [nickname] [hp]' - Modifies hp of a player.");
      console.log("'give [nickname] [item] [amount]' - Gives an item to a player.");
      console.log("'tp [nickname] [x] [y]' - Teleports a player to specified coordinates.");
      console.log("'tp [nickname] to [target-nickname]' - Teleports a player to another player.");

      console.log("\n----------------------------------\n");
    }
    else console.log("Incorrect command. Type 'help' for documentation.");
  }
}
listenForMessages();

//Command functions
class Commands
{
    static save()
    {
        SaveAllNow();
        console.log("Data saved manually");
    }
    static players()
    {
        var i,current_players=0,plmem=[];
        for(i=0;i<max_players;i++)
        {
            if(se3_wsS[i]!="")
            {
                plmem.push("[" + i + "]: " + plr.nicks[i] + " (" + se3_wsS[i] + ") "+IPv4s[i]);
                current_players++;
            }
        }
        console.log("\nPlayers ["+current_players+"/"+max_players+"]:");
        for(i=0;i<max_players;i++) if(plmem[i]) console.log(plmem[i]);
        console.log("");
    }
    static connections()
    {
        console.log('\nConnections:');
        con_map.forEach((value, key) => {
            console.log(`${key} (${value})`);
        });
        console.log("");
    }
    static difficultyGet()
    {
        console.log("The current difficulty is "+difficulty_name_tab[config.difficulty]+" ("+config.difficulty+")");
    }
    static stop()
    {
        rl.close();
        process.emit("SIGINT");
    }
    static banlist()
    {
        console.log("All banned players: ");
        console.log(config.banned_players);
    }
    static baniplist()
    {
        console.log("All banned IPv4s: ");
        console.log(config.banned_ips);
    }
    static whitelistList()
    {
        console.log("All players on whitelist: ");
        console.log(config.whitelist);
    }
    static whitelistToggle()
    {
        config.whitelist_enabled = !config.whitelist_enabled;
        console.log("Whitelist toggled to: "+config.whitelist_enabled);
        saveConfig();
    }
    static say(arg)
    {
        arg.splice(0,1); arg = arg.join("\t");
        console.log("[server] " + arg.replaceAll("\t"," "));
        sendToAllPlayers("/RetChatMessage [server] " + arg + " X X");
    }
    static getip(arg)
    {
        var indof = plr.nicks.indexOf(arg[1]);
        if(indof!=-1 && !nickWrong(arg[1]))
            console.log("Player "+arg[1]+" has the following IPv4 address: "+IPv4s[indof]);
        else
            console.log("This player is offline.");
    }
    static difficultySet(arg)
    {
        var diff_pars = Parsing.IntU(arg[2]);
        if(!Parsing.IntC(arg[2]) || diff_pars < 0 || diff_pars > 5)
        {
            console.log("Couldn't set such difficulty.");
        }
        else if(diff_pars==config.difficulty)
        {
            console.log("Difficulty is already "+difficulty_name_tab[diff_pars]);
        }
        else
        {
            config.difficulty = diff_pars;
            difficulty = tab_difficulty[diff_pars];
            sendToAllPlayers("/RetDifficultySet "+diff_pars+" X X");
            var info_difficulty = "Difficulty set to "+difficulty_name_tab[diff_pars];
            console.log(info_difficulty);
            sendToAllPlayers(
                "/RetInfoClient " +
                (info_difficulty).replaceAll(" ", "`") +
                " -1 X X"
            );
            saveConfig();
        }
    }
    static ban(arg)
    {
        if(!config.banned_players.includes(arg[1]))
        {
            config.banned_players.push(arg[1]);
            console.log("Banned player: "+arg[1]);
            saveConfig();
            Commands.kick(["kick ",arg[1]]);
        }
        else console.log("This player is already banned.");
    }
    static unban(arg)
    {
        var indof = config.banned_players.indexOf(arg[1]);
        if(indof!=-1)
        {
            config.banned_players.splice(indof,1);
            console.log("Unbanned player: "+arg[1]);
            saveConfig();
        }
        else console.log("This player is not banned.");
    }
    static banip(arg)
    {
        if(!config.banned_ips.includes(arg[1]))
        {
            config.banned_ips.push(arg[1]);
            console.log("Banned IPv4: "+arg[1]);
            saveConfig();

            var i;
            for(i=0;i<max_players;i++) {
                if(IPv4s[i]!=undefined)
                if(IPv4s[i].endsWith(arg[1]) && plr.nicks[i]!="0") kick(i);
            }
      }
      else console.log("This IPv4 is already banned.");
    }
    static unbanip(arg)
    {
        var indof = config.banned_ips.indexOf(arg[1]);
        if(indof!=-1)
        {
            config.banned_ips.splice(indof,1);
            console.log("Unbanned IPv4: "+arg[1]);
            saveConfig();
        }
        else console.log("This IPv4 is not banned.");
    }
    static whitelistAdd(arg)
    {
        if(!config.whitelist.includes(arg[2]))
        {
            config.whitelist.push(arg[2]);
            console.log("Added player to whitelist: "+arg[2]);
            saveConfig();
        }
        else console.log("This player is already whitelisted.");
    }
    static whitelistRemove(arg)
    {
        var indof = config.whitelist.indexOf(arg[2]);
        if(indof!=-1)
        {
            config.whitelist.splice(indof,1);
            console.log("Removed player from whitelist: "+arg[1]);
            saveConfig();
        }
        else console.log("This player is not whitelisted.");
    }
    static kick(arg)
    {
        var indof = plr.nicks.indexOf(arg[1]);
        if(indof!=-1 && !nickWrong(arg[1]))
        {
            kick(indof);
            console.log("Kicked player: "+arg[1]);
        }
        else console.log("This player is not on a server.");
    }
    static health(arg)
    {      
        var indof = plr.nicks.indexOf(arg[1]);
        if(indof!=-1 && !nickWrong(arg[1]) && se3_wsS[indof]=="game" && !inHeaven(arg[1]))
        {
            var amnt = Parsing.FloatU(arg[2]);
            if(!Parsing.FloatC(arg[2]) || amnt==0)
            {
                console.log("Couldn't modify hp in such way.");
            }
            else if(amnt>0)
            {
                HealFLOAT(indof,amnt);
                console.log("Healed player "+arg[1]+": "+arg[2]+"hp");
            }
            else if(amnt<0)
            {
                DamageFLOAT(indof,-amnt);
                console.log("Hurted player "+arg[1]+": "+arg[2]+"hp");
            }
        }
        else console.log("This player is not in a physical form.");
    }
    static give(arg)
    {
        var indof = plr.nicks.indexOf(arg[1]);
        if(indof!=-1 && !nickWrong(arg[1]) && se3_wsS[indof]=="game" && !inHeaven(arg[1]))
        {
            var item = Parsing.IntU(arg[2]);
            var count = Parsing.IntU(arg[3]);
            
            if(!Parsing.IntC(arg[2]) || !Parsing.IntC(arg[3])) {
                console.log("Item and count should be integers.");
            }
            if(item<=0 || count<=0 || item>127) {
                console.log("Wrong numbers were used.");
            }
            else {
                give_array_add_temp("give "+arg[1]+" "+item+" "+count);
                sendTo(se3_ws[indof],"/RetCommandGive "+item+" "+count+" X X");
            }
        }
        else console.log("This player is not in a physical form.");
    }
    static tpCoords(arg)
    {
        var indof = plr.nicks.indexOf(arg[1]);
        if(indof!=-1 && !nickWrong(arg[1]) && se3_wsS[indof]=="game" && !inHeaven(arg[1]))
        {
            var x = Parsing.FloatU(arg[2])+"";
            var y = Parsing.FloatU(arg[3])+"";
            give_array_add_temp("tp "+arg[1]+" "+x+" "+y);
            sendTo(se3_ws[indof],"/RetMemoryData "+MemoriesForClient([x,y,0,0,0,0,0,0].join(";"))+" X X");
            sendTo(se3_ws[indof],"/RetCommandTp "+x+" "+y+" X "+plr.livID[indof]);
        }
        else console.log("This player is not in a physical form.");
    }
    static tpPlayer(arg)
    {
        var indof1 = plr.nicks.indexOf(arg[1]);
        var indof2 = plr.nicks.indexOf(arg[3]);
        if(indof1!=-1 && !nickWrong(arg[1]) && se3_wsS[indof1]=="game" && !inHeaven(indof1) &&
           indof2!=-1 && !nickWrong(arg[3]) && se3_wsS[indof2]=="game" && !inHeaven(indof2))
        {
            var bef = plr.players[indof2].split(";");
            Commands.tpCoords(["tp",arg[1],bef[0],bef[1]]);
        }
        else console.log("At least one of these players is not in a physical form.");
    }
}

//Starting ending
console.log("-------------------------------");
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
  console.log("\nClosing the server:");
  console.log(error);
});
