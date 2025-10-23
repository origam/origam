$ErrorActionPreference = 'Stop'

# Create installation directory
$installDir = "C:\Program Files\CustomApps"
New-Item -ItemType Directory -Force -Path $installDir | Out-Null

# Install MinGit (smaller version of Git)
Write-Host "Installing MinGit..."
$gitUrl = "https://github.com/git-for-windows/git/releases/download/v2.47.0.windows.2/MinGit-2.47.0.2-64-bit.zip"
$gitZip = "$env:TEMP\git.zip"
$gitInstallPath = "$installDir\Git"

try {
    Invoke-WebRequest -Uri $gitUrl -OutFile $gitZip
    Expand-Archive -Path $gitZip -DestinationPath $gitInstallPath -Force

    # Add Git to PATH
    $env:PATH = "$gitInstallPath\cmd;$env:PATH"
    [Environment]::SetEnvironmentVariable('PATH', $env:PATH, [EnvironmentVariableTarget]::Machine)

    Write-Host "Git installation completed"
} catch {
    Write-Host "Error installing Git: $_"
    throw
} finally {
    Remove-Item $gitZip -ErrorAction SilentlyContinue
}

# Install portable OpenSSL
Write-Host "Installing portable OpenSSL..."
$opensslUrl = "https://www.firedaemon.com/download-firedaemon-openssl-3-5-zip"
$opensslZip = "$env:TEMP\openssl.zip"
$opensslPath = "$installDir\openssl-3\x64\bin"
$opensslInstallFolder = "$installDir\openssl-3"

try {
    Invoke-WebRequest -Uri $opensslUrl -OutFile $opensslZip
    New-Item -ItemType Directory -Force -Path $opensslInstallFolder | Out-Null
    Expand-Archive -Path $opensslZip -DestinationPath $opensslInstallFolder -Force

    # Add OpenSSL to PATH
    $env:PATH = "$opensslPath;$env:PATH"
    [Environment]::SetEnvironmentVariable('PATH', $env:PATH, [EnvironmentVariableTarget]::Machine)

    # Copy OpenSSL config to expected location
    $configSourcePath = "$installDir\openssl-3\ssl\openssl.cnf"
    $configDestDir = "C:\Program Files\Common Files\FireDaemon SSL 3"
    New-Item -Path $configDestDir -ItemType Directory -Force | Out-Null
    Copy-Item -Path $configSourcePath -Destination "$configDestDir\openssl.cnf" -Force

    Write-Host "OpenSSL installation completed"
} catch {
    Write-Host "Error installing OpenSSL: $_"
    throw
} finally {
    Remove-Item $opensslZip -ErrorAction SilentlyContinue
}

# Verify installations
Write-Host "Verifying installations..."
try {
    $gitVersion = git --version
    Write-Host "Git version: $gitVersion"

    $opensslVersion = openssl version
    Write-Host "OpenSSL version: $opensslVersion"
} catch {
    Write-Host "Verification failed: $_"
    throw
}

# Install Node.js (Windows, PowerShell version)
$env:NODE_VERSION = "22.21.0"
$env:NODE_OPTIONS = "--max-old-space-size=4096"

Write-Host "Downloading Node.js version $env:NODE_VERSION..."

$nodeUrl = "https://nodejs.org/dist/v$($env:NODE_VERSION)/node-v$($env:NODE_VERSION)-win-x64.zip"
Invoke-WebRequest -Uri $nodeUrl -OutFile "node.zip"

Expand-Archive -Path "node.zip" -DestinationPath "C:\" -Force
Remove-Item "node.zip"

Rename-Item -Path "C:\node-v$($env:NODE_VERSION)-win-x64" -NewName "C:\node"

# Update PATH system-wide
$nodePaths = "C:\node", "C:\node\bin", "C:\node\node_modules\npm\bin"
$existingPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine)
$newPath = "$existingPath;" + ($nodePaths -join ";")
[Environment]::SetEnvironmentVariable("Path", $newPath, [EnvironmentVariableTarget]::Machine)

Write-Host "Installation completed successfully."
