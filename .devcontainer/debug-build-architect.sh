#!/bin/bash
# Build + stage configs for debugging Origam.Architect.Server (no run — the
# debugger launches the DLL). PreLaunchTask for the VS Code debug config.
set -e
cd /workspaces/origam/backend

# DatabaseType is a required env var (compose); fill_origam_settings_config validates it.
# shellcheck disable=SC2154
: "${DatabaseType:-}"

CONFIG="Debug Architect Server"
dotnet build Origam.Architect.Server/Origam.Architect.Server.csproj \
    /p:Configuration="$CONFIG" -v:minimal

BIN="Origam.Architect.Server/bin/$CONFIG/net8.0"

# Model settings: the shared main-stack template + fill helper (upstream ships
# no OrigamSettings template for the architect — its HOWTOSTART has you hand-
# write one).
cp ../docker/server/_OrigamSettings.template "$BIN/OrigamSettings.config"
source ../docker/server/linux/fill_origam_settings_config.sh
fill_origam_settings_config "$BIN/OrigamSettings.config" "${DatabaseType}"

# appsettings per the upstream dev flow (see backend/Origam.AI.Agent/README.md):
# ConfigTemplates/_appsettings.json, plus the Development overlay below
# (container SpaConfig + Ai stub). cp -n so a user's
# Ai:ApiKey edit in bin survives rebuilds. Requires ASPNETCORE_ENVIRONMENT=
# Development at run time (set in run-architect.sh / netcoredbg-server.sh /
# launch.json).
cp Origam.Architect.Server/ConfigTemplates/_appsettings.json "$BIN/appsettings.json"
cp -n /workspaces/origam/.devcontainer/architect-appsettings.Development.json "$BIN/appsettings.Development.json"
cp ../docker/dev/log4net.config "$BIN/log4net.config"
mkdir -p /home/origam/ClientApplication
