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
    public DatabaseType DatabaseType { get; init; } = DatabaseType.MsSql;
    public required string DatabaseHost { get; init; }
    public int DatabasePort { get; init; }
    public string DatabaseUserName { get; init; }
    public string DatabasePassword { get; init; }
    public bool DatabaseIntegratedAuthentication { get; init; } // TODO: In Docker this will be a problem
    public required string DatabaseName { get; init; }
    public string UserPassword { get; } =
        Guid.NewGuid().ToString().Replace("-", "").Substring(1, 9); // TODO: Really? Maybe use real password generator
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
    public string DockerEnvPathLinux { get; internal set; }
    public string DockerCmdPathLinux { get; set; }
    public string DockerCmdPathLinuxArchitect { get; set; }
    public string DockerCmdPathWinArchitect { get; set; }
    public string DockerEnvPathWindows { get; internal set; }
    public string DockerCmdPathWindows { get; set; }
    #endregion

    #region Git
    public bool IsGitInit { get; set; }
    public string GitUsername { get; set; }
    public string GitEmail { get; set; }
    #endregion
}
