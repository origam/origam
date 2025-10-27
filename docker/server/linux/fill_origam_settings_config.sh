#!/bin/bash

fill_origam_settings_config() {
    local config_file="$1"
    local database_type="$2"

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
    if [[ "$database_type" == "mssql" ]]; then
      local connection_string="Encrypt=True;TrustServerCertificate=True;Data Source=${OrigamSettings__DatabaseHost},${OrigamSettings__DatabasePort};Initial Catalog=${OrigamSettings__DatabaseName};User ID=${OrigamSettings__DatabaseUsername};Password=${OrigamSettings__DatabasePassword};"
      local schema_data_service="Origam.DA.Service.MsSqlDataService, Origam.DA.Service"
      local data_data_service="Origam.DA.Service.MsSqlDataService, Origam.DA.Service"
    elif [[ "$database_type" == "postgresql" ]]; then
      local connection_string="sslmode=require;Host=${OrigamSettings__DatabaseHost};Port=${OrigamSettings__DatabasePort};Database=${OrigamSettings__DatabaseName};Username=${OrigamSettings__DatabaseUsername};Password=${OrigamSettings__DatabasePassword};"
      local schema_data_service="Origam.DA.Service.PgSqlDataService, Origam.DA.Service"
      local data_data_service="Origam.DA.Service.PgSqlDataService, Origam.DA.Service"
    else
      echo "Unsupported or missing DatabaseType. Please set DatabaseType to mssql or postgresql."
      exit 1
    fi

    # DataConnectionString (update or create)
    if xmlstarlet sel -t -v "${OrigamSettingNodeXpath}/DataConnectionString" "$config_file" >/dev/null 2>&1; then
      xmlstarlet ed -L -u "${OrigamSettingNodeXpath}/DataConnectionString" -v "$connection_string" "$config_file"
    else
      xmlstarlet ed -L -s "$OrigamSettingNodeXpath" -t elem -n "DataConnectionString" -v "$connection_string" "$config_file"
    fi

    # SchemaDataService (update or create)
    if xmlstarlet sel -t -v "${OrigamSettingNodeXpath}/SchemaDataService" "$config_file" >/dev/null 2>&1; then
      xmlstarlet ed -L -u "${OrigamSettingNodeXpath}/SchemaDataService" -v "$schema_data_service" "$config_file"
    else
      xmlstarlet ed -L -s "$OrigamSettingNodeXpath" -t elem -n "SchemaDataService" -v "$schema_data_service" "$config_file"
    fi

    # DataDataService (update or create)
    if xmlstarlet sel -t -v "${OrigamSettingNodeXpath}/DataDataService" "$config_file" >/dev/null 2>&1; then
      xmlstarlet ed -L -u "${OrigamSettingNodeXpath}/DataDataService" -v "$data_data_service" "$config_file"
    else
      xmlstarlet ed -L -s "$OrigamSettingNodeXpath" -t elem -n "DataDataService" -v "$data_data_service" "$config_file"
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
