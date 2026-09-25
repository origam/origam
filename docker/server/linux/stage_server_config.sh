#!/bin/bash

# Stage the ORIGAM server config files (appsettings.json + OrigamSettings.config)
# from the templates next to the built server binaries.
#
# Usage: source this file, then
#   stage_server_config <template_dir> <target_dir>
#
# <template_dir> holds _appsettings.template + _OrigamSettings.template
# (in the published image that is /home/origam/server_bin), <target_dir> is the
# directory the server runs from. Required environment: DatabaseType plus the
# OrigamSettings__* variables consumed by fill_origam_settings_config.sh.
# Optional: ExternalDomain_SetOnStart (substituted into appsettings.json),
# EnableChat=true (points ChatConfig:PathToChatApp at CHAT_APP_DIR, default
# /home/origam/server_bin/clients/chat), CHAT_APP_DIR.

# DatabaseType is set by the caller; fill_origam_settings_config validates it.
# shellcheck disable=SC2154
: "${DatabaseType:-}"

stage_server_config() {
    local template_dir="$1"
    local target_dir="$2"

    cp "$template_dir/_appsettings.template" "$target_dir/appsettings.json"

    if [[ ! -z ${ExternalDomain_SetOnStart} ]]; then
        sed -i "s|ExternalDomain|${ExternalDomain_SetOnStart}|g" "$target_dir/appsettings.json"
    fi

    if [[ ! -z ${EnableChat} && ${EnableChat} == true ]]; then
        sed -i "s|pathchatapp|${CHAT_APP_DIR:-/home/origam/server_bin/clients/chat}|" "$target_dir/appsettings.json"
        sed -i "s|chatinterval|10000|" "$target_dir/appsettings.json"
    else
        sed -i "s|pathchatapp||" "$target_dir/appsettings.json"
        sed -i "s|chatinterval|0|" "$target_dir/appsettings.json"
    fi

    cp "$template_dir/_OrigamSettings.template" "$target_dir/OrigamSettings.config"

    source "$(dirname "${BASH_SOURCE[0]}")/fill_origam_settings_config.sh"
    fill_origam_settings_config "$target_dir/OrigamSettings.config" "${DatabaseType}"
}
