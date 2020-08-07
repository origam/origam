const express = require("express");
const https = require("https");
const selfsigned = require("selfsigned");
const { createProxyMiddleware } = require("http-proxy-middleware");
const handlebars = require("express-handlebars");

const app = express();
const port = 5599;

app.set("view engine", "handlebars");
app.engine("handlebars", handlebars());

app.use(
  createProxyMiddleware(["/connect", "/assets", "/account", "/Account", "/home", "/.well-known", "/api"], {
    logLevel: "debug",
    target: "https://localhost:44356/",
    secure: false,
  })
);


app.use("/", express.static("static"));

const pems = selfsigned.generate();

https
  .createServer({ key: pems.private, cert: pems.cert }, app)
  .listen(port, () => console.log(`Example app listening on port ${port}!`));
