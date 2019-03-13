using Origam.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam.ProjectAutomation.Builders
{
    class GitBuilder : AbstractBuilder
    {
        public override string Name => "Initialize Git";
        public override void Execute(Project project)
        {
            if (project.GitRepository)
            {
                GitManager.CreateRepository(project.SourcesFolder);
                GitManager gitmanager = new GitManager(project.SourcesFolder);
                gitmanager.Init(project.Gitusername,project.Gitemail);
            }
        }

        public override void Rollback()
        {
            OrigamEngine.OrigamEngine.UnloadConnectedServices();
        }
    }
}
