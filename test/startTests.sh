#!/bin/bash

# Function definitions
print_file_contents() {
    local filename="$1"
    if [[ -f "$filename" ]]; then
        cat "$filename"
    else
        echo "Error: File '$filename' not found."
    fi
}

# Main script
sudo node /root/https-proxy/index.js &
cd /home/origam/HTML5
./startServer.sh 
echo "Trying to connect to SQL server..."
DATAOUT=$(dotnet origam-utils.dll test-db --attempts 10 --delay 5000 --sql-command "select 1")
if [[ "$DATAOUT" != True ]]; then
  echo "Initial database connection test failed, SQL server is not responding" >&2
  exit 1
else
  echo "Initial database connection test passed, SQL server responds"
fi 
export ASPNETCORE_URLS="http://+:8080"
dotnet Origam.Server.dll > origam-output.txt 2>&1 &
echo "Waiting for Origam.Server.dll to initialize DB..."
DATAOUT=$(dotnet origam-utils.dll test-db --attempts 5 --delay 5000 --sql-command "SELECT 1 FROM dbo.\"OrigamModelVersion\" where \"refSchemaExtensionId\"='${OrigamSettings_SchemaExtensionGuid}'")
if [[ "$DATAOUT" != True ]]; then
  echo "DB initialization timed out" >&2
  echo "Origam.Server.dll output:"
  print_file_contents origam-output.txt
  exit 1
else
  echo "DB initialized"
fi
echo "Running frontend integration tests"
cd tests_e2e
yarn install --ignore-engines > /dev/null 2>&1
yarn test:e2e
if [[ $? -eq 0 ]]; then
  sudo cp frontend-integration-test-results.trx /home/origam/output/
  echo "Success."
else
  sudo cp frontend-integration-test-results.trx /home/origam/output/
  echo "Scripts failed" >&2
  exit 1
fi

echo "Running workflow integration tests"
cd /home/origam/HTML5_TESTS
cp _OrigamSettings.wf.mssql.template OrigamSettings.config
sed -i "s|OrigamSettings_ModelName|\/home\/origam\/HTML5\/data\/origam${OrigamSettings_ModelSubDirectory}|" OrigamSettings.config
sed -i "s|OrigamSettings_ModelName|\/home\/origam\/HTML5\/data\/origam${OrigamSettings_ModelSubDirectory}|" OrigamSettings.config
sed -i "s/OrigamSettings_DbHost/${OrigamSettings_DbHost}/" OrigamSettings.config
sed -i "s/OrigamSettings_DbHost/${OrigamSettings_DbHost}/" OrigamSettings.config
sed -i "s/OrigamSettings_DbPort/${OrigamSettings_DbPort}/" OrigamSettings.config
sed -i "s/OrigamSettings_DbPort/${OrigamSettings_DbPort}/" OrigamSettings.config
sed -i "s/OrigamSettings_DbUsername/${OrigamSettings_DbUsername}/" OrigamSettings.config
sed -i "s/OrigamSettings_DbUsername/${OrigamSettings_DbUsername}/" OrigamSettings.config
sed -i "s/OrigamSettings_DbPassword/${OrigamSettings_DbPassword}/" OrigamSettings.config
sed -i "s/OrigamSettings_DbPassword/${OrigamSettings_DbPassword}/" OrigamSettings.config
sed -i "s/OrigamSettings_DatabaseName/${DatabaseName}/" OrigamSettings.config
sed -i "s/OrigamSettings_DatabaseName/${DatabaseName}/" OrigamSettings.config

dotnet test --logger "trx;logfilename=workflow-integration-test-results.trx" Origam.WorkflowTests.dll
if [[ $? -eq 0 ]]; then
  sudo cp /home/origam/HTML5_TESTS/TestResults/workflow-integration-test-results.trx /home/origam/output/
  echo "Success."
else
  sudo cp /home/origam/HTML5_TESTS/TestResults/workflow-integration-test-results.trx /home/origam/output/
  echo "Scripts failed" >&2
  exit 1
fi