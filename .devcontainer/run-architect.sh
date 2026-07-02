#!/bin/bash
# Build + run Origam.Architect.Server from source (devcontainer).
# The architect doesn't deploy the DB schema on a fresh database — run the
# runtime server once first (it deploys the schema), e.g.:
#   docker compose up server database
# To debug instead of run, see netcoredbg-server.sh.

set -e
cd /workspaces/origam/backend

CONFIG="${CONFIG:-Debug Architect Server}"

dotnet build Origam.Architect.Server/Origam.Architect.Server.csproj \
    /p:Configuration="$CONFIG" -v:minimal

# Run from bin: content root = cwd, where the app reads appsettings.json /
# log4net.config; OrigamSettings.config sits next to the DLL.
BIN="Origam.Architect.Server/bin/$CONFIG/net8.0"

cp ../docker/server/_OrigamSettings.template "$BIN/OrigamSettings.config"
source ../docker/server/linux/fill_origam_settings_config.sh
fill_origam_settings_config "$BIN/OrigamSettings.config" "${DatabaseType}"

cp ../docker/dev/appsettings.architect.json "$BIN/appsettings.json"
cp ../docker/dev/log4net.config "$BIN/log4net.config"
mkdir -p "$BIN/ClientApplication"

cd "$BIN"
export ASPNETCORE_URLS="http://+:8081"
exec dotnet Origam.Architect.Server.dll
