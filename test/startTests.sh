#!/bin/bash

# Constants
RED='\033[0;31m' # red color
GREEN='\033[0;32m' # green color
YELLOW='\033[1;33m' # yellow color
NC='\033[0m' # no color

# Function definitions
print_error() {
  echo -e "${RED}$1${NC}"
}

print_note() {
  echo -e "${YELLOW}$1${NC}"
}

print_title() {
  echo
  echo -e "${GREEN}$1${NC}"
  echo
}

print_file_contents() {
  local filename="$1"
  if [[ -f "$filename" ]]; then
      cat "$filename"
  else
      echo "Error: File '$filename' not found."
  fi
}

# Will remove exceptions with the error message
# "Intentional test error" their stack traces and empty lines
filter_test_output() {
    gawk '
    BEGIN { skip = 0 }
    /Intentional test error/ { skip = 1; next }
    {
        if (skip) {
            if ($0 !~ /^\s*at /) {
                skip = 0
                sub(/\r?\n$/, "")
                if ($0 != "") print $0
            }
        } else {
            sub(/\r?\n$/, "")
            if ($0 != "") print $0
        }
    }
    '
}

# Main script
sudo node /root/https-proxy/index.js &
cd /home/origam/HTML5

print_title "Start server and wait for database to be available"
./startServer.sh
echo "Trying to connect to SQL server..."
utils_result=$(dotnet origam-utils.dll test-db --attempts 10 --delay 5000 --sql-command "select 1")
if [[ "$utils_result" != True ]]; then
  print_error "Initial database connection test failed, SQL server is not responding"
  exit 1
else
  echo "Initial database connection test passed, SQL server responds"
fi 
export ASPNETCORE_URLS="http://+:8080"
dotnet Origam.Server.dll > origam-output.txt 2>&1 &
echo "Waiting for Origam.Server.dll to initialize DB..."
utils_result=$(dotnet origam-utils.dll test-db --attempts 5 --delay 5000 --sql-command "SELECT 1 FROM dbo.\"OrigamModelVersion\" where \"refSchemaExtensionId\"='${OrigamSettings_SchemaExtensionGuid}'")
if [[ "$utils_result" != True ]]; then
  print_error "DB initialization timed out"
  echo "Origam.Server.dll output:"
  print_file_contents origam-output.txt
  exit 1
else
  echo "DB initialized"
fi

print_title "Run frontend integration tests"
cd tests_e2e
yarn install --ignore-engines > /dev/null 2>&1
yarn test:e2e
if [[ $? -eq 0 ]]; then
  sudo cp frontend-integration-test-results.trx /home/origam/output/
  echo "Success."
else
  sudo cp frontend-integration-test-results.trx /home/origam/output/
  print_error "Scripts failed"
  exit 1
fi

print_title "Run workflow integration tests"
print_note "Some workflow steps will fail. This is part of the tests."
echo
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

dotnet test --logger "trx;logfilename=workflow-integration-test-results.trx" Origam.WorkflowTests.dll | filter_test_output
if [[ $? -eq 0 ]]; then
  sudo cp /home/origam/HTML5_TESTS/TestResults/workflow-integration-test-results.trx /home/origam/output/
  echo "Success."
else
  sudo cp /home/origam/HTML5_TESTS/TestResults/workflow-integration-test-results.trx /home/origam/output/
  print_error "Scripts failed"
  exit 1
fi