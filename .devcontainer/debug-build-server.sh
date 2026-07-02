#!/bin/bash
# Build + stage configs for debugging Origam.Server (no run — the debugger
# launches the DLL). Mirrors docker/dev/entrypoint.dev.sh + configureServer.sh
# but against the source build's bin output.
set -e
cd /workspaces/origam/backend

CONFIG="Debug Server"
dotnet build Origam.Server/Origam.Server.csproj /p:Configuration="$CONFIG" -v:minimal

BIN="Origam.Server/bin/Debug/net8.0"

# appsettings.json from the shared template, substituting ExternalDomain
# (OIDC redirect URIs). Default to the server's own URL if unset.
EXTERNAL_DOMAIN="${ExternalDomain_SetOnStart:-https://localhost:5173}"
cp ../docker/server/_appsettings.template "$BIN/appsettings.json"
sed -i "s|ExternalDomain|${EXTERNAL_DOMAIN}|g" "$BIN/appsettings.json"
# Match configureServer.sh's chat substitutions + fix the template's trailing
# comma after ServerClient.ClientSecret.
sed -i "s|pathchatapp||" "$BIN/appsettings.json"
sed -i "s|chatinterval|0|" "$BIN/appsettings.json"
sed -i "/serverSecret/s/,$//" "$BIN/appsettings.json"

# OrigamSettings.config from the shared template + OrigamSettings__* env.
cp ../docker/server/_OrigamSettings.template "$BIN/OrigamSettings.config"
source ../docker/server/linux/fill_origam_settings_config.sh
fill_origam_settings_config "$BIN/OrigamSettings.config" "${DatabaseType}"

cp ../docker/dev/log4net.config "$BIN/log4net.config"

# PathToClientApp is hardcoded to /home/origam/server_bin/clients/origam in the
# template; Startup builds a PhysicalFileProvider for it unconditionally. Create
# it empty (dev frontend runs in Vite).
mkdir -p /home/origam/server_bin/clients/origam
