#!/bin/bash
# Build + run Origam.Architect.Server from source (devcontainer).
# The architect doesn't deploy the DB schema on a fresh database — run the
# runtime server once first (it deploys the schema): .devcontainer/run-server.sh,
# or F5 with the Debug Origam.Server configuration.
# To debug instead of run, see netcoredbg-server.sh.

set -e
cd /workspaces/origam/backend

# DatabaseType is a required env var (compose); fill_origam_settings_config validates it.
# shellcheck disable=SC2154
: "${DatabaseType:-}"

CONFIG="${CONFIG:-Debug Architect Server}"

dotnet build Origam.Architect.Server/Origam.Architect.Server.csproj \
    /p:Configuration="$CONFIG" -v:minimal

# Run from bin: content root = cwd, where the app reads appsettings.json /
# log4net.config; OrigamSettings.config sits next to the DLL.
BIN="Origam.Architect.Server/bin/$CONFIG/net8.0"

# Model settings: the shared main-stack template + fill helper.
cp ../docker/server/_OrigamSettings.template "$BIN/OrigamSettings.config"
source ../docker/server/linux/fill_origam_settings_config.sh
fill_origam_settings_config "$BIN/OrigamSettings.config" "${DatabaseType}"

# appsettings per the upstream dev flow (see backend/Origam.AI.Agent/README.md):
# ConfigTemplates/_appsettings.json, plus the Development overlay below
# (container SpaConfig + Ai stub). cp -n so a user's
# Ai:ApiKey edit in bin survives rebuilds.
cp Origam.Architect.Server/ConfigTemplates/_appsettings.json "$BIN/appsettings.json"
cp -n /workspaces/origam/.devcontainer/architect-appsettings.Development.json "$BIN/appsettings.Development.json"
cp ../docker/dev/log4net.config "$BIN/log4net.config"
mkdir -p /home/origam/ClientApplication

cd "$BIN"
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://+:8081"
exec dotnet Origam.Architect.Server.dll
