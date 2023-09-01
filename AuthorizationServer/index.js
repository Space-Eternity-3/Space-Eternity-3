const WebSocket = require("ws");
const fs = require("fs");
const path = require("path");
const {createHash} = require('node:crypto');

//Global variables
var serverVersion = "1.0.0";

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
function sendTo(ws,data) {
  try{
    console.log(data);
    ws.send(data);
  }catch{return;}
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

//Nick functions
function nickWrong(stam) {
  return (
    stam.includes("\\") ||
    stam.includes("/") ||
    stam.includes(":") ||
    stam.includes("*") ||
    stam.includes("?") ||
    stam.includes('"') ||
    stam.includes("<") ||
    stam.includes(">") ||
    stam.includes("|") ||
    stam.length > 16 ||
    stam == "0" ||
    stam == ""
  );
}
function getNickPath(nick) {
  return "./Accounts/"+nick+".txt";
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

//Websocket brain
wss.on("connection", function connection(ws) {
  ws.on("close", (code, reason) => {
    
  });

  //Return codes:
  // 1 -> success
  // 2 -> wrong nick format
  // 3 -> username already exists
  // 4 -> user doesn't exist
  // 5 -> wrong password
  // 6 -> server not responding
  // 7 -> here you have connection code (next argument)
  // 8 -> new password too short

  ws.on("message", (msg) => {

    try {
      var str = (msg).toString(); if(str.length > 1000) {ws.close(); return;}
      var arg = str.split(" ");
    } catch {ws.close(); return;}
    var argsize = arg.length;

    //REGISTER
    if(arg[0]=="/Register" && argsize==3) //Register 1[nickname] 2[password] NP
    {
      if(arg[2].length <=6 || arg[2].length >=33) {sendTo(ws,"/RetLogin 8"); return;}
      sendTo(ws,"/RetLogin "+Register(arg[1],arg[2]));
    }

    //LOGIN
    if(arg[0]=="/Login" && argsize==3) //Login 1[nickname] 2[password] NP
    {
      sendTo(ws,"/RetLogin "+Login(arg[1],arg[2]));
    }

    //CHANGEPASSWORD
    if(arg[0]=="/ChangePassword" && argsize==4) //ChangePassword 1[nickname] 2[password] 3[passwordNew] NPP
    {
      if(arg[3].length <=6 || arg[2].length >=33) {sendTo(ws,"/RetChangePassword 8"); return;}
      var ef = Login(arg[1],arg[2]);
      if(ef==1)
      {
        var file = getNickPath(arg[1]);
        removeF(file);
        sendTo(ws,"/RetChangePassword "+Register(arg[1],arg[3]));
      }
      else sendTo(ws,"/RetChangePassword "+ef);
    }

    //AUTHORIZE
    if(arg[0]=="/Authorize" && argsize==4) //Authorize 1[nickname] 2[password] 3[serverAddress] NPA
    {
      var ef = Login(arg[1],arg[2]);
      if(ef==1)
      {
        sendTo(ws,"/RetAuthorize 6");
      }
      else sendTo(ws,"/RetAuthorize "+ef);
    }
    
  });
});

//Starting ending
console.log("Communication version: " + serverVersion + "\nPort: "+connectionOptions.port+"\nStarting authorization server for SE3...");
console.log("Server started succesfully!")
