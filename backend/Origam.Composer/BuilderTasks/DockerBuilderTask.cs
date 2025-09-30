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
using static Origam.DA.Common.Enums;

namespace Origam.Composer.BuilderTasks;

public class DockerBuilderTask : AbstractBuilderTask
{
    public override string Name => "Create Docker run scripts";

    public override void Execute(Project project)
    {
        Directory.CreateDirectory(project.DockerFolder);

        DockerConfig dockerConfigLinux = GetDockerConfigLinux(project);
        CreateEnvFile(project, dockerConfigLinux);
        CreateCmdFile(project, dockerConfigLinux);
        CreateCmdFileArchitect(project, dockerConfigLinux);

        DockerConfig dockerConfigWindows = GetDockerConfigWindows(project);
        CreateEnvFile(project, dockerConfigWindows);
        CreateCmdFile(project, dockerConfigWindows);
    }

    private void CreateEnvFile(Project project, DockerConfig config)
    {
        string dbType =
            project.DatabaseType == DatabaseType.PgSql
                ? "postgresql"
                : project.DatabaseType.ToString().ToLower();

        string dbUserName =
            project.DatabaseType == DatabaseType.PgSql ? project.Name : project.DatabaseUserName;

        string dbPassword =
            project.DatabaseType == DatabaseType.PgSql
                ? project.UserPassword
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
        var sb = new StringBuilder();
        sb.AppendLine($"docker run --env-file \"{config.EnvFilePath}\" ^");
        sb.AppendLine($"  -it --name {project.Name}_Client ^");
        sb.AppendLine($"  -v \"{project.ModelFolder}\":{config.ModelPath} ^");
        sb.AppendLine(
            $"  -v \"{project.ProjectFolder}\\customAssets\":{config.CustomAssetsPath} ^"
        );
        sb.AppendLine($"  -p {project.DockerPort}:443 ^");
        sb.AppendLine($"  {config.BaseImage}");
        sb.AppendLine();
        sb.AppendLine("REM Open Client web application: https://localhost");
        sb.AppendLine();
        sb.AppendLine("REM Official releases:");
        sb.Append("REM https://github.com/origam/origam/releases");

        File.WriteAllText(config.CmdFilePath, sb.ToString());
    }

    private void CreateCmdFileArchitect(Project project, DockerConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"docker run --env-file \"{config.EnvFilePath}\" ^");
        sb.AppendLine($"  -it --name {project.Name}_Architect ^");
        sb.AppendLine($"  -v \"{project.ModelFolder}\":{config.ModelPath} ^");
        sb.AppendLine($"  -p {project.ArchitectPort}:8081 ^");
        sb.AppendLine($"  {project.ArchitectDockerImage}");
        sb.AppendLine();
        sb.AppendLine(
            $"REM Open Architect web application: https://localhost:{project.ArchitectPort}"
        );
        sb.AppendLine();
        sb.AppendLine("REM Official releases:");
        sb.Append("REM https://github.com/origam/origam/releases");

        File.WriteAllText(project.DockerCmdPathLinuxArchitect, sb.ToString());
    }

    private DockerConfig GetDockerConfigLinux(Project project)
    {
        var dockerConfig = new DockerConfig
        {
            EnvFilePath = project.DockerEnvPathLinux,
            CustomAssetsPath = "/home/origam/projectData/customAssets",
            ModelPath = "/home/origam/projectData/model",
            CmdFilePath = project.DockerCmdPathLinux,
            BaseImage = "origam/server:master-latest.linux", // TODO: Fetch from parameters
        };
        return dockerConfig;
    }

    private DockerConfig GetDockerConfigWindows(Project project)
    {
        var dockerConfig = new DockerConfig
        {
            EnvFilePath = project.DockerEnvPathWindows,
            CustomAssetsPath = @"C:\home\origam\projectData\customAssets",
            ModelPath = @"C:\home\origam\projectData\model",
            CmdFilePath = project.DockerCmdPathWindows,
            BaseImage = "origam/server:master-latest.win", // TODO: Fetch from parameters
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

    public override void Rollback() { }

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
        public string EnvFilePath { get; init; }
        public string CustomAssetsPath { get; init; }
        public string ModelPath { get; init; }
        public string CmdFilePath { get; init; }
        public string BaseImage { get; init; }
    }
}
