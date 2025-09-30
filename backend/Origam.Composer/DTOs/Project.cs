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
    #region DB
    public DatabaseType DatabaseType { get; set; } = DatabaseType.MsSql;
    public required string DatabaseHost { get; set; }
    public int DatabasePort { get; set; }
    public string DatabaseUserName { get; set; }
    public string DatabasePassword { get; set; }
    public bool DatabaseIntegratedAuthentication { get; set; } // TODO: In Docker this will be a problem
    public required string DatabaseName { get; set; }
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
    public string NewPackageId { get; set; }

    #endregion

    #region Architect
    public string ArchitectDockerImage { get; set; }
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
    public int DockerPort { get; set; } = Common.Constants.DefaultHttpsPort;
    public string DockerFolder { get; set; }
    public string DockerEnvPathLinux { get; internal set; }
    public string DockerCmdPathLinux { get; set; }
    public string DockerCmdPathLinuxArchitect { get; set; }
    public string DockerEnvPathWindows { get; internal set; }
    public string DockerCmdPathWindows { get; set; }
    #endregion

    #region Git
    public bool IsGitInit { get; set; }
    public string GitUsername { get; set; }
    public string GitEmail { get; set; }
    #endregion
}
