#!/bin/bash

# Function definitions
print_file_contents() {
    local filename="$1"
    if [ -f "$filename" ]; then
        cat "$filename"
    else
        echo "Error: File '$filename' not found."
    fi
}

# Main script
sudo node /root/https-proxy/index.js &
cd /home/origam/HTML5
./startServer.sh 
echo "TEST DB Connection"
DATAOUT=$(dotnet origam-utils.dll test-db -a 10 -d 5000 -c "select 1")
if [[ "$DATAOUT" != True ]]; then
  echo "Initial database connection failed.";
  exit 1;
fi 
export ASPNETCORE_URLS="http://+:8080"
dotnet Origam.Server.dll > origam-output.txt 2>&1 &
echo "TEST DB Connection"
DATAOUT=$(dotnet origam-utils.dll test-db -a 5 -d 5000 -c "SELECT 1 FROM dbo.\"OrigamModelVersion\" where \"refSchemaExtensionId\"='${OrigamSettings_SchemaExtensionGuid}'")
if [[ "$DATAOUT" != True ]]; then
  echo "Database connection failed";
  echo "Origam.Server.dll output:"
  print_file_contents origam-output.txt
  exit 1;
fi
echo "Running frontend integration tests"
cd tests_e2e
yarn install --ignore-engines > /dev/null 2>&1
yarn test:e2e
if [ $? -eq 0 ]
then
  sudo cp frontend-integration-test-results.trx /home/origam/output/
  echo "Success."
else
  sudo cp frontend-integration-test-results.trx /home/origam/output/
  echo "Scripts failed" >&2
  exit 1
fi

echo "Running workflow integration tests";
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
#cat OrigamSettings.config
dotnet test --logger "trx;logfilename=workflow-integration-test-results.trx" Origam.WorkflowTests.dll
if [ $? -eq 0 ]
then
  sudo cp /home/origam/HTML5_TESTS/TestResults/workflow-integration-test-results.trx /home/origam/output/
  echo "Success."
else
  sudo cp /home/origam/HTML5_TESTS/TestResults/workflow-integration-test-results.trx /home/origam/output/
  echo "Scripts failed" >&2
  exit 1
fi