$RepoRoot = Resolve-Path "$PSScriptRoot/../.."
Set-Location $RepoRoot
Write-Host "📁 Scanning from: $RepoRoot"

# Define exclusions
$ExcludeDirs = @(
    '\bin\',
    '\obj\',
    '\node_modules\',
    '\dist\',
    '\.git\',
    '\MIMEParser\',
    '\frontend-html\src\fonts\',
    '\Origam.Server\assets\identity\css\',
    '\Origam.Server\IdentityServerGui\',
    '\Origam.Windows.Editor\',
    '\ScheduleTimer\'
)

$ExcludeFiles = @(
    'AssemblyInfo.cs',
    'vite.config.ts',
    'Designer.cs',
    'Reference.cs',
    'DesignerTransactionImpl.cs',
    'font-ibm-plex-sans.css'
)

# Define license patterns
$LicensePatternOther = @"
/\*[\s\n]*Copyright 2005 - 20\d\d Advantage Solutions, s\. r\. o\.[\s\n]*This file is part of ORIGAM \(http:\/\/www\.origam\.org\)\.[\s\n]*ORIGAM is free software: you can redistribute it and\/or modify[\s\n]*it under the terms of the GNU General Public License as published by[\s\n]*the Free Software Foundation, either version 3 of the License, or[\s\n]*\(at your option\) any later version\.[\s\n]*ORIGAM is distributed in the hope that it will be useful,[\s\n]*but WITHOUT ANY WARRANTY; without even the implied warranty of[\s\n]*MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE\. See the[\s\n]*GNU General Public License for more details\.[\s\n]*You should have received a copy of the GNU General Public License[\s\n]*along with ORIGAM\. If not, see <http:\/\/www\.gnu\.org\/licenses\/>\.[\s\n]*\*\/
"@ -replace "`r`n", "`n"

# derive Razor (@* *@) version of the same pattern for .cshtml ---
$LicensePatternOtherTrimmed = $LicensePatternOther.Trim()
# remove the leading "/*" (4 chars: '\/\*') and trailing "*/" (4 chars: '\*\/')
$LicenseBodyPattern = $LicensePatternOtherTrimmed.Substring(4, $LicensePatternOtherTrimmed.Length - 8)
# wrap the same body into Razor comment @* ... *@
$LicensePatternCshtml = "@\*" + $LicenseBodyPattern + "\*@"

$LicensePatternCS = "\#region license[\s\w]*$LicensePatternOther[\s\w]*\#endregion"
$LicenseRegexCS = [regex]::new($LicensePatternCS, "IgnoreCase, Multiline")
$LicenseRegexOther = [regex]::new($LicensePatternOther, "IgnoreCase, Multiline")
$LicenseRegexCshtml = [regex]::new($LicensePatternCshtml, "IgnoreCase, Multiline")

$global:ErrorFiles = @()

function Check-LicenseHeader($file, $regex)
{
    $content = [string](Get-Content $file -Raw) -replace "`r`n", "`n"
    if (-not $regex.IsMatch($content))
    {
        $global:ErrorFiles += $file
    }
}

function Is-Excluded($filePath)
{
    $normalizedPath = $filePath.ToLower() -replace '/', '\'
    foreach ($pattern in $ExcludeDirs)
    {
        if ($normalizedPath -like "*$($pattern.ToLower() )*")
        {
            return $true
        }
    }
    foreach ($filename in $ExcludeFiles)
    {
        if ($normalizedPath -like "*$($filename.ToLower() )*")
        {
            return $true
        }
    }
    return $false
}

$FilesToCheck = Get-ChildItem -Recurse -File |
        Where-Object { $_.Extension -in ".cs", ".ts", ".tsx", ".css", ".scss", ".cshtml" } |
        Where-Object { -not (Is-Excluded $_.FullName) }

foreach ($file in $FilesToCheck)
{
    if ($file.Extension -eq ".cs")
    {
        Check-LicenseHeader $file.FullName $LicenseRegexCS
    }
    elseif ($file.Extension -eq ".cshtml")
    {
        Check-LicenseHeader $file.FullName $LicenseRegexCshtml
    }
    else
    {
        Check-LicenseHeader $file.FullName $LicenseRegexOther
    }
}

if ($global:ErrorFiles.Count -gt 0)
{
    Write-Host "`n❌ The following files are missing the correct license header:`n"
    $global:ErrorFiles | ForEach-Object { Write-Host " - $_" }
    Write-Host "`nIf you think the listed files should not have the license header, modify the ExcludeDirs and/or ExcludeFiles variables in the check-license-headers.ps1 script`n"
    exit 1
}
else
{
    Write-Host "✅ All source files have the correct license header."
}
