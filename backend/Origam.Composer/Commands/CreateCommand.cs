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

using Origam.Composer.DTOs;
using Origam.Composer.Interfaces.Services;
using Origam.Git;
using Spectre.Console.Cli;

namespace Origam.Composer.Commands;

public class CreateCommand(
    IVisualService visualService,
    IProjectStarterService projectStarterService
) : Command<AppSettings>
{
    public override int Execute(CommandContext context, AppSettings settings)
    {
        Tuple<string, string> gitIdentity = GitIdentityResolver(settings);
        ShowVisualBanner(settings, gitIdentity);

        // TODO: Use builder pattern
        var DockerFolder = Path.Combine(settings.ProjectFolder, "docker");
        var project = new Project
        {
            DatabaseType = settings.DbType.Equals(
                "postgres",
                StringComparison.CurrentCultureIgnoreCase
            )
                ? DA.Common.Enums.DatabaseType.PgSql
                : DA.Common.Enums.DatabaseType.MsSql,
            DatabaseHost = settings.DbHost,
            DatabasePort = settings.DbPort,

            DatabaseUserName = settings.DbUsername,
            DatabasePassword = settings.DbPassword,
            DatabaseIntegratedAuthentication = false,
            DatabaseName = settings.DbName,

            Name = settings.ProjectName,
            ModelFolder = Path.Combine(settings.ProjectFolder, "model"),
            ProjectFolder = settings.ProjectFolder,

            ArchitectDockerImage = settings.ArchitectDockerImage,
            ArchitectPort = settings.ArchitectPort,

            WebUserName = settings.ProjectAdminName,
            WebUserPassword = settings.ProjectAdminPassword,
            WebEmail = settings.ProjectAdminEmail,

            NewPackageId = Guid.NewGuid().ToString(),

            IsGitInit = settings.GitEnabled,
            GitUsername = gitIdentity.Item1,
            GitEmail = gitIdentity.Item2,

            // Docker
            DockerFolder = DockerFolder,
            DockerEnvPathLinux = Path.Combine(
                DockerFolder,
                settings.ProjectName + "_Environments_Linux.env"
            ),
            DockerCmdPathLinux = Path.Combine(
                DockerFolder,
                settings.ProjectName + "_Client_Linux.cmd"
            ),
            DockerCmdPathLinuxArchitect = Path.Combine(
                DockerFolder,
                settings.ProjectName + "_Architect_Linux.cmd"
            ),
            DockerEnvPathWindows = Path.Combine(
                DockerFolder,
                settings.ProjectName + "_Environments_Windows.env"
            ),
            DockerCmdPathWindows = Path.Combine(
                DockerFolder,
                settings.ProjectName + "_Client_Windows.cmd"
            ),
        };

        // Prepare tasks
        projectStarterService.CreateTasks(project);
        visualService.PrintProjectCreateTasks(projectStarterService.GetTasks());

        // Execute
        projectStarterService.Create(project);
        visualService.PrintBye();

        return 0;
    }

    private void ShowVisualBanner(AppSettings settings, Tuple<string, string> gitIdentity)
    {
        visualService.PrintHeader("Create New Project");
        visualService.PrintDatabaseValues(
            settings.DbHost,
            settings.DbPort,
            settings.DbName,
            settings.DbUsername
        );
        visualService.PrintProjectValues(
            settings.ProjectName,
            settings.ProjectFolder,
            settings.ProjectDockerImage,
            settings.ProjectAdminName,
            settings.ProjectAdminEmail
        );
        visualService.PrintArchitectValues(settings.ArchitectDockerImage, settings.ArchitectPort);
        visualService.PrintGitValues(settings.GitEnabled, gitIdentity.Item1, gitIdentity.Item2);
    }

    // TODO: Maybe record, not tuple
    private Tuple<string, string> GitIdentityResolver(AppSettings settings)
    {
        var gitUser = "";
        var gitEmail = "";
        string[] gitCredentials = new GitManager().GitConfig();
        if (gitCredentials != null)
        {
            gitUser = gitCredentials[0];
            gitEmail = gitCredentials[1];
        }
        if (!string.IsNullOrWhiteSpace(settings.GitUser))
        {
            gitUser = settings.GitUser;
        }
        if (!string.IsNullOrWhiteSpace(settings.GitEmail))
        {
            gitEmail = settings.GitEmail;
        }

        return new Tuple<string, string>(gitUser, gitEmail);
    }
}
