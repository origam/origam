<#
.SYNOPSIS
Creates the signing and encryption certificates used by the Origam OpenIddict server.

.DESCRIPTION
Creates two self-signed RSA 4096-bit certificates valid for two years, with
DigitalSignature and KeyEncipherment key usages respectively. Exports their
private keys to password-protected signing.pfx and encryption.pfx files in the
current working directory by default.
Generates an independent password for each PFX from 32 cryptographically random
bytes, encoded as 43 URL-safe characters (letters, digits, hyphens, underscores).
Saves the passwords as plain-text signing-password.txt and
encryption-password.txt files in the current working directory, independently
of OutputDirectory. Keep these files private.


.PARAMETER OutputDirectory
Destination for the PFX files. Defaults to the current working directory.

.EXAMPLE
.\backend\scripts\create-token-certificates.ps1

.EXAMPLE
.\backend\scripts\create-token-certificates.ps1 -OutputDirectory C:\OrigamCertificates
#>
#Requires -Version 5.1
#Requires -Modules PKI

[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$OutputDirectory = (Get-Location).Path
)

$ErrorActionPreference = 'Stop'

function New-CertificatePassword {
    $randomBytes = [byte[]]::new(32)
    $randomGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $randomGenerator.GetBytes($randomBytes)
    }
    finally {
        $randomGenerator.Dispose()
    }

    return [Convert]::ToBase64String($randomBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$signingPath = Join-Path $OutputDirectory 'signing.pfx'
$encryptionPath = Join-Path $OutputDirectory 'encryption.pfx'
$signingPasswordPath = Join-Path (Get-Location).Path 'signing-password.txt'
$encryptionPasswordPath = Join-Path (Get-Location).Path 'encryption-password.txt'
foreach ($generatedFilePath in @($signingPath, $encryptionPath, $signingPasswordPath, $encryptionPasswordPath)) {
    if (Test-Path -LiteralPath $generatedFilePath) {
        throw "Refusing to overwrite existing certificate or password file: $generatedFilePath"
    }
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$signingPasswordText = New-CertificatePassword
$encryptionPasswordText = New-CertificatePassword
$signingPassword = ConvertTo-SecureString $signingPasswordText -AsPlainText -Force
$encryptionPassword = ConvertTo-SecureString $encryptionPasswordText -AsPlainText -Force

$certificateOptions = @{
    Type              = 'Custom'
    CertStoreLocation = 'Cert:\CurrentUser\My'
    KeyAlgorithm      = 'RSA'
    KeyLength         = 4096
    HashAlgorithm     = 'SHA256'
    KeyExportPolicy   = 'Exportable'
    NotAfter          = (Get-Date).AddYears(2)
}

$signingCertificate = New-SelfSignedCertificate @certificateOptions `
    -Subject 'CN=Origam Token Signing' `
    -KeyUsage DigitalSignature

$encryptionCertificate = New-SelfSignedCertificate @certificateOptions `
    -Subject 'CN=Origam Token Encryption' `
    -KeyUsage KeyEncipherment

Export-PfxCertificate -Cert $signingCertificate `
    -FilePath $signingPath `
    -Password $signingPassword `
    -CryptoAlgorithmOption AES256_SHA256 -NoClobber | Out-Null

Export-PfxCertificate -Cert $encryptionCertificate `
    -FilePath $encryptionPath `
    -Password $encryptionPassword `
    -CryptoAlgorithmOption AES256_SHA256 -NoClobber | Out-Null

$signingPasswordText |
    Out-File -LiteralPath $signingPasswordPath -Encoding utf8 -NoNewline -NoClobber

$encryptionPasswordText |
    Out-File -LiteralPath $encryptionPasswordPath -Encoding utf8 -NoNewline -NoClobber

Write-Output "Created signing certificate: $signingPath"
Write-Output "Created encryption certificate: $encryptionPath"
Write-Output "Saved signing password: $signingPasswordPath"
Write-Output "Saved encryption password: $encryptionPasswordPath"
Write-Output "Certificates expire on $($certificateOptions.NotAfter.ToString('yyyy-MM-dd'))."
