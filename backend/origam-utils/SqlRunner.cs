using System;
using System.Data.SqlClient;
using System.Threading;
using log4net;
using Origam.DA.Service;

namespace Origam.Utils;

class SqlRunner
{
    private readonly log4net.ILog log;

    public SqlRunner(ILog log)
    {
        this.log = log;
    }

    public int GetRootVersion(Program.GetRootVersionOptions arguments)
    {
        OrigamSettings origamSettings;
        try
        {
            origamSettings = GetOrigamSettings();
        } 
        catch(Exception ex )
        {
            Console.Error.WriteLine(ex);
            log.Error(ex);
            return 1;
        }

        string sqlCommand = "";
        if (origamSettings.DataDataService.Contains(nameof(PgSqlDataService)))
        {
            sqlCommand =
                "SELECT 'Root package version: ' || \"Version\" FROM \"OrigamModelVersion\" where \"refSchemaExtensionId\"='147fa70d-6519-4393-b5d0-87931f9fd609'";
        }
        else if(origamSettings.DataDataService.Contains(nameof(MsSqlDataService)))
        {
            sqlCommand =
                "SELECT  'Root package version: ' + \"Version\" FROM dbo.\"OrigamModelVersion\" where \"refSchemaExtensionId\"='147fa70d-6519-4393-b5d0-87931f9fd609'";
        }
        else
        {
            throw new ArgumentException($"Unknown DataDataService: \"{origamSettings.DataDataService}\"");
        }

        return RunSqlCommand(new Program.RunSqlCommandOptions
        {
            Delay = arguments.Delay,
            Attempts = arguments.Attempts,
            SqlCommand = sqlCommand
        });
    }

    public int RunSqlProcedure(Program.RunSqlProcedureCommandOptions arguments)
    {
        OrigamSettings origamSettings;
        try
        {
            origamSettings = GetOrigamSettings();
        } 
        catch(Exception ex )
        {
            Console.Error.WriteLine(ex);
            log.Error(ex);
            return 1;
        }
        
        string sqlCommand = "";
        if (origamSettings.DataDataService.Contains(nameof(PgSqlDataService)))
        {
            string name = !arguments.ProcedureName.StartsWith("\"") && !arguments.ProcedureName.EndsWith("\"")
                ? "\"" + arguments.ProcedureName + "\"" 
                : arguments.ProcedureName;
            sqlCommand = $"CALL {name}();";
        }
        else if(origamSettings.DataDataService.Contains(nameof(MsSqlDataService)))
        {
            sqlCommand = $"EXEC {arguments.ProcedureName}";
        }
        else
        {
            throw new ArgumentException($"Unknown DataDataService: \"{origamSettings.DataDataService}\"");
        }
        
        return RunSqlCommand(new Program.RunSqlCommandOptions
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
        catch(Exception ex )
        {
            Console.Error.WriteLine(ex);
            log.Error(ex);
            return 1;
        }

        var connString = origamSettings.DataConnectionString;
        for (var i = 0; i < arguments.Attempts; i++)
        {
            try
            {
                if (origamSettings.DataDataService.Contains(nameof(PgSqlDataService)))
                {
                    using var connection = new Npgsql.NpgsqlConnection(connString);
                    var query = arguments.SqlCommand;
                    using var command = new Npgsql.NpgsqlCommand(query, connection);
                    connection.Open();
                    var resultObj = command.ExecuteScalar();
                    Console.Write(resultObj?.ToString());
                    return 0;
                }
                if(origamSettings.DataDataService.Contains(nameof(MsSqlDataService)))
                {
                    using var connection = new SqlConnection(connString);
                    var query = arguments.SqlCommand;
                    var command = new SqlCommand(query, connection);
                    connection.Open();
                    var resultObj = command.ExecuteScalar();
                    Console.Write(resultObj?.ToString());
                    return 0;
                }
                else
                {
                    throw new ArgumentException($"Unknown DataDataService: \"{origamSettings.DataDataService}\"");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                log.Error("Failure:", ex);
            }
            Thread.Sleep(arguments.Delay);
        }
        return 1;
    }
    
    private static OrigamSettings GetOrigamSettings()
    {
        OrigamSettingsCollection configurations = ConfigurationManager.GetAllConfigurations();
        if (configurations.Count != 1)
        {
            throw new Exception("Exactly one configuration in OrigamSettings is required.");
        }
        return configurations[0];
    }
}