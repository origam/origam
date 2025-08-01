const util = require('util');
const execAsync = util.promisify(require('child_process').exec);

// we need to use origam-utils to execute the procedures, access via node mssql client doesn't yield any results
async function executeProcedure(procedureName) {
  const script = `dotnet ../origam-utils.dll run-sql-procedure --attempts 5 --delay 5000 --sql-command "${procedureName}"`;

  try {
    const { stdout, stderr } = await execAsync(script);
    if (stdout && stdout.trim()) console.log(stdout);
    if (stderr && stderr.trim()) console.error(stderr);
  } catch (error) {
    // Print output if exit code is 1 (error)
    if (error.code === 1) {
      console.error('Script failed with exit code 1');
      if (error.stdout?.trim()) console.log(error.stdout);
      if (error.stderr?.trim()) console.error(error.stderr);
    } else {
      // For any other error, rethrow or handle as needed
      throw error;
    }
  }
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