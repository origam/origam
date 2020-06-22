const express = require("express");
const proxy = require("http-proxy-middleware");
const winston = require("winston");
const expressWinston = require("express-winston");

const app = express();
const port = 5566;

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
    ignoreRoute: function(req, res) {
      return false;
    } // optional: allows to skip some log messages based on request and/or response
  })
);

app.use(
  "/",
  proxy({
    // logProvider: () => winston,
    logLevel: 'debug',
    target: "http://localhost:3000/",
    secure: false,
    changeOrigin: true,
    ws: true,
    router: {
      // 'localhost:5566/internalApi': 'https://localhost:44356'
    }
  })
);


app.listen(port, () => console.log(`Example app listening on port ${port}!`));
