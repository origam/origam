using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Origam.DA.Common.Enums;

namespace Origam.ProjectAutomation.Builders
{
    public class DockerBuilder : AbstractBuilder
    {
        public override string Name => "Create Docker run script";
        private string newProjectFolder;
        public override void Execute(Project project)
        {
            newProjectFolder = Path.Combine(project.SourcesFolder, "NewProject");
            if (Directory.Exists(newProjectFolder))
            {
                string envfile = Path.Combine(newProjectFolder, project.Name + ".env");
                if (Directory.GetFiles(newProjectFolder, "*.env").Length == 0)
                {
                    File.Copy(Path.Combine(newProjectFolder, "docker.env.template"), Path.Combine(newProjectFolder, envfile));
                }
                envfile = Directory.GetFiles(newProjectFolder, "*.env")[0];
                ProcessEnviromentFile(envfile,project);
                ProcessCmdFile(project,envfile);
            }
        }
        private void ProcessCmdFile(Project project, string envfile)
        {
            string cmdfile = Path.Combine(newProjectFolder, project.Name + ".cmd");
            if (!File.Exists(cmdfile))
            {
                File.Copy(Path.Combine(newProjectFolder, "docker.cmd.template"), Path.Combine(newProjectFolder, cmdfile));
            }
            string text = File.ReadAllText(cmdfile);
            text = text.Replace("{envfilepath}", Path.Combine(project.SourcesFolder, "NewProject", envfile));
            text = text.Replace("{parentpathproject}", project.RootSourceFolder);
            text = text.Replace("{dockerport}", project.DockerPort.ToString());
            File.WriteAllText(cmdfile, text);
        }
        private void ProcessEnviromentFile(string envfile, Project project)
        {
            List<string> dockerparameters = FillDockerParameters();
            List<string> dockerCustomAssetsparameters = FillCustomAssetsParameters();
            string customassetsDirectory = Path.Combine(project.SourcesFolder, "customAssets");
            string[] envfileline = File.ReadAllLines(envfile);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary = envfileline.Select(line => line.Split('=')).
                ToDictionary(line => line[0], line => CheckValue(line, dockerparameters, project));
            List<string> missingEnviroments = dockerparameters.
                Where(missing => !dictionary.TryGetValue(missing, out string val)).
                Select(missing => { return missing; }).ToList();
            foreach(string missing in missingEnviroments)
            {
                    dictionary.Add(missing, CheckValue(new string[] { missing, "" }, dockerparameters, project));
            }
         if (Directory.Exists(customassetsDirectory))
            {
                List<string>  missingCustomAssetsEnviroments = dockerCustomAssetsparameters.
                Where(missing => !dictionary.TryGetValue(missing, out string val)).
                Select(missing => { return missing; }).ToList();
                foreach (string missing in missingCustomAssetsEnviroments)
                {
                    dictionary.Add(missing, CheckValue(new string[] { missing, "" }, missingCustomAssetsEnviroments, project));
                }
            }
            string[] lines = dictionary.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray();
            File.WriteAllLines(envfile, lines);
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

        private string CheckValue(string[] line, List<string> dockerparameters, Project project)
        {
            foreach(string dockerparameter in dockerparameters)
            {
                if(line[0].Equals(dockerparameter) && line[1].Length==0)
                {
                    switch (dockerparameter)
                    {
                        case "OrigamSettings_SchemaExtensionGuid":
                            return project.NewPackageId;
                        case "OrigamSettings_DbHost":
                            return project.DatabaseServerName;
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
                            return project.Url;
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
        private List<string> FillDockerParameters()
        {
            List<string> parameters = new List<string>
            {
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