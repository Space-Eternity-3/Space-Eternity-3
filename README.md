# Space-Eternity-3
 An open-world sandbox game, where you explore the universe, gather resources and fight bosses for artefacts.
 
 | ![Maze-Game-Preview](https://raw.githubusercontent.com/Kamiloso/Kamiloso/main/se3.png) |
 | -------------------------------------------------------------------------------------- |

## Opening project

### Prerequisites
- Unity Hub
- Unity Editor `2023.2.20f1`
- Blender (for `.blend` files to properly render)
- node.js (for running the server)

### Steps
- Open project from Unity Hub using the specified Unity Editor version.
- Open `ServerReady` folder (contains node.js multiplayer server project).

### How to run
- To build the client, open Unity Editor and follow:  
  **File -> Build Settings... -> Build And Run.**
  
- To start the server, run the following commands in the directory with multiplayer server:
  ```bash
  npm i
  npm start
  ```

- If `npm start` doesn't work, try `node index.js`.

## Download the game
Download SE3 from a dedicated website: [se3.page](https://se3.page)
