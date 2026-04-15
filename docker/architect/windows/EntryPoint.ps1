. "./Utils.ps1"

$ErrorActionPreference = 'Stop'
$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine")

Write-Host "Origam Architect Server (Windows) starting..."

$projectDataPath = "C:\home\origam\projectData"
if (-not (Test-Path "$projectDataPath\model")) {
    Write-Host "Error: No model found at $projectDataPath\model. Check your volume mount." -ForegroundColor Red
    exit 1
}

Set-Location C:\home\origam\Architect
Initialize-OrigamSettingsConfig

$env:ASPNETCORE_URLS = "http://+:8081"
Write-Host "Starting Origam.Architect.Server on port 8081..."
& dotnet Origam.Architect.Server.dll
