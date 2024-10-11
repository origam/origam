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

using Origam.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Origam.DA.Common.Enums;

namespace Origam.ProjectAutomation.Builders;
public class DockerBuilder : AbstractBuilder
{
    public static readonly string DockerFolderName = "Docker";
    public override string Name => "Create Docker run script";
    private string newProjectFolder;
    public override void Execute(Project project)
    {
        newProjectFolder = Path.Combine(project.SourcesFolder, DockerFolderName);
        project.DockerEnvPath = Path.Combine(newProjectFolder, project.Name + ".env");
        project.DockerCmdPath = Path.Combine(newProjectFolder, project.Name + ".cmd");
        Directory.CreateDirectory(newProjectFolder);
        CreateEnvFile(project);
        CreateCmdFile(project);
    }
    private void CreateEnvFile(Project project)
    {
        string dbType = project.DatabaseType == DatabaseType.PgSql 
            ? "postgresql" :
            project.DatabaseType.ToString().ToLower();
        string dbUserName =project.DatabaseType == DatabaseType.PgSql 
            ? project.Name 
            : project.DatabaseUserName; 
        string dbPassword = project.DatabaseType == DatabaseType.PgSql
            ? project.UserPassword
            : project.DatabasePassword;
        string content =
            $"OrigamSettings_SetOnStart=true\n" +
            $"OrigamSettings_SchemaExtensionGuid={project.NewPackageId}\n" +
            $"OrigamSettings_DbHost={GetDbHost(project)}\n" +
            $"OrigamSettings_DbPort={project.DatabasePort}\n" +
            $"OrigamSettings_DbUsername={dbUserName}\n" +
            $"OrigamSettings_DbPassword={dbPassword}\n" +
            $"DatabaseName={project.DataDatabaseName.ToLower()}\n" +
            $"DatabaseType={dbType}\n" +
            $"ExternalDomain_SetOnStart={WebSiteUrl(project)}\n" +
            "TZ=Europe/Prague";
        File.WriteAllText(project.DockerEnvPath, content);
    }
    private void CreateCmdFile(Project project)
    {
        string content =
            $"docker run --env-file \"{project.DockerEnvPath}\" ^\n" +
            $"    -it --name {project.Name} ^\n" +
            $"    -v \"{project.SourcesFolder}\\model\":/home/origam/HTML5/data/origam ^\n" +
            $"    -p {project.DockerPort}:443 ^\n" +
            $"    origam/server:master-latest.linux\n" +
            "\n" +
            "REM After you run the above command go to https://localhost to open the client web application.\n" +
            "\n" +
            "REM origam/server:master-latest.linux is the latest version, that may not be what you want.\n" +
            "REM Here you can find current releases and their docker images:\n" +
            "REM https://github.com/origam/origam/releases";
        File.WriteAllText(project.DockerCmdPath, content);
    }
    private string GetDbHost(Project project)
    {
        if (project.DatabaseServerName.Equals("localhost") ||
            project.DatabaseServerName.Equals(".") ||
            project.DatabaseServerName.Equals("127.0.0.1"))
        {
            return "host.docker.internal";
        }
        return project.DatabaseServerName;
    }
    public override void Rollback()
    {
    }
    public string WebSiteUrl (Project project)
    {
        if (project.DockerPort == Constants.DefaultHttpsPort)
        {
            return "https://localhost";
        }
        return "https://localhost:" + project.DockerPort;
    }
}
