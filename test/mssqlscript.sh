#!/bin/bash
if [[ -f "first" ]]; then
until /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -Q "SELECT 1"
do 
  sleep 1
done 
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -Q "CREATE DATABASE origam"
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -Q "CREATE LOGIN origam WITH PASSWORD = '$USER_PASSWORD'; USE origam;CREATE USER origam FOR LOGIN origam; EXEC sp_addrolemember N'db_owner', N'origam'"
 rm first
fi
