#!/bin/bash
# Build + stage configs for debugging Origam.Server (no run — the debugger
# launches the DLL). Staging is shared with configureServer.sh via
# docker/server/linux/stage_server_config.sh.
set -e
cd /workspaces/origam/backend

CONFIG="${CONFIG:-Debug Server}"
dotnet build Origam.Server/Origam.Server.csproj /p:Configuration="$CONFIG" -v:minimal

# The Server csproj flattens OutputPath to bin\<first word of Configuration>\,
# so "Debug Server" lands in bin/Debug/, not bin/Debug Server/.
BIN="Origam.Server/bin/${CONFIG%% *}/net8.0"

# EnableChat=true builds chat-html and stages it where ChatConfig:PathToChatApp
# points (mirrors the published image, which ships a prebuilt chat in
# server_bin/clients/chat). Requires node/corepack (in the devcontainer image).
if [[ "${EnableChat:-false}" == "true" ]]; then
    (cd ../chat-html && corepack yarn install --immutable && corepack yarn build)
    rm -rf "$BIN/clients/chat"
    mkdir -p "$BIN/clients/chat"
    cp -r ../chat-html/dist/. "$BIN/clients/chat/"
    export CHAT_APP_DIR="/workspaces/origam/backend/$BIN/clients/chat"
fi

# Staged config: appsettings.json (ExternalDomain + chat substitution) and
# OrigamSettings.config from the shared templates. Default ExternalDomain to
# the frontend origin when unset (e.g. a bare docker exec shell, no compose env).
export ExternalDomain_SetOnStart="${ExternalDomain_SetOnStart:-https://localhost:${FRONTEND_PORT:-5173}}"
source ../docker/server/linux/stage_server_config.sh
stage_server_config ../docker/server "$BIN"

cp ../docker/dev/log4net.config "$BIN/log4net.config"

# PathToClientApp is hardcoded to /home/origam/server_bin/clients/origam in the
# template; Startup builds a PhysicalFileProvider for it unconditionally. Create
# it empty (dev frontend runs in Vite).
mkdir -p /home/origam/server_bin/clients/origam
