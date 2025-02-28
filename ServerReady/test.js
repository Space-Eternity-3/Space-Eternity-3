const WebSocket = require("ws");
const child_process = require("child_process");

console.log("Spawning server process...");
const serverProcess = child_process.fork("./index.js");

serverProcess.on("spawn", () => {
    setTimeout(() => {
        const ws = new WebSocket(`ws://localhost:27683`);

        ws.on("open", function open() {
            console.log("Connection successful! ✔️");

            serverProcess.kill();
            process.exit(0);
        });
    }, 5000);
});