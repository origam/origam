#!/bin/bash
if [[ -f "first" ]]; then
until /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -Q "SELECT 1" -No
do 
  sleep 1
done 
/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -Q "CREATE DATABASE origam" -No
/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -Q "CREATE LOGIN origam WITH PASSWORD = '$USER_PASSWORD'; USE origam;CREATE USER origam FOR LOGIN origam; EXEC sp_addrolemember N'db_owner', N'origam'" -No
 rm first
fi
