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
using System.Threading;
using log4net;
using Origam.DA.Service;

namespace Origam.Utils.Sql;

public interface ISqlRunner
{
    int GetRootVersion(Program.GetRootVersionOptions arguments);
    int RunSqlProcedure(Program.RunSqlProcedureCommandOptions arguments);
    int RunSqlCommand(Program.RunSqlCommandOptions arguments);
}

abstract class SqlRunner : ISqlRunner
{
    protected readonly ILog log;

    protected SqlRunner(ILog log)
    {
        this.log = log;
    }

    public static ISqlRunner Create(ILog log)
    {
        OrigamSettings origamSettings = GetOrigamSettings();
        if (origamSettings.DataDataService.Contains(value: nameof(PgSqlDataService)))
        {
            return new PgSqlRunner(log: log);
        }
        if (origamSettings.DataDataService.Contains(value: nameof(MsSqlDataService)))
        {
            return new MsSqlRunner(log: log);
        }
        throw new NotSupportedException(message: "DataService is not supported");
    }

    public int GetRootVersion(Program.GetRootVersionOptions arguments)
    {
        string sqlCommand = BuildRootVersionSql();
        return RunSqlCommand(arguments: new Program.RunSqlCommandOptions
        {
            Delay = arguments.Delay,
            Attempts = arguments.Attempts,
            SqlCommand = sqlCommand
        });
    }

    public int RunSqlProcedure(Program.RunSqlProcedureCommandOptions arguments)
    {
        string sqlCommand = BuildProcedureCall(procedureName: arguments.ProcedureName);
        return RunSqlCommand(arguments: new Program.RunSqlCommandOptions
        {
            Delay = arguments.Delay,
            Attempts = arguments.Attempts,
            SqlCommand = sqlCommand
        });
    }

    public int RunSqlCommand(Program.RunSqlCommandOptions arguments)
    {
        OrigamSettings origamSettings;
        try
        {
            origamSettings = GetOrigamSettings();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(value: ex);
            log.Error(message: ex);
            return 1;
        }
        for (int i = 0; i < arguments.Attempts; i++)
        {
            try
            {
                ExecuteSqlCommand(connectionString: origamSettings.DataConnectionString, sqlCommand: arguments.SqlCommand);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(value: ex);
                log.Error(message: "Failure:", exception: ex);
                Thread.Sleep(millisecondsTimeout: arguments.Delay);
            }
        }
        return 1;
    }

    protected static OrigamSettings GetOrigamSettings()
    {
        OrigamSettingsCollection configurations = ConfigurationManager.GetAllConfigurations();
        if (configurations.Count != 1)
        {
            throw new Exception(message: "Exactly one configuration in OrigamSettings is required.");
        }
        return configurations[index: 0];
    }

    protected abstract string BuildRootVersionSql();
    protected abstract string BuildProcedureCall(string procedureName);
    protected abstract void ExecuteSqlCommand(string connectionString, string sqlCommand);
}
