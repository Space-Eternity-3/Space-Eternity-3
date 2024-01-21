|--- README ---|

You can see how to start the server here: https://se3.page/server

------ Datapacks ------

To load custom datapack to a new server, you need to copy it to the server
folder (with index.js file) and call it "Datapack.jse3". You can also use the "Default.jse3" datapack copy and edit it.
Be sure, that the folder ServerUniverse doesn't exist.
When you start your server, the new universe with the new datapack will be generated.

If you want to use a new datapack in the old universe (which is not recommended if generator settings are different due to
potential world corruption problems) you should go to the ServerUniverse folder and delete UniverseInfo.se3 file, which
stores all previous datapack. Such procedure will update the universe version too, so you can use it to update
versions of previous universes when their datapack is not default.

------ Universes ------

All universe data is stored in the "ServerUniverse" folder. You can change this name in the "config.json" file.
You can copy the universe to the singleplayer "saves" folder or import any universe from it.
All singleplayer universes are saved into the "%appdata%\Space Eternity 3\saves\" folder on Windows.

The player data is saved in the same format on multiplayer and on singleplayer, however file paths are different.
On singleplayer the game uses "PlayerData.se3" file. On multiplayer all player data is saved into the folder
"Players". The individual player's data is called "NICKNAME.se3", where NICKNAME is the nickname of the player.

------ Protection ------

If your server is commercial, you may need to protect it from hackers. You should activate the nickname verification
system to require from players the valid SE3 account to join. You will also be able to customize your server address. Start
your server to see the instruction.

If you want to protect your connection using wss:// protocol, search how to do that in the internet. If you will be using
a proxy, go to "config.json" file and change "trust_proxy" option to "true". If you don't use any proxy, it's not safe to activate it.