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

    [CommandOption(template: "--commands-add-windows-containers")]
    public bool CommandsAddWindowsContainers { get; set; }

    [CommandOption(template: "--commands-output-format <FORMAT>", isRequired: true)]
    [AllowedValues(values: ["cmd", "sh"])]
    public required string CommandsOutputFormat { get; set; }

    #endregion

    #region Database

    [CommandOption(template: "--db-type <TYPE>", isRequired: true)]
    [AllowedValues(values: ["mssql", "postgres"])]
    public required string DbType { get; set; }

    [CommandOption(template: "--db-host <HOST>", isRequired: true)]
    public required string DbHost { get; set; }

    [CommandOption(template: "--db-port <PORT>", isRequired: true)]
    public required int DbPort { get; set; }

    [CommandOption(template: "--db-name <NAME>", isRequired: true)]
    public required string DbName { get; set; }

    [CommandOption(template: "--db-username <USERNAME>", isRequired: true)]
    public required string DbUsername { get; set; }

    [CommandOption(template: "--db-password <PASSWORD>", isRequired: true)]
    public required string DbPassword { get; set; }

    #endregion

    #region Project

    [CommandOption(template: "--p-docker-image-linux <IMAGE>", isRequired: true)]
    public required string ProjectDockerImageLinux { get; set; }

    [CommandOption(template: "--p-docker-image-win <IMAGE>", isRequired: true)]
    public required string ProjectDockerImageWin { get; set; }

    [CommandOption(template: "--p-name <NAME>", isRequired: true)]
    public required string ProjectName { get; set; }

    [CommandOption(template: "--p-folder <FOLDER>", isRequired: true)]
    public required string ProjectFolder { get; set; }

    [CommandOption(template: "--p-admin-username <NAME>", isRequired: true)]
    public required string ProjectWebAdminUsername { get; set; }

    [CommandOption(template: "--p-admin-password <PASSWORD>", isRequired: true)]
    public required string ProjectWebAdminPassword { get; set; }

    [CommandOption(template: "--p-admin-email <EMAIL>", isRequired: true)]
    public required string ProjectWebAdminEmail { get; set; }

    #endregion

    #region Architect

    [CommandOption(template: "--arch-docker-image-linux <IMAGE>", isRequired: true)]
    public required string ArchitectDockerImageLinux { get; set; }

    [CommandOption(template: "--arch-docker-image-win <IMAGE>", isRequired: true)]
    public required string ArchitectDockerImageWin { get; set; }

    [CommandOption(template: "--arch-port <PORT>", isRequired: true)]
    public int ArchitectPort { get; set; }

    #endregion

    #region Git

    [CommandOption(template: "--git-enabled")]
    public bool GitEnabled { get; set; }

    [CommandOption(template: "--git-user <USER>")]
    public string GitUser { get; set; }

    [CommandOption(template: "--git-email <EMAIL>")]
    public string GitEmail { get; set; }

    #endregion
}
