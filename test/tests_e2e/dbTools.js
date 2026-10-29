const connectionString = process.env.OrigamSettings__DataConnectionString;
const databaseType = (process.env.DatabaseType || "mssql").toLowerCase();

function parseConnectionString(value) {
  return Object.fromEntries(
    value.split(";").filter(Boolean).map(part => {
      const separator = part.indexOf("=");
      return [part.slice(0, separator).trim().toLowerCase(), part.slice(separator + 1).trim()];
    })
  );
}

function valueFor(settings, ...names) {
  for (const name of names) {
    const value = settings[name.toLowerCase()];
    if (value !== undefined) return value;
  }
  return undefined;
}

async function executeSqlServerProcedure(procedureName) {
  const settings = parseConnectionString(connectionString);
  const serverValue = valueFor(settings, "server", "data source");
  const [server, portText] = serverValue.includes(",")
    ? serverValue.split(",", 2)
    : [serverValue, undefined];
  const trustedConnection = /^(true|yes|sspi)$/i.test(
    valueFor(settings, "integrated security", "trusted_connection") || ""
  );
  let driver;
  const config = {
    server,
    port: portText ? Number(portText) : undefined,
    database: valueFor(settings, "database", "initial catalog"),
    user: valueFor(settings, "user id", "uid", "user"),
    password: valueFor(settings, "password", "pwd"),
    requestTimeout: 270_000,
    options: {
      encrypt: /^(true|yes)$/i.test(valueFor(settings, "encrypt") || "false"),
      trustServerCertificate: /^(true|yes)$/i.test(
        valueFor(settings, "trustservercertificate") || "true"
      ),
    },
  };

  if (trustedConnection) {
    driver = require("mssql/msnodesqlv8");
    const odbcDriver = valueFor(settings, "driver") || "ODBC Driver 18 for SQL Server";
    config.connectionString = `Driver={${odbcDriver}};${connectionString}`
      .replace(/Data Source=/i, "Server=")
      .replace(/Initial Catalog=/i, "Database=")
      .replace(/Integrated Security=(true|yes|sspi)/i, "Trusted_Connection=Yes")
      .replace(/Encrypt=false/i, "Encrypt=No")
      .replace(/TrustServerCertificate=true/i, "TrustServerCertificate=Yes");
  } else {
    driver = require("mssql");
  }

  const pool = await driver.connect(config);
  try {
    await pool.request().execute(procedureName);
  } finally {
    await pool.close();
  }
}

async function executePostgreSqlProcedure(procedureName) {
  const { Client } = require("pg");
  const settings = parseConnectionString(connectionString);
  const searchPath = valueFor(settings, "search path");
  const client = new Client({
    application_name: valueFor(settings, "application name"),
    database: valueFor(settings, "database"),
    host: valueFor(settings, "host", "server"),
    options: searchPath ? `-c search_path=${searchPath}` : undefined,
    password: valueFor(settings, "password"),
    port: Number(valueFor(settings, "port") || 5432),
    ssl: !/^(disable|false)$/i.test(valueFor(settings, "sslmode", "ssl mode") || "disable"),
    user: valueFor(settings, "username", "user id", "user"),
  });
  await client.connect();
  try {
    await client.query(`CALL "${procedureName}"()`);
  } finally {
    await client.end();
  }
}

async function executeProcedure(procedureName) {
  if (!connectionString) {
    throw new Error("OrigamSettings__DataConnectionString is required for E2E database setup.");
  }

  const execute = databaseType === "postgresql"
    ? executePostgreSqlProcedure
    : executeSqlServerProcedure;
  await execute(procedureName);
}

async function restoreAllDataTypesTable() { await executeProcedure("restoreAllDataTypes"); }
async function clearScreenConfiguration() { await executeProcedure("clearScreenConfiguration"); }
async function restoreWidgetSectionTestMaster() { await executeProcedure("restoreWidgetSectionTestMaster"); }
async function restoreWidgetSectionTestMasterForRefreshTest() {
  await executeProcedure("restoreWidgetSectionTestMasterForRefreshTest");
}
async function putNumericTestDataToAllDataTypes() { await executeProcedure("putNumericTestDataToAllDataTypes"); }

module.exports = {
  clearScreenConfiguration,
  putNumericTestDataToAllDataTypes,
  restoreAllDataTypesTable,
  restoreWidgetSectionTestMaster,
  restoreWidgetSectionTestMasterForRefreshTest,
};
