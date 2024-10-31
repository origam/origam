$certPath = "C:\ssl"
$keyFile = "$certPath\https.key"
$csrFile = "$certPath\https.csr"
$crtFile = "$certPath\https.crt"
$pfxFile = "$certPath\https.pfx"
$certPassword = "NotAVerySecurePasswordSinceThisIsJustADevelopmnetCertificate"

# Check if files already exist
if ((Test-Path $pfxFile)) {
    Write-Host "SSL certificate files already exist. No files will be created."
    exit 0
}

try {
    # Create CSR with default values
    openssl req -new -newkey rsa:2048 -nodes -keyout $keyFile -out $csrFile -subj "/C=US/ST=State/L=City/O=Organization/OU=OrganizationalUnit/CN=localhost" -quiet

    # Generate self-signed certificate
    openssl x509 -req -days 365 -in $csrFile -signkey $keyFile -out $crtFile

    # Convert to PFX for Windows/ASP.NET Core
    openssl pkcs12 -export -out $pfxFile -inkey $keyFile -in $crtFile -password pass:$certPassword

    # Store the password
    $certPassword | Out-File "$certPath\https-cert-password.txt"

    Write-Host "Development self-signed SSL certificate files created."
}
catch {
    Write-Host "An error occurred during certificate generation: $_"
    exit 1
}