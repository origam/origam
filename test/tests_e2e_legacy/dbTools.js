const util = require('util');
const execFileAsync = util.promisify(require('child_process').execFile);
const path = require('path');

// we need to use origam-utils to execute the procedures, access via node mssql client doesn't yield any results
async function executeProcedure(procedureName) {
  const utilsPath = path.resolve(process.env.ORIGAM_E2E_UTILS_PATH || path.join(__dirname, '../origam-utils.dll'));

  try {
    const { stdout, stderr } = await execFileAsync('dotnet', [
      utilsPath, 'run-sql-procedure', '--attempts', '5', '--delay', '5000', '--sql-command', procedureName,
    ], { cwd: path.dirname(utilsPath) });
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
