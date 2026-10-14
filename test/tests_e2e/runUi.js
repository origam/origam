const { readFileSync } = require("fs");
const { join } = require("path");
const { spawnSync } = require("child_process");
const { parse } = require("dotenv");

const databaseType = process.argv[2];
if (!["mssql", "postgresql"].includes(databaseType)) {
  throw new Error("Choose a database profile: mssql or postgresql.");
}

const envFile = `.env.${databaseType}`;
let fileEnvironment;
try {
  fileEnvironment = parse(readFileSync(join(__dirname, envFile)));
} catch (error) {
  if (error.code !== "ENOENT") throw error;
  console.error(`Copy ${envFile}.example to ${envFile} and configure your local test database first.`);
  process.exit(1);
}

if (!fileEnvironment.OrigamSettings__DataConnectionString) {
  console.error(`Set OrigamSettings__DataConnectionString in ${envFile}.`);
  process.exit(1);
}

const result = spawnSync(process.execPath, [
  require.resolve("@playwright/test/cli"), "test", "--ui", ...process.argv.slice(3),
], {
  cwd: __dirname,
  stdio: "inherit",
  env: { ...process.env, ...fileEnvironment, DatabaseType: databaseType, CI: "" },
});

if (result.error) throw result.error;
process.exit(result.status ?? 1);
