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

start_server() {
  if [ -z "${OrigamSettings__ModelSourceControlLocation}" ]; then
    export OrigamSettings__ModelSourceControlLocation="/home/origam/projectData/model"
  fi

  sudo /root/updateTimezone.sh
  cd /home/origam/Setup
  ./cleanUpEnvironment.sh
  sudo ./cleanUpEnvironmentRoot.sh
  
  cd /etc/nginx/ssl
  sudo /etc/nginx/ssl/createSslCertificate.sh
  sudo /etc/init.d/nginx start
  cd /home/origam/HTML5
  ./configureServer.sh
  export ASPNETCORE_URLS="http://+:8080"
  dotnet Origam.Server.dll > origam-output.txt 2>&1 &
}

fill_origam_settings_for_workflow_tests(){
  if [[ ${DatabaseType} == mssql ]]; then
    cp _OrigamSettings.wf.mssql.template OrigamSettings.config
  fi
  if [[ ${DatabaseType} == postgresql ]]; then
    cp _OrigamSettings.wf.pgsql.template OrigamSettings.config
  fi
  if [[ ! -f "OrigamSettings.config" ]]; then
    echo "Please set 'DatabaseType' Type of Database (mssql/postgresql)"
    exit 1
  fi
  sed -i "s/OrigamSettings__DataConnectionString/${OrigamSettings__DataConnectionString}/g" OrigamSettings.config
}

# Main script
cd /home/origam/HTML5

print_title "Start server and wait for database to be available"
start_server

echo "Waiting for Origam.Server.dll to initialize DB..."
dotnet origam-utils.dll get-root-version --attempts 5 --delay 5000
return_code=$?
if [[ "$return_code" != 0 ]]; then
  print_error "DB initialization failed"
  echo "Origam.Server.dll output:"
  print_file_contents origam-output.txt
  exit 1
else
  echo " DB initialized"
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
  print_error "Frontend integration tests failed"
  exit 1
fi

print_title "Run workflow integration tests"
print_note "Some workflow steps will fail. This is part of the tests."
echo
cd /home/origam/HTML5_TESTS
fill_origam_settings_for_workflow_tests

dotnet test --logger "trx;logfilename=workflow-integration-test-results.trx" Origam.WorkflowTests.dll | filter_test_output
if [[ $? -eq 0 ]]; then
  sudo cp /home/origam/HTML5_TESTS/TestResults/workflow-integration-test-results.trx /home/origam/output/
  echo "Success."
else
  sudo cp /home/origam/HTML5_TESTS/TestResults/workflow-integration-test-results.trx /home/origam/output/
  print_error "Workflow integration tests failed"
  exit 1
fi