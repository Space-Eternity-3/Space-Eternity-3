const WebSocket = require("ws");
const fs = require("fs");
const path = require("path");
const {createHash} = require('node:crypto');

const config = require("./config.json");

//Websocket functions
let connectionOptions = {
  port: 27684,
};

const wss = new WebSocket.Server(connectionOptions);

function sendToAllClients(data) {
  wss.clients.forEach(function (client) {
    sendTo(client,data);
  });
}
function sendTo(ws,req,data) {
  try{
    ws.send(data);
    let client_ip;
    if (config.useForwardedFor) {
      client_ip = req.headers['x-forwarded-for']?.split(',')[0]?.trim() || req.socket.remoteAddress;
    }
    else client_ip = req.socket.remoteAddress;
    logConsole(client_ip+" << "+data);
  }catch{return;}
}

//Other functions
function passwordHide(msg) {
  var arg = msg.split(" ");
  if(arg[2]!=undefined) arg[2] = "*password*";
  return arg.join(" ");
}
function logConsole(msg) {
  const dzisiaj = new Date();
  const rok = dzisiaj.getFullYear();
  const miesiac = dzisiaj.getMonth() + 1;
  const dzien = dzisiaj.getDate();
  const godzina = dzisiaj.getHours();
  const minuta = dzisiaj.getMinutes();
  const sekunda = dzisiaj.getSeconds();

  console.log(`[${rok}-${miesiac}-${dzien} ${godzina}:${minuta}:${sekunda}] `+msg);
}

//File functions
function readF(nate) {
  if (existsF(nate)) return fs.readFileSync(nate, { flag: "r" }).toString();
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
function getAllTxtFiles(folderPath) {
  const files = fs.readdirSync(folderPath);
  const textFiles = files.filter(file => {
      const filePath = path.join(folderPath, file);
      const fileStats = fs.statSync(filePath);
      return fileStats.isFile() && file.endsWith('.txt');
  });
  return textFiles;
}
function removeFileExtension(filename) {
  const lastDotIndex = filename.lastIndexOf('.');
  if (lastDotIndex === -1) return filename;
  else return filename.substring(0, lastDotIndex);
}

//Nick functions
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
function getNickPath(nick) {
  return "./Accounts/"+nick+".txt";
}

//Verify functions
function okServerAddress(s) {
  if(s.includes("\n") || s.includes(" ")) return false;
  return (s!="");
}
function okServerName(s) {
  if(s.includes("\n") || s.includes(" ")) return false;
  return (s!="");
}
function okConID(s) {
  var n = parseInt(s);
  if(isNaN(n)) return false;
  return (n>=0 && n<=999999999);
}

//Account functions
function sha256(s) {
  return createHash('sha256').update(s).digest('hex');
}
function Register(nick, password) {
  if(nickWrong(nick)) return 2; //wrong nick format
  var file = getNickPath(nick);
  if(!existsF(file))
  {
    writeF(file,sha256(nick+password));
    return 1;
  }
  else return 3; //username already used
}
function Login(nick,password) {
  if(nickWrong(nick)) return 2; //wrong nick format
  var file = getNickPath(nick);
  if(!existsF(file)) return 4; //user doesn't exist
  else {
    var pass = readF(file);
    if(sha256(nick+password)==pass) return 1;
    else return 5; //wrong password
  }
}

//Registered servers
var serverList = [];
class CServer {
  constructor(_ws,_owner,_name,_address) {
    this.ws = _ws;
    this.owner = _owner;
    this.name = _name;
    this.address = _address;
  }
  Save() {
    writeF("./RegisteredServers/"+this.owner+".txt",this.name+"\n"+this.address);
  }
  Load() {
    var tab = readF("./RegisteredServers/"+this.owner+".txt").split("\n");
    this.name = tab[0];
    this.address = tab[1];
  }
}
function getServerMemoryInfo(obj) {
  return [
    obj.owner,
    obj.name,
    obj.address,
  ];
}

//Connection limiter variables
const con_map = new Map();
const reg_map = new Map();
const msg_map = new Map();

const max_cons = 5; //at one moment
const max_regs = 3; //per minute
const max_msgs = 15; //per second
const max_msgs_kick = 50; //per second

//Connection limiter functions
function coMinuteFunction() {
  reg_map.clear();
}
function coSekundeFunction() {
  msg_map.clear();
}
setInterval(coMinuteFunction, 60 * 1000);
setInterval(coSekundeFunction, 1 * 1000);

//Websocket brain
wss.on("connection", function connection(ws,req) {
  //Check connection limiter and add to map
  let client_ip;
  if (config.useForwardedFor) {
    client_ip = req.headers['x-forwarded-for']?.split(',')[0]?.trim() || req.socket.remoteAddress;
  }
  else client_ip = req.socket.remoteAddress;

  if(!con_map.has(client_ip)) con_map.set(client_ip,0);

  var cur_cons = con_map.get(client_ip);
  if(cur_cons >= max_cons) { ws.close(); return; }
  else con_map.set(client_ip, cur_cons+1);

  logConsole(client_ip+" -> started connection");

  ws.on("close", (code, reason) => {
    var cur_cons = con_map.get(client_ip);
    con_map.set(client_ip, cur_cons-1);

    logConsole(client_ip+" -> closed connection");
  });

  //Return codes:
  // 1 -> success
  // 2 -> wrong nick format
  // 3 -> username already exists
  // 4 -> user doesn't exist
  // 5 -> wrong password
  // 6 -> server not found
  // 7 -> server address given
  // 8 -> new password too short
  // 9 -> serverName already exists
  // 10 -> running server name registered
  // 11 -> wrong serverName format
  // 12 -> wrong serverAddress format
  // 13 -> running server name reloaded
  // 14 -> wrong conID format
  // 15 -> too many registrations

  ws.on("message", (msg) => {

    const str = msg.toString();
    if(str.length > 1000) {ws.close(); return;}
    var arg = str.split(" ");
    var argsize = arg.length;

    logConsole(client_ip+" >> "+passwordHide(str));

    //look for spam (msgs)
    if(!msg_map.has(client_ip)) msg_map.set(client_ip,1);
    else {
      var amt = msg_map.get(client_ip);
      msg_map.set(client_ip, amt+1);
      if(amt >= max_msgs_kick) { ws.close(); return; }
      if(amt >= max_msgs) { return; }
    }
    if(arg[0]=="/Register") {
      if(!reg_map.has(client_ip)) reg_map.set(client_ip,1);
      else {
        var amt = reg_map.get(client_ip);
        reg_map.set(client_ip, amt+1);
        if(amt >= max_regs) { sendTo(ws,req,"/RetLogin 15"); return; }
      }
    }

    //REGISTER
    if(arg[0]=="/Register" && argsize==3) //Register 1[nickname] 2[password] NP
    {
      if(arg[2].length <=6 || arg[2].length >=33) {sendTo(ws,req,"/RetLogin 8"); return;}
      sendTo(ws,req,"/RetLogin "+Register(arg[1],arg[2]));
    }

    //LOGIN
    if(arg[0]=="/Login" && argsize==3) //Login 1[nickname] 2[password] NP
    {
      sendTo(ws,req,"/RetLogin "+Login(arg[1],arg[2]));
    }

    //CHANGEPASSWORD
    if(arg[0]=="/ChangePassword" && argsize==4) //ChangePassword 1[nickname] 2[password] 3[passwordNew] NPP
    {
      if(arg[3].length <=6 || arg[2].length >=33) {sendTo(ws,req,"/RetChangePassword 8"); return;}
      var ef = Login(arg[1],arg[2]);
      if(ef==1)
      {
        var file = getNickPath(arg[1]);
        removeF(file);
        sendTo(ws,req,"/RetChangePassword "+Register(arg[1],arg[3]));
      }
      else sendTo(ws,req,"/RetChangePassword "+ef);
    }

    //AUTHORIZE
    if(arg[0]=="/Authorize" && argsize==5) //Authorize 1[nickname] 2[password] 3[serverName] 4[conID] NPSI
    {
      if(!okServerName(arg[3])) {sendTo(ws,req,"/RetAuthorize 11"); return;}
      if(!okConID(arg[4])) {sendTo(ws,req,"/RetAuthorize 14"); return;}
      var ef = Login(arg[1],arg[2]);
      if(ef==1)
      {
        var i,lngt=serverList.length;
        for(i=0;i<lngt;i++) {
          if(serverList[i].name==arg[3] && serverList[i].ws.readyState==1) {
            sendTo(serverList[i].ws,req,"/RetAuthorizeUser "+arg[1]+" "+arg[4]);
            sendTo(ws,req,"/RetAuthorize 7 "+serverList[i].address);
            return;
          }
        }
        sendTo(ws,req,"/RetAuthorize 6");
      }
      else sendTo(ws,req,"/RetAuthorize "+ef);
    }

    //RUNNING SERVER ADD
    if(arg[0]=="/RunningServerAdd" && argsize==5) //RunningServerAdd 1[nickname] 2[password] 3[serverName] 4[serverAddress] NPSA
    {
      if(!okServerName(arg[3])) {sendTo(ws,req,"/RetServer 11"); return;}
      if(!okServerAddress(arg[4])) {sendTo(ws,req,"/RetServer 12"); return;}
      var ef = Login(arg[1],arg[2]);
      if(ef==1)
      {
        var i,lngt=serverList.length;
        for(i=0;i<lngt;i++)
        {
          if(serverList[i].owner!=arg[1] && serverList[i].name==arg[3])
          {
            sendTo(ws,req,"/RetServer 9"); return; //unable to register name
          }
        }
        for(i=0;i<lngt;i++)
        {
          if(serverList[i].owner == arg[1])
          {
            serverList[i].ws = ws;
            serverList[i].name = arg[3];
            serverList[i].address = arg[4];
            serverList[i].Save(); console.log(getServerMemoryInfo(serverList[i]));
            sendTo(ws,req,"/RetServer 13"); return; //reloading
          }
        }
        serverList.push(new CServer(ws,arg[1],arg[3],arg[4]));
        serverList[lngt].Save(); console.log(getServerMemoryInfo(serverList[lngt]));
        sendTo(ws,req,"/RetServer 10"); return; //registering
      }
      else sendTo(ws,req,"/RetServer "+ef);
    }
    
  });
});

//Starting server
console.log("\nPort: "+connectionOptions.port+"\nStarting authorization server for SE3...");

var reservedServerNicknames = getAllTxtFiles("./RegisteredServers/");
var i,lngt=reservedServerNicknames.length;
for(i=0;i<lngt;i++) {
  serverList.push(new CServer("",removeFileExtension(reservedServerNicknames[i]),"",""));
  serverList[i].Load();
  console.log(getServerMemoryInfo(serverList[i]));
}

console.log("Server started succesfully!");
