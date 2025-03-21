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


#export OrigamSettings_DbUsername
#export OrigamSettings_DbPassword
#./updateEnvironment.sh
#sudo ./updateEnvironmentRoot.sh
