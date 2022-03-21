#!/bin/bash
sudo node /root/https-proxy/index.js &
cd /home/origam/HTML5
./startServerTest.sh 
echo "TEST DB Connection"
DATAOUT=$(dotnet origam-utils.dll test-db -t 10 -d 5000 -c "select 1")
#echo "Result IS $DATAOUT !!!!!!!!!!!!!!!!!!" ;
if [[ "$DATAOUT" != True ]]; then
echo "Database connection failed";
exit 1;
fi 
export ASPNETCORE_URLS="http://+:8080"
dotnet Origam.Server.dll &
echo "TEST DB Connection"
DATAOUT=$(dotnet origam-utils.dll test-db -t 5 -d 5000 -c "SELECT 1 FROM dbo.\"OrigamModelVersion\" where \"refSchemaExtensionId\"='${OrigamSettings_SchemaExtensionGuid}'")
if [[ "$DATAOUT" != True ]]; then
echo "Database connection failed";
exit 1;
fi
cd tests_e2e
yarn install --ignore-engines
yarn test:e2e
