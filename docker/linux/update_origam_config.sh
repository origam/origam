#!/bin/bash

# Helper function to convert an underscore separated string to CamelCase.
# Example: "DEFAULT_SCHEMA_EXTENSION_ID" -> "DefaultSchemaExtensionId"
toCamelCase() {
    local input="$1"
    local output=""
    IFS='_' read -ra words <<< "$input"
    for word in "${words[@]}"; do
        # Lowercase the entire word, then capitalize the first letter.
        local first_char=$(echo "${word:0:1}" | tr '[:lower:]' '[:upper:]')
        local rest=$(echo "${word:1}" | tr '[:upper:]' '[:lower:]')
        output="${output}${first_char}${rest}"
    done
    echo "$output"
}

update_origam_config() {
    local config_file="$1"

    if [[ ! -f "$config_file" ]]; then
      echo "Error: File '$config_file' not found."
      exit 1
    fi

    local required_vars=(
      ORIGAM_SETTINGS__DATABASE_HOST
      ORIGAM_SETTINGS__DATABASE_PORT
      ORIGAM_SETTINGS__DATABASE_NAME
      ORIGAM_SETTINGS__DATABASE_USERNAME
      ORIGAM_SETTINGS__DATABASE_PASSWORD
      ORIGAM_SETTINGS__DEFAULT_SCHEMA_EXTENSION_ID
      ORIGAM_SETTINGS__NAME
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
    local CONNECTION_STRING="Data Source=${ORIGAM_SETTINGS__DATABASE_HOST},${ORIGAM_SETTINGS__DATABASE_PORT};Initial Catalog=${ORIGAM_SETTINGS__DATABASE_NAME};User ID=${ORIGAM_SETTINGS__DATABASE_USERNAME};Password=${ORIGAM_SETTINGS__DATABASE_PASSWORD};"
    xmlstarlet ed -L -u "${OrigamSettingNodeXpath}/DataConnectionString" -v "${CONNECTION_STRING}" "$config_file"

    # Loop through all environment variables starting with ORIGAM_SETTINGS__
    # Each variable maps to an XML node name by stripping the prefix and converting to CamelCase.
for env_entry in $(env | grep '^ORIGAM_SETTINGS__' | grep -v '^ORIGAM_SETTINGS__DATABASE'); do
    key="${env_entry%%=*}"
    value="${env_entry#*=}"

    raw_node_name="${key#ORIGAM_SETTINGS__}"
    node_name=$(toCamelCase "$raw_node_name")
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
