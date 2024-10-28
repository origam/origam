# Function to refresh PATH
function Update-PathEnvironment {
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
}

# Create installation directory
$installDir = "C:\ProgramFiles\CustomApps"
New-Item -ItemType Directory -Force -Path $installDir | Out-Null

# Install Git
Write-Host "Installing Git..."
$gitUrl = "https://github.com/git-for-windows/git/releases/download/v2.42.0.windows.2/MinGit-2.42.0.2-64-bit.zip"
$gitZip = "$env:TEMP\git.zip"
$gitInstallPath = "$installDir\Git"
try {
    Invoke-WebRequest -Uri $gitUrl -OutFile $gitZip
    Expand-Archive -Path $gitZip -DestinationPath $gitInstallPath -Force
} catch {
    Write-Host "Error installing Git: $_"
    exit 1
} finally {
    Remove-Item $gitZip -ErrorAction SilentlyContinue
}

# Install Visual C++ Redistributable
Write-Host "Installing Visual C++ 2022 Redistributable..."
$vcRedistUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe"
$vcRedistInstaller = "$env:TEMP\vc_redist.x64.exe"

try {
    Invoke-WebRequest -Uri $vcRedistUrl -OutFile $vcRedistInstaller
    $process = Start-Process $vcRedistInstaller -ArgumentList "/install /quiet /norestart" -Wait -NoNewWindow -PassThru
    if ($process.ExitCode -ne 0) {
        throw "VC++ Redistributable installation failed with exit code $($process.ExitCode)"
    }
} catch {
    Write-Host "Error installing VC++ Redistributable: $_"
    exit 1
} finally {
    Remove-Item $vcRedistInstaller -ErrorAction SilentlyContinue
}

# Install OpenSSL
Write-Host "Installing OpenSSL Light..."
$opensslUrl = "https://slproweb.com/download/Win64OpenSSL_Light-3_4_0.exe"
$opensslInstaller = "$env:TEMP\openssl-installer.exe"

try {
    Invoke-WebRequest -Uri $opensslUrl -OutFile $opensslInstaller
    $process = Start-Process $opensslInstaller -ArgumentList "/VERYSILENT /NORESTART" -Wait -NoNewWindow -PassThru
    if ($process.ExitCode -ne 0) {
        throw "OpenSSL installation failed with exit code $($process.ExitCode)"
    }
} catch {
    Write-Host "Error installing OpenSSL: $_"
    exit 1
} finally {
    Remove-Item $opensslInstaller -ErrorAction SilentlyContinue
}

# Update PATH
$paths = @(
    "$installDir\Git\cmd",
    "C:\Program Files\OpenSSL-Win64\bin",
    [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine)
) | Select-Object -Unique # Avoid duplicates

$newPath = $paths -join ";"
[Environment]::SetEnvironmentVariable("Path", $newPath, [EnvironmentVariableTarget]::Machine)
Update-PathEnvironment

# Verify installations with error checking
Write-Host "Verifying installations..."
try {
    $gitVersion = (git --version) | Out-String
    Write-Host "Git version: $gitVersion"
} catch {
    Write-Host "Error verifying Git installation: $_"
    exit 1
}

try {
    $opensslVersion = (openssl version) | Out-String
    Write-Host "OpenSSL version: $opensslVersion"
} catch {
    Write-Host "Error verifying OpenSSL installation: $_"
    exit 1
}

Write-Host "Installation completed successfully."w