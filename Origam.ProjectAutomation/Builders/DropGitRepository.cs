using Origam.Git;

namespace Origam.ProjectAutomation.Builders
{
    class DropGitRepository : AbstractBuilder
    {
        public override string Name => "Remove Git From Project";

        public override void Execute(Project project)
        {
            GitManager.RemoveRepository(project.SourcesFolder);
        }

        public override void Rollback()
        {
            
        }
    }
}
