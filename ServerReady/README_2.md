# README

## Server Setup

You can find instructions on how to start the server [here](https://se3.page/server).

## Datapacks

You can load a custom datapack by moving it into the `Datapacks` folder and starting the server. This will only work if the universe does not already exist or if the `UniverseInfo.se3` file has been manually removed.

If you want to use a new datapack in an existing universe (which is not recommended if the generator settings differ, due to potential world corruption), you should navigate to the `ServerUniverse` folder and delete the `UniverseInfo.se3` file. This file stores the previous datapack information and the game version of the universe. Deleting it will allow you to import a new datapack.

## Universes

All universe data is stored in the `ServerUniverse` folder. You can change this folder's name in the `config.json` file. You can copy a universe to the singleplayer `saves` folder (remember to change the folder's name) or import any universe from it. All singleplayer universes are saved in the `%appdata%\Space Eternity 3\saves\` folder on Windows and in
user folder on other systems.

Player data is saved in the same format for both multiplayer and singleplayer modes, though the file paths differ. In singleplayer mode, the game uses the `PlayerData.se3` file. In multiplayer mode, all player data is saved in the `Players` folder. Individual player data is stored in files named `NICKNAME.se3`, where `NICKNAME` is the player's nickname.

## Protection

If your server is commercial, you may need to protect it from hackers. You should activate the nickname verification system, which will require players to have a valid SE3 account to join. You will also be able to customize your server address. Start your server to see the instructions.

If you want to secure your connection using the `wss://` protocol, search online for instructions on how to do this. If you are using a proxy, go to the `config.json` file and set the `trust_proxy` option to `true`. If you are not using a proxy, it is unsafe to leave this option activated.
