using Origam.Git;
using System;

namespace Origam.ProjectAutomation.Builders
{
    class CreateGitRepository : AbstractBuilder
    {
        public override string Name => "Init Git";

        public override void Execute(Project project)
        {
            GitManager.CreateRepository(project.SourcesFolder);
            GitManager gitmanager = new GitManager(project.SourcesFolder);
            gitmanager.Init(project.Gitusername, project.Gitemail);
        }

        public override void Rollback()
        {
          
        }
    }
}
