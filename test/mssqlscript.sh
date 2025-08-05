#!/bin/bash

echo "Starting database initialization script..."

if [[ -f "first" ]]; then
  echo "'first' file detected. Proceeding with SQL Server setup."

  echo "Waiting for SQL Server to become available..."
  until /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -Q "SELECT 1" -No
  do
    echo "SQL Server not ready yet. Retrying in 1 second..."
    sleep 1
  done

  echo "SQL Server is up. Creating database 'origam'..."
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -Q "CREATE DATABASE origam" -No
  echo "Database 'origam' created."

  echo "Creating login and user for 'origam' with db_owner role..."
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -Q "CREATE LOGIN origam WITH PASSWORD = '$USER_PASSWORD'; USE origam; CREATE USER origam FOR LOGIN origam; EXEC sp_addrolemember N'db_owner', N'origam'" -No
  echo "Login and user 'origam' created and assigned db_owner role."

  echo "Removing 'first' file to prevent re-initialization..."
  rm first
  echo "Initialization complete."

else
  echo "'first' file not found. Skipping database initialization."
fi
