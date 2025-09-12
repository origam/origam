#!/bin/bash

# ENV variable default values specific to linux
# OrigamSettings.config
if [ -z "${OrigamSettings__ModelSourceControlLocation}" ]; then
  export OrigamSettings__ModelSourceControlLocation="/home/origam/projectData/model"
fi


PROJECT_DATA_DIRECTORY="/home/origam/projectData"
cd /home/origam/Architect

if [ ! -d "$PROJECT_DATA_DIRECTORY" ]; then
	echo "Server has no model!!! Review the instance setup.";
	exit 1
fi

ORIGAM_SETTINGS_FILE="OrigamSettings.config"
cp _OrigamSettings.template "$ORIGAM_SETTINGS_FILE"

source fill_origam_settings_config.sh
fill_origam_settings_config "$ORIGAM_SETTINGS_FILE" "${DatabaseType}"
