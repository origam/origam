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
using Microsoft.Data.SqlClient;
using log4net;

namespace Origam.Utils.Sql;

class MsSqlRunner : SqlRunner
{
    public MsSqlRunner(ILog log)
        : base(log) { }

    protected override string BuildRootVersionSql()
    {
        return "SELECT 'Root package version: ' + \"Version\" FROM dbo.\"OrigamModelVersion\" "
            + "WHERE \"refSchemaExtensionId\"='147fa70d-6519-4393-b5d0-87931f9fd609'";
    }

    protected override string BuildProcedureCall(string procedureName)
    {
        return $"EXEC {procedureName}";
    }

    protected override void ExecuteSqlCommand(string connectionString, string sqlCommand)
    {
        // Diagnostic logging of platform and runtime to help troubleshoot PlatformNotSupportedException
        try
        {
            Console.WriteLine(
                "[DIAG] .NET Runtime: "
                    + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
            );
            Console.WriteLine(
                "[DIAG] OS: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription
            );
            Console.WriteLine(
                "[DIAG] OS Architecture: "
                    + System.Runtime.InteropServices.RuntimeInformation.OSArchitecture
            );
            Console.WriteLine(
                "[DIAG] Process Architecture: "
                    + System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture
            );
            Console.WriteLine(
                "[DIAG] Is Linux: "
                    + System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.Linux
                    )
            );
            Console.WriteLine(
                "[DIAG] Is Windows: "
                    + System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.Windows
                    )
            );
            Console.WriteLine(
                "[DIAG] Is OSX: "
                    + System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.OSX
                    )
            );
            Console.WriteLine("[DIAG] Environment.OSVersion: " + Environment.OSVersion);
            Console.WriteLine("[DIAG] Is64BitProcess: " + Environment.Is64BitProcess);
            Console.WriteLine("[DIAG] CurrentDirectory: " + Environment.CurrentDirectory);
            Console.WriteLine("[DIAG] MachineName: " + Environment.MachineName);
            Console.WriteLine("[DIAG] ProcessorCount: " + Environment.ProcessorCount);
            Console.WriteLine(
                "[DIAG] DOTNET_SYSTEM_GLOBALIZATION_INVARIANT: "
                    + Environment.GetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT")
            );
            Console.WriteLine(
                "[DIAG] ASPNETCORE_ENVIRONMENT: "
                    + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            );

            try
            {
                var sqlConnType = typeof(SqlConnection);
                var asm = sqlConnType.Assembly;
                Console.WriteLine("[DIAG] SqlClient Assembly: " + asm.FullName);
                try
                {
                    Console.WriteLine(
                        "[DIAG] SqlClient Assembly Location: " + (asm.Location ?? "<none>")
                    );
                }
                catch
                { /* single-file or trimmed */
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DIAG] Failed to detect SqlClient assembly: " + ex);
            }

            // Log sanitized connection info (no password)
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
                    connectionString
                );
                if (builder.ContainsKey("Password"))
                {
                    builder.Password = "***";
                }
                if (builder.ContainsKey("Pwd"))
                {
                    builder["Pwd"] = "***";
                }
                Console.WriteLine(
                    $"[DIAG] ConnectionString (sanitized): DataSource={builder.DataSource}; InitialCatalog={builder.InitialCatalog}; UserID={builder.UserID}; IntegratedSecurity={builder.IntegratedSecurity}"
                );
            }
            catch
            {
                Console.WriteLine("[DIAG] Could not parse connection string for diagnostics.");
            }

            // Try to print Linux-specific info
            if (
                System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Linux
                )
            )
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo(
                        "/bin/sh",
                        "-c 'uname -a; cat /etc/os-release || true'"
                    )
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    };
                    var p = System.Diagnostics.Process.Start(psi);
                    if (p != null)
                    {
                        string output = p.StandardOutput.ReadToEnd();
                        string err = p.StandardError.ReadToEnd();
                        p.WaitForExit(2000);
                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            Console.WriteLine("[DIAG] Linux Info:\n" + output);
                        }
                        if (!string.IsNullOrWhiteSpace(err))
                        {
                            Console.WriteLine("[DIAG] Linux Info (stderr):\n" + err);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[DIAG] Failed to read Linux info: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[DIAG] Platform diagnostics failed: " + ex);
        }

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sqlCommand, connection);
        connection.Open();
        var result = command.ExecuteScalar();
        Console.Write(result?.ToString());
    }
}
