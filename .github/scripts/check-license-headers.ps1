# Move to repo root
$RepoRoot = Resolve-Path "$PSScriptRoot/../.."
Set-Location $RepoRoot
Write-Host "📁 Scanning from: $RepoRoot"

# Define common license regex (without region)
$LicensePatternOther = @"
\/\*[\s\n]*Copyright 2005 - 20\d\d Advantage Solutions, s\. r\. o\.[\s\n]*This file is part of ORIGAM \(http:\/\/www\.origam\.org\)\.[\s\n]*ORIGAM is free software: you can redistribute it and\/or modify[\s\n]*it under the terms of the GNU General Public License as published by[\s\n]*the Free Software Foundation, either version 3 of the License, or[\s\n]*\(at your option\) any later version\.[\s\n]*ORIGAM is distributed in the hope that it will be useful,[\s\n]*but WITHOUT ANY WARRANTY; without even the implied warranty of[\s\n]*MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE\. See the[\s\n]*GNU General Public License for more details\.[\s\n]*You should have received a copy of the GNU General Public License[\s\n]*along with ORIGAM\. If not, see <http:\/\/www\.gnu\.org\/licenses\/>\.[\s\n]*\*\/
"@ -replace "`r`n", "`n"

# Extend for .cs with region tags
$LicensePatternCS = "\#region license[\s\w]*$LicensePatternOther[\s\w]*\#endregion"

# Escape any excess newlines
$LicenseRegexCS = [regex]::new($LicensePatternCS, "IgnoreCase, Multiline")
$LicenseRegexOther = [regex]::new($LicensePatternOther, "IgnoreCase, Multiline")

$global:ErrorFiles = @()

function Check-LicenseHeader($file, $regex) {
    $content = [string](Get-Content $file -Raw) -replace "`r`n", "`n"
    if (-not $regex.IsMatch($content)) {
        $global:ErrorFiles += $file
    }
}

$FilesToCheck = Get-ChildItem -Recurse -File |
    Where-Object { $_.Extension -in ".cs", ".ts", ".tsx", ".css", ".scss" } |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|node_modules|dist)\\' }

foreach ($file in $FilesToCheck) {
    if ($file.Extension -eq ".cs") {
        Check-LicenseHeader $file.FullName $LicenseRegexCS
    } else {
        Check-LicenseHeader $file.FullName $LicenseRegexOther
    }
}

if ($global:ErrorFiles.Count -gt 0) {
    Write-Host "`n❌ The following files are missing the correct license header:`n"
    $global:ErrorFiles | ForEach-Object { Write-Host " - $_" }
    exit 1
} else {
    Write-Host "✅ All source files have the correct license header."
}
