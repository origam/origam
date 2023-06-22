const express = require("express");
const { createProxyMiddleware, httpProxy } = require("http-proxy-middleware");
const selfsigned = require("selfsigned");
const https = require("https");
const app = express();
const port = 443;



var pems = selfsigned.generate(null, { days: 365, keySize: 2048 });
//console.log(pems);

/*
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
  ["/chatrooms/Chat", "/chatrooms/Avatar", "/internalApi/DeepLink"],
  createProxyMiddleware({
    logLevel: "debug",
    target: "http://server:8080/",
    secure: false,
    changeOrigin: false,
    xfwd: true,
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
    target: "http://server:8080/",
    secure: false,
    changeOrigin: false,
    xfwd: true,
    router: {
      // 'localhost:5566/internalApi': 'https://localhost:44356'
    },
  })
);
*/

function logProvider(provider) {
  const logger = console;

  const myCustomProvider = {
    log: logger.log,
    debug: logger.debug,
    info: logger.info,
    warn: logger.warn,
    error: logger.error,
  };
  return myCustomProvider;
}

app.use(
  "/",
  createProxyMiddleware({
//    logProvider: logProvider,
 //   logLevel: "info",
    target: "http://localhost:8080/",
    secure: false,
    changeOrigin: false,
    xfwd: true,
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

server.listen(port, () =>
  console.log(`Proxy server listening on port ${port}!`)
);
