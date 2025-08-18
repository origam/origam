cd /home/origam/Scheduler

ORIGAM_SETTINGS_FILE="OrigamSettings.config"
if [[ "${DatabaseType}" == "mssql" ]]; then
  cp _OrigamSettings.mssql.template "$ORIGAM_SETTINGS_FILE"
elif [[ "${DatabaseType}" == "postgresql" ]]; then
  cp _OrigamSettings.postgres.template "$ORIGAM_SETTINGS_FILE"
else
  echo "Unsupported or missing DatabaseType. Please set DatabaseType to mssql or postgresql."
  exit 1
fi

source fill_origam_settings_config.sh
fill_origam_settings_config "$ORIGAM_SETTINGS_FILE"