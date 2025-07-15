const util = require('util');
const exec = util.promisify(require('child_process').exec);

// we need to use origam-utils to execute the procedures, access via node mssql client doesn't yield any results
async function executeProcedure(procedureName){
  const script = "dotnet ../origam-utils.dll run-sql -a 1 -d 0 -c  \"EXEC " + procedureName + "\"";
  await exec(script);
}

// These procedures are defined in the test model deployment scripts in
// "origam\model-tests\model\AutomaticTests\DeploymentVersion\AutomaticTests"
// These deployment scripts are run before tests, so it is safe to call the procedures here.
async function restoreAllDataTypesTable(){
  await executeProcedure("restoreAllDataTypes");
}

async function clearScreenConfiguration(){
  await executeProcedure("clearScreenConfiguration");
}

// restores original state of the WidgetSectionTestMaster and dependent tables
async function restoreWidgetSectionTestMaster(){
  await executeProcedure("restoreWidgetSectionTestMaster");
}

// clears all data and insert new data used for the test
async function restoreWidgetSectionTestMasterForRefreshTest(){
  await executeProcedure("restoreWidgetSectionTestMasterForRefreshTest");
}

async function putNumericTestDataToAllDataTypes(){
  await executeProcedure("putNumericTestDataToAllDataTypes");
}

module.exports = { restoreAllDataTypesTable, restoreWidgetSectionTestMaster, clearScreenConfiguration,
  putNumericTestDataToAllDataTypes, restoreWidgetSectionTestMasterForRefreshTest }