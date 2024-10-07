# Build OrigamSetup.msi locally
# Example of use: powershell ./prepareSetup.ps1 -PRODUCT_VERSION 100.1.0.1 -PRODUCT_NUMBER 100.1.0.1-Test

param([Parameter(Mandatory=$true)] $PRODUCT_NUMBER, [Parameter(Mandatory=$true)] $PRODUCT_VERSION)
$WIX="c:\Program Files (x86)\WiX Toolset v3.11\"

(Get-Content ArchitectSetup.wxs).replace("@product_version@", "$($PRODUCT_VERSION)").replace("@branch@", "$($PRODUCT_NUMBER)") | Set-Content ArchitectSetup-$($PRODUCT_VERSION).wxs

& "$($WIX)bin\candle.exe" -ext WixSqlExtension -ext WixUtilExtension ArchitectSetup-$($PRODUCT_VERSION).wxs
& "$($WIX)bin\light.exe" -ext WixSqlExtension -ext WixUtilExtension -ext WixNetFxExtension -sice:ICE20 -cultures:en-us -loc resources.en-us.wxl -out origam-architect.msi ArchitectSetup-$($PRODUCT_VERSION).wixobj
