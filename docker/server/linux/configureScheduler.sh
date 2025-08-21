cd /home/origam/Scheduler

ORIGAM_SETTINGS_FILE="OrigamSettings.config"
cp _OrigamSettings.template "$ORIGAM_SETTINGS_FILE"

source fill_origam_settings_config.sh
fill_origam_settings_config "$ORIGAM_SETTINGS_FILE" "${DatabaseType}"