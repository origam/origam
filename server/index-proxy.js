const express = require("express");
const { createProxyMiddleware, httpProxy } = require("http-proxy-middleware");
const winston = require("winston");
const expressWinston = require("express-winston");
const selfsigned = require("selfsigned");
const https = require("https");
const app = express();
const port = 3000;

var pems = selfsigned.generate(null, { days: 365 });
//console.log(pems);

app.use(
  expressWinston.logger({
    transports: [new winston.transports.Console()],
    format: winston.format.combine(
      winston.format.colorize(),
      // winston.format.json()
      winston.format.simple()
    ),
    meta: false, // optional: control whether you want to log the meta data about the request (default to true)
    msg: "HTTP {{req.method}} {{req.url}}", // optional: customize the default logging message. E.g. "{{res.statusCode}} {{req.method}} {{res.responseTime}}ms {{req.url}}"
    expressFormat: false, // Use the default Express/morgan request formatting. Enabling this will override any msg if true. Will only output colors with colorize set to true
    colorize: true, // Color the text and status code, using the Express/morgan color palette (text: gray, status: default green, 3XX cyan, 4XX yellow, 5XX red).
    ignoreRoute: function (req, res) {
      return false;
    }, // optional: allows to skip some log messages based on request and/or response
  })
);

app.use(
  ["/sockjs-node-origam"],
  createProxyMiddleware({
    logLevel: "debug",
    target: "wss://localhost:3001",
    secure: false,
  })
);

app.use(
  ["/sockjs-node-orichat"],
  createProxyMiddleware({
    logLevel: "debug",
    target: "ws://localhost:3002",
    secure: false,
  })
);

app.use(
  ["/chatrooms/Chat", "/chatrooms/Avatar", "/internalApi/HashTag"],
  createProxyMiddleware({
    // logProvider: () => winston,
    logLevel: "debug",
    target: "https://localhost:5001/",
    secure: false,
    changeOrigin: false,
    router: {
      // 'localhost:5566/internalApi': 'https://localhost:44356'
    },
  })
);

app.use(
  "/chatrooms",
  createProxyMiddleware({
    // logProvider: () => winston,
    logLevel: "debug",
    target: "http://localhost:3002/",
    secure: false,
    changeOrigin: false,
    router: {
      // 'localhost:5566/internalApi': 'https://localhost:44356'
    },
  })
);

app.use(
  "/",
  createProxyMiddleware({
    // logProvider: () => winston,
    logLevel: "debug",
    target: "https://localhost:3001/",
    secure: false,
    changeOrigin: false,
    router: {
      // 'localhost:5566/internalApi': 'https://localhost:44356'
    },
  })
);

const server = https.createServer(
  {
    key: pems.private,
    cert: pems.cert,
  },
  app
);

server.listen(port, () => console.log(`Proxy server listening on port ${port}!`));
