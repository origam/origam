const fsp = require("fs").promises;
const fs = require("fs");

const readline = require("readline");
const { writableNoopStream } = require("noop-stream");

async function main() {
  const result = [];
  const rl = readline.createInterface({
    input: fs.createReadStream("./audit.log"),
    output: writableNoopStream(),
    terminal: false,
  });

  const prefixes = new Set();

  let lineCount = 0;
  let relevantLineCount = 0;

  rl.on("line", (lineRaw) => {
    lineCount++;
    console.log(lineCount);
    if (lineRaw.trim().length === 0) return;
    const line = JSON.parse(lineRaw);
    if (
      line.type === "auditAdvisory" &&
      !line.data.resolution.path.startsWith("webpack") &&
      !line.data.resolution.path.startsWith("terser-webpack-plugin") &&
      !line.data.resolution.path.startsWith("resolve-url-loader") &&
      !line.data.resolution.path.startsWith("webpack") &&
      !line.data.resolution.path.startsWith("react-dev-utils") &&
      !line.data.resolution.path.startsWith("@babel") &&
      !line.data.resolution.path.startsWith("eslint") &&
      !line.data.resolution.path.startsWith("babel-preset") &&
      !line.data.resolution.path.startsWith("babel-jest") &&
      !line.data.resolution.path.startsWith("html-webpack-plugin") &&
      !line.data.resolution.path.startsWith("@svgr/webpack") &&
      !line.data.resolution.path.startsWith("@typescript") &&
      !line.data.resolution.path.startsWith("babel-eslint") &&
      !line.data.resolution.path.startsWith("jest") &&
      !line.data.resolution.path.startsWith("node-sass") &&
      !line.data.resolution.path.startsWith("optimize-css-assets") &&
      !line.data.resolution.path.startsWith("@testing")
    ) {
      result.push(line);
      prefixes.add(line.data.resolution.path.slice(0, 20));
      relevantLineCount++;
    }
  });

  rl.on("close", () => {
    console.log("TOTAL:", lineCount, relevantLineCount);
    console.log(Array.from(prefixes));
    fs.writeFileSync(
      "./audit.filtered.log",
      result.map((line) => JSON.stringify(line, null, 2)).join("\r\n\r\n"),
      { encoding: "utf-8" }
    );
  });
}

main();
