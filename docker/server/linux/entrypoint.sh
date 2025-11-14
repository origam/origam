#!/bin/bash

# ENV variable default values specific to linux
# OrigamSettings.config
if [ -z "${OrigamSettings__ModelSourceControlLocation}" ]; then
  export OrigamSettings__ModelSourceControlLocation="/home/origam/projectData/model"
fi

sudo /root/updateTimezone.sh
cd /home/origam/Setup
./cleanUpEnvironment.sh
sudo ./cleanUpEnvironmentRoot.sh

if [ -z "$ContainerMode" ] || [ "$ContainerMode" = 'server' ]; then
  cd /etc/nginx/ssl
  sudo /etc/nginx/ssl/createSslCertificate.sh
  sudo /etc/init.d/nginx start
  cd /home/origam/server_bin
  ./configureServer.sh
  export ASPNETCORE_URLS="http://+:8080"
  exec dotnet Origam.Server.dll
elif [ "$ContainerMode" = "scheduler" ]; then
  cd /home/origam/scheduler_bin
  ./configureScheduler.sh
  exec dotnet OrigamScheduler.dll
#  bash
else
  echo "Unsupported ContainerMode $ContainerMode"
  exit 1
fi