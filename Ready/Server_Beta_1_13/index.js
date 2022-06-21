const WebSocket = require("ws");
const fs = require("fs");
const path = require("path");

//Global variables
var serverVersion = "Beta 1.13";
var serverRedVersion = "Beta_1_13";
var clientDatapacksVar = "";
var seed;
var max_players = 10;
var gpl_number = 27;

var chunk_data = [];
var chunk_names = [];
var chunk_waiter = [];

var plr = {
  waiter: [0],
  players: ["0"],
  nicks: ["0"],
  pingTemp: ["0"],
  conID: ["0"],
  livID: ["0"],

  data: ["0"],
  inventory: ["0"],
  backpack: ["0"],
  upgrades: ["0"],
  pushInventory: ["0"],
};

var ki, kj, kd = Object.keys(plr);
var klngt = kd.length;
for(ki=1;ki<max_players;ki++)
  for(kj=0;kj<klngt;kj++)
    plr[kd[kj]].push(plr[kd[kj]][0]);

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
  }
};

var bulletsT = [];

var growSolid = [];
growSolid[5] = "4500;12000;6";
growSolid[6] = "4500;12000;7";
growSolid[25] = "150;150;23";

var jse3Var = [];
var jse3Dat = [];

var datName = "";
var version = "";
var craftings = "";
var craftMaxPage = "";
var biomeChances = "";
var drillLoot = new Array(16);
var fobGenerate = new Array(16);
var biomeTags = new Array(32);
var typeSet = new Array(224);
var gameplay = new Array(32);
var modifiedDrops = new Array(128);
var translateFob = [];
var translateAsteroid = [];

drillLoot.fill(""); Object.seal(drillLoot);
fobGenerate.fill(""); Object.seal(fobGenerate);
biomeTags.fill(""); Object.seal(biomeTags);
typeSet.fill(""); Object.seal(typeSet);
gameplay.fill(""); Object.seal(gameplay);
modifiedDrops.fill(""); Object.seal(modifiedDrops);

//Websocket functions
const wss = new WebSocket.Server({
  port: 27683,
});
function sendToAllClients(data) {
  wss.clients.forEach(function (client) {
    client.send(data);
  });
}

//Variable functions
String.prototype.replaceAll = function replaceAll(search, replace) {
  return this.split(search).join(replace);
};
Array.prototype.remove = function (ind) {
  this.splice(ind, 1);
  return this;
};
function randomInteger(min, max) {
  return Math.round(Math.random() * (max - min)) + parseInt(min);
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
  for (i = 0; i < 6; i++) if (isNaN(parseFloatP(elem0[i]))) return false;
  if (isNaN(parseIntP(elem0[6]))) return false;
  if (isNaN(parseFloatP(elem0[7]))) return false;
  for (i = 0; i < 18; i++) if (isNaN(parseIntP(elem1[i]))) return false;
  for (i = 0; i < 42; i++) if (isNaN(parseIntP(elem2[i]))) return false;
  for (i = 0; i < 5; i++) if (isNaN(parseIntP(elem3[i]))) return false;

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
    0,
    0,
    0,
    0,
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
  writeF("ServerUniverse/Players/" + plr.nicks[n] + ".se3", effect);
}
function readPlayer(n) {
  var srcTT, i, dd, di, db, du, eff, lngt;
  if (existsF("ServerUniverse/Players/" + plr.nicks[n] + ".se3"))
    srcTT = readF("ServerUniverse/Players/" + plr.nicks[n] + ".se3").split("\r\n");
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
  var x = sqrt / 2 + 0.5,
    y = -sqrt / 2 - 0.5;
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
  var i,
    j,
    eff = seed + "\r\n";
  if (existsF("ServerUniverse/Asteroids/Generated_" + ind + ".se3")) {
    var datT = readF(
      "ServerUniverse/Asteroids/Generated_" + ind + ".se3"
    ).split("\r\n");
    var pom;

    try {
      if (datT[0] == seed) {
        for (i = 1; i <= 100; i++) {
          var datM = datT[i].split(";");
          var lnt = datM.length;
          if (lnt > 61) lnt = 61;
          for (j = 0; j < lnt; j++) if (datM[j] != "") pom = parseIntE(datM[j]);
          for (j = lnt; j < 61; j++) datT[i] += ";";
          eff += datT[i] + "\r\n";
        }
        return eff;
      } else nev++;
    } catch {
      console.log(
        "Asteroid file [" + ind + "] is invalid. Generating new data..."
      );
    }
  }
  for (i = 0; i < 100; i++)
    eff +=
      ";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;" + "\r\n";
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
  writeF("ServerUniverse/Asteroids/Generated_" + chunk_names[n] + ".se3", eff);
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

//ParseE functions
function parseFloatE(str) {
  str = parseFloatP(str);
  if (!isNaN(str)) return str;
  else return not_existing_variable;
}
function parseIntE(str) {
  str = parseIntP(str);
  if (!isNaN(str)) return str;
  else return not_existing_variable;
}
function parseIntP(str) {
  var i,
    lngt = str.length;
  for (i = 0; i < lngt; i++)
    if (
      !["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "-"].includes(str[i])
    )
      return parseInt("not_number");
  return parseInt(str);
}
function parseFloatP(str) {
  var i,
    lngt = str.length;
  for (i = 0; i < lngt; i++)
    if (
      !["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ",", "-"].includes(
        str[i]
      )
    )
      return parseInt("not_number");
  return parseFloat(str.replaceAll(",", "."));
}

//Data functions
function nickCorrect(stam) {
  if (
    stam.includes("\\") ||
    stam.includes("/") ||
    stam.includes(":") ||
    stam.includes("*") ||
    stam.includes("?") ||
    stam.includes('"') ||
    stam.includes("<") ||
    stam.includes(">") ||
    stam.includes("|")
  )
    return true;
  else if (stam != "") return false;
  else return true;
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
    biomeTags.join("'"),
    biomeChances,
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
  for (i = 0; i < max_players; i++) if (checkPlayer(i, plr.conID[i])) savePlayer(i);
  for (i = 0; i < lngt; i++) chunkSave(i);
}
setInterval(function () {
  //var dn1=Date.now();
  SaveAllNow();
  //var dn2=Date.now();
  //var dn3=dn2-dn1;
  //console.log("Saved all in "+dn3+"ms");

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
}, 5000); //Save all once per 5 seconds

//Kick functions
function kick(i) {
  SaveAllNow(i);
  console.log(plr.nicks[i] + " disconnected [" + i + "]");
  sendToAllClients("/RetKickConnection " + i + " X X");
  if (plr.players[i] != "0")
    sendToAllClients(
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
  plr.pingTemp[i] = "0";
  plr.upgrades[i] = "0";
  plr.backpack[i] = "0";
  plr.inventory[i] = "0";
  plr.pushInventory[i] = "0";
}
setInterval(function () {
  var i;
  for (i = 0; i < max_players; i++) {
    if (plr.waiter[i] > 0) {
      plr.waiter[i]--;
      if (plr.waiter[i] == 0) kick(i);
    }
  }

}, 20);

//STERID UPDATE (optimalization important!)
var date_before = Date.now();
setInterval(function () {
  while(Date.now() >= date_before + 20)
  {
    date_before += 20;
    
    //Approximately once per unity frame
    var i, lngt = bulletsT.length;
    for(i=0;i<lngt;i++)
    {
      bulletsT[i].age++;
      if(bulletsT[i].age>=bulletsT[i].max_age)
      {
        bulletsT.remove(i);
        lngt--; i--; continue;
      }
      bulletsT[i].pos.x += bulletsT[i].vector.x;
      bulletsT[i].pos.y += bulletsT[i].vector.y;
    }
  }
}, 1);

//Bullet functions
function spawnBullet(tpl,arg)
{
  bulletsT.push(tpl);
  sendToAllClients(
    "/RetNewBulletSend " +
      arg[1]+" " +
      arg[2]+" " +
      arg[3]+" " +
      arg[4]+" " +
      arg[5]+" " +
      arg[6]+" " +
      arg[7]+" " +
      " X X"
  );
}
function destroyBullet(n,arg)
{
  bulletsT[n].max_age = parseIntP(arg[3]);
  sendToAllClients(
    "/RetNewBulletDestroy "+
    arg[1]+" "+
    arg[2]+" "+
    arg[3]+" "+
    " X X"
  );
}

//Check functions
function checkPlayer(idm, cn) {
  if (plr.nicks[idm] == "0") return false;
  if (plr.conID[idm] != cn) return false;
  return true;
}

//RetPlayerUpdate
setInterval(function () {
  var i,lngt = plr.players.length;
  var eff = "/RetPlayerUpdate " + max_players + " ";
  for(i=0;i<lngt;i++)
    eff += plr.players[i] + "|" + plr.nicks[i] + "|" + plr.pingTemp[i] + " ";
  eff += "X X"
  sendToAllClients(eff);
}, 20);

//Grow functions
setInterval(function () {
  //[Grow]
  var i,
    lngt = growT.length,
    localData12;
  for (i = 0; i < lngt; i++) {
    growW[i]--;
    if (growW[i] > 0) {
      var ulam = growT[i].split("g")[0];
      var place = growT[i].split("g")[1];
      var det = asteroidIndex(ulam);
      var tume = chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)];
      tume -= 5;

      if (tume > 0) {
        chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] = tume;
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

        drillT.remove(i);
        drillW.remove(i);
        drillC.remove(i);
        lngt--;
        i--;
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
}, 100);
function serverGrow(ulam, place) {
  var det = asteroidIndex(ulam);
  if (chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] != "") {
    var bef = chunk_data[det[0]][det[1]][parseInt(place) + 1];
    if (!["5", "6", "25"].includes(bef)) return;
    chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] = "";
    chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)] = "";
    chunk_data[det[0]][det[1]][parseInt(place) + 1] =
      growSolid[bef].split(";")[2];
    sendToAllClients("/RetGrowNow " + ulam + " " + place + " X X");
  }
}
function serverDrill(ulam, place) {
  var det = asteroidIndex(ulam);
  if (chunk_data[det[0]][det[1]][parseInt(place) + 1] == "2") {
    var gItem = drillGet(det);
    if (gItem == "0") return;
    var gCountEnd = parseInt(
      chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)]
    );

    if (isNaN(gCountEnd)) gCountEnd = 0;
    gCountEnd++;

    chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] = gItem;
    chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)] = gCountEnd;

    sendToAllClients(
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
function drillGet(det) {
  var typp = chunk_data[det[0]][det[1]][0];
  var ltdt = drillLoot[typp].split(";");
  var lngt = ltdt.length;

  var rnd = randomInteger(0, 999);
  var i;

  for (i = 0; i * 3 + 2 < lngt; i++) {
    if (rnd >= ltdt[i * 3 + 1] && rnd <= ltdt[i * 3 + 2]) return ltdt[i * 3];
  }
  return 0;

  return 8;
}
function growActive(ulam) {
  var i, block, localData11, tim, ind;
  var tab = [];
  var blockTab = [];

  var det = asteroidIndex(ulam);
  for (i = 0; i < 20; i++) blockTab[i] = chunk_data[det[0]][det[1]][i + 1];

  for (i = 0; i < 20; i++) {
    block = blockTab[i];
    if (
      ["25"].includes(block) ||
      (["5", "6"].includes(block) && chunk_data[det[0]][det[1]][0] == 6)
    ) {
      if (!growT.includes(ulam + "g" + i)) {
        if (chunk_data[det[0]][det[1]][21 + 2 * i] == "") {
          tab = growSolid[block].split(";");
          tim = randomInteger(tab[0], tab[1]);
          chunk_data[det[0]][det[1]][21 + 2 * i] = tim;
        }
        growT.push(ulam + "g" + i);
        growW.push(10);
      }
      ind = growT.indexOf(ulam + "g" + i);
      growW[ind] = 10;
    }
    if (
      ["2"].includes(block) &&
      (chunk_data[det[0]][det[1]][22 + 2 * i] == "" ||
        chunk_data[det[0]][det[1]][22 + 2 * i] < 5)
    ) {
      if (!drillT.includes(ulam + "w" + i)) {
        tim = randomInteger(180, 420);
        drillT.push(ulam + "w" + i);
        drillW.push(10);
        drillC.push(tim);
      }
      ind = drillT.indexOf(ulam + "w" + i);
      drillW[ind] = 10;
    }
  }
}
function nbtReset(ulam, place) {
  var ind,
    det = asteroidIndex(ulam);
  chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] = "";
  chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)] = "";

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
    if (!(itemS == item && parseInt(countS) + parseInt(count) >= 0))
      return false;
  } else return true;

  effTab[slot * 2] = item;
  effTab[slot * 2 + 1] = parseInt(effTab[slot * 2 + 1]) + parseInt(count);

  if (mode == "INV") plr.inventory[invID] = effTab.join(";");
  else plr.backpack[invID] = effTab.join(";");

  return true;
}

//Fobs change functions
function checkFobChange(ulam, place, start1, start2) {
  var det = asteroidIndex(ulam);
  if (
    chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)] != "" &&
    (start1 == 21 || start2 == 21)
  )
    return false; //2 not required, driller item might disappear

  if (
    chunk_data[det[0]][det[1]][parseInt(place) + 1] == start1 ||
    chunk_data[det[0]][det[1]][parseInt(place) + 1] == start2 ||
    chunk_data[det[0]][det[1]][parseInt(place) + 1] == ""
  )
    return true;
  else return false;
}
function fobChange(ulam, place, end) {
  var det = asteroidIndex(ulam);
  chunk_data[det[0]][det[1]][parseInt(place) + 1] = end;
  nbtReset(ulam, place);
}

//Fob21 change functions
function checkFobDataChange(ulam, place, item, deltaCount, id21) {
  var det = asteroidIndex(ulam);
  if (chunk_data[det[0]][det[1]][parseInt(place) + 1] != id21) {
    return false;
  }

  var max_count;
  if (id21 == 21) max_count = 35;
  if (id21 == 2) max_count = 5;

  if (
    chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] == "" ||
    chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] == item
  ) {
    var countEnd = parseInt(
      chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)]
    );
    if (isNaN(countEnd)) countEnd = 0;
    countEnd += parseInt(deltaCount);
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
  var countEnd = parseInt(chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)]);

  if (isNaN(countEnd)) countEnd = 0;
  countEnd += parseInt(deltaCount);

  if (countEnd != 0) {
    chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] = item;
    chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)] = countEnd;
  } else {
    chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] = "";
    chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)] = "";
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
      chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] != "" &&
      chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)] != ""
    )
      return (
        chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] +
        ";" +
        chunk_data[det[0]][det[1]][22 + 2 * parseInt(place)]
      );
    else return nw;
  }
  if (lt == "g") {
    if (chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)] != "")
      return chunk_data[det[0]][det[1]][21 + 2 * parseInt(place)];
    else return nw;
  }
}
function getBlockAt(ulam, place) {
  var det = asteroidIndex(ulam);
  var eff = chunk_data[det[0]][det[1]][parseInt(place) + 1];
  if (eff != "") return eff;
  else return -1;
}

//Asteroid functions
function sazeConvert(saze) {
  return parseIntE(saze.split("b")[1]) * 7 + parseIntE(saze.split("b")[0]) - 4;
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

  var rand = randomInteger(0, 999);
  var i = 0,
    j,
    k;
  while (!(rand >= typeDatas[i + 1] && rand <= typeDatas[i + 2]) && i < 1000)
    i += 3;
  if (i >= 1000) td = "0";
  else td = typeDatas[i];

  var strobj = "";
  var typeDatas2 = fobGenerate[td].split(";");
  var how_many = 2 * parseInt(saze); //parse all before first letter

  for (j = 0; j < how_many; j++) {
    k = 0;
    rand = randomInteger(0, 999);
    while (
      !(rand >= typeDatas2[k + 1] && rand <= typeDatas2[k + 2]) &&
      k < 1000
    )
      k += 3;
    if (saze.split("").includes("t") && saze.split("t")[2].split(";")[j] != "")
      strobj = strobj + ";" + parseIntE(saze.split("t")[2].split(";")[j]);
    else if (k >= 1000) strobj = strobj + ";0";
    else strobj = strobj + ";" + typeDatas2[k];
  }
  return td + strobj;
}

//Websocket brain
wss.on("connection", function connection(ws) {
  ws.on("message", (msg) => {
    var i,
      arg = (msg + "").split(" ");
    var msl = arg.length;

    if (arg[0] == "/PlayerUpdate") {
      //PlayerUpdate 1[PlayerID] 2<PlayerData> 3[pingTemp] 4[time]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      if (plr.waiter[arg[1]] > 1) plr.waiter[arg[1]] = arg[4];

      plr.players[arg[1]] = arg[2];
      plr.pingTemp[arg[1]] = arg[3];

      if (arg[2] != "1") {
        plr.data[arg[1]] = arg[2];
      }
      return;
    }
    if (arg[0] == "/ImConnected") {
      //ImConnected 1[PlayerID] 2[time]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      if (plr.waiter[arg[1]] > 1) plr.waiter[arg[1]] = arg[2];
      return;
    }
    if (arg[0] == "/AllowConnection") {
      //AllowConnection 1[nick] 2[password] 3[conID]
      for (i = 8; i <= max_players; i++) {
        if (
          i == max_players ||
          arg[2] != serverRedVersion ||
          plr.nicks.includes(arg[1]) ||
          nickCorrect(arg[1])
        ) {
          ws.send("/RetAllowConnection -1 X X");
          console.log("Connection dennied");
          break;
        }
        if (plr.waiter[i] == 0) {
          plr.waiter[i] = 250;
          plr.nicks[i] = arg[1];

          readPlayer(i);
          plr.pushInventory[i] = "1;2;3;4;5;6;7;8;9";
          if (plr.data[i] == "0") plr.data[i] = "0;0;0;0;0;0;0;0;1;0;0;0";
          if (plr.inventory[i] == "0")
            plr.inventory[i] = "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
          if (plr.backpack[i] == "0")
            plr.backpack[i] =
              "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
          if (plr.upgrades[i] == "0") plr.upgrades[i] = "0;0;0;0;0";

          SaveAllNow();
          plr.conID[i] = arg[3];
          ws.send(
            "/RetAllowConnection " +
              i +
              " " +
              plr.data[i] +
              " " +
              plr.inventory[i] +
              " " +
              plr.pushInventory[i] +
              " " +
              clientDatapacksVar +
              " " +
              plr.upgrades[i] +
              " " +
              plr.backpack[i] +
              " " +
              seed +
              " X X"
          );
          console.log(plr.nicks[i] + " connected [" + i + "]");
          break;
        }
      }
    }
    if (arg[0] == "/ImDisconnected") {
      //ImDisconnected 1[PlayerID]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      plr.waiter[arg[1]] = 1;
    }
    if (arg[0] == "/EmitParticles") {
      //EmitParticles 1[PlayerID] 2[type] 3[posX] 4[posY]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      sendToAllClients(
        "/RetEmitParticles " +
          arg[1] +
          " " +
          arg[2] +
          " " +
          arg[3] +
          " " +
          arg[4] +
          " X X"
      );
    }
    if (arg[0] == "/GrowLoaded") {
      //GrowLoaded 1[Data] 2[PlayerID]
      if (!checkPlayer(arg[2], arg[msl - 2])) return;

      var glTab = arg[1].split(";");
      var ji,
        glLngt = glTab.length - 1;
      for (ji = 1; ji < glLngt; ji++) growActive(glTab[ji]);
    }
    if (arg[0] == "/AsteroidData") {
      //AsteroidData 1[UlamID] 2[generation_code] 3[PlayerID]
      if (!checkPlayer(arg[3], arg[msl - 2])) return;

      var ulamID = arg[1];
      var localSize = arg[2];

      var lc3T, lc3;
      var det = asteroidIndex(ulamID);
      if (chunk_data[det[0]][det[1]][0] == "") {
        lc3 = generateAsteroid(localSize);
        lc3T = lc3.split(";");
        for (i = 0; i <= 20; i++) chunk_data[det[0]][det[1]][i] = lc3T[i];
      } else {
        lc3 = chunk_data[det[0]][det[1]][0];
        for (i = 1; i <= 20; i++) lc3 += ";" + chunk_data[det[0]][det[1]][i];
      }
      growActive(ulamID);

      ws.send(
        "/RetAsteroidData " +
          ulamID +
          " " +
          lc3 +
          " " +
          nbts(ulamID, "n", "0;0") +
          " X X"
      );
    }
    if (arg[0] == "/FobsChange") {
      //FobsChange 1[PlayerID] 2[UlamID] 3[PlaceID] 4[startFob1] 5[startFob2] 6[EndFob] 7[DropID] 8[Count] 9[Slot] 10![CandyCount]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      var fPlayerID = arg[1];
      var fUlamID = arg[2];
      var fPlaceID = arg[3];
      var fStartFob1 = arg[4];
      var fStartFob2 = arg[5];
      var fEndFob = arg[6];
      var fDropID = arg[7];
      var fCount = arg[8];
      var fSlot = arg[9];
      var fCandyCount = arg[10];

      var fFob21TT = nbt(fUlamID, fPlaceID, "n", "0;0");

      if (
        checkFobChange(fUlamID, fPlaceID, fStartFob1, fStartFob2) ||
        (["13", "23", "25", "27"].includes(fStartFob1) &&
          checkFobChange(fUlamID, fPlaceID, "40", "-1"))
      ) {
        if (invChangeTry(fPlayerID, fDropID, fCount, fSlot)) {
          fobChange(fUlamID, fPlaceID, fEndFob);
          fFob21TT = nbt(fUlamID, fPlaceID, "n", "0;0");
          sendToAllClients(
            "/RetFobsChange " +
              fUlamID +
              " " +
              fPlaceID +
              " " +
              fEndFob +
              " " +
              fFob21TT +
              " X X"
          );
          ws.send(
            "/RetInventory " +
              fPlayerID +
              " " +
              fDropID +
              " 0 " +
              fSlot +
              " " +
              -fCount +
              " X " +
              plr.livID[fPlayerID]
          );
          return;
        } else kick(fPlayerID);
      }

      //If failied
      ws.send(
        "/RetFobsChange " +
          fUlamID +
          " " +
          fPlaceID +
          " " +
          getBlockAt(fUlamID, fPlaceID) +
          " " +
          fFob21TT +
          " X X"
      );
      ws.send(
        "/RetInventory " +
          fPlayerID +
          " " +
          fDropID +
          " " +
          -fCount +
          " " +
          fSlot +
          " " +
          fCount +
          " X " +
          plr.livID[fPlayerID]
      );
    }
    if (arg[0] == "/FobsDataChange") {
      //FobsDataChange 1[PlayerID] 2[UlamID] 3[PlaceID] 4[Item] 5[DeltaCount] 6[Slot] 7[Id21]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      var gPlayerID = arg[1];
      var gUlamID = arg[2];
      var gPlaceID = arg[3];
      var gItem = arg[4];
      var gDeltaCount = arg[5];
      var gSlot = arg[6];
      var gID21 = arg[7];

      if (checkFobDataChange(gUlamID, gPlaceID, gItem, gDeltaCount, gID21)) {
        if (invChangeTry(gPlayerID, gItem, -gDeltaCount, gSlot)) {
          var gCountEnd = fobDataChange(gUlamID, gPlaceID, gItem, gDeltaCount);
          sendToAllClients(
            "/RetFobsDataChange " +
              gUlamID +
              " " +
              gPlaceID +
              " " +
              gItem +
              " " +
              gDeltaCount +
              " " +
              gPlayerID +
              " " +
              gCountEnd +
              " " +
              gID21 +
              " X X"
          );
          ws.send(
            "/RetInventory " +
              gPlayerID +
              " " +
              gItem +
              " 0 " +
              gSlot +
              " " +
              gDeltaCount +
              " X " +
              plr.livID[gPlayerID]
          );
          return;
        } else kick(gPlayerID);
      }

      //If failied
      ws.send(
        "/RetFobsDataCorrection " +
          gUlamID +
          " " +
          gPlaceID +
          " " +
          nbt(gUlamID, gPlaceID, "n", "0;0") +
          ";" +
          gDeltaCount +
          " " +
          gPlayerID +
          " " +
          gID21 +
          " X X"
      );
      ws.send(
        "/RetInventory " +
          gPlayerID +
          " " +
          gItem +
          " " +
          gDeltaCount +
          " " +
          gSlot +
          " " +
          -gDeltaCount +
          " X " +
          plr.livID[gPlayerID]
      );
    }
    if (arg[0] == "/FobsTurn") {
      //FobsTurn 1[PlayerID] 2[ulam] 3[place] 4[start1] 5[start2] 6[end]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      var turPlaID = arg[1];
      var turUlam = arg[2];
      var turPlace = arg[3];
      var turStart1 = arg[4];
      var turStart2 = arg[5];
      var turEnd = arg[6];
      if (checkFobChange(turUlam, turPlace, turStart1, turStart2)) {
        fobChange(turUlam, turPlace, turEnd);
        sendToAllClients(
          "/RetFobsTurn " +
            turPlaID +
            " " +
            turUlam +
            " " +
            turPlace +
            " " +
            turEnd +
            " X X"
        );
      }
    }
    if (arg[0] == "/GeyzerTurnTry") {
      //GeyzerTurnTry 1[PlayerID] 2[ulam] 3[place]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      var tgrPlaID = arg[1];
      var tgrUlam = arg[2];
      var tgrPlace = arg[3];

      if (
        checkFobChange(tgrUlam, tgrPlace, "13", "23") ||
        checkFobChange(tgrUlam, tgrPlace, "25", "27")
      ) {
        fobChange(tgrUlam, tgrPlace, "40");
        sendToAllClients("/RetGeyzerTurn " + tgrUlam + " " + tgrPlace + " X X");
      }
    }
    if (arg[0] == "/FobsPing") {
      //FobsPing 1[id]
      var fpID = arg[1];
      ws.send("/RetFobsPing " + fpID + " X X");
    }
    if (arg[0] == "/InventoryChange") {
      //InventoryChange 1[PlayerID] 2[Item] 3[Count] 4[Slot]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      var liPlaID = arg[1];
      var liItem = arg[2];
      var liCount = arg[3];
      var liSlot = arg[4];

      if (!invChangeTry(liPlaID, liItem, liCount, liSlot)) kick(liPlaID);
    }
    if (arg[0] == "/Upgrade") {
      //Upgrade 1[PlayerID] 2[item] 3[count] 4[upgID] 5[slot]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      var ljPlaID = arg[1];
      var ljItem = arg[2];
      var ljCount = arg[3];
      var ljUpgID = arg[4];
      var ljSlot = arg[5];

      if (invChangeTry(ljPlaID, ljItem, -ljCount, ljSlot)) {
        var ljTab = plr.upgrades[ljPlaID].split(";");
        ljTab[ljUpgID] = parseInt(ljTab[ljUpgID]) + 1;
        var ij,
          eff = ljTab[0];
        for (ij = 1; ij < 5; ij++) eff += ";" + ljTab[ij];
        plr.upgrades[ljPlaID] = eff;
        ws.send(
          "/RetUpgrade " + ljPlaID + " " + ljUpgID + " X " + plr.livID[ljPlaID]
        );
      } else kick(ljPlaID);
    }
    if (arg[0] == "/InventoryReset") {
      //InventoryReset 1[PlayerID] 2[NewLivID]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;
      plr.livID[arg[1]] = arg[2];

      var iID = arg[1];

      plr.data[iID] =
        plr.data[iID].split(";")[6] +
        ";" +
        plr.data[iID].split(";")[7] +
        ";0;0;0;0;" +
        plr.data[iID].split(";")[6] +
        ";" +
        plr.data[iID].split(";")[7] +
        ";1;0;0;0";
      plr.inventory[iID] = "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
      plr.upgrades[iID] = "0;0;0;0;0";
      plr.backpack[iID] =
        "0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
    }
    if (arg[0] == "/Craft") {
      //Craft 1[PlaID] 2[Id1] 3[Co1] 4[Sl1] 5[Id2] 6[Co2] 7[Sl2] 8[IdE] 9[CoE] 10[SlE]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      var cPlaID = arg[1];

      var cId1 = arg[2];
      var cCo1 = arg[3];
      var cSl1 = arg[4];

      var cId2 = arg[5];
      var cCo2 = arg[6];
      var cSl2 = arg[7];

      var cIdE = arg[8];
      var cCoE = arg[9];
      var cSlE = arg[10];

      var safeCopyI = plr.inventory[cPlaID];
      var safeCopyB = plr.backpack[cPlaID];

      if (invChangeTry(cPlaID, cId1, cCo1, cSl1))
        if (invChangeTry(cPlaID, cId2, cCo2, cSl2))
          if (invChangeTry(cPlaID, cIdE, cCoE, cSlE)) return;

      plr.inventory[cPlaID] = safeCopyI;
      plr.backpack[cPlaID] = safeCopyB;
      kick(cPlaID);
    }
    if (arg[0] == "/InventoryPush") {
      //InventoryPush 1[PlayerID] 2[PushID]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

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
    if (arg[0] == "/NewBulletSend") {
      //NewBulletSend 1[PlayerID] 2[type] 3,4[position] 5,6[vector] 7[ID]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      var tpl = Object.assign({},bulletTemplate);
      tpl.start = Object.assign({},bulletTemplate.start);
      tpl.vector = Object.assign({},bulletTemplate.vector);
      tpl.pos = Object.assign({},bulletTemplate.pos);

      tpl.ID = parseIntP(arg[7]);
      tpl.owner = parseIntP(arg[1]);
      tpl.type = parseIntP(arg[2]);
      tpl.start.x = parseFloatP(arg[3]);
      tpl.start.y = parseFloatP(arg[4]);
      tpl.vector.x = parseFloatP(arg[5]);
      tpl.vector.y = parseFloatP(arg[6]);
      tpl.pos = tpl.start;

      spawnBullet(tpl,arg);
    }
    if (arg[0] == "/NewBulletRemove") {
      //NewBulletRemove 1[PlayerID] 2[ID] 3[age]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;
      
      var lngt = bulletsT.length;
      for(i=0;i<lngt;i++)
        if(bulletsT[i].owner==arg[1] && bulletsT[i].ID==arg[2])
          destroyBullet(i,arg);
    }
    if (arg[0] == "/InfoUp") {
      //InfoUp 1[info] 2[PlayerID]
      if (!checkPlayer(arg[2], arg[msl - 2])) return;

      sendToAllClients("/RetInfoClient " + arg[1] + " " + arg[2] + " X X");
    }
    if (arg[0] == "/InvisibilityPulse") {
      //InvisibilityPulse 1[PlayerID] 2[DataString]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

      sendToAllClients(
        "/RetInvisibilityPulse " + arg[1] + " " + arg[2] + " X X"
      );
    }
    if (arg[0] == "/Backpack") {
      //Backpack 1[PlayerID] 2[Item] 3[Count] 4[Slot]
      if (!checkPlayer(arg[1], arg[msl - 2])) return;

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
    if (arg[0] == "/ImNotKicked") {
      //ImNotKicked 1[PlayerID]
      var imkConID = arg[1];
      if (!checkPlayer(arg[1], arg[msl - 2]))
        ws.send(
          "/RetKickConnection " + imkConID + " " + plr.conID[imkConID] + " X"
        );
      else {
        console.log(plr.nicks[imkConID] + " joined [" + imkConID + "]");
        sendToAllClients(
          "/RetInfoClient " +
            (plr.nicks[imkConID] + " joined the game").replaceAll(" ", "`") +
            " " +
            imkConID +
            " X X"
        );
      }
    }
  });
});

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
    }
  } else if (mod == 2) {
    for (i = 0; i < 128; i++) {
      if (translateFob[i] == str) return i + "";
    }
  }

  //If just translated
  try {
    var ipr = parseIntE(str) + "";
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
      totalChance += parseIntE(parseFloatE(pom) * 10 + "");
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
    if (c == "'" && !comment) {
      catch_all = !catch_all;
      continue;
    }
    if (
      (c != " " || catch_all) &&
      c != "\t" &&
      c != "\r" &&
      c != "\n" &&
      !comment
    )
      raw = raw + c;
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
          //Normal gameplay
          if (psPath[1] == "turbo_regenerate_multiplier")
            gameplay[0] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "turbo_use_multiplier")
            gameplay[1] = parseFloatE(jse3Dat[i]) + "";

          if (psPath[1] == "health_level_add")
            gameplay[26] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "drill_level_add")
            gameplay[2] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "bullet_level_add")
            gameplay[3] = parseFloatE(jse3Dat[i]) + "";

          if (psPath[1] == "health_regenerate_cooldown")
            gameplay[4] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "health_regenerate_multiplier")
            gameplay[5] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "crash_minimum_energy")
            gameplay[6] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "crash_damage_multiplier")
            gameplay[7] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "spike_damage")
            gameplay[8] = parseFloatE(jse3Dat[i]) + "";

          if (psPath[1] == "player_normal_speed")
            gameplay[9] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "player_brake_speed")
            gameplay[10] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "player_turbo_speed")
            gameplay[11] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "drill_normal_speed")
            gameplay[12] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "drill_brake_speed")
            gameplay[13] = parseFloatE(jse3Dat[i]) + "";

          if (psPath[1] == "vacuum_drag_multiplier")
            gameplay[14] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "all_speed_multiplier")
            gameplay[15] = parseFloatE(jse3Dat[i]) + "";

          //Artefact gameplay
          if (psPath[1] == "at_protection_health_level_add")
            gameplay[16] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "at_protection_health_regenerate_multiplier")
            gameplay[17] = parseFloatE(jse3Dat[i]) + "";

          if (psPath[1] == "at_impulse_power_regenerate_multiplier")
            gameplay[18] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "at_impulse_time")
            gameplay[19] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "at_impulse_speed")
            gameplay[20] = parseFloatE(jse3Dat[i]) + "";

          if (psPath[1] == "at_illusion_power_regenerate_multiplier")
            gameplay[21] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "at_illusion_power_use_multiplier")
            gameplay[22] = parseFloatE(jse3Dat[i]) + "";

          if (psPath[1] == "at_unstable_normal_avarage_time")
            gameplay[23] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "at_unstable_special_avarage_time")
            gameplay[24] = parseFloatE(jse3Dat[i]) + "";
          if (psPath[1] == "at_unstable_force")
            gameplay[25] = parseFloatE(jse3Dat[i]) + "";
        } catch {
          datapackError("Error in variable: " + jse3Var[i]);
        }
      }
    } else if (lgt == 3) {
      if (psPath[0] == "game_translate") {
        if (psPath[1] == "Asteroids") {
          try {
            mID = parseIntE(psPath[2]);
            translateAsteroid[mID] = jse3Dat[i];
          } catch {
            datapackError("Error in variable: " + jse3Var[i]);
          }
        } else if (psPath[1] == "Items_and_objects") {
          try {
            mID = parseIntE(psPath[2]);
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
            mID = parseIntE(translate(psPath[1], 2));
          else mID = parseIntE(translate(psPath[1], 1));
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
          mID = parseIntE(psPath[1]);
          if (psPath[2] == "title_image") {
            mID2 = 7;
            mID = 7 * (mID - 1) + 6;
            jse3Dat[i] = translate(jse3Dat[i], 2) + ";1;0;0;-1;1";
          } else {
            mID2 = parseIntE(psPath[2]);
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
          mID = parseIntE(psPath[1]);
          if (mID < 0 || mID > 31) nev++;

          if (psPath[2] == "settings") {
            biomeTags[mID] = jse3Dat[i].replaceAll(" ", "_");
          } else if (psPath[2] != "chance") {
            if (psPath[2] == "all_sizes") mID2 = -4;
            else mID2 = parseIntE(psPath[2]) - 4;
            if ((mID2 < 0 || mID2 > 6) && mID2 != -4) nev++;

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
          mID = parseIntE(psPath[1]);
          if (mID < 0 || mID > 31) nev++;

          if (psPath[2] == "chance") {
            if (mID == 0) nev++;
            var efe = mID + ";" + cur1000biome + ";";

            var le = jse3Dat[i].length;
            if (jse3Dat[i][le - 1] == "%")
              jse3Dat[i] = percentRemove(jse3Dat[i]);
            else nev++;

            var mno;
            if (tagContains(biomeTags[mID], "structural")) mno = 2;
            else mno = 1;

            cur1000biome += mno * parseIntE(parseFloatE(jse3Dat[i]) * 10 + "");
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
        pom = parseIntE(strs[i]);
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
      if (actual + 1 != parseIntE(strT[i])) return false;
      actual++;
      ended = false;
    } else {
      if (actual > parseIntE(strT[i]) + 1) return false;
      actual = parseIntE(strT[i]);
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
    if (parseIntE(strT[i + 1]) < 0) return false;
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
          strT[i] == strT[i + 2] ||
          strT[i + 2] == strT[i + 4] ||
          strT[i + 4] == strT[i]
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
  if (!intsAll(craftings, 6) || !goodItems(craftings, true)) nev++;
  if (!intsAll(biomeChances, 3) || !in1000(biomeChances, false)) nev++;
  for (i = 0; i < 16; i++) {
    if (
      !intsAll(drillLoot[i], 3) ||
      !in1000(drillLoot[i], false) ||
      !drillGoodItem(drillLoot[i])
    )
      nev++;
    if (!intsAll(fobGenerate[i], 3) || !in1000(fobGenerate[i], false)) nev++;
  }
  for (i = 0; i < 224; i++) {
    if (typeSet[i] != "")
      if (!intsAll(typeSet[i], 3) || !in1000(typeSet[i], true)) nev++;
  }
  for (i = 0; i < 128; i++) {
    if (!intsAll(modifiedDrops[i], 2) || !goodItems(modifiedDrops[i], false))
      nev++;
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
    craftMaxPage = parseIntE(raws[1]) + "";

    for (i = 0; i < 16; i++) drillLoot[i] = raws[2].split("'")[i];
    for (i = 0; i < 16; i++) fobGenerate[i] = raws[3].split("'")[i];
    for (i = 0; i < 224; i++) typeSet[i] = raws[4].split("'")[i];
    for (i = 0; i < gpl_number; i++) {
      if (false) gameplay[i] = parseIntE(raws[5].split("'")[i]) + "";
      else gameplay[i] = parseFloatE(raws[5].split("'")[i]) + "";
    }
    for (i = 0; i < 128; i++) modifiedDrops[i] = raws[6].split("'")[i];
    for (i = 0; i < 32; i++)
      biomeTags[i] = raws[7].replaceAll(" ", "_").split("'")[i];

    checkDatapackGoodE();
  } catch {
    crash(
      "Failied loading imported datapack\r\nDelete ServerUniverse/UniverseInfo.se3 file and try again"
    );
  }
}

//Start functions
console.log("-------------------------------");

if (!existsF("ServerUniverse/UniverseInfo.se3")) {
  if (existsF("Datapack.jse3"))
    datapackTranslate("NoName~" + readF("Datapack.jse3"));
  else crash("File Datapack.se3 doesn't exists");

  clientDatapacksVar = clientDatapacks();
  uniTime = 0;
  uniMiddle = "Server Copy~" + clientDatapacksVar;
  uniVersion = serverVersion;
  writeF(
    "ServerUniverse/UniverseInfo.se3",
    [uniTime, uniMiddle, uniVersion, ""].join("\r\n")
  );

  console.log("Datapack imported: [" + datName + "]");
} else {
  var uiSource = readF("ServerUniverse/UniverseInfo.se3").split("\r\n");
  if (uiSource.length < 3) crash("Error in ServerUniverse/UniverseInfo.se3");
  if (uiSource[2] != serverVersion)
    crash(
      "Loaded universe has a wrong version: " +
        uiSource[2] +
        " != " +
        serverVersion
    );

  var dataGet = uiSource[1].split("~");
  if (dataGet.length < 2) crash("Error in ServerUniverse/UniverseInfo.se3");
  datapackPaste(uiSource[1]);

  clientDatapacksVar = clientDatapacks();
  uniTime = 0;
  uniMiddle = "Server Copy~" + clientDatapacksVar;
  uniVersion = serverVersion;
  writeF(
    "ServerUniverse/UniverseInfo.se3",
    [uniTime, uniMiddle, uniVersion, ""].join("\r\n")
  );

  console.log("Datapack loaded");
}

if (!existsF("ServerUniverse/Seed.se3")) {
  seed = randomInteger(0, 10000000);
  writeF("ServerUniverse/Seed.se3", seed + "\r\n");
  console.log("New seed generated: [" + seed + "]");
} else seed = parseIntE(readF("ServerUniverse/Seed.se3").split("\r\n")[0]);

console.log("Server started on version: [" + serverVersion + "]");
console.log("Port: [27683]");
console.log("-------------------------------");
