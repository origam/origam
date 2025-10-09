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

    [CommandOption("--commands-platform <PLATFORM>", true)]
    [AllowedValues("windows", "linux")]
    public required string CommandsForPlatform { get; set; }

    #endregion

    #region Database

    [CommandOption("--db-type <TYPE>", true)]
    [AllowedValues("mssql", "postgres")]
    public required string DbType { get; set; }

    [CommandOption("--db-host <HOST>", true)]
    public required string DbHost { get; set; }

    [CommandOption("--db-port <PORT>", true)]
    public required int DbPort { get; set; }

    [CommandOption("--db-name <NAME>", true)]
    public required string DbName { get; set; }

    [CommandOption("--db-username <USERNAME>", true)]
    public required string DbUsername { get; set; }

    [CommandOption("--db-password <PASSWORD>", true)]
    public required string DbPassword { get; set; }

    #endregion

    #region Project

    [CommandOption("--p-docker-image-linux <IMAGE>", true)]
    public required string ProjectDockerImageLinux { get; set; }

    [CommandOption("--p-docker-image-win <IMAGE>", true)]
    public required string ProjectDockerImageWin { get; set; }

    [CommandOption("--p-name <NAME>", true)]
    public required string ProjectName { get; set; }

    [CommandOption("--p-folder <FOLDER>", true)]
    public required string ProjectFolder { get; set; }

    [CommandOption("--p-admin-username <NAME>", true)]
    public required string ProjectWebAdminUsername { get; set; }

    [CommandOption("--p-admin-password <PASSWORD>", true)]
    public required string ProjectWebAdminPassword { get; set; }

    [CommandOption("--p-admin-email <EMAIL>", true)]
    public required string ProjectWebAdminEmail { get; set; }

    #endregion

    #region Architect

    [CommandOption("--arch-docker-image-linux <IMAGE>", true)]
    public required string ArchitectDockerImageLinux { get; set; }

    [CommandOption("--arch-docker-image-win <IMAGE>")]
    public required string ArchitectDockerImageWin { get; set; }

    [CommandOption("--arch-port <PORT>")]
    public int ArchitectPort { get; set; }

    #endregion

    #region Git

    [CommandOption("--git-enabled")]
    public bool GitEnabled { get; set; }

    [CommandOption("--git-user <USER>")]
    public string? GitUser { get; set; }

    [CommandOption("--git-email <EMAIL>")]
    public string? GitEmail { get; set; }

    #endregion
}
