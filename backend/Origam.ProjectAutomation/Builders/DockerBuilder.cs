#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;
using System.IO;
using Origam.DA.Common.DatabasePlatform;

namespace Origam.ProjectAutomation.Builders;

public class DockerBuilder : AbstractBuilder
{
    private readonly DataDatabaseBuilder dataDatabaseBuilder;
    private static readonly string DockerFolderName = "Docker";
    public override string Name => "Create Docker run script";
    private string newProjectFolder;

    public DockerBuilder(DataDatabaseBuilder dataDatabaseBuilder)
    {
        this.dataDatabaseBuilder = dataDatabaseBuilder;
    }

    public override void Execute(Project project)
    {
        newProjectFolder = Path.Combine(project.SourcesFolder, DockerFolderName);
        project.DockerEnvPathLinux = Path.Combine(newProjectFolder, project.Name + "_Linux.env");
        project.DockerCmdPathLinux = Path.Combine(newProjectFolder, project.Name + "_Linux.cmd");
        project.DockerEnvPathWindows = Path.Combine(
            newProjectFolder,
            project.Name + "_Windows.env"
        );
        project.DockerCmdPathWindows = Path.Combine(
            newProjectFolder,
            project.Name + "_Windows.cmd"
        );
        Directory.CreateDirectory(newProjectFolder);
        DockerConfig dockerConfigLinux = GetDockerConfig(Platform.Linux, project);
        CreateEnvFile(project, dockerConfigLinux);
        CreateCmdFile(project, dockerConfigLinux);
        DockerConfig dockerConfigWindows = GetDockerConfig(Platform.Windows, project);
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

        string connectionString = dataDatabaseBuilder.BuildConnectionString(
            new ConnectionStringData(
                databaseType: project.DatabaseType,
                databaseServerName: "host.docker.internal",
                databasePort: project.DatabasePort,
                dataDatabaseName: project.DataDatabaseName,
                databaseUserName: project.DatabaseUserName,
                databasePassword: project.DatabasePassword,
                databaseIntegratedAuthentication: project.DatabaseIntegratedAuthentication
            ),
            true
        );

        string content =
            $"OrigamSettings__DefaultSchemaExtensionId={project.NewPackageId}\n"
            + $"OrigamSettings__DataConnectionString={connectionString}\n"
            + $"OrigamSettings__Name={project.Name}\n"
            + $"CustomAssetsConfig__PathToCustomAssetsFolder={config.CustomAssetsPath}\n"
            + $"CustomAssetsConfig__RouteToCustomAssetsFolder=/customAssets\n"
            + $"DatabaseType={dbType}\n"
            + $"ExternalDomain_SetOnStart={WebSiteUrl(project)}\n"
            + "TZ=Europe/Prague";
        File.WriteAllText(config.EnvFilePath, content);
    }

    private void CreateCmdFile(Project project, DockerConfig config)
    {
        string content =
            $"docker run --env-file \"{config.EnvFilePath}\" ^\n"
            + $"    -it --name {project.Name} ^\n"
            + $"    -v \"{project.SourcesFolder}\\model\":{config.ModelPath} ^\n"
            + $"    -v \"{project.SourcesFolder}\\customAssets\":{config.CustomAssetsPath} ^\n"
            + $"    -p {project.DockerPort}:443 ^\n"
            + $"    {config.BaseImage}\n"
            + "\n"
            + "REM After you run the above command go to https://localhost to open the client web application.\n"
            + "\n"
            + $"REM {config.BaseImage} is the latest version, that may not be what you want.\n"
            + "REM Here you can find current releases and their docker images:\n"
            + "REM https://github.com/origam/origam/releases";
        File.WriteAllText(config.CmdFilePath, content);
    }

    private static DockerConfig GetDockerConfig(Platform platform, Project project)
    {
        return platform switch
        {
            Platform.Linux => new DockerConfig
            {
                EnvFilePath = project.DockerEnvPathLinux,
                CustomAssetsPath = "/home/origam/projectData/customAssets",
                ModelPath = "/home/origam/projectData/model",
                CmdFilePath = project.DockerCmdPathLinux,
                BaseImage = "origam/server:master-latest.linux",
            },
            Platform.Windows => new DockerConfig
            {
                EnvFilePath = project.DockerEnvPathWindows,
                CustomAssetsPath = @"C:\home\origam\projectData\customAssets",
                ModelPath = @"C:\home\origam\projectData\model",
                CmdFilePath = project.DockerCmdPathWindows,
                BaseImage = "origam/server:master-latest.win-core",
            },
            _ => throw new ArgumentOutOfRangeException(nameof(platform), "Unknown platform"),
        };
    }

    private string GetDbHost(Project project)
    {
        if (
            project.DatabaseServerName.Equals("localhost")
            || project.DatabaseServerName.Equals(".")
            || project.DatabaseServerName.Equals("127.0.0.1")
        )
        {
            return "host.docker.internal";
        }
        return project.DatabaseServerName;
    }

    public override void Rollback() { }

    public string WebSiteUrl(Project project)
    {
        if (project.DockerPort == Constants.DefaultHttpsPort)
        {
            return "https://localhost";
        }
        return "https://localhost:" + project.DockerPort;
    }

    enum Platform
    {
        Windows,
        Linux,
    }

    class DockerConfig
    {
        public string EnvFilePath { get; init; }
        public string CustomAssetsPath { get; init; }
        public string ModelPath { get; init; }
        public string CmdFilePath { get; init; }
        public string BaseImage { get; init; }
    }
}
