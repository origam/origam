# createSslCertificate.ps1
$certPath = "C:\ssl"
$keyFile = "$certPath\server.key"
$csrFile = "$certPath\server.csr"
$crtFile = "$certPath\server.crt"

# Check if files already exist
if ((Test-Path $keyFile) -or (Test-Path $csrFile) -or (Test-Path $crtFile)) {
    Write-Host "SSL certificate files already exist. No files will be created."
    exit 0
}

$errorOccurred = $false

try {
    # Generate private key
    openssl genrsa -out $keyFile 2048
    if (-not $?) {
        Write-Host "Error generating private key"
        $errorOccurred = $true
    }

    # Create CSR with default values
    openssl req -new -key $keyFile -out $csrFile -subj "/C=US/ST=State/L=City/O=Organization/OU=OrganizationalUnit/CN=example.com"
    if (-not $?) {
        Write-Host "Error creating CSR"
        $errorOccurred = $true
    }

    # Generate self-signed certificate
    openssl x509 -req -days 365 -in $csrFile -signkey $keyFile -out $crtFile
    if (-not $?) {
        Write-Host "Error generating self-signed certificate"
        $errorOccurred = $true
    }

    if (-not $errorOccurred) {
        Write-Host "Development self-signed SSL certificate files created."
    }
}
catch {
    Write-Host "An error occurred during certificate generation: $_"
    exit 1
}