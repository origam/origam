param(
    [string]$Configuration = "Debug Server",
    [string]$Platform = "Any CPU",
    [string]$SolutionPath = "$(Join-Path $PSScriptRoot '..' 'Origam.sln')",
    [string]$MSBuildPath
)

# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts/build-solution.ps1 -Configuration "Debug Server" -Platform "Any CPU"
# This script forces MSBuild to use the standard Build target to avoid IDE invocations that pass an empty /t:.

function Get-MSBuildPath {
    param([string]$Override)
    if ($Override -and (Test-Path $Override)) { return $Override }

    $candidates = @(
        "$Env:VSINSTALLDIR\MSBuild\Current\Bin\amd64\MSBuild.exe",
        "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\MSBuild\\Current\\Bin\\amd64\\MSBuild.exe",
        "C:\\Program Files\\Microsoft Visual Studio\\2022\\Professional\\MSBuild\\Current\\Bin\\amd64\\MSBuild.exe",
        "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\amd64\\MSBuild.exe"
    ) | Where-Object { $_ -and (Test-Path $_) }

    if ($candidates.Count -gt 0) { return $candidates[0] }

    # Fallback to dotnet msbuild if VS MSBuild is not found
    $dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue)
    if ($dotnet) { return "$($dotnet.Source) msbuild" }

    throw "MSBuild not found. Specify -MSBuildPath explicitly."
}

$msbuild = Get-MSBuildPath -Override $MSBuildPath

if (-not (Test-Path $SolutionPath)) {
    throw "Solution file not found: $SolutionPath"
}

Write-Host "Building solution: $SolutionPath" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration | Platform: $Platform" -ForegroundColor Cyan

$arguments = @(
    '"{0}"' -f $SolutionPath,
    '/t:Build',
    '/m',
    '/restore',
    '/verbosity:minimal',
    ('/p:Configuration="{0}"' -f $Configuration),
    ('/p:Platform="{0}"' -f $Platform)
)

if ($msbuild.EndsWith(' msbuild')) {
    # dotnet msbuild path case
    & $msbuild $arguments
} else {
    & "$msbuild" $arguments
}

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Build succeeded." -ForegroundColor Green
