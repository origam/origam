
. "./Utils.ps1"
$ErrorActionPreference = 'Stop'
$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine")
$projectDataPath = "C:\home\origam\projectData"

# First generate the HTTPS SSL certificate
Write-Host "Generating HTTPS SSL certificate..."
& './CreateSslCertificate.ps1'
if ($LASTEXITCODE -ne 0)
{
    Write-Host "HTTPS SSL certificate generation failed"
    exit $LASTEXITCODE
}

# Git operations
if ($Env:gitPullOnStart -eq "true")
{
    Write-Host "Git pull on start is enabled. Cloning/pulling repository..."

    # Remove existing data directory if it exists
    if (Test-Path $projectDataPath)
    {
        Remove-Item $projectDataPath -Recurse -Force
    }

    # Create new data directory
    New-Item -Name $projectDataPath -ItemType Directory
    Set-Location $projectDataPath

    # Clone the repository
    if ($Env:gitUsername -and $Env:gitPassword -and $Env:gitUrl)
    {
        & git clone "https://$($env:gitUsername):$($env:gitPassword)@$($env:gitUrl)"
        if ($LASTEXITCODE -ne 0)
        {
            Write-Host "Error: Git clone failed"
            exit 1
        }
    }
    else
    {
        Write-Host "Error: Git credentials or URL not provided"
        exit 1
    }
    Set-Location C:\home\origam\HTML5
}
else
{
    Write-Host "Git pull on start is disabled. Skipping repository clone."
}

try
{
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
        "chatinterval" = if ( [string]::IsNullOrEmpty($Env:chatinterval))
        {
            "0"
        }
        else
        {
            $Env:chatinterval
        }
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

    Fill-ConfigFromTemplate -TemplateFile ".\_appsettings.template" `
                        -OutputFile ".\appsettings.json" `
                        -Replacements $replacements `
                        -PrintResult ($Env:OrigamDockerDebug -eq "true")
}
catch
{
    Write-Host "Error during configuration generation: $_" -ForegroundColor Red
    throw $_
}

try
{
    Write-Host "Starting OrigamSettings generation..."
    if (-not $env:OrigamSettings__ModelSourceControlLocation)
    {
        $env:OrigamSettings__ModelSourceControlLocation = "C:\home\origam\projectData\model"
    }
    Copy-Item -Path "_OrigamSettings.mssql.template" -Destination "OrigamSettings.config"
    Fill-OrigamSettingsConfig -ConfigFile "OrigamSettings.config"
}
catch
{
    Write-Host "Error during database configuration: $_" -ForegroundColor Red
    throw $_
}

# Start the application
$env:ASPNETCORE_URLS = 'http://+:8080;https://+:443'
& dotnet Origam.Server.dll
