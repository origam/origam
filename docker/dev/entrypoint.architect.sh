#!/bin/bash
# Development entrypoint: run Origam.Architect.Server.dll from architect_bin,
# against the same DB + model as the runtime server. The architect-html Vite
# container proxies API requests here.

set -e

# bootstrap.sh is a no-op unless ORIGAM_PROJECT_BOOTSTRAP=true (consumer flow).
. /home/origam/architect_bin/bootstrap.sh

if [ -z "${OrigamSettings__ModelSourceControlLocation}" ]; then
  export OrigamSettings__ModelSourceControlLocation="/home/origam/projectData/model"
fi

# Docker creates named-volume mount points as root; chown so origam can write
# Data Protection keys.
sudo chown -R origam:origam /home/origam/.aspnet/DataProtection-Keys 2>/dev/null || true

cd /home/origam/architect_bin
cp _OrigamSettings.template OrigamSettings.config
source fill_origam_settings_config.sh
fill_origam_settings_config OrigamSettings.config "${DatabaseType}"

# Startup builds a StaticFileProvider for ClientApplication unconditionally; in
# dev the SPA runs in Vite, so create it empty.
mkdir -p /home/origam/ClientApplication

export ASPNETCORE_URLS="http://+:8081"
exec dotnet Origam.Architect.Server.dll
