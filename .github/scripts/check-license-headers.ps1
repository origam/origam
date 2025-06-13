# Ensure we scan from repo root
$RepoRoot = Resolve-Path "$PSScriptRoot/../.."
Set-Location $RepoRoot
Write-Host "📁 Scanning from: $RepoRoot"

$CurrentYear = (Get-Date).Year
$LicenseTextCS = @"
#region license
/*
Copyright 2005 - $CurrentYear Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
"@

$LicenseTextOther = $LicenseTextCS -replace "#region license`r?`n", "" -replace "`r?`n#endregion", ""
$ErrorFiles = @()

function Check-LicenseHeader($file, $expectedHeader) {
    $fileContent = [string](Get-Content $file -Raw)
    $normalizedHeader = $expectedHeader -replace "`r`n", "`n"
    $normalizedFile = $fileContent -replace "`r`n", "`n"

    if (-not $normalizedFile.StartsWith($normalizedHeader + "`n")) {
        $global:ErrorFiles += $file
    }
}

$FilesToCheck = Get-ChildItem -Recurse -File |
    Where-Object { $_.Extension -in ".cs", ".ts", ".tsx", ".css", ".scss" } |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|node_modules|dist)\\' }

Write-Host "`nScanning files:"
$FilesToCheck | ForEach-Object { Write-Host " - $_.FullName" }

foreach ($file in $FilesToCheck) {
    if ($file.Extension -eq ".cs") {
        Check-LicenseHeader $file.FullName $LicenseTextCS
    } else {
        Check-LicenseHeader $file.FullName $LicenseTextOther
    }
}

if ($ErrorFiles.Count -gt 0) {
    Write-Host "`n❌ The following files are missing the correct license header:`n"
    $ErrorFiles | ForEach-Object { Write-Host " - $_" }
    exit 1
} else {
    Write-Host "✅ All source files have the correct license header."
}
