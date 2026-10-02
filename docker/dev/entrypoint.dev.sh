#!/bin/bash
# Development entrypoint: run the published DLL from server_bin so ASP.NET's
# content root is where configureServer.sh writes the config (same as prod).
# No nginx/TLS — Vite proxies API/auth to :8080.

set -e

# bootstrap.sh is a no-op unless ORIGAM_PROJECT_BOOTSTRAP=true (consumer flow).
. /home/origam/server_bin/bootstrap.sh

# Default model location; the compose file bind-mounts the model here.
if [ -z "${OrigamSettings__ModelSourceControlLocation}" ]; then
  export OrigamSettings__ModelSourceControlLocation="/home/origam/projectData/model"
fi

# Docker creates named-volume mount points as root; chown so origam can write
# Data Protection keys.
sudo chown -R origam:origam /home/origam/.aspnet/DataProtection-Keys 2>/dev/null || true

# Generate appsettings.json + OrigamSettings.config from templates + env vars.
cd /home/origam/server_bin
./configureServer.sh

# Startup builds a StaticFileProvider for PathToClientApp unconditionally; in
# dev the SPA runs in Vite, so create the dir empty.
mkdir -p /home/origam/server_bin/clients/origam

export ASPNETCORE_URLS="http://+:8080"
exec dotnet Origam.Server.dll
