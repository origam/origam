# Origam.Composer
This tool downloads the model, initializes a database (MS SQL/Postgres), and generates bash/cmd scripts for the ORIGAM project.

## Install database (optional)
You can use docker to install `MS SQL` database:
```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=3NHjcSMajQejgrpKGAD8egxNfEc7" -p 1433:1433 --name mssql2022 -d mcr.microsoft.com/mssql/server:2022-latest
```

or you can install `PostgreSQL` database:
```
docker volume create postgress_volume
docker run --name Postgres_DB -e POSTGRES_PASSWORD=3NHjcSMajQejgrpKGAD8egxNfEc7 -v postgress_volume:/var/lib/postgresql/data -p 5432:5432 -d postgres
```

## Build executable application (optional)
Windows executable can be built by command:
```
dotnet publish ./Origam.Composer/Origam.Composer.csproj -c "Release Server" -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None
```

MacOS executable can be built by command:
```
dotnet publish ./Origam.Composer/Origam.Composer.csproj -c "Release Server" -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None
```

## Usage

Parameter `--commands-output-format` can be either `sh` (bash scripts for Linux + MacOS) or `cmd` (scripts for Windows).

If add parameter `--commands-add-windows-containers`, the generated scripts will contain commands for both Linux and Windows docker containers.

All docker images are available on ORIGAM Github: https://github.com/origam/origam/releases

You can insert parameters into `Jetbrains Rider` > `Edit Configurations` > `Program arguments`.

On `MacOS` you can use something like this: `--p-folder "/Users/JohnDoe/OrigamProjects/MyOrigamApp"`.

You can use custom git identity `--git-user "Origam Dev"` `--git-email "no-reply@origam.com"`

Example for `MS SQL`:
```
Origam.Composer.exe create 
--commands-output-format cmd

--db-type mssql
--db-host localhost
--db-port 1433
--db-name MyOrigamApp
--db-username sa
--db-password 3NHjcSMajQejgrpKGAD8egxNfEc7

--p-name MyOrigamApp
--p-folder "C:\OrigamProjects\MyOrigamApp"
--p-admin-username admin
--p-admin-password 5axg1zr8
--p-admin-email "john.doe@example.com"
--p-docker-image-linux "origam/server:2025.11.alpha.4051.linux"
--p-docker-image-win "origam/server:2025.11.alpha.4051.win"

--arch-docker-image-linux "origam/architect:2025.11.alpha.4051.linux"
--arch-docker-image-win "origam/architect:2025.11.alpha.4051.win"
--arch-port 8081

--git-enabled
```

Example for `PostgreSQL`:
```
Origam.Composer.exe create
--commands-output-format cmd

--db-type postgres
--db-host localhost
--db-port 5432
--db-name MyOrigamApp
--db-username postgres
--db-password 3NHjcSMajQejgrpKGAD8egxNfEc7

--p-name MyOrigamApp
--p-folder "C:\OrigamProjects\MyOrigamApp"
--p-admin-username admin
--p-admin-password 5axg1zr8
--p-admin-email "john.doe@example.com"
--p-docker-image-linux "origam/server:2025.11.alpha.4051.linux"
--p-docker-image-win "origam/server:2025.11.alpha.4051.win"

--arch-docker-image-linux "origam/architect:2025.11.alpha.4051.linux"
--arch-docker-image-win "origam/architect:2025.11.alpha.4051.win"
--arch-port 8081

--git-enabled
```

## Warning!
Do not forget to change the passwords. The passwords in the examples are **only for demonstration** purposes.