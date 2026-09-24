#!/bin/bash
# Build + run Origam.Server from source (devcontainer, no debugger).
# Build + staging are shared with the F5 debug path via debug-build-server.sh.
set -e
cd /workspaces/origam/backend

CONFIG="${CONFIG:-Debug Server}"
# Passed explicitly so the child stages the same config this script then runs;
# CONFIG stays local (not exported into the environment).
CONFIG="$CONFIG" bash /workspaces/origam/.devcontainer/debug-build-server.sh

# The Server csproj flattens OutputPath to bin\<first word of Configuration>\,
# so "Debug Server" lands in bin/Debug/, not bin/Debug Server/.
BIN="Origam.Server/bin/${CONFIG%% *}/net8.0"

cd "$BIN"
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://+:8080"
exec dotnet Origam.Server.dll
