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
using log4net;
using Npgsql;

namespace Origam.Utils.Sql;

class PgSqlRunner : SqlRunner
{
    public PgSqlRunner(ILog log) : base(log) { }

    protected override string BuildRootVersionSql()
    {
        return "SELECT 'Root package version: ' || \"Version\" FROM \"OrigamModelVersion\" " +
               "WHERE \"refSchemaExtensionId\"='147fa70d-6519-4393-b5d0-87931f9fd609'";
    }

    protected override string BuildProcedureCall(string procedureName)
    {
        string name = !procedureName.StartsWith("\"") && !procedureName.EndsWith("\"")
            ? $"\"{procedureName}\""
            : procedureName;
        return $"CALL {name}();";
    }

    protected override void ExecuteSqlCommand(string connectionString, string sqlCommand)
    {
        using var connection = new NpgsqlConnection(connectionString);
        using var command = new NpgsqlCommand(sqlCommand, connection);
        connection.Open();
        var result = command.ExecuteScalar();
        Console.Write(result?.ToString());
    }
}