<#
.SYNOPSIS
Creates the signing and encryption certificates used by the Origam OpenIddict server.

.DESCRIPTION
Creates two self-signed RSA 4096-bit certificates valid for two years, with
DigitalSignature and KeyEncipherment key usages respectively. Creates the keys
and certificates in memory and saves password-protected signing.pfx and
encryption.pfx files in the current working directory by default. Does not add
certificates or persist private keys in an OS key store.
Generates an independent password for each PFX from 32 cryptographically random
bytes, encoded as 43 URL-safe characters (letters, digits, hyphens, underscores).
Saves the passwords as plain-text signing-password.txt and
encryption-password.txt files in the current working directory, independently
of OutputDirectory. Keep these files private.
Requires Windows PowerShell 5.1 with .NET Framework 4.7.2 or later, or PowerShell
7 on Windows.

.PARAMETER OutputDirectory
Destination for the PFX files. Defaults to the current working directory.

.EXAMPLE
.\backend\scripts\create-token-certificates.ps1

.EXAMPLE
.\backend\scripts\create-token-certificates.ps1 -OutputDirectory C:\OrigamCertificates
#>
#Requires -Version 5.1

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

function New-TokenCertificateFile {
    param(
        [string]$Subject,
        [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]$KeyUsage,
        [DateTimeOffset]$NotAfter,
        [string]$Path,
        [string]$Password
    )

    # RSACng creates an ephemeral key; neither the key nor certificate enters a store.
    $key = [System.Security.Cryptography.RSACng]::new(4096)
    try {
        $request = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
            $Subject,
            $key,
            [System.Security.Cryptography.HashAlgorithmName]::SHA256,
            [System.Security.Cryptography.RSASignaturePadding]::Pkcs1
        )
        $request.CertificateExtensions.Add(
            [System.Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new($KeyUsage, $true)
        )
        $certificate = $request.CreateSelfSigned([DateTimeOffset]::UtcNow.AddMinutes(-5), $NotAfter)
        try {
            $contents = $certificate.Export(
                [System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx,
                $Password
            )
            $stream = [System.IO.File]::Open(
                $Path,
                [System.IO.FileMode]::CreateNew,
                [System.IO.FileAccess]::Write,
                [System.IO.FileShare]::None
            )
            try {
                $stream.Write($contents, 0, $contents.Length)
            }
            finally {
                $stream.Dispose()
            }
        }
        finally {
            $certificate.Dispose()
        }
    }
    finally {
        $key.Dispose()
    }
}

$certificateOutputDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$signingPath = Join-Path $certificateOutputDirectory 'signing.pfx'
$encryptionPath = Join-Path $certificateOutputDirectory 'encryption.pfx'
$signingPasswordPath = Join-Path (Get-Location).Path 'signing-password.txt'
$encryptionPasswordPath = Join-Path (Get-Location).Path 'encryption-password.txt'
foreach ($generatedFilePath in @($signingPath, $encryptionPath, $signingPasswordPath, $encryptionPasswordPath)) {
    if (Test-Path -LiteralPath $generatedFilePath) {
        throw "Refusing to overwrite existing certificate or password file: $generatedFilePath"
    }
}

New-Item -ItemType Directory -Path $certificateOutputDirectory -Force | Out-Null

$signingPasswordText = New-CertificatePassword
$encryptionPasswordText = New-CertificatePassword
$notAfter = [DateTimeOffset]::UtcNow.AddYears(2)

New-TokenCertificateFile -Subject 'CN=Origam Token Signing' `
    -KeyUsage DigitalSignature -NotAfter $notAfter `
    -Path $signingPath -Password $signingPasswordText

New-TokenCertificateFile -Subject 'CN=Origam Token Encryption' `
    -KeyUsage KeyEncipherment -NotAfter $notAfter `
    -Path $encryptionPath -Password $encryptionPasswordText

$signingPasswordText |
    Out-File -LiteralPath $signingPasswordPath -Encoding utf8 -NoNewline -NoClobber

$encryptionPasswordText |
    Out-File -LiteralPath $encryptionPasswordPath -Encoding utf8 -NoNewline -NoClobber

Write-Output "Created signing certificate: $signingPath"
Write-Output "Created encryption certificate: $encryptionPath"
Write-Output "Saved signing password: $signingPasswordPath"
Write-Output "Saved encryption password: $encryptionPasswordPath"
Write-Output "Certificates expire on $($notAfter.ToString('yyyy-MM-dd'))."
