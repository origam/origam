const { existsSync, readFileSync } = require("fs");
const { join } = require("path");
const { spawnSync } = require("child_process");
const { parse } = require("./tests_e2e/node_modules/dotenv");

const [databaseType, selection = "both", ...args] = process.argv.slice(2);
if (!["mssql", "postgresql"].includes(databaseType) || !["both", "legacy", "playwright"].includes(selection)) {
  console.error("Usage: node run-e2e.js <mssql|postgresql> [both|legacy|playwright] [test arguments]");
  process.exit(1);
}

const envFile = join(__dirname, "tests_e2e", `.env.${databaseType}`);
if (!existsSync(envFile)) {
  console.error(`Copy ${envFile}.example to ${envFile} and configure your test application first.`);
  process.exit(1);
}
const env = {
  ...process.env, ...parse(readFileSync(envFile)), DatabaseType: databaseType,
  ORIGAM_E2E_HEADLESS: "true", CI: "",
};

env.PUPPETEER_EXECUTABLE_PATH ||= [
  join(process.env.ProgramFiles || "C:/Program Files", "Google/Chrome/Application/chrome.exe"),
  join(process.env["ProgramFiles(x86)"] || "C:/Program Files (x86)", "Google/Chrome/Application/chrome.exe"),
  "/usr/bin/google-chrome",
].find(existsSync);
env.ORIGAM_E2E_UTILS_PATH ||= [
  join(__dirname, "server_bin/origam-utils.dll"),
  join(__dirname, "../backend/Origam.Server/bin/Debug/net10.0/origam-utils.dll"),
  join(__dirname, "../backend/Origam.Server/bin/Release/net10.0/origam-utils.dll"),
].find(existsSync);

let exitCode = 0;
for (const runner of selection === "both" ? ["legacy", "playwright"] : [selection]) {
  const directory = join(__dirname, runner === "legacy" ? "tests_e2e_legacy" : "tests_e2e");
  const cli = require.resolve(runner === "legacy" ? "jest/bin/jest" : "@playwright/test/cli", { paths: [directory] });
  console.log(`Running ${runner} against ${databaseType} in headless mode.`);
  const result = spawnSync(process.execPath, [cli, ...(
    runner === "legacy" ? ["--runInBand", "--config", "jest.config.js"] : ["test"]
  ), ...args], { cwd: directory, env, stdio: "inherit" });
  if (result.error) throw result.error;
  console.log(`${runner}: ${result.status === 0 ? "passed" : "failed"}`);
  if (result.status !== 0) exitCode = 1;
}
process.exit(exitCode);
