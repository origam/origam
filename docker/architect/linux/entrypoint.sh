#!/bin/bash
. /home/origam/Architect/bootstrap.sh
cd /home/origam/Architect
./configureServer.sh
export ASPNETCORE_URLS="http://+:8081"
exec dotnet Origam.Architect.Server.dll
#bash