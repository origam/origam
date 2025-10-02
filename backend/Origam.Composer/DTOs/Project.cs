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

using static Origam.DA.Common.Enums;

namespace Origam.Composer.DTOs;

public class Project
{
    #region General
    public bool CommandsOnlyLinux { get; set; }
    public Enums.Platform CommandsForPlatform { get; set; }
    public string OrigamRepositoryUrl => "https://github.com/origam/origam/archive/master.zip";
    #endregion

    #region DB
    public DatabaseType DatabaseType { get; init; } = DatabaseType.MsSql;
    public required string DatabaseHost { get; init; }
    public int DatabasePort { get; init; }
    public string DatabaseUserName { get; init; }
    public string DatabasePassword { get; init; }
    public bool DatabaseIntegratedAuthentication { get; init; }
    public required string DatabaseName { get; init; }
    public string UserPassword { get; } =
        Guid.NewGuid().ToString().Replace("-", "").Substring(1, 9); // TODO: Really? Maybe use real password generator
    public string GetDataDataService
    {
        get
        {
            return DatabaseType switch
            {
                DatabaseType.MsSql => "Origam.DA.Service.MsSqlDataService, Origam.DA.Service",
                DatabaseType.PgSql => "Origam.DA.Service.PgSqlDataService, Origam.DA.Service",
                _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType)),
            };
        }
    }
    public string BuilderDataConnectionString { get; set; }
    #endregion

    #region Project
    public required string Name { get; init; }
    public required string ProjectFolder { get; init; }
    public required string ModelFolder { get; init; }

    /// <summary>
    /// ./model-root/root menu/.origamPackage
    /// </summary>
    public string BasePackageId { get; set; } = "b9ab12fe-7f7d-43f7-bedc-93747647d6e4";
    public string NewPackageId { get; init; }

    #endregion

    #region Architect
    public string ArchitectDockerImageLinux { get; set; }
    public string ArchitectDockerImageWin { get; set; }
    public int ArchitectPort { get; set; }
    #endregion

    #region WebUser
    public string WebUserName { get; set; }
    public string WebUserPassword { get; set; }
    public string WebFirstName { get; set; }
    public string WebSurname { get; set; }
    public string WebEmail { get; set; }
    #endregion

    #region Docker
    public string ClientDockerImageLinux { get; set; }
    public string ClientDockerImageWin { get; set; }
    public int DockerPort { get; set; } = Common.Constants.DefaultHttpsPort;
    public string DockerFolder { get; set; }
    public string DockerEnvironmentsPathLinux { get; internal set; }
    public string DockerClientPathLinux { get; set; }
    public string DockerArchitectPathLinux { get; set; }
    public string DockerArchitectPathWindows { get; set; }
    public string DockerEnvironmentsPathWindows { get; internal set; }
    public string DockerClientPathWindows { get; set; }
    #endregion

    #region Git
    public bool IsGitInit { get; set; }
    public string GitUsername { get; set; }
    public string GitEmail { get; set; }
    #endregion
}
