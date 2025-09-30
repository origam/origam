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

using Origam.Composer.BuilderTasks;
using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.Composer.Interfaces.Services;
using Spectre.Console;

namespace Origam.Composer.Services;

public class ProjectBuilderService : IProjectBuilderService
{
    private readonly List<IBuilderTask> Tasks = [];
    private readonly CreateDatabaseBuilderTask CreateDatabaseBuilderTask = new();

    public ProjectBuilderService()
    {
        SecurityManager.SetServerIdentity();
    }

    public void Create(Project project)
    {
        project.BuilderDataConnectionString =
            CreateDatabaseBuilderTask.BuildConnectionStringArchitect(project, false);

        IBuilderTask activeTask = null;
        try
        {
            foreach (IBuilderTask builder in Tasks)
            {
                activeTask = builder;
                builder.State = BuilderTaskState.Running;
                AnsiConsole.MarkupLine($"[orange1][bold]Executing:[/][/] {builder.Name}");
                builder.Execute(project);
                builder.State = BuilderTaskState.Finished;
            }
        }
        catch
        {
            activeTask.State = BuilderTaskState.Failed;
            for (var i = Tasks.Count - 1; i >= 0; i--)
            {
                RollbackTask(Tasks[i]);
            }
            throw;
        }
    }

    public void PrepareTasks(Project project)
    {
        Tasks.Add(new DownloadFileModelBuilderTask());
        Tasks.Add(CreateDatabaseBuilderTask);
        // Tasks.Add(new ApplyDatabasePermissionsBuilder());

        Tasks.Add(new InitFileModelBuilderTask());
        Tasks.Add(new CreateDatabaseStructureBuilderTask());
        Tasks.Add(new CreateNewPackageBuilderTask());
        Tasks.Add(new CreateNewUserBuilderTask());
        Tasks.Add(new DockerBuilderTask());

        if (project.IsGitInit)
        {
            Tasks.Add(new CreateGitRepositoryBuilderTask());
        }
    }

    public List<IBuilderTask> GetTasks()
    {
        return Tasks;
    }

    private void RollbackTask(IBuilderTask builderTask)
    {
        try
        {
            if (builderTask.State == BuilderTaskState.Finished)
            {
                builderTask.State = BuilderTaskState.RollingBack;
                builderTask.Rollback();
                builderTask.State = BuilderTaskState.RolledBack;
            }
        }
        catch
        {
            builderTask.State = BuilderTaskState.RollbackFailed;
        }
    }
}
