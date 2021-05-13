const readline = require("readline");
const path = require("path");
const fs = require("fs");
const { exec } = require("child_process");
const { promisify } = require("util");
const { Writable } = require("stream");

const execPr = promisify(exec);
const fstmpRawAuditDir = path.join(".", "scripts", ".temp");
const fstmpRawAuditFile = path.join(fstmpRawAuditDir, "audit.jsonl");

const resultArray = [];

async function main() {
  if (!fs.existsSync(fstmpRawAuditDir)) fs.mkdirSync(fstmpRawAuditDir);
  try {
    await execPr(`yarn audit --json > ${fstmpRawAuditFile}`);
  } catch (e) {
    if (e.code > 31) throw e;
  }
  const readInterface = readline.createInterface({
    input: fs.createReadStream("scripts/.temp/audit.jsonl"),
    output: new Writable(),
    console: false,
  });

  function processLine(line) {
    const lineObj = JSON.parse(line);
    //console.log(lineObj.data)
    try {
      if (lineObj.type !== "auditAdvisory") return;
      if (lineObj.data.resolution.path.startsWith("terser-webpack-plugin")) return;
      if (lineObj.data.resolution.path.startsWith("jest")) return;
      if (lineObj.data.resolution.path.startsWith("webpack")) return;
      if (lineObj.data.resolution.path.startsWith("html-webpack-plugin")) return;
      if (lineObj.data.resolution.path.startsWith("resolve-url-loader")) return;
      if (lineObj.data.resolution.path.startsWith("react-dev-utils")) return;
      if (lineObj.data.resolution.path.startsWith("@babel")) return;
      if (lineObj.data.resolution.path.startsWith("@svgr/webpack")) return;
      if (lineObj.data.resolution.path.startsWith("babel-preset-react-app")) return;
      if (lineObj.data.resolution.path.startsWith("babel-jest")) return;
      if (lineObj.data.resolution.path.startsWith("babel-eslint")) return;
      if (lineObj.data.resolution.path.startsWith("eslint")) return;
      if (lineObj.data.resolution.path.startsWith("@testing-library/jest-dom")) return;
      if (lineObj.data.resolution.path.startsWith("@typescript-eslint")) return;
      if (lineObj.data.resolution.path.startsWith("node-sass")) return;
      if (lineObj.data.resolution.path.startsWith("optimize-css-assets-webpack-plugin")) return;

      if (lineObj.data.resolution.path.startsWith("css-loader")) return;
      if (lineObj.data.resolution.path.startsWith("postcss")) return;
      if (lineObj.data.resolution.path.startsWith("file-cli")) return;
      if (lineObj.data.resolution.path.startsWith("postcss-preset-env")) return;


    } catch (e) {
      console.log(lineObj);
      throw e;
    }
    resultArray.push(lineObj);
  }

  readInterface.on("line", processLine);

  readInterface.on("close", function () {
    console.log(resultArray.length);
    console.log(resultArray);
    for(let item of resultArray) {
      console.log(item.data.resolution.path);
    }
    debugger
  });
}

main();
