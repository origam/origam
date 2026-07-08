# Origam Local Development Environment

Builds and runs the Origam backend from source alongside a database (SQL Server
by default, PostgreSQL optional) and a Vite dev server for `frontend-html`.
The compose file lives at the **repository root** (`docker-compose.yml`);
this directory holds the supporting files (entrypoint, DB init scripts, env
overrides, log4net config).

## Quick Start

```bash
# From the repository root:
cp docker/dev/.env.example docker/dev/.env   # set the DB password (gitignored)
docker compose --env-file docker/dev/.env up -d

# First run builds the backend image (a few minutes). Subsequent starts reuse
# the built image and are fast.

# Frontend (the app):    https://localhost:5173  (accept the self-signed cert)
# Backend API / Swagger: http://localhost:8080
# SQL Server:            localhost:1433          (sa — password in docker/dev/.env)
```

The DB password is required (no default is baked in). `up` reads it from
`docker/dev/.env` via `--env-file`; `ps`/`logs`/`config`/`build` run without it.
`docker/dev/.env.example` documents the variables; other settings have defaults.

## Services

| Service  | Image                      | Port  | Notes                                            |
|----------|----------------------------|-------|--------------------------------------------------|
| database | mcr.microsoft.com/mssql/server:2019-latest | 1433 | SQL Server; bootstraps `origam-dev` DB on first start |
| server   | origam/server:dev (built)  | 8080  | .NET backend, built from `backend/`              |
| frontend | origam/frontend:dev (built)| 5173  | Vite dev server for `frontend-html` (HMR)        |

The server runs the published `Origam.Server.dll` (no nginx in dev). The Vite
dev server proxies API/auth paths (`/internalApi`, `/connect`, `/Account`, …)
to `http://server:8080`.

## Model

The bundled demo model (`model-tests/model`) is mounted by default. It is the
Origam demo project — used for automated testing, so it has lots of features to
poke at.

To point the stack at your own model, use a compose override (do **not** edit
`docker-compose.yml` directly — it is committed):

```bash
cp docker-compose.override.yml.example docker-compose.override.yml
# edit the model path and OrigamSettings__DefaultSchemaExtensionId (your root package id)
docker compose up
```

`docker-compose.override.yml` is gitignored; `docker compose up` auto-merges it
on top of the committed `docker-compose.yml`. Verify the merged config with
`docker compose config`.

`MSSQL_SA_PASSWORD` (and `POSTGRES_PASSWORD` for the postgres stack) are required in
`docker/dev/.env`; a model swap needs the override file above because it also
changes the bind mount.

## Login

The database starts empty (no users). Create the initial admin via the app's
built-in bootstrap endpoint: open
`https://localhost:5173/Account/RegisterInitialUser` and fill the form. It
creates a super-user, signs you in, and locks itself after one use.

## Rebuilding after source changes

- **Frontend:** no rebuild needed — Vite hot-reloads on save.
- **Backend (C#):** `docker compose build server` then `docker compose up -d`.

## Databases

SQL Server is the default. To use PostgreSQL instead:

```bash
docker compose --env-file docker/dev/.env -f docker-compose.yml -f docker-compose.postgres.yml up -d
# Frontend: https://localhost:5173
# Postgres: localhost:5432  (origam — password in docker/dev/.env)
```

The postgres file overrides the `database` service and points the backend at
it; no other changes needed. The model deploys onto a fresh Postgres DB on
first boot.

## Useful Commands

```bash
docker compose logs -f server        # follow backend logs
docker compose logs -f frontend      # follow Vite logs
docker compose down                  # stop the stack
docker compose down -v               # stop and WIPE the database volume
docker compose exec server bash      # shell into the server container
```

## Troubleshooting

- **Port conflicts:** change the host-side mapping in `docker-compose.yml`.
- **Model not loading:** check the mount (`docker compose exec server ls -la
  /home/origam/projectData/model`) and the server logs. A mismatched
  `OrigamSettings__DefaultSchemaExtensionId` is the usual cause.
