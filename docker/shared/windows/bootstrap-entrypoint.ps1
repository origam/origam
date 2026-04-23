# Dot-sourced by EntryPoint.ps1. Prepares project data and env vars when
# ORIGAM_PROJECT_BOOTSTRAP=true, then returns control to the caller.

if ($env:ORIGAM_PROJECT_BOOTSTRAP -ne 'true') {
    return
}

if (-not $env:PROJECT_NAME) {
    Write-Error 'PROJECT_NAME is required when ORIGAM_PROJECT_BOOTSTRAP=true'
    exit 1
}

$envFile = "C:\model-src\$($env:PROJECT_NAME)_Environments.env"

Write-Host "Waiting for $($env:PROJECT_NAME) to be generated..."
while (-not (Test-Path $envFile)) { Start-Sleep -Seconds 1 }

Write-Host 'Linking project data...'
$dataPath = 'C:\home\origam\projectData'
New-Item -ItemType Directory -Path $dataPath -Force | Out-Null

function New-OrigamLink {
    param([string]$Path, [string]$Target)
    if (Test-Path $Path) { Remove-Item -Recurse -Force $Path }
    New-Item -ItemType SymbolicLink -Path $Path -Target $Target | Out-Null
}

New-OrigamLink -Path "$dataPath\model" -Target 'C:\model-src\model'
if (Test-Path 'C:\model-src\customAssets') {
    New-OrigamLink -Path "$dataPath\customAssets" -Target 'C:\model-src\customAssets'
}

Get-Content $envFile | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith('#')) {
        $kv = $line -split '=', 2
        if ($kv.Length -eq 2) {
            [Environment]::SetEnvironmentVariable($kv[0].Trim(), $kv[1].Trim(), 'Process')
        }
    }
}

[Environment]::SetEnvironmentVariable('CustomAssetsConfig__PathToCustomAssetsFolder', "$dataPath\customAssets", 'Process')
