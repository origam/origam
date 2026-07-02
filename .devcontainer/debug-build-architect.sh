#!/bin/bash
# Build + stage configs for debugging Origam.Architect.Server (no run — the
# debugger launches the DLL). PreLaunchTask for the VS Code debug config.
set -e
cd /workspaces/origam/backend

CONFIG="Debug Architect Server"
dotnet build Origam.Architect.Server/Origam.Architect.Server.csproj \
    /p:Configuration="$CONFIG" -v:minimal

BIN="Origam.Architect.Server/bin/$CONFIG/net8.0"
cp ../docker/server/_OrigamSettings.template "$BIN/OrigamSettings.config"
source ../docker/server/linux/fill_origam_settings_config.sh
fill_origam_settings_config "$BIN/OrigamSettings.config" "${DatabaseType}"
cp ../docker/dev/appsettings.architect.json "$BIN/appsettings.json"
cp ../docker/dev/log4net.config "$BIN/log4net.config"
mkdir -p "$BIN/ClientApplication"
