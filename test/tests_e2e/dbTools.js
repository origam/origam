const util = require('util');
const exec = util.promisify(require('child_process').exec);

// we need to use origam-utils to execute the procedures, access via node mssql client doesn't yield any results
async function executeProcedure(procedureName){
  const script = "dotnet ../origam-utils.dll test-db -a 1 -d 0 -c  \"EXEC " + procedureName + "\"";
  await exec(script);
}

async function restoreAllDataTypesTable(){
  await executeProcedure("dbo.restoreAllDataTypes");
}

async function clearScreenConfiguration(){
  await executeProcedure("dbo.clearScreenConfiguration");
}

// restores original state of the WidgetSectionTestMaster and dependent tables
async function restoreWidgetSectionTestMaster(){
  await executeProcedure("dbo.restoreWidgetSectionTestMaster");
}

module.exports = { restoreAllDataTypesTable, restoreWidgetSectionTestMaster, clearScreenConfiguration }

