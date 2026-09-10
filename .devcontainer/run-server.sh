#!/bin/bash
# Build + run Origam.Server from source (devcontainer, no debugger).
# For debugging, use the F5 launch config.
set -e
cd /workspaces/origam/backend

CONFIG="${CONFIG:-Debug Server}"
dotnet build Origam.Server/Origam.Server.csproj /p:Configuration="$CONFIG" -v:minimal

# The Server csproj flattens OutputPath to bin\<first word of Configuration>\,
# so "Debug Server" lands in bin/Debug/, not bin/Debug Server/.
BIN="Origam.Server/bin/${CONFIG%% *}/net8.0"

EXTERNAL_DOMAIN="${ExternalDomain_SetOnStart:-https://localhost:5173}"
cp ../docker/server/_appsettings.template "$BIN/appsettings.json"
sed -i "s|ExternalDomain|${EXTERNAL_DOMAIN}|g" "$BIN/appsettings.json"
# Match configureServer.sh's chat substitutions + fix the template's trailing
# comma after ServerClient.ClientSecret.
sed -i "s|pathchatapp||" "$BIN/appsettings.json"
sed -i "s|chatinterval|0|" "$BIN/appsettings.json"
sed -i "/serverSecret/s/,$//" "$BIN/appsettings.json"

cp ../docker/server/_OrigamSettings.template "$BIN/OrigamSettings.config"
source ../docker/server/linux/fill_origam_settings_config.sh
fill_origam_settings_config "$BIN/OrigamSettings.config" "${DatabaseType}"

cp ../docker/dev/log4net.config "$BIN/log4net.config"
mkdir -p /home/origam/server_bin/clients/origam

cd "$BIN"
export ASPNETCORE_URLS="http://+:8080"
exec dotnet Origam.Server.dll
