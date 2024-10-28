$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# First generate the HTTPS SSL certificate
Write-Host "Generating HTTPS SSL certificate..."
& 'C:\ssl\createSslCertificate.ps1'
if ($LASTEXITCODE -ne 0) {
    Write-Host "HTTPS SSL certificate generation failed"
    exit $LASTEXITCODE
}

if ($Env:gitPullOnStart -eq "true") {
    Write-Host "Git pull on start is enabled. Cloning/pulling repository..."

    # Remove existing data directory if it exists
    if (Test-Path "data") {
        rm data -r -force
    }

    # Create new data directory
    New-Item -Name "data" -ItemType Directory
    cd data

    # Clone the repository
    if ($Env:gitUsername -and $Env:gitPassword -and $Env:gitUrl) {
        git.exe clone https://$Env:gitUsername:$Env:gitPassword@$Env:gitUrl
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Git clone failed"
            exit 1
        }
    } else {
        Write-Host "Error: Git credentials or URL not provided"
        exit 1
    }
    cd ..
} else {
    Write-Host "Git pull on start is disabled. Skipping repository clone."
}
# Generate SSL certificate for HTTPS
Powershell.exe -executionpolicy remotesigned -File C:\ssl\createSslCertificate.ps1

cd c:\home\origam\HTML5

try {
    Write-Host "Starting configuration file generation..."

    # Check if template exists
    if (-not (Test-Path ".\_appsettings.template")) {
        throw "Template file _appsettings.template not found!"
    }

    # Read template content
    $templateContent = Get-Content ".\_appsettings.template" -Raw
    if ([string]::IsNullOrEmpty($templateContent)) {
        throw "Template file is empty!"
    }
    Write-Host "Successfully read template file"

    # Generate SSL certificate for jwt tokens
    Write-Host "Generating JWT SSL certificate..."
    openssl.exe rand -base64 10 | Set-Content -NoNewline certpass
    openssl.exe req -batch -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 728 -out serverCore.cer
    openssl.exe pkcs12 -export -in serverCore.cer -inkey serverCore.key -passout file:certpass -out serverCore.pfx
    Write-Host "JWT SSL certificate generated"

    # Get the certificate password for JWT
    $certPass = Get-Content .\certpass
    Write-Host "Retrieved JWT certificate password"

    # Create initial appsettings.json
    $templateContent = $templateContent -replace "certpassword", $certPass
    $templateContent | Set-Content .\appsettings.json
    Write-Host "Created initial appsettings.json"

    # Add HTTPS configuration
    $httpsPassword = Get-Content "C:\ssl\https-cert-password.txt" -ErrorAction Stop
    $httpsConfig = @"
,
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://+:443",
        "Certificate": {
          "Path": "C:\\ssl\\https.pfx",
          "Password": "$httpsPassword"
        }
      },
      "Http": {
        "Url": "http://+:8080"
      }
    }
  }
"@

    # Update appsettings.json with HTTPS config
    $appSettings = Get-Content .\appsettings.json -Raw
    $appSettings = $appSettings -replace "}$", "$httpsConfig`n}"
    $appSettings | Set-Content .\appsettings.json
    Write-Host "Added HTTPS configuration"

    # Replace environment variables
    $replacements = @{
        "ExternalDomain" = $Env:ExternalDomain_SetOnStart
        "pathchatapp" = $Env:pathchatapp
        "chatinterval" = if ([string]::IsNullOrEmpty($Env:chatinterval)) { "0" } else { $Env:chatinterval }
    }

    foreach ($key in $replacements.Keys) {
        if ([string]::IsNullOrEmpty($replacements[$key])) {
            Write-Host "Warning: Environment variable for $key is empty"
        }
        $appSettings = Get-Content .\appsettings.json -Raw
        $appSettings = $appSettings -replace $key, $replacements[$key]
        $appSettings | Set-Content .\appsettings.json
        Write-Host "Replaced $key with $($replacements[$key])"
    }

    Write-Host "Configuration file generation completed successfully"
#    Write-Host "Final appsettings.json content:"
#    Write-Host $finalContent

} catch {
    Write-Host "Error during configuration generation: $_" -ForegroundColor Red
    throw $_
}

(Get-Content .\_OrigamSettings.mssql.template) -replace "OrigamSettings_SchemaExtensionGuid", $Env:OrigamSettings_SchemaExtensionGuid | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_DbHost", $Env:OrigamSettings_DbHost | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_DbPort", $Env:OrigamSettings_DbPort | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_DbUsername", $Env:OrigamSettings_DbUsername | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_DatabaseName", $Env:DatabaseName | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_DatabaseName", $Env:DatabaseName | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_DbPassword", $Env:OrigamSettings_DbPassword | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_Title", $Env:OrigamSettings_TitleName | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_Title", $Env:OrigamSettings_TitleName | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_ReportDefinitionsPath", $Env:OrigamSettings_ReportDefinitionsPath | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_RuntimeModelConfigurationPath", $Env:OrigamSettings_RuntimeModelConfigurationPath | Set-Content .\OrigamSettings.config
(Get-Content .\OrigamSettings.config) -replace "OrigamSettings_ModelName", "data\$Env:OrigamSettings_ModelSubDirectory" | Set-Content .\OrigamSettings.config

[System.Environment]::SetEnvironmentVariable('ASPNETCORE_URLS','http://+:8080')
dotnet.exe Origam.Server.dll