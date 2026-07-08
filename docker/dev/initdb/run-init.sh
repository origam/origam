#!/bin/bash
set -e

export PATH="$PATH:/opt/mssql-tools18/bin"

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
