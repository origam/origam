const sql = require("mssql/msnodesqlv8");
const { sqlDatabase, sqlServer, sqlPort, sqlUser, sqlPassword, sqlOptions } = require('./additionalConfig');

async function connectToDb() {
  const connectionPool = new sql.ConnectionPool({
    database: sqlDatabase ,
    server: sqlServer ,
    driver: "msnodesqlv8",
    port: sqlPort,
    user: sqlUser,
    password: sqlPassword,
    options: sqlOptions
  });

  return connectionPool.connect();
}

async function executeProcedure(procedureName){
  const pool = await connectToDb();
  try{
    const request = pool.request();
    const result = await request.query(`EXEC ${procedureName};`);
  }
  finally{
    pool.close()
  }
}

// restores original state of the AllDataTypes and dependent tables
async function restoreAllDataTypesTable(){
  await executeProcedure("dbo.restoreAllDataTypes");
}

async function putNumericTestDataToAllDataTypes(){
  await executeProcedure("dbo.putNumericTestDataToAllDataTypes");
}

async function clearScreenConfiguration(){
  await executeProcedure("dbo.clearScreenConfiguration");
}

// restores original state of the WidgetSectionTestMaster and dependent tables
async function restoreWidgetSectionTestMaster(){
  await executeProcedure("dbo.restoreWidgetSectionTestMaster");
}

module.exports = { restoreAllDataTypesTable, restoreWidgetSectionTestMaster, clearScreenConfiguration,
  putNumericTestDataToAllDataTypes }
