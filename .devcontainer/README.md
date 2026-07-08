# ORIGAM devcontainer (full-stack dev/debug on Linux)

A sandboxed, editor-attached dev environment. One `docker compose up` brings up
SQL Server + the dev container + **both** frontends (frontend-html, architect-html).
Reopen in VS Code/Cursor to debug the .NET backends with breakpoints.

Builds the **net8.0 subset** of Origam (Server, Architect.Server, Scheduler). The
net472 projects (old WinForms architect, Gui.Win) don't build on Linux and aren't
needed here. The C# extension will warn about them on load — cosmetic, ignorable.

## Open it

VS Code / Cursor (Dev Containers extension) → **Dev Containers: Reopen in Container**.
Compose brings up: `database` (mssql), `devcontainer` (editor attaches here),
`frontend` (5173), `architect-frontend` (5174).

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
```

## Use the frontends

- `https://localhost:5173` — runtime app (login via the debugged server). Accept
  Vite's self-signed cert.
- `http://localhost:5174` — architect UI (no auth; HTTP).

Both proxy to `devcontainer:8080`/`8081` — i.e. to whatever backend you launched
from the editor. So: F5 the server, then load the frontend in a browser, hit
breakpoints as you click.

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
(server). Then attach your DAP client to `localhost:47000` (forwarded to the host).

The devcontainer spec has no `extends`, so the repo can only declare one C#
extension — the table above is how non-VS-Code editors layer netcoredbg on top.

## Relationship to the main dev stack (`docker compose up`)

Two separate things, both kept intentionally:

- **Main stack** (`docker-compose.yml` at repo root): published DLLs, prod-shaped
  entrypoint, for running/evaluating/QA — no editor.
- **This devcontainer**: source + PDBs, editor-attached, for changing code.

They share Dockerfiles/templates but stage config differently. **If you change
appsettings substitutions in one path, update the other** — e.g.
`debug-build-server.sh` and `docker/server/linux/configureServer.sh` both do the
`pathchatapp`/`chatinterval`/`ExternalDomain` sed substitutions; keep them in sync.

## Not included

- No hot reload (`dotnet watch`). Edit → F5 (rebuilds).
- Frontend JS breakpoints: Vite serves source maps, so use browser DevTools or
  VS Code's JS debugger — standard, not wired here.
