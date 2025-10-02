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

using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Origam.Composer.Commands;

public class CreateCommandSettings : CommandSettings
{
    #region General

    [CommandOption("--commands-only-linux")]
    public bool CommandsOnlyLinux { get; set; }

    [CommandOption("--commands-platform <PLATFORM>")]
    [AllowedValues("windows", "linux")]
    public string CommandsForPlatform { get; set; }

    #endregion

    #region Database

    [CommandOption("--db-type <TYPE>")]
    [AllowedValues("mssql", "postgres")]
    public string DbType { get; set; }

    [CommandOption("--db-host <HOST>")]
    public string DbHost { get; set; }

    [CommandOption("--db-port <PORT>")]
    public int DbPort { get; set; }

    [CommandOption("--db-name <NAME>")]
    public string DbName { get; set; }

    [CommandOption("--db-username <USERNAME>")]
    public string DbUsername { get; set; }

    [CommandOption("--db-password <PASSWORD>")]
    public string DbPassword { get; set; }

    #endregion

    #region Project

    [CommandOption("--p-docker-image-linux <IMAGE>")]
    public string ProjectDockerImageLinux { get; set; }

    [CommandOption("--p-docker-image-win <IMAGE>")]
    public string ProjectDockerImageWin { get; set; }

    [CommandOption("--p-name <NAME>")]
    public string ProjectName { get; set; }

    [CommandOption("--p-folder <FOLDER>")]
    public string ProjectFolder { get; set; }

    [CommandOption("--p-admin-name <NAME>")]
    public string ProjectAdminName { get; set; }

    [CommandOption("--p-admin-password <PASSWORD>")]
    public string ProjectAdminPassword { get; set; }

    [CommandOption("--p-admin-email <EMAIL>")]
    public string ProjectAdminEmail { get; set; }

    #endregion

    #region Architect

    [CommandOption("--arch-docker-image-linux <IMAGE>")]
    public string ArchitectDockerImageLinux { get; set; }

    [CommandOption("--arch-docker-image-win <IMAGE>")]
    public string ArchitectDockerImageWin { get; set; }

    [CommandOption("--arch-port <PORT>")]
    public int ArchitectPort { get; set; }

    #endregion

    #region Git

    [CommandOption("--git-enabled")]
    public bool GitEnabled { get; set; }

    [CommandOption("--git-user <USER>")]
    public string GitUser { get; set; }

    [CommandOption("--git-email <EMAIL>")]
    public string GitEmail { get; set; }

    #endregion
}
