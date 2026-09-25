#!/bin/bash
# Build + run OrigamScheduler from source (devcontainer). The scheduler is the
# product's work-queue runtime; the repo's compose stacks don't run it, so this
# is the way to exercise it locally (e.g. ScheduledTime / ScheduledWorkQueueEntry).
set -e
cd /workspaces/origam/backend

# DatabaseType is a required env var (compose); fill_origam_settings_config validates it.
# shellcheck disable=SC2154
: "${DatabaseType:-}"

CONFIG="Debug Server"
dotnet build OrigamScheduler/OrigamScheduler.csproj /p:Configuration="$CONFIG" -v:minimal

BIN="OrigamScheduler/bin/Debug/net8.0"

# Stage from the project's own templates + the shared main-stack
# _OrigamSettings.template (same as the published image's configureScheduler.sh).
cp OrigamScheduler/TemplateFiles/_appsettings.json "$BIN/appsettings.json"
cp OrigamScheduler/TemplateFiles/_log4net.config "$BIN/log4net.config"
cp ../docker/server/_OrigamSettings.template "$BIN/OrigamSettings.config"
source ../docker/server/linux/fill_origam_settings_config.sh
fill_origam_settings_config "$BIN/OrigamSettings.config" "${DatabaseType}"

cd "$BIN"
exec dotnet OrigamScheduler.dll
