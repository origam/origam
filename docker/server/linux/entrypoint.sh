#!/bin/bash

sudo /root/updateTimezone.sh
cd /home/origam/Setup
./updateEnvironment.sh
sudo ./updateEnvironmentRoot.sh

if [ -z "$CONTAINER_MODE" ] || [ "$CONTAINER_MODE" = 'server' ]; then
  cd /etc/nginx/ssl
  sudo /etc/nginx/ssl/createSslCertificate.sh
  sudo /etc/init.d/nginx start
  cd /home/origam/HTML5
  ./configureServer.sh
  export ASPNETCORE_URLS="http://+:8080"
  exec dotnet Origam.Server.dll
elif [ "$CONTAINER_MODE" = "scheduler" ]; then
  cd /home/origam/Scheduler
  ./configureScheduler.sh
  exec dotnet OrigamScheduler.dll
#  bash
else
  echo "Unsupported CONTAINER_MODE $CONTAINER_MODE"
  exit 1
fi