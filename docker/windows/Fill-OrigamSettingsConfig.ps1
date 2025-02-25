function Fill-OrigamSettingsConfig {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConfigFile
    )

    # Check if file exists.
    if (-not (Test-Path $ConfigFile)) {
        Write-Error "Error: File '$ConfigFile' not found."
        exit 1
    }

    # Define the required environment variables.
    $requiredVars = @(
        "OrigamSettings__DatabaseHost",
        "OrigamSettings__DatabasePort",
        "OrigamSettings__DatabaseName",
        "OrigamSettings__DatabaseUsername",
        "OrigamSettings__DatabasePassword",
        "OrigamSettings__DefaultSchemaExtensionId",
        "OrigamSettings__Name",
        "OrigamSettings__ModelSourceControlLocation"
    )

    # Check for missing environment variables.
    $missingVars = @()
    foreach ($var in $requiredVars) {
        if (-not [System.Environment]::GetEnvironmentVariable($var)) {
            $missingVars += $var
        }
    }
    if ($missingVars.Count -gt 0) {
        Write-Host "The following required environment variables are missing: $($missingVars -join ' ')" -ForegroundColor Red
        exit 1
    }

    # Define the XPath for the target <OrigamSettings> node.
    $origamSettingNodeXpath = "/OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings/OrigamSettings"

    # Load the XML config file.
    [xml]$xml = Get-Content $ConfigFile

    # Compose the DataConnectionString.
    $connectionString = "Data Source=$($env:OrigamSettings__DatabaseHost),$($env:OrigamSettings__DatabasePort);Initial Catalog=$($env:OrigamSettings__DatabaseName);User ID=$($env:OrigamSettings__DatabaseUsername);Password=$($env:OrigamSettings__DatabasePassword);"

    # Update the DataConnectionString node.
    $dataConnNode = $xml.SelectSingleNode("$origamSettingNodeXpath/DataConnectionString")
    if ($dataConnNode) {
        $dataConnNode.InnerText = $connectionString
    }
    else {
        # If the node doesn't exist, create it.
        $parentNode = $xml.SelectSingleNode($origamSettingNodeXpath)
        if ($parentNode) {
            $newNode = $xml.CreateElement("DataConnectionString")
            $newNode.InnerText = $connectionString
            $parentNode.AppendChild($newNode) | Out-Null
        }
    }

    # Iterate through environment variables starting with 'OrigamSettings__'
    # excluding those starting with 'OrigamSettings__Database'
    $envVars = [System.Environment]::GetEnvironmentVariables().GetEnumerator() |
        Where-Object { $_.Key -like "OrigamSettings__*" -and $_.Key -notlike "OrigamSettings__Database*" }

    foreach ($envVar in $envVars) {
        $key = $envVar.Key
        $value = $envVar.Value
        # Remove the prefix to get the node name.
        $nodeName = $key -replace "^OrigamSettings__", ""
        $xpath = "$origamSettingNodeXpath/$nodeName"

        $targetNode = $xml.SelectSingleNode($xpath)
        if ($targetNode) {
            $targetNode.InnerText = $value
        }
        else {
            # Create the node if it does not exist.
            $parentNode = $xml.SelectSingleNode($origamSettingNodeXpath)
            if ($parentNode) {
                $newElem = $xml.CreateElement($nodeName)
                $newElem.InnerText = $value
                $parentNode.AppendChild($newElem) | Out-Null
            }
        }
        Write-Host "---------------------------------------"
    }

    # Save the updated XML back to file.
    $xml.Save($ConfigFile)
    Write-Host "$ConfigFile file updated successfully."
}

