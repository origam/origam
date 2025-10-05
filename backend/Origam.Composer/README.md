# Origam.Composer

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

## Usage

Parameter `--commands-platform` specifies the platform for which the commands are generated. It can be either `linux` (bash scripts) or `windows` (cmd scripts).

Both docker containers for Linux and Windows will always be generated, but you can disable Windows version by parameter `--commands-only-linux`.

All docker images are available on ORIGAM Github: https://github.com/origam/origam/releases

You can insert parameters into `Jetbrains Rider` > `Edit Configurations` > `Program arguments`.

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
--p-admin-username admin
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
--p-admin-username admin
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

## Warning!
Do not forget to change the passwords. The passwords in the examples are **only for demonstration** purposes.