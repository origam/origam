# ORIGAM devcontainer (full-stack dev/debug on Linux)

A sandboxed, editor-attached dev environment. One `docker compose up` brings up
SQL Server + the dev container — backends **and** all three Vite frontends run
inside the dev container (started on demand), so the editor sees one machine.
Reopen in VS Code/Cursor to debug the .NET backends with breakpoints.

Builds the **net8.0 subset** of Origam (Server, Architect.Server, Scheduler). The
net472 projects (old WinForms architect, Gui.Win) don't build on Linux and aren't
needed here. The C# extension will warn about them on load — cosmetic, ignorable.

## Open it

VS Code / Cursor (Dev Containers extension) → **Dev Containers: Reopen in Container**.
Compose brings up two services: `database` (mssql) and `devcontainer` (the editor
attaches here; it carries the .NET SDK, netcoredbg, node/corepack, and hosts the
Vite dev servers on demand).

All ports are published by compose (no editor port-forwarding): server 8080,
architect 8081, frontends 5173/5174, DAP 47000 — each overridable in `.env`.

## Debug the backends (F5)

`.vscode/launch.json` has two configs, each with a build task that stages config
into `bin/` before launch:

- **Debug Origam.Server** — runtime server on 8080. Full OIDC; deploys the schema
  on a fresh DB. Seed a user once via `https://localhost:5173/Account/RegisterInitialUser`,
  then login through the frontend.
- **Debug Architect.Server** — architect backend on 8081. Note: the architect does
  **not** deploy schema on a fresh DB, so run the server once first to warm it.

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

Both proxy to `localhost:8080`/`8081` — i.e. to whatever backend you launched
from the editor. So: F5 the server, then load the frontend in a browser, hit
breakpoints as you click.

node_modules live in named volumes (Linux binaries, off the host tree), so the
Vite tasks run without touching the host, and the editor gets TypeScript
intellisense. `postCreateCommand` installs them on first container creation
(yarn 4 via corepack, pinned by `packageManager` in each package.json).
Install/upgrade packages inside the container: `cd frontend-html && yarn ...`.

### Origam AI (architect)

`backend/Origam.AI.Agent` is hosted by the architect server; see its README for
the settings. Inside the container you follow the same flow: edit
`Ai:ApiKey` in the architect bin's `appsettings.Development.json`
(`backend/Origam.Architect.Server/bin/Debug Architect Server/net8.0/`) — it's
staged from `.devcontainer/architect-appsettings.Development.json` with `cp -n`,
so your key survives rebuilds.

### Chat (optional)

The Origam chat UI (`chat-html`) is served by the runtime server at `/chatrooms`
in production. In the devcontainer it's off by default; `EnableChat=true` in
`.env` makes the server build task build chat-html and stage it into
`bin/clients/chat` (mirroring the published image). Open it from the app's user
menu — it loads in the same browser origin, so login carries over.

## DB password / model

The DB password is NOT baked in — copy `.devcontainer/.env.example` to
`.devcontainer/.env` (gitignored) and set `MSSQL_SA_PASSWORD`. Compose auto-loads
`.env` from `.devcontainer/`, so no `--env-file` prefix is needed:

```
MSSQL_SA_PASSWORD=YourPassword
OrigamSettings__ModelSourceControlLocation=/workspaces/origam/your-model
OrigamSettings__DefaultSchemaExtensionId=<your root package id>
```

The `OrigamSettings__*` vars are `${VAR:-default}` in `docker-compose.yml`, so
they're optional (defaults point at the bundled test model); `MSSQL_SA_PASSWORD` is
required. The model path must be inside the repo mount (`/workspaces/origam/...`).

## Debugging in your editor

`devcontainer.json` declares `ms-dotnettools.csharp` (official C# extension,
which uses MS-proprietary **vsdbg**) — that's the most-used path and it works
out of the box in base VS Code. `netcoredbg` (Samsung, MIT, OSS) is also
installed on PATH (`/usr/local/bin/netcoredbg`) for everyone else. Pick your row:

| Editor | What to do |
|---|---|
| **VS Code** | Already wired. `.vscode/launch.json` has `Debug Origam.Server` and `Debug Architect.Server`; F5 builds, launches, and hits breakpoints (vsdbg). |
| **Cursor** | Cursor ships its own `anysphere.csharp` (netcoredbg, May 2025). If it and the repo-declared official extension are both active you'll get a conflict prompt — disable `ms-dotnettools.csharp` for the workspace and use Anysphere C#. |
| **Rider** | Connect via JetBrains Gateway / Remote Development (Dev Containers). The IDE backend runs in the container and brings its own .NET debugger — no vsdbg/netcoredbg needed. Rider ignores `customizations.vscode`, so it works as-is. |
| **VSCodium / code-server / Gitpod** | vsdbg won't run on non-Microsoft builds. Install `muhammad-sammy.csharp` from open-vsx — a drop-in fork that swaps in netcoredbg under the same `coreclr` type, so `.vscode/launch.json` works unchanged. |
| **Anything else** (Zed, neovim, helix, agents) | `netcoredbg` is on PATH at `/usr/local/bin/netcoredbg` — point your editor's DAP client at it. Or use `netcoredbg-server.sh` (below) and attach over TCP. |

### Editor-agnostic DAP (TCP)

```bash
.devcontainer/netcoredbg-server.sh architect     # default; port 47000
.devcontainer/netcoredbg-server.sh server        # sets the OIDC issuer env too
PORT=48000 .devcontainer/netcoredbg-server.sh server
```

Build + stage configs first (the script launches the already-built DLL):
`bash .devcontainer/debug-build-architect.sh` (architect) or `bash .devcontainer/debug-build-server.sh`
(server). Then attach your DAP client to `localhost:47000` (published by
compose as `${DAP_PORT:-47000}:47000`).

The devcontainer spec has no `extends`, so the repo can only declare one C#
extension — the table above is how non-VS-Code editors layer netcoredbg on top.

## Relationship to the main dev stack (`docker compose up`)

Two separate things, both kept intentionally:

- **Main stack** (`docker-compose.yml` at repo root): published DLLs, prod-shaped
  entrypoint, for running/evaluating/QA — no editor.
- **This devcontainer**: source + PDBs, editor-attached, for changing code.

They share Dockerfiles, templates, and the staging helper: `configureServer.sh`
and `debug-build-server.sh` both source `docker/server/linux/stage_server_config.sh`,
so the appsettings substitutions stay in sync. The architect additionally stages
an `appsettings.Development.json` (see above).

## Using Origam in downstream projects

Products built on Origam pin a published runtime image in their own devcontainer
(`FROM origam/server:<version>.<build>.linux`) and layer project-specific
extensions on top. To try a newer Origam version there:

1. Bump the `FROM` tag. Docker Hub has per-build tags (`2026.8.1.4368.linux`),
   per-version aliases (`2026.8.1.linux`), rolling (`master-latest.linux`), and
   alpha pre-releases (`2026.9.alpha.N.linux`).
2. The image's `_OrigamSettings.template` sets `ExecuteUpgradeScriptsOnStart=true`,
   so the first start against an older model database applies the upgrade
   scripts — expect a longer first boot, and back the database up first.
3. Rebuild the project's extension projects against the new source if APIs
   changed, and check the release notes for plugin-facing breaks (e.g. the
   frontend plugin changes in 2026.7).

## Not included

- No hot reload (`dotnet watch`). Edit → F5 (rebuilds).
- Frontend JS breakpoints: Vite serves source maps, so use browser DevTools or
  VS Code's JS debugger — standard, not wired here.
