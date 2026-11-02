<#
.SYNOPSIS
Prints the standard ORIGAM license header with the current year.
.EXAMPLE
./.github/scripts/get-license-header.ps1 -Extension cs
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('cs', 'ts', 'tsx', 'css', 'scss', 'cshtml')]
    [string]$Extension
)

$Year = (Get-Date).Year
$Body = @"
Copyright 2005 - $Year Advantage Solutions, s. r. o.

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
"@

switch ($Extension)
{
    'cs' { "#region license`n`n/*`n$Body`n*/`n`n#endregion" }
    'cshtml' { "@*`n$Body`n*@" }
    default { "/*`n$Body`n*/" }
}
