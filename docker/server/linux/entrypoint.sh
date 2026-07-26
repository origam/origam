#!/bin/bash
set -e
. /home/origam/server_bin/bootstrap.sh
trap 'echo; echo "=== Origam server output ==="; cat /home/origam/server_bin/origam-output.txt 2>/dev/null || true; echo "=== End of output ==="' ERR

# ENV variable default values specific to linux
# OrigamSettings.config
if [ -z "${OrigamSettings__ModelSourceControlLocation}" ]; then
  export OrigamSettings__ModelSourceControlLocation="/home/origam/projectData/model"
fi

container_mode="${ContainerMode:-server}"

if [ "$container_mode" != "server-direct" ]; then
  sudo /root/updateTimezone.sh
fi
cd /home/origam/Setup
./cleanUpEnvironment.sh
if [ "$container_mode" != "server-direct" ]; then
  sudo ./cleanUpEnvironmentRoot.sh
fi

if [ "$container_mode" = "server" ]; then
  cd /etc/nginx/ssl
  sudo /etc/nginx/ssl/createSslCertificate.sh
  sudo /etc/init.d/nginx start
  cd /home/origam/server_bin
  ./configureServer.sh
  export ASPNETCORE_URLS="http://+:8080"
  exec dotnet Origam.Server.dll
elif [ "$container_mode" = "server-direct" ]; then
  cd /home/origam/server_bin
  export ORIGAM_SKIP_NGINX=true
  ./configureServer.sh
  export ASPNETCORE_URLS="http://+:8080"
  exec dotnet Origam.Server.dll
elif [ "$container_mode" = "scheduler" ]; then
  cd /home/origam/scheduler_bin
  ./configureScheduler.sh
  exec dotnet OrigamScheduler.dll
#  bash
else
  echo "Unsupported ContainerMode $container_mode"
  exit 1
fi
