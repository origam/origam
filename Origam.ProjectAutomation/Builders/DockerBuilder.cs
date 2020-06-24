using System.IO;

namespace Origam.ProjectAutomation.Builders
{
    public class DockerBuilder : AbstractBuilder
    {
        public override string Name => "Create Docker run script";

        public override void Execute(Project project)
        {
            if(Directory.Exists(project.SourcesFolder))
            {
                string scriptsdirectory = Path.Combine(project.SourcesFolder, "scripts");
                Directory.CreateDirectory(scriptsdirectory);
                File.WriteAllText(Path.Combine(scriptsdirectory, "env.list"), project.DockerEnviroment);
                File.WriteAllText(Path.Combine(scriptsdirectory, "runDocker"), project.DockerRun);
            }
        }

        public override void Rollback()
        {
        }
    }
}
