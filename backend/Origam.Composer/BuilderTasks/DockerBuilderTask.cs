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
using Origam.Composer.Interfaces.Services;
using Origam.DA.Common.DatabasePlatform;

namespace Origam.Composer.BuilderTasks;

public class DockerBuilderTask(IConnectionStringService connectionStringService)
    : IDockerBuilderTask
{
    public string Name => Strings.BuilderTask_Create_Docker_run_scripts;
    public BuilderTaskState State { get; set; } = BuilderTaskState.Prepared;

    public void Execute(Project project)
    {
        Directory.CreateDirectory(path: project.DockerFolder);

        DockerConfig dockerConfigLinux = GetDockerConfigLinux(project: project);
        CreateEnvFile(project: project, config: dockerConfigLinux);
        CreateCmdFile(project: project, config: dockerConfigLinux);
        CreateCmdFileArchitect(project: project, config: dockerConfigLinux);

        if (project.CommandsAddWindowsContainers)
        {
            DockerConfig dockerConfigWin = GetDockerConfigWindows(project: project);
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

        var sb = new StringBuilder();
        sb.AppendLine(handler: $"OrigamSettings__DefaultSchemaExtensionId={project.NewPackageId}");
        sb.AppendLine(
            handler: $"OrigamSettings__DataConnectionString={connectionStringService.GetConnectionString(project: project)}"
        );
        sb.AppendLine(handler: $"OrigamSettings__Name={project.Name}");
        sb.AppendLine(
            handler: $"CustomAssetsConfig__PathToCustomAssetsFolder={config.CustomAssetsPath}"
        );
        sb.AppendLine(value: $"CustomAssetsConfig__RouteToCustomAssetsFolder=/customAssets");
        sb.AppendLine(handler: $"DatabaseType={dbType}");
        sb.AppendLine(handler: $"ExternalDomain_SetOnStart={WebSiteUrl(project: project)}");
        sb.Append(value: "TZ=Europe/Prague");

        File.WriteAllText(path: config.EnvFilePath, contents: sb.ToString());
    }

    private void CreateCmdFile(Project project, DockerConfig config)
    {
        var endChar = project.CommandsOutputFormat == CommandOutputFormat.Cmd ? '^' : '\\';
        var commentChar = project.CommandsOutputFormat == CommandOutputFormat.Cmd ? "REM" : "#";

        var sb = new StringBuilder();
        if (project.CommandsOutputFormat == CommandOutputFormat.Sh)
        {
            sb.AppendLine(value: "#!/bin/bash");
            sb.AppendLine();
        }
        sb.AppendLine(handler: $"docker run --env-file \"{config.EnvFilePath}\" {endChar}");
        sb.AppendLine(handler: $"  -it --name {project.Name}_Client {endChar}");
        sb.AppendLine(handler: $"  -v \"{project.ModelFolder}\":{config.ModelPath} {endChar}");
        sb.AppendLine(
            handler: $"  -v \"{project.ProjectFolder}\\customAssets\":{config.CustomAssetsPath} {endChar}"
        );
        sb.AppendLine(handler: $"  -p {project.DockerPort}:443 {endChar}");
        sb.AppendLine(handler: $"  {config.ClientBaseImage}");
        sb.AppendLine();
        sb.AppendLine(handler: $"{commentChar} Open Client web application: https://localhost");
        sb.AppendLine();
        sb.AppendLine(handler: $"{commentChar} Official releases:");
        sb.Append(handler: $"{commentChar} https://github.com/origam/origam/releases");

        File.WriteAllText(
            path: config.ClientCmdFilePath + config.CmdFileExtension,
            contents: sb.ToString()
        );
    }

    private void CreateCmdFileArchitect(Project project, DockerConfig config)
    {
        var endChar = project.CommandsOutputFormat == CommandOutputFormat.Cmd ? '^' : '\\';
        var commentChar = project.CommandsOutputFormat == CommandOutputFormat.Cmd ? "REM" : "#";

        var sb = new StringBuilder();
        if (project.CommandsOutputFormat == CommandOutputFormat.Sh)
        {
            sb.AppendLine(value: "#!/bin/bash");
            sb.AppendLine();
        }
        sb.AppendLine(handler: $"docker run --env-file \"{config.EnvFilePath}\" {endChar}");
        sb.AppendLine(handler: $"  -it --name {project.Name}_Architect {endChar}");
        sb.AppendLine(handler: $"  -v \"{project.ModelFolder}\":{config.ModelPath} {endChar}");
        sb.AppendLine(handler: $"  -p {project.ArchitectPort}:8081 {endChar}");
        sb.AppendLine(handler: $"  {config.ArchitectBaseImage}");
        sb.AppendLine();
        sb.AppendLine(
            handler: $"{commentChar} Open Architect web application: https://localhost:{project.ArchitectPort}"
        );
        sb.AppendLine();
        sb.AppendLine(handler: $"{commentChar} Official releases:");
        sb.Append(handler: $"{commentChar} https://github.com/origam/origam/releases");

        File.WriteAllText(
            path: config.ArchitectCmdFilePath + config.CmdFileExtension,
            contents: sb.ToString()
        );
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
            CmdFileExtension =
                project.CommandsOutputFormat == CommandOutputFormat.Cmd ? "cmd" : "sh",
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
            CmdFileExtension =
                project.CommandsOutputFormat == CommandOutputFormat.Cmd ? "cmd" : "sh",
        };
        return dockerConfig;
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
