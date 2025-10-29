function Fill-ConfigFromTemplate
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$TemplateFile,

        [Parameter(Mandatory = $true)]
        [string]$OutputFile,

        [Parameter(Mandatory = $true)]
        [hashtable]$Replacements,

        [Parameter(Mandatory = $false)]
        [boolean]$PrintResult = $false
    )

    if (-not (Test-Path $TemplateFile))
    {
        throw "Template file $TemplateFile not found!"
    }

    $templateContent = Get-Content $TemplateFile -Raw
    if ( [string]::IsNullOrEmpty($templateContent))
    {
        throw "Template file is empty!"
    }
    Write-Host "Successfully read template file"

    foreach ($key in $Replacements.Keys)
    {
        if ( [string]::IsNullOrEmpty($Replacements[$key]))
        {
            Write-Host "Warning: Value for $key is empty" -ForegroundColor Yellow
        }
        $templateContent = $templateContent -replace $key, $Replacements[$key]
    }

    Write-Host "Configuration file generation completed successfully"
    if ($PrintResult)
    {
        Write-Host "Final $OutputFile content:"
        Write-Host $templateContent
    }

    $templateContent | Set-Content $OutputFile
}

function Initialize-OrigamSettingsConfig {
    try
    {
        Write-Host "Starting OrigamSettings generation..."
        if (-not $env:OrigamSettings__ModelSourceControlLocation)
        {
            $env:OrigamSettings__ModelSourceControlLocation = "C:\home\origam\projectData\model"
        }
        Copy-Item -Path "..\_OrigamSettings.template" -Destination "OrigamSettings.config"
        Fill-OrigamSettingsConfig -ConfigFile "OrigamSettings.config" -DatabaseType $env:DatabaseType
    }
    catch
    {
        Write-Host "Error during database configuration: $_" -ForegroundColor Red
        throw $_
    }
}

 # Helper to set or create a simple text node at a given xpath
function Set-OrCreateNode([xml]$xmlDoc, [string]$parentXpath, [string]$nodeName, [string]$value) {
    $node = $xmlDoc.SelectSingleNode("$parentXpath/$nodeName")
    if ($node) {
        $node.InnerText = $value
    } else {
        $parent = $xmlDoc.SelectSingleNode($parentXpath)
        if ($parent) {
            $newNode = $xmlDoc.CreateElement($nodeName)
            $newNode.InnerText = $value
            $parent.AppendChild($newNode) | Out-Null
        }
    }
}

function Fill-OrigamSettingsConfig {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConfigFile,

        [Parameter(Mandatory = $true)]
        [string]$DatabaseType
    )

    # Check if file exists.
    if (-not (Test-Path $ConfigFile)) {
        Write-Error "Error: File '$ConfigFile' not found."
        exit 1
    }

    # Define the required environment variables.
    $requiredVars = @(
        "OrigamSettings__DataConnectionString",
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

    # Compose connection string + data service types
    switch ($DatabaseType) {
        "mssql" {
            $schemaDataService  = "Origam.DA.Service.MsSqlDataService, Origam.DA.Service"
            $dataDataService    = "Origam.DA.Service.MsSqlDataService, Origam.DA.Service"
        }
        "postgresql" {
            $schemaDataService  = "Origam.DA.Service.PgSqlDataService, Origam.DA.Service"
            $dataDataService    = "Origam.DA.Service.PgSqlDataService, Origam.DA.Service"
        }
        default {
            Write-Error "Unsupported or missing DatabaseType. Use: mssql or postgresql."
            exit 1
        }
    }
    # Update/create SchemaDataService, DataDataService
    Set-OrCreateNode -xmlDoc $xml -parentXpath $origamSettingNodeXpath -nodeName "SchemaDataService"  -value $schemaDataService
    Set-OrCreateNode -xmlDoc $xml -parentXpath $origamSettingNodeXpath -nodeName "DataDataService"    -value $dataDataService

    # Iterate through environment variables starting with 'OrigamSettings__'
    # excluding those starting with 'OrigamSettings__Database'
    $envVars = [System.Environment]::GetEnvironmentVariables().GetEnumerator() |
        Where-Object { $_.Key -like "OrigamSettings__*" }

    foreach ($envVar in $envVars) {
        $key = $envVar.Key
        $value = $envVar.Value
        
        # Strip surrounding quotes if present
        if ($value -match '^".*"$') {
            $value = $value.Substring(1, $value.Length - 2)
        }
        
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
    $xml.Save((Join-Path (Get-Location) $ConfigFile))
    Write-Host "$ConfigFile file updated successfully."
}

