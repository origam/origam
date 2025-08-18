#!/bin/bash

fill_origam_settings_config() {
    local config_file="$1"

    if [[ ! -f "$config_file" ]]; then
      echo "Error: File '$config_file' not found."
      exit 1
    fi

    local required_vars=(
      OrigamSettings__DatabaseHost
      OrigamSettings__DatabasePort
      OrigamSettings__DatabaseName
      OrigamSettings__DatabaseUsername
      OrigamSettings__DatabasePassword
      OrigamSettings__DefaultSchemaExtensionId
      OrigamSettings__Name
      OrigamSettings__ModelSourceControlLocation
    )
    local missing_vars=""
    for var in "${required_vars[@]}"; do
        if [[ -z "${!var}" ]]; then
            missing_vars+="${var} "
        fi
    done
    if [[ -n "$missing_vars" ]]; then
        echo -e "\e[31mThe following required environment variables are missing: $missing_vars\e[0m"
        exit 1
    fi

    # Define the XPath for the target <OrigamSetting> node.
    OrigamSettingNodeXpath="/OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings/OrigamSettings"

    # Compose the DataConnectionString using the required connection string variables.
    local CONNECTION_STRING="Data Source=${OrigamSettings__DatabaseHost},${OrigamSettings__DatabasePort};Initial Catalog=${OrigamSettings__DatabaseName};User ID=${OrigamSettings__DatabaseUsername};Password=${OrigamSettings__DatabasePassword};"

    if xmlstarlet sel -t -v "${OrigamSettingNodeXpath}/DataConnectionString" "$config_file" &>/dev/null; then
        xmlstarlet ed -L -u "${OrigamSettingNodeXpath}/DataConnectionString" -v "${CONNECTION_STRING}" "$config_file"
    else
        xmlstarlet ed -L -s "$OrigamSettingNodeXpath" -t elem -n "DataConnectionString" -v "${CONNECTION_STRING}" "$config_file"
    fi

    for env_entry in $(env | grep '^OrigamSettings__' | grep -v '^OrigamSettings__Database'); do
        key="${env_entry%%=*}"
        value="${env_entry#*=}"

        node_name="${key#OrigamSettings__}"
        xpath="${OrigamSettingNodeXpath}/${node_name}"

        # Check if the node exists; if it does, update its value, otherwise create it.
        if xmlstarlet sel -t -v "$xpath" "$config_file" &>/dev/null; then
            xmlstarlet ed -L -u "$xpath" -v "${value}" "$config_file"
        else
            xmlstarlet ed -L -s "$OrigamSettingNodeXpath" -t elem -n "${node_name}" -v "${value}" "$config_file"
        fi
    done

    echo "${config_file} file updated successfully."
}
