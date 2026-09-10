#!/bin/bash
# Launch netcoredbg as a DAP TCP server for any DAP client (nvim-dap, helix,
# a DAP bridge, an MCP agent) — the editor-agnostic alternative to vsdbg.
# Build + stage first (the debugger launches the built DLL):
#   bash .devcontainer/debug-build-architect.sh   # or debug-build-server.sh
# Then attach to localhost:$PORT (forwarded by devcontainer.json). The launched
# process inherits cwd + the env below unless the client overrides them.

set -e
TARGET="${1:-architect}"
PORT="${PORT:-47000}"

case "$TARGET" in
  architect)
    BIN="/workspaces/origam/backend/Origam.Architect.Server/bin/Debug Architect Server/net8.0"
    DLL="Origam.Architect.Server.dll"
    export ASPNETCORE_URLS="http://+:8081"
    BUILD_HINT="bash .devcontainer/debug-build-architect.sh"
    ;;
  server)
    BIN="/workspaces/origam/backend/Origam.Server/bin/Debug/net8.0"
    DLL="Origam.Server.dll"
    export ASPNETCORE_URLS="http://+:8080"
    # Must match the browser-facing origin or login 401s (ID2088).
    export OpenIddictConfig__AccessTokenIssuer="${OpenIddictConfig__AccessTokenIssuer:-https://localhost:5173}"
    BUILD_HINT="bash .devcontainer/debug-build-server.sh"
    ;;
  *)
    echo "Usage: $0 [architect|server]" >&2
    exit 2
    ;;
esac

if [ ! -f "$BIN/$DLL" ]; then
  echo "Build first: $BUILD_HINT" >&2
  exit 1
fi

# cwd = bin = content root (appsettings.json / log4net.config live here).
cd "$BIN"
exec netcoredbg --interpreter=vscode --server="$PORT" \
  -- "$(command -v dotnet)" "$BIN/$DLL"
