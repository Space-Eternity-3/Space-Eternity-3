|--- README ---|

You can see how to start the server here: https://se3.page/server

------ Datapacks ------

You can load a custom datapack by moving it into Datapacks folder and starting a server. It will work only
if the universe doesn't exist yet or UniverseInfo.se3 file was manually removed.

If you want to use a new datapack in the old universe (which is not recommended if generator settings are different due to
potential world corruption problems) you should go to the ServerUniverse folder and delete UniverseInfo.se3 file, which
stores all previous datapack and the game version of the univere. It will allow you to import a new datapack.

------ Universes ------

All universe data is stored in the "ServerUniverse" folder. You can change this name in the "config.json" file.
You can copy the universe to the singleplayer "saves" folder (remember about changing folder's name) or import any universe from it.
All singleplayer universes are saved into the "%appdata%\Space Eternity 3\saves\" folder on Windows.

The player data is saved in the same format on multiplayer and on singleplayer, however file paths are different.
On singleplayer the game uses "PlayerData.se3" file. On multiplayer all player data is saved into the folder
"Players". The individual player's data is called "NICKNAME.se3", where NICKNAME is the nickname of the player.

------ Protection ------

If your server is commercial, you may need to protect it from hackers. You should activate the nickname verification
system to require from players the valid SE3 account to join. You will also be able to customize your server address. Start
your server to see the instructions.

If you want to protect your connection using wss:// protocol, search how to do this in the internet. If you will be using
a proxy, go to "config.json" file and change "trust_proxy" option to "true". If you don't use any proxy, it's not safe to
have it activated.