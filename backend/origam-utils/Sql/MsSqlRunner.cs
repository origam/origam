#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Data.SqlClient;
using log4net;

namespace Origam.Utils.Sql;

class MsSqlRunner : SqlRunner
{
    public MsSqlRunner(ILog log) : base(log) { }

    protected override string BuildRootVersionSql()
    {
        return "SELECT 'Root package version: ' + \"Version\" FROM dbo.\"OrigamModelVersion\" " +
               "WHERE \"refSchemaExtensionId\"='147fa70d-6519-4393-b5d0-87931f9fd609'";
    }

    protected override string BuildProcedureCall(string procedureName)
    {
        return $"EXEC {procedureName}";
    }

    protected override void ExecuteSqlCommand(string connectionString, string sqlCommand)
    {
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sqlCommand, connection);
        connection.Open();
        var result = command.ExecuteScalar();
        Console.Write(result?.ToString());
    }
}