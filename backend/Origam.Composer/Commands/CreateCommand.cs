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

using Origam.Composer.Common;
using Origam.Composer.DTOs;
using Origam.Composer.Interfaces.Services;
using Spectre.Console.Cli;

namespace Origam.Composer.Commands;

public class CreateCommand(
    IVisualService visualService,
    IPasswordGeneratorService passwordGeneratorService,
    IProjectBuilderService projectBuilderService,
    IGitService gitService
) : Command<CreateCommandSettings>
{
    public override int Execute(CommandContext context, CreateCommandSettings settings)
    {
        GitIdentity gitIdentity = GitIdentityResolver(settings);
        ShowVisualBanner(settings: settings, gitIdentity: gitIdentity);

        var dockerFolder = Path.Combine(settings.ProjectFolder, "docker");
        var project = new Project
        {
            #region General
            CommandsAddWindowsContainers = settings.CommandsAddWindowsContainers,
            CommandsOutputFormat = settings.CommandsOutputFormat.Equals(
                "cmd",
                StringComparison.CurrentCultureIgnoreCase
            )
                ? Enums.CommandOutputFormat.Cmd
                : Enums.CommandOutputFormat.Sh,
            #endregion

            #region DB
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
            DatabaseName = StringHelper.RemoveAllWhitespace(settings.DbName).ToLower(),
            DatabaseInternalUserName = StringHelper.RemoveAllWhitespace(settings.DbName).ToLower(),
            DatabaseInternalUserPassword = passwordGeneratorService.Generate(length: 24),
            #endregion

            #region Project and client web app
            NewPackageId = Guid.NewGuid().ToString(),
            Name = StringHelper.RemoveAllWhitespace(settings.ProjectName),
            ModelFolder = Path.Combine(settings.ProjectFolder, "model"),
            ProjectFolder = settings.ProjectFolder,

            // Admin user account for client web app
            WebAdminUsername = settings.ProjectWebAdminUsername,
            WebAdminPassword = settings.ProjectWebAdminPassword,
            WebAdminEmail = settings.ProjectWebAdminEmail,

            ClientDockerImageLinux = settings.ProjectDockerImageLinux,
            ClientDockerImageWin = settings.ProjectDockerImageWin,
            #endregion

            ArchitectDockerImageLinux = settings.ArchitectDockerImageLinux,
            ArchitectDockerImageWin = settings.ArchitectDockerImageWin,
            ArchitectPort = settings.ArchitectPort,

            #region Git
            IsGitEnabled = settings.GitEnabled,
            GitUsername = gitIdentity.User,
            GitEmail = gitIdentity.Email,
            #endregion

            #region Docker
            DockerFolder = dockerFolder,

            // Linux
            DockerEnvironmentsPathLinux = Path.Combine(
                dockerFolder,
                settings.ProjectName + "_Environments_Linux.env"
            ),
            DockerClientPathLinux = Path.Combine(
                dockerFolder,
                settings.ProjectName + "_Client_Linux."
            ),
            DockerArchitectPathLinux = Path.Combine(
                dockerFolder,
                settings.ProjectName + "_Architect_Linux."
            ),

            // Windows
            DockerEnvironmentsPathWindows = Path.Combine(
                dockerFolder,
                settings.ProjectName + "_Environments_Windows.env"
            ),
            DockerClientPathWindows = Path.Combine(
                dockerFolder,
                settings.ProjectName + "_Client_Windows."
            ),
            DockerArchitectPathWindows = Path.Combine(
                dockerFolder,
                settings.ProjectName + "_Architect_Windows."
            ),
            #endregion
        };

        // Prepare tasks
        projectBuilderService.PrepareTasks(project: project);
        visualService.PrintProjectCreateTasks(tasks: projectBuilderService.GetTasks());

        // Execute
        projectBuilderService.Create(project: project);
        visualService.PrintBye();

        return 0;
    }

    private void ShowVisualBanner(CreateCommandSettings settings, GitIdentity gitIdentity)
    {
        visualService.PrintHeader(title: Strings.Create_New_Project);
        visualService.PrintDatabaseValues(
            type: settings.DbType,
            host: settings.DbHost,
            port: settings.DbPort,
            name: settings.DbName,
            username: settings.DbUsername
        );
        visualService.PrintProjectValues(
            name: settings.ProjectName,
            folder: settings.ProjectFolder,
            dockerImageLinux: settings.ProjectDockerImageLinux,
            dockerImageWindows: settings.ProjectDockerImageWin,
            adminName: settings.ProjectWebAdminUsername,
            adminEmail: settings.ProjectWebAdminEmail
        );
        visualService.PrintArchitectValues(
            dockerImageLinux: settings.ArchitectDockerImageLinux,
            dockerImageWindows: settings.ArchitectDockerImageWin,
            port: settings.ArchitectPort
        );
        visualService.PrintGitValues(
            isEnabled: settings.GitEnabled,
            user: gitIdentity.User,
            email: gitIdentity.Email
        );
    }

    private GitIdentity GitIdentityResolver(CreateCommandSettings settings)
    {
        var gitUser = "";
        var gitEmail = "";
        string[] gitCredentials = gitService.FetchGitUserFromGlobalConfig();
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

        return new GitIdentity(gitUser, gitEmail);
    }
}
