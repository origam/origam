#curl https://dot.net/v1/dotnet-install.ps1 -O dotnet-install.ps1
#.\dotnet-install.ps1 -Channel 6.0 -Runtime aspnetcore

cd c:\home\origam\HTML5

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


$httpsPassword = Get-Content "C:\ssl\https-cert-password.txt"
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


# Insert the HTTPS configuration into appsettings.json before the closing brace
(Get-Content .\appsettings.json) -replace "}$", "$httpsConfig`n}" | Set-Content .\appsettings.json

# Generate SSL certificate for jwt tokens
openssl.exe rand -base64 10 | Set-Content -NoNewline certpass
openssl.exe req -batch -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 728 -out serverCore.cer
openssl.exe pkcs12 -export -in serverCore.cer -inkey serverCore.key -passout file:certpass -out serverCore.pfx

(Get-Content .\_appsettings.template) -replace "certpassword", (Get-Content .\certpass) | Set-Content .\appsettings.json


(Get-Content .\appsettings.json) -replace "ExternalDomain", $Env:ExternalDomain_SetOnStart | Set-Content .\appsettings.json
(Get-Content .\appsettings.json) -replace "pathchatapp", $Env:pathchatapp | Set-Content .\appsettings.json
(Get-Content .\appsettings.json) -replace "chatinterval", $Env:chatinterval | Set-Content .\appsettings.json

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