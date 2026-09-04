#!/bin/bash
set -e

export PATH="$PATH:/opt/mssql-tools18/bin"

# The compose files default MSSQL_SA_PASSWORD to empty (so ps/logs/build run
# without the .env). Fail fast here so a bare `up` without the .env gets a clear
# message instead of MSSQL's cryptic "Password validation failed ... too short".
if [ -z "${MSSQL_SA_PASSWORD:-}" ]; then
  echo "ERROR: MSSQL_SA_PASSWORD is not set. Provide it via the dev .env file:" >&2
  echo "  - main stack:   cp docker/dev/.env.example docker/dev/.env, then: docker compose --env-file docker/dev/.env up" >&2
  echo "  - devcontainer: cp .devcontainer/.env.example .devcontainer/.env (auto-loaded by Compose)" >&2
  exit 1
fi

# Run the original entrypoint to set permissions, then start sqlservr in background
# Use full path since sqlservr is not in PATH
/opt/mssql/bin/permissions_check.sh /opt/mssql/bin/sqlservr &
SQL_PID=$!

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
while ! sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q 'SELECT 1' 2>/dev/null; do
    sleep 2
done
echo "SQL Server is ready."

# Run init scripts
if [ -d /docker-entrypoint-initdb.d ]; then
    for f in /docker-entrypoint-initdb.d/*.sql; do
        if [ -f "$f" ]; then
            echo "Running init script: $f"
            sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i "$f" || true
        fi
    done
fi

# Wait for the background SQL Server process (keeps container alive)
wait $SQL_PID
