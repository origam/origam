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

using System.Text;
using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;
using static Origam.DA.Common.Enums;

namespace Origam.Composer.BuilderTasks;

public class DockerBuilderTask : IDockerBuilderTask
{
    public string Name => Strings.BuilderTask_Create_Docker_run_scripts;
    public BuilderTaskState State { get; set; } = BuilderTaskState.Prepared;

    public void Execute(Project project)
    {
        Directory.CreateDirectory(project.DockerFolder);

        DockerConfig dockerConfigLinux = GetDockerConfigLinux(project);
        CreateEnvFile(project: project, config: dockerConfigLinux);
        CreateCmdFile(project: project, config: dockerConfigLinux);
        CreateCmdFileArchitect(project: project, config: dockerConfigLinux);

        if (!project.CommandsOnlyLinux)
        {
            DockerConfig dockerConfigWin = GetDockerConfigWindows(project);
            CreateEnvFile(project: project, config: dockerConfigWin);
            CreateCmdFile(project: project, config: dockerConfigWin);
            CreateCmdFileArchitect(project: project, config: dockerConfigWin);
        }
    }

    private void CreateEnvFile(Project project, DockerConfig config)
    {
        string dbType =
            project.DatabaseType == DatabaseType.PgSql
                ? "postgresql"
                : project.DatabaseType.ToString().ToLower();

        string dbUserName =
            project.DatabaseType == DatabaseType.PgSql
                ? project.DatabaseInternalUserName
                : project.DatabaseUserName;

        string dbPassword =
            project.DatabaseType == DatabaseType.PgSql
                ? project.DatabaseInternalUserPassword
                : project.DatabasePassword;

        var sb = new StringBuilder();
        sb.AppendLine($"OrigamSettings__DefaultSchemaExtensionId={project.NewPackageId}");
        sb.AppendLine($"OrigamSettings__DatabaseHost={GetDbHost(project)}");
        sb.AppendLine($"OrigamSettings__DatabasePort={project.DatabasePort}");
        sb.AppendLine($"OrigamSettings__DatabaseUsername={dbUserName}");
        sb.AppendLine($"OrigamSettings__DatabasePassword={dbPassword}");
        sb.AppendLine($"OrigamSettings__DatabaseName={project.DatabaseName}");
        sb.AppendLine($"OrigamSettings__Name={project.Name}");
        sb.AppendLine($"CustomAssetsConfig__PathToCustomAssetsFolder={config.CustomAssetsPath}");
        sb.AppendLine($"CustomAssetsConfig__RouteToCustomAssetsFolder=/customAssets");
        sb.AppendLine($"DatabaseType={dbType}");
        sb.AppendLine($"ExternalDomain_SetOnStart={WebSiteUrl(project)}");
        sb.Append("TZ=Europe/Prague");

        File.WriteAllText(config.EnvFilePath, sb.ToString());
    }

    private void CreateCmdFile(Project project, DockerConfig config)
    {
        var endChar = project.CommandsForPlatform == Enums.Platform.Windows ? '^' : '\\';
        var sb = new StringBuilder();
        if (project.CommandsForPlatform == Enums.Platform.Linux)
        {
            sb.AppendLine("#!/bin/bash");
            sb.AppendLine();
        }
        sb.AppendLine($"docker run --env-file \"{config.EnvFilePath}\" {endChar}");
        sb.AppendLine($"  -it --name {project.Name}_Client {endChar}");
        sb.AppendLine($"  -v \"{project.ModelFolder}\":{config.ModelPath} {endChar}");
        sb.AppendLine(
            $"  -v \"{project.ProjectFolder}\\customAssets\":{config.CustomAssetsPath} {endChar}"
        );
        sb.AppendLine($"  -p {project.DockerPort}:443 {endChar}");
        sb.AppendLine($"  {config.ClientBaseImage}");
        sb.AppendLine();
        sb.AppendLine("REM Open Client web application: https://localhost");
        sb.AppendLine();
        sb.AppendLine("REM Official releases:");
        sb.Append("REM https://github.com/origam/origam/releases");

        File.WriteAllText(config.ClientCmdFilePath + config.CmdFileExtension, sb.ToString());
    }

    private void CreateCmdFileArchitect(Project project, DockerConfig config)
    {
        var endChar = project.CommandsForPlatform == Enums.Platform.Windows ? '^' : '\\';
        var sb = new StringBuilder();
        if (project.CommandsForPlatform == Enums.Platform.Linux)
        {
            sb.AppendLine("#!/bin/bash");
            sb.AppendLine();
        }
        sb.AppendLine($"docker run --env-file \"{config.EnvFilePath}\" {endChar}");
        sb.AppendLine($"  -it --name {project.Name}_Architect {endChar}");
        sb.AppendLine($"  -v \"{project.ModelFolder}\":{config.ModelPath} {endChar}");
        sb.AppendLine($"  -p {project.ArchitectPort}:8081 {endChar}");
        sb.AppendLine($"  {config.ArchitectBaseImage}");
        sb.AppendLine();
        sb.AppendLine(
            $"REM Open Architect web application: https://localhost:{project.ArchitectPort}"
        );
        sb.AppendLine();
        sb.AppendLine("REM Official releases:");
        sb.Append("REM https://github.com/origam/origam/releases");

        File.WriteAllText(config.ArchitectCmdFilePath + config.CmdFileExtension, sb.ToString());
    }

    private DockerConfig GetDockerConfigLinux(Project project)
    {
        var dockerConfig = new DockerConfig
        {
            EnvFilePath = project.DockerEnvironmentsPathLinux,
            CustomAssetsPath = "/home/origam/projectData/customAssets",
            ModelPath = "/home/origam/projectData/model",
            ClientCmdFilePath = project.DockerClientPathLinux,
            ArchitectCmdFilePath = project.DockerArchitectPathLinux,
            ClientBaseImage = project.ClientDockerImageLinux,
            ArchitectBaseImage = project.ArchitectDockerImageLinux,
            CmdFileExtension = project.CommandsForPlatform == Enums.Platform.Windows ? "cmd" : "sh",
        };
        return dockerConfig;
    }

    private DockerConfig GetDockerConfigWindows(Project project)
    {
        var dockerConfig = new DockerConfig
        {
            EnvFilePath = project.DockerEnvironmentsPathWindows,
            CustomAssetsPath = @"C:\home\origam\projectData\customAssets",
            ModelPath = @"C:\home\origam\projectData\model",
            ClientCmdFilePath = project.DockerClientPathWindows,
            ArchitectCmdFilePath = project.DockerArchitectPathWindows,
            ClientBaseImage = project.ClientDockerImageWin,
            ArchitectBaseImage = project.ArchitectDockerImageWin,
            CmdFileExtension = project.CommandsForPlatform == Enums.Platform.Windows ? "cmd" : "sh",
        };
        return dockerConfig;
    }

    private string GetDbHost(Project project)
    {
        if (
            project.DatabaseHost.Equals("localhost")
            || project.DatabaseHost.Equals(".")
            || project.DatabaseHost.Equals("127.0.0.1")
        )
        {
            return "host.docker.internal";
        }
        return project.DatabaseHost;
    }

    public void Rollback(Project project) { }

    private string WebSiteUrl(Project project)
    {
        if (project.DockerPort == Common.Constants.DefaultHttpsPort)
        {
            return "https://localhost";
        }
        return "https://localhost:" + project.DockerPort;
    }

    private class DockerConfig
    {
        public required string EnvFilePath { get; init; }
        public required string CustomAssetsPath { get; init; }
        public required string ModelPath { get; init; }
        public required string ClientCmdFilePath { get; init; }
        public required string ArchitectCmdFilePath { get; init; }
        public required string ClientBaseImage { get; init; }
        public required string ArchitectBaseImage { get; init; }
        public required string CmdFileExtension { get; init; }
    }
}
