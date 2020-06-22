import express, { Request, Response, NextFunction } from "express";
import path from "path";
import { createProxyMiddleware } from "http-proxy-middleware";
import cookieParser from "cookie-parser";

const app = express();
const port = process.env.PORT || 8080;

app.use(express.static(path.join(__dirname, "../../build")));
app.use(cookieParser());

/*app.get("*", (req, res) => {
  res.sendFile(path.join(__dirname, "../../build/index.html"));
});*/

app.use(
  "*",
  createProxyMiddleware({
    // logProvider: () => winston,
    //logLevel: "debug",
    target: "http://",
    //target: "http://admindevh5.wy.by/",
    router: (req) => {
      //console.log('Targetting', req.cookies.backendUrl)
      return req.cookies.backendUrl || "http://"
    },
    secure: false,
    changeOrigin: true,
  })
);

app.listen(port, () => console.log(`Example app listening at http://localhost:${port}`));
