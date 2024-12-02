const http = require('http');
const socketIo = require('socket.io');
const ngrok = require('@ngrok/ngrok');

// Crée le serveur HTTP
const server = http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'text/html' });
    res.end('Congrats, you have created an Ngrok web server with Socket.IO!');
});

// Ajoute Socket.IO
const io = socketIo(server, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"],
    },
});
let gameNamespace = io.of('/api/game')
// Gestion des connexions Socket.IO
gameNamespace.on('connection', (socket) => {
    console.log('A user connected');
    socket.emit('response', `Server received:`);
    // Écoute d'événements spécifiques
    socket.on('message', (data) => {
        console.log('Message received:', data);
        socket.emit('response', `Server received: ${data}`);
    });

    // Déconnexion
    socket.on('disconnect', () => {
        console.log('A user disconnected');
        socket.emit('response', `Server received: `);
    });
});

// Lancer le serveur
server.listen(8080, async () => {
    console.log('Node.js web server at port 4040 is running...');

    // Configure Ngrok
    const tunnel = await ngrok.connect({ addr: 8080, authtoken_from_env: true });
    console.log(`Ngrok tunnel established at: ${tunnel.url()}`);
});
