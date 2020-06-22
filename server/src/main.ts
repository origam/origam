import express from "express";
import path from "path";
import cors from "cors";
import {createProxyMiddleware} from 'http-proxy-middleware';

const app = express();
const port = process.env.PORT || 8080;

app.use(express.static(path.join(__dirname, "../../build")));

/*app.get("*", (req, res) => {
  res.sendFile(path.join(__dirname, "../../build/index.html"));
});*/


app.use(
  "*",
  createProxyMiddleware({
    // logProvider: () => winston,
    logLevel: 'debug',
    target: "https://localhost:44356/",
    //target: "http://admindevh5.wy.by/",
    secure: false,
    //changeOrigin: true,
    router: {
      // 'localhost:5566/internalApi': 'https://localhost:44356'
    }
  })
);

app.listen(port, () => console.log(`Example app listening at http://localhost:${port}`));
