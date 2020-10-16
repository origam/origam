using System;
using System.IO;
using static Origam.DA.Common.Enums;

namespace Origam.ProjectAutomation.Builders
{
    public class DockerBuilder : AbstractBuilder
    {
        public override string Name => "Create Docker run script";

        public override void Execute(Project project)
        {
            if(Directory.Exists(project.RootSourceFolder))
            {
                string scriptsdirectory = project.RootSourceFolder;
                File.WriteAllText(Path.Combine(scriptsdirectory, project.Url+ ".env"), CreateEnvFile(project));
                File.WriteAllText(Path.Combine(scriptsdirectory, project.Url + ".cmd"), CreateDockerRunFile(project));
            }
        }

        private string CreateDockerRunFile(Project _project)
        {
            return "docker run --env-file " + _project.SourcesFolder + ".env -it  -v " + _project.RootSourceFolder + ":/home/origam/HTML5/data -p "+_project.DockerPort+":8080 origam/server";
        }

        private string CreateEnvFile(Project _project)
        {
            string Dockerenviroment = "gitPullOnStart=false";
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "OrigamSettings_SetOnStart=true";
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "OrigamSettings_SchemaExtensionGuid=" + _project.NewPackageId;
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "OrigamSettings_DbHost=" + _project.DatabaseServerName;
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "OrigamSettings_DbPort=" + (_project.Port);
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "OrigamSettings_DbUsername=" + _project.DatabaseUserName;
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "OrigamSettings_DbPassword=" + _project.DatabasePassword;
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "DatabaseName=" + _project.DataDatabaseName.ToLower();
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "OrigamSettings_ModelName=" + _project.Url;
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "DatabaseType=" + (_project.DatabaseType==DatabaseType.PgSql?"postgresql":_project.DatabaseType.ToString().ToLower());
            Dockerenviroment += Environment.NewLine;
            Dockerenviroment += "ExternalDomain_SetOnStart=" + WebSiteUrl(_project);
            return Dockerenviroment;
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
