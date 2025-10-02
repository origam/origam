# Origam.Composer

## Install database (optional)
You can use docker to install MS SQL database:
```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=3NHjcSMajQejgrpKGAD8egxNfEc7" -p 1433:1433 --name mssql2022 -d mcr.microsoft.com/mssql/server:2022-latest
```

## Building
```
dotnet publish ./Origam.Composer/Origam.Composer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Usage

Parameter `--commands-platform` specifies the platform for which the commands are generated. It can be either `linux` (bash scripts) or `windows` (cmd scripts).

Both docker containers for Linux and Windows will always be generated.

All docker images are available on ORIGAM Github: https://github.com/origam/origam/releases

Example for `MS SQL`:
```
Origam.Composer.exe create 
--commands-only-linux
--commands-platform linux

--db-type mssql
--db-host localhost
--db-port 1433
--db-name MyOrigamApp
--db-username sa
--db-password 3NHjcSMajQejgrpKGAD8egxNfEc7

--p-name MyOrigamApp
--p-folder "C:\OrigamProjects\MyOrigamApp"
--p-admin-name admin
--p-admin-password 5axg1zr8
--p-admin-email "john.doe@example.com"
--p-docker-image-linux "origam/server:2025.9.alpha.3984.linux"
--p-docker-image-win "origam/server:2025.9.alpha.3984.win"

--arch-docker-image-linux "origam/architect:2025.9.alpha.3984.linux"
--arch-docker-image-win "origam/architect:2025.9.alpha.3984.win"
--arch-port 8081

--git-enabled
--git-user "Origam Dev"
--git-email "no-reply@origam.com"
```

Example for `PostgreSQL`:
```
Origam.Composer.exe create
--commands-only-linux
--commands-platform linux

--db-type postgres
--db-host localhost
--db-port 5432
--db-name MyOrigamApp
--db-username postgres
--db-password 3NHjcSMajQejgrpKGAD8egxNfEc7

--p-name MyOrigamApp
--p-folder "C:\OrigamProjects\MyOrigamApp"
--p-admin-name admin
--p-admin-password 5axg1zr8
--p-admin-email "john.doe@example.com"
--p-docker-image-linux "origam/server:2025.9.alpha.3984.linux"
--p-docker-image-win "origam/server:2025.9.alpha.3984.win"

--arch-docker-image-linux "origam/architect:2025.9.alpha.3984.linux"
--arch-docker-image-win "origam/architect:2025.9.alpha.3984.win"
--arch-port 8081

--git-enabled
--git-user "Origam Dev"
--git-email "no-reply@origam.com"
```