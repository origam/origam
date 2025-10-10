# Origam.Composer in Docker

This `Dockerfile` allows running `Origam.Composer` in a Docker container. The database runs separately on the host machine or in another container.

## Building the Docker image

Change to the backend root directory
```cmd
cd C:\Repos\origam\backend
```

Build the Docker image
```cmd
docker build -f Origam.Composer\Dockerfile -t origam-composer:latest .
```

## Running the container

```cmd
docker run --rm ^
  --add-host=host.docker.internal:host-gateway ^
  -v C:\OrigamProjects\MyOrigamApp:/origam/MyOrigamApp ^
  origam-composer create ^
  --commands-only-linux ^
  --commands-platform linux ^
  --db-type mssql ^
  --db-host host.docker.internal ^
  --db-port 1433 ^
  --db-name MyOrigamApp ^
  --db-username sa ^
  --db-password 3NHjcSMajQejgrpKGAD8egxNfEc7 ^
  --p-name MyOrigamApp ^
  --p-folder /origam/MyOrigamApp ^
  --p-admin-username admin ^
  --p-admin-password 5axg1zr8 ^
  --p-admin-email john.doe@example.com ^
  --p-docker-image-linux origam/server:2025.9.alpha.3984.linux ^
  --p-docker-image-win origam/server:2025.9.alpha.3984.win ^
  --arch-docker-image-linux origam/architect:2025.9.alpha.3984.linux ^
  --arch-docker-image-win origam/architect:2025.9.alpha.3984.win ^
  --arch-port 8081 ^
  --git-enabled ^
  --git-user "Origam Dev" ^
  --git-email no-reply@origam.com
```