# ORIGAM devcontainer (full-stack dev/debug on Linux)

A sandboxed, editor-attached dev environment: a SQL Server database plus one
dev container — the backends **and** the Vite frontends run inside it (started
on demand). Reopen in VS Code/Cursor to debug the .NET backends with
breakpoints.

Builds the **net8.0 subset** of Origam (Server, Architect.Server, Scheduler). The
net472 projects (the WinForms architect, Gui.Win) don't build on Linux. The C#
extension will warn about them on load — cosmetic, ignorable.

(The **architect** below = Origam's modeler: the `architect-html/` UI plus its
backend, `Origam.Architect.Server`. Not the legacy WinForms modeler, Gui.Win,
which won't build here.)

## Open it

1. Create the env file — the default password works out of the box:
   ```bash
   cp -n .devcontainer/.env.example .devcontainer/.env
   ```
2. VS Code / Cursor (Dev Containers extension) → **Dev Containers: Reopen in
   Container**.

Compose brings up two long-running services, `database` (mssql) and `devcontainer`
(the editor attaches here), plus a one-shot `db-init` that creates the database
and exits.

All ports are published by compose (no editor port-forwarding): server 8080,
architect 8081, frontends 5173/5174, and 47000 for debugger attach (DAP,
Debug Adapter Protocol) — each overridable in `.env`.

### First run

1. Run and Debug view (⇧⌘D / Ctrl+Shift+D) → pick **Debug Origam.Server** →
   F5 (builds, stages config, deploys the DB schema on a fresh database).
   (Cursor: resolve the C#-extension conflict first — see
   [Debugging in your editor](#debugging-in-your-editor).)
2. Terminal → Run Task → **serve-frontend**.
3. Open [https://localhost:5173](https://localhost:5173) (accept Vite's
   self-signed cert) →
   [Account/RegisterInitialUser](https://localhost:5173/Account/RegisterInitialUser)
   → create the admin user → log in.

That's the full loop: breakpoints in the server, clicks in the browser. The
sections below cover the rest (architect, scheduler, other editors, swapping
the database or the model).

## Debug the backends (F5)

`.vscode/launch.json` has two configs, each with a build task that stages config
into `bin/` before launch:

- **Debug Origam.Server** — runtime server on 8080. Full OIDC; deploys the schema
  on a fresh DB. Seed a user once via
  [Account/RegisterInitialUser](https://localhost:5173/Account/RegisterInitialUser),
  then login through the frontend.
- **Debug Architect.Server** — architect backend on 8081. Doesn't deploy schema
  on a fresh DB; run the server once first to warm it.

Server console logging runs at Information with request lines and single-line
timestamps. The shared default is `.devcontainer/server-appsettings.Development.json`,
staged into the server's bin as `appsettings.Development.json`; edit the bin copy
at `backend/Origam.Server/bin/Debug/net8.0/appsettings.Development.json`
(a deleted `bin/` restores the default).

Run the backends without debugging:

```bash
.devcontainer/run-architect.sh         # architect, 8081
.devcontainer/run-server.sh            # runtime server, 8080
.devcontainer/run-scheduler.sh         # work-queue scheduler (no port)
```

## Use the frontends

Vite does **not** auto-run — start what you need from VS Code (Terminal → Run
Task) or a shell in the container:

- task **serve-frontend** → `https://localhost:5173` — runtime app (login via the
  debugged server; accept Vite's self-signed cert).
- task **serve-architect-frontend** → `http://localhost:5174` — architect UI (no auth).

`FRONTEND_PORT`/`ARCHITECT_FRONTEND_PORT` in `.devcontainer/.env` remap the
published host ports (the runtime one also retunes OIDC, so login keeps working).

Both proxy to `localhost:8080`/`8081`. With the server running (see
[First run](#first-run)), load the frontend in a browser, click through, hit
breakpoints.

node_modules live in named volumes and are installed on first container creation
by `postCreateCommand`. Install or upgrade packages inside the container:
`cd frontend-html && yarn ...`.

### Origam AI (architect)

Edit `Ai:ApiKey` in the architect bin's `appsettings.Development.json`
(`backend/Origam.Architect.Server/bin/Debug Architect Server/net8.0/`), staged
from `.devcontainer/architect-appsettings.Development.json`.

### Chat (optional)

The Origam chat UI (`chat-html`) is served by the runtime server at `/chatrooms`
in production. In the devcontainer it's off by default; `EnableChat=true` in
`.env` makes the server build task build chat-html and stage it into
`bin/clients/chat`. Open it from the app's user menu.

### PostgreSQL instead of SQL Server

The app layer is database-agnostic (`PgSqlDataService`). Switch:

1. In `.devcontainer/devcontainer.json`, uncomment
   `"docker-compose.postgres.yml"` in the `dockerComposeFile` array.
2. Set `POSTGRES_PASSWORD` in `.devcontainer/.env` (see the template's
   PostgreSQL section); the database is `origam-dev`.
3. Wipe the postgres volume (postgres only creates its database on a fresh
   volume): `docker volume rm <checkoutFolder>_devcontainer_devcontainer-postgres-data`
   (the editor's project name is `<checkoutFolder>_devcontainer`), then
   **Dev Containers: Rebuild Container**.

## DB password / model

The password lives in `.devcontainer/.env` (gitignored; created in [Open it](#open-it)) —
compose auto-loads it, no `--env-file` flag needed. To use your own model instead
of the bundled test model, add:

```
OrigamSettings__ModelSourceControlLocation=/workspaces/origam/your-model
OrigamSettings__DefaultSchemaExtensionId=<your root package id>
```

The `OrigamSettings__*` vars are optional. The model path must be inside the repo
mount (`/workspaces/origam/...`).

## Debugging in your editor

`devcontainer.json` declares `ms-dotnettools.csharp` (the official C#
extension; it uses MS-proprietary **vsdbg**). `netcoredbg` (Samsung, MIT, OSS)
is also installed on PATH. Pick your row:

| Editor | What to do |
|---|---|
| **VS Code** | Already wired. `.vscode/launch.json` has `Debug Origam.Server` and `Debug Architect.Server`; F5 builds, launches, and hits breakpoints (vsdbg). |
| **Cursor** | Cursor ships its own `anysphere.csharp` (netcoredbg). If it and `ms-dotnettools.csharp` are both active you'll get a conflict prompt — disable `ms-dotnettools.csharp` for the workspace and use Anysphere C#. |
| **Rider** | Connect via JetBrains Gateway / Remote Development (Dev Containers). The IDE backend runs in the container and brings its own .NET debugger — no vsdbg/netcoredbg needed. |
| **VSCodium / code-server / Gitpod** | vsdbg won't run on non-Microsoft builds. Install `muhammad-sammy.csharp` from open-vsx — a drop-in fork that swaps in netcoredbg under the same `coreclr` type, so `.vscode/launch.json` works unchanged. |
| **Anything else** (Zed, neovim, helix, agents) | `netcoredbg` is on PATH at `/usr/local/bin/netcoredbg` — point your editor's DAP client at it. Or use `netcoredbg-server.sh` (below) and attach over TCP. |

### Editor-agnostic DAP (TCP)

```bash
.devcontainer/netcoredbg-server.sh architect     # default; port 47000
.devcontainer/netcoredbg-server.sh server        # sets the OIDC issuer env too
```

Build + stage configs first (the script launches the already-built DLL):
`bash .devcontainer/debug-build-architect.sh` (architect) or `bash .devcontainer/debug-build-server.sh`
(server). Then attach your DAP client to `localhost:47000`.

## Relationship to the main dev stack (`docker compose up`)

Two separate stacks:

- **Main stack** (`docker-compose.yml` at repo root): published DLLs, prod-shaped
  entrypoint, for running/evaluating/QA — no editor.
- **This devcontainer**: source + PDBs, editor-attached, for changing code.

They share Dockerfiles and templates; `configureServer.sh` and
`debug-build-server.sh` both source `docker/server/linux/stage_server_config.sh`,
so the appsettings substitutions stay in sync.

## Using Origam in downstream projects

Products built on Origam pin a published runtime image in their own devcontainer
(`FROM origam/server:<version>.<build>.linux`) and layer project-specific
extensions on top. To try a newer Origam version there:

1. Bump the `FROM` tag. Docker Hub has version tags (`2026.8.1.linux`, or
   `2026.8.1.4368.linux` to pin a build), `master-latest.linux` for master, and
   alpha tags (`2026.9.alpha.N.linux`).
2. The first start against an older model database applies the upgrade scripts
   (`ExecuteUpgradeScriptsOnStart=true`). Back the database up beforehand; the
   boot takes longer.
3. Rebuild the project's extension projects against the new source if APIs
   changed, and check the release notes for plugin-facing breaks (e.g. the
   frontend plugin changes in 2026.7).

## Not included

- No hot reload (`dotnet watch`). Edit → F5 (rebuilds).
- Frontend JS breakpoints: Vite serves source maps, so use browser DevTools or
  VS Code's JS debugger.
