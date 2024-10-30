function Update-ConfigFromTemplate {
    param(
        [Parameter(Mandatory=$true)]
        [string]$TemplateFile,

        [Parameter(Mandatory=$true)]
        [string]$OutputFile,

        [Parameter(Mandatory=$true)]
        [hashtable]$Replacements,

        [Parameter(Mandatory=$false)]
        [boolean]$PrintResult = $false
    )

    if (-not (Test-Path $TemplateFile)) {
        throw "Template file $TemplateFile not found!"
    }

    $templateContent = Get-Content $TemplateFile -Raw
    if ([string]::IsNullOrEmpty($templateContent)) {
        throw "Template file is empty!"
    }
    Write-Host "Successfully read template file"

    foreach ($key in $Replacements.Keys) {
        if ([string]::IsNullOrEmpty($Replacements[$key])) {
            Write-Host "Warning: Value for $key is empty" -ForegroundColor Yellow
        }
        $templateContent = $templateContent -replace $key, $Replacements[$key]
    }

    Write-Host "Configuration file generation completed successfully"
    if ($PrintResult) {
        Write-Host "Final $OutputFile content:"
        Write-Host $templateContent
    }

    $templateContent | Set-Content $OutputFile
}

$ErrorActionPreference = 'Stop'
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine")

# First generate the HTTPS SSL certificate
Write-Host "Generating HTTPS SSL certificate..."
& './createSslCertificate.ps1'
if ($LASTEXITCODE -ne 0) {
    Write-Host "HTTPS SSL certificate generation failed"
    exit $LASTEXITCODE
}

# Git operations
if ($Env:gitPullOnStart -eq "true") {
    Write-Host "Git pull on start is enabled. Cloning/pulling repository..."

    # Remove existing data directory if it exists
    if (Test-Path "data") {
        Remove-Item data -Recurse -Force
    }

    # Create new data directory
    New-Item -Name "data" -ItemType Directory
    Set-Location data

    # Clone the repository
    if ($Env:gitUsername -and $Env:gitPassword -and $Env:gitUrl) {
        & git clone "https://$Env:gitUsername:$Env:gitPassword@$Env:gitUrl"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Git clone failed"
            exit 1
        }
    } else {
        Write-Host "Error: Git credentials or URL not provided"
        exit 1
    }
    Set-Location ..
} else {
    Write-Host "Git pull on start is disabled. Skipping repository clone."
}

try {
    Write-Host "Starting appsettings generation..."

    # Generate SSL certificate for jwt tokens
    Write-Host "Generating JWT SSL certificate..."
    openssl rand -base64 10 | Set-Content -NoNewline certpass
    openssl req -batch -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 728 -out serverCore.cer -quiet
    openssl pkcs12 -export -in serverCore.cer -inkey serverCore.key -passout file:certpass -out serverCore.pfx
    Write-Host "JWT SSL certificate generated"

    $JwtcertificatePassword = Get-Content .\certpass
    Write-Host "Retrieved JWT certificate password"

    $httpsPassword = Get-Content "C:\ssl\https-cert-password.txt" -ErrorAction Stop

    $replacements = @{
        "ExternalDomain" = $Env:ExternalDomain_SetOnStart
        "pathchatapp" = $Env:pathchatapp
        "certpassword" = $JwtcertificatePassword
        "chatinterval" = if ([string]::IsNullOrEmpty($Env:chatinterval)) { "0" } else { $Env:chatinterval }
        '"Kestrel": \{\}' = @"
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
    }

    Update-ConfigFromTemplate -TemplateFile ".\_appsettings.template" `
                        -OutputFile ".\appsettings.json" `
                        -Replacements $replacements `
                        -PrintResult ($Env:OrigamDockerDebug -eq "true")

} catch {
    Write-Host "Error during configuration generation: $_" -ForegroundColor Red
    throw $_
}

try {
    Write-Host "Starting OrigamSettings generation..."

#    Write-Host "Current Environment Variables:"
#    Get-ChildItem Env: | Format-Table Name, Value

    $replacements = @{
        "OrigamSettings_SchemaExtensionGuid" = $Env:OrigamSettings_SchemaExtensionGuid
        "OrigamSettings_DbHost" = $Env:OrigamSettings_DbHost
        "OrigamSettings_DbPort" = if ([string]::IsNullOrEmpty($Env:OrigamSettings_DbPort)) { "1433" } else { $Env:OrigamSettings_DbPort }
        "OrigamSettings_DbUsername" = $Env:OrigamSettings_DbUsername
        "OrigamSettings_DatabaseName" = $Env:DatabaseName
        "OrigamSettings_DbPassword" = $Env:OrigamSettings_DbPassword
        "OrigamSettings_Title" = $Env:OrigamSettings_TitleName
        "OrigamSettings_ReportDefinitionsPath" = $Env:OrigamSettings_ReportDefinitionsPath
        "OrigamSettings_RuntimeModelConfigurationPath" = $Env:OrigamSettings_RuntimeModelConfigurationPath
        "OrigamSettings_ModelName" = "data\$Env:OrigamSettings_ModelSubDirectory"
    }


    Update-ConfigFromTemplate -TemplateFile ".\_OrigamSettings.mssql.template" `
                        -OutputFile ".\OrigamSettings.config" `
                        -Replacements $replacements `
                        -PrintResult ($Env:OrigamDockerDebug -eq "true")

} catch {
    Write-Host "Error during database configuration: $_" -ForegroundColor Red
    throw $_
}

# Start the application
$env:ASPNETCORE_URLS = 'http://+:8080;https://+:443'
& dotnet Origam.Server.dll