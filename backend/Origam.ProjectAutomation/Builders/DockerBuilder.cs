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

namespace Origam.ProjectAutomation.Builders
{
    public class DockerBuilder : AbstractBuilder
    {
        public static readonly string DockerFolderName = "Docker";
        public override string Name => "Create Docker run script";
        private string newProjectFolder;
        public override void Execute(Project project)
        {
            newProjectFolder = Path.Combine(project.SourcesFolder, DockerFolderName);
            if (Directory.Exists(newProjectFolder))
            {
                string envFile = Path.Combine(newProjectFolder, project.Name + ".env");
                if (Directory.GetFiles(newProjectFolder, "*.env").Length == 0)
                {
                    File.Create(envFile).Dispose();
                }
                project.DockerEnvPath = Directory.GetFiles(newProjectFolder, "*.env")[0]; 
                ProcessEnvironmentFile(project);
                ProcessCmdFile(project);
            }
        }
        private void ProcessCmdFile(Project project)
        {
            string cmdFile = Path.Combine(newProjectFolder, project.Name + ".cmd");
            if (File.Exists(cmdFile))
            {
                if (project.Deployment == DeploymentType.Docker)
                {
                    string text = File.ReadAllText(cmdFile);
                    text = text.Replace("{envFilePath}", Path.Combine(project.SourcesFolder,
                        DockerFolderName, project.DockerEnvPath));
                    text = text.Replace("{parentPathProject}", project.SourcesFolder);
                    text = text.Replace("{dockerPort}", project.DockerPort.ToString());
                    text = text.Replace("{projectName}", project.Name);
                    File.WriteAllText(cmdFile, text);
                }
                else if(project.Deployment == DeploymentType.DockerPostgres)
                {
                    File.Delete(cmdFile);
                    cmdFile = Path.Combine(newProjectFolder, "StartWebServer_" + project.Name + 
                        project.DockerOs.FileNameExtension);
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("docker start " + project.Name);
                    stringBuilder.Append("docker exec --env-file " + project.DockerEnvPath + 
                        " -it " + project.Name + 
                        " bash startOrigamServer.sh");
                    File.WriteAllText(cmdFile, stringBuilder.ToString());
                    cmdFile = Path.Combine(newProjectFolder, "StartContainer_" + project.Name +
                        project.DockerOs.FileNameExtension);
                    string text = "docker start " + project.Name;
                    File.WriteAllText(cmdFile, text);
                    cmdFile = Path.Combine(newProjectFolder, "CreateContainer_" + project.Name + 
                        project.DockerOs.FileNameExtension);
                    File.WriteAllText(cmdFile, FillDockerParameter(project));
                }
            }
        }

        private string FillDockerParameter(Project project)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("docker run ");
            stringBuilder.AppendFormat("--env-file {0} ", project.DockerEnvPath);
            stringBuilder.AppendFormat("--mount source={0},target=/var/lib/postgresql ",project.Name);
            stringBuilder.AppendFormat("-v {0}:/home/origam/HTML5/data/origam ",project.DockerSourcePath);
            stringBuilder.AppendFormat("-p {0}:5433 -p {1}:8080 ",project.Port,project.DockerPort);
            stringBuilder.AppendFormat("-e OrigamSettings_DbPassword={0} ",project.DatabaseAdminPassword);
            stringBuilder.AppendFormat("origam/server:pg_{0} ", "master-latest".GetAssemblyVersion());
            return stringBuilder.ToString();
        }

        private void ProcessEnvironmentFile(Project project)
        {
            List<string> dockerParameters = FillDockerParameters();
            List<string> dockerCustomAssetsParameters = FillCustomAssetsParameters();
            string customAssetsDirectory = Path.Combine(project.SourcesFolder, "customAssets");
            string[] envFileLine = File.ReadAllLines(project.DockerEnvPath);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary = envFileLine.Select(line => line.Split('=')).
                ToDictionary(line => line[0], line => CheckValue(line, dockerParameters, project));
            List<string> missingEnvironments = dockerParameters.
                Where(missing => !dictionary.TryGetValue(missing, out string val)).
                Select(missing => { return missing; }).ToList();
            foreach(string missing in missingEnvironments)
            {
                    dictionary.Add(missing, CheckValue(new string[] { missing, "" }, 
                        dockerParameters, project));
            }
            if (Directory.Exists(customAssetsDirectory) &&
                Directory.GetFiles(customAssetsDirectory).Length > 0)
            {
                List<string>  missingCustomAssetsEnvironments = dockerCustomAssetsParameters.
                Where(missing => !dictionary.TryGetValue(missing, out string val)).
                Select(missing => { return missing; }).ToList();
                foreach (string missing in missingCustomAssetsEnvironments)
                {
                    dictionary.Add(missing, CheckValue(new string[] { missing, "" }, 
                        missingCustomAssetsEnvironments, project));
                }
            }
            string[] lines = dictionary.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray();
            File.WriteAllLines(project.DockerEnvPath, lines);
            dictionary.Clear();
        }
        private List<string> FillCustomAssetsParameters()
        {
            List<string> parameters = new List<string>
            {
                "CustomAssetsConfig__PathToCustomAssetsFolder",
                "CustomAssetsConfig__RouteToCustomAssetsFolder",
                "CustomAssetsConfig__IdentityGuiLogoUrl",
                "CustomAssetsConfig__Html5ClientLogoUrl"
            };
            return parameters;
        }

        private string CheckValue(string[] line, List<string> dockerParameters, Project project)
        {
            foreach(string dockerParameter in dockerParameters)
            {
                if(line[0].Equals(dockerParameter) && line[1].Length==0)
                {
                    switch (dockerParameter)
                    {
                        case "gitPullOnStart":
                            return "false";
                        case "OrigamSettings_SetOnStart":
                            return "true";
                        case "OrigamSettings_SchemaExtensionGuid":
                            return project.NewPackageId;
                        case "OrigamSettings_DbHost":
                            return SetDbHost(project);
                        case "OrigamSettings_DbPort":
                            return project.Port.ToString();
                        case "OrigamSettings_DbUsername":
                            return project.DatabaseType == DatabaseType.PgSql ?
                                                        project.Name : project.DatabaseUserName; 
                        case "OrigamSettings_DbPassword":
                            return project.DatabaseType == DatabaseType.PgSql ?
                                                        project.UserPassword : project.DatabasePassword;
                        case "DatabaseName":
                            return project.DataDatabaseName.ToLower();
                        case "OrigamSettings_ModelName":
                            return "origam";
                        case "DatabaseType":
                            return project.DatabaseType == DatabaseType.PgSql ? "postgresql" :
                                                                project.DatabaseType.ToString().ToLower();
                        case "ExternalDomain_SetOnStart":
                            return WebSiteUrl(project);
                        case "CustomAssetsConfig__PathToCustomAssetsFolder":
                            return "/home/origam/HTML5/data/" + project.Url + "/customAssets";
                        case "CustomAssetsConfig__IdentityGuiLogoUrl":
                            return "/customAssets/login-logo.png";
                        case "CustomAssetsConfig__RouteToCustomAssetsFolder":
                            return "/customAssets";
                        case "CustomAssetsConfig__Html5ClientLogoUrl":
                            return "/customAssets/logo-left.png";
                        default:
                            return "";
                    }
                }
            }
            return line[1];
        }

        private string SetDbHost(Project project)
        {
            if (project.Deployment == DeploymentType.DockerPostgres)
            {
                return "localhost";
            }
            if (project.Deployment == DeploymentType.Docker)
            {
                if (project.DatabaseServerName.Equals("localhost") ||
                    project.DatabaseServerName.Equals("127.0.0.1"))
                {
                    return "host.docker.internal";
                }
            }
            return project.DatabaseServerName;
        }

        private List<string> FillDockerParameters()
        {
            List<string> parameters = new List<string>
            {
                "gitPullOnStart",
                "OrigamSettings_SetOnStart",
                "OrigamSettings_SchemaExtensionGuid",
                "OrigamSettings_DbHost",
                "OrigamSettings_DbPort",
                "OrigamSettings_DbUsername",
                "OrigamSettings_DbPassword",
                "DatabaseName",
                "OrigamSettings_ModelName",
                "DatabaseType",
                "ExternalDomain_SetOnStart",
            };
            return parameters;
        }
        public override void Rollback()
        {
        }
        public string WebSiteUrl (Project project)
        {
            return "http://localhost:" + project.DockerPort;
        }
    }
}