#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using static Origam.DA.Common.Enums;

namespace Origam.ProjectAutomation;
public class Project
{
    public string GitUsername { get; set; }
    public string GitEmail { get; set; }
    public DatabaseType DatabaseType { get; set; } = DatabaseType.MsSql;
    public int Port { get; set; }
    public string UserPassword { get; } = CreatePassword();
    public static string CreatePassword()
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(1, 9);
    }

    public string GetDataDataService
    {
        get
        {
            return DatabaseType switch
            {
                DatabaseType.MsSql =>
                    "Origam.DA.Service.MsSqlDataService, Origam.DA.Service",
                DatabaseType.PgSql =>
                    "Origam.DA.Service.PgSqlDataService, Origam.DA.Service",
                _ => throw new ArgumentOutOfRangeException("DatabaseType")
            };
        }
    }
    #region Properties
    public string Name { get; set; }
    public string ModelSourceFolder { get; set; }
    public string DataDatabaseName { get; set; }
    public string ModelDatabaseName { get; set; }
    public string DatabaseServerName { get; set; }
    public string DatabaseUserName { get; set; }
    public string DatabasePassword { get; set; }
    public bool DatabaseIntegratedAuthentication { get; set; }
    public bool GitRepository { get; set; }
    public string Url { get; set; }
    public string DataConnectionString { get; set; }
    public string BuilderDataConnectionString { get; set; }
    public string BasePackageId { get; set; } = "b9ab12fe-7f7d-43f7-bedc-93747647d6e4";
    public string NewPackageId { get; set; }
    public string ArchitectUserName { get; set; }
    public string DefaultModelPath { get; set; }
    public string SourcesFolder { get; set; }
    public string BaseUrl { get; set; }
    public string RootSourceFolder { get; set; }
    public int DockerPort { get; set; }
    public string WebUserName { get; set; }
    public string WebUserPassword { get; set; }
    public string WebFirstName { get;  set; }
    public string WebSurname { get;  set; }
    public string WebEmail { get;  set; }
    public int ActiveConfigurationIndex { get; set; }
    public string DockerEnvPath { get; internal set; }
    public string DockerCmdPath { get; set; }

    #endregion
}
