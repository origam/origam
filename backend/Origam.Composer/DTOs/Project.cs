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
    public required string DatabaseUserName { get; init; }
    public required string DatabasePassword { get; init; }
    public bool DatabaseIntegratedAuthentication { get; init; }
    public required string DatabaseName { get; init; }
    public required string DatabaseInternalUserName { get; init; }
    public required string DatabaseInternalUserPassword { get; init; }

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
    #endregion

    #region Project
    public required string Name { get; init; }
    public required string ProjectFolder { get; init; }
    public required string ModelFolder { get; init; }

    /// <summary>
    /// ./model-root/root menu/.origamPackage
    /// </summary>
    public string BasePackageId { get; set; } = "b9ab12fe-7f7d-43f7-bedc-93747647d6e4";
    public required string NewPackageId { get; init; }

    #endregion

    #region Architect
    public required string ArchitectDockerImageLinux { get; init; }
    public required string ArchitectDockerImageWin { get; init; }
    public int ArchitectPort { get; init; }
    #endregion

    #region WebUser
    public required string WebAdminUsername { get; set; }
    public required string WebAdminPassword { get; set; }
    public required string WebAdminEmail { get; set; }
    #endregion

    #region Docker
    public required string ClientDockerImageLinux { get; init; }
    public required string ClientDockerImageWin { get; init; }
    public int DockerPort { get; set; } = Common.Constants.DefaultHttpsPort;
    public required string DockerFolder { get; init; }
    public required string DockerEnvironmentsPathLinux { get; init; }
    public required string DockerClientPathLinux { get; init; }
    public required string DockerArchitectPathLinux { get; init; }
    public required string DockerArchitectPathWindows { get; init; }
    public required string DockerEnvironmentsPathWindows { get; init; }
    public required string DockerClientPathWindows { get; init; }
    #endregion

    #region Git
    public bool IsGitEnabled { get; init; }
    public string? GitUsername { get; init; }
    public string? GitEmail { get; init; }
    #endregion
}
