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

using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.Composer.Interfaces.Services;
using Spectre.Console;

namespace Origam.Composer.Services;

public class ProjectBuilderService(
    IDownloadFileModelBuilderTask downloadFileModelBuilderTask,
    ICreateDatabaseBuilderTask createDatabaseBuilderTask,
    IApplyDatabasePermissionsBuilderTask applyDatabasePermissionsBuilderTask,
    IInitFileModelBuilderTask initFileModelBuilderTask,
    ICreateDatabaseStructureBuilderTask createDatabaseStructureBuilderTask,
    ICreateNewPackageBuilderTask createNewPackageBuilderTask,
    ICreateNewUserBuilderTask createNewUserBuilderTask,
    IPrintOrigamSettingsBuilderTask printOrigamSettingsBuilderTask
) : IProjectBuilderService
{
    private readonly List<IBuilderTask> Tasks = [];

    public void Create(Project project)
    {
        SecurityManager.SetServerIdentity();

        IBuilderTask activeTask = null;
        try
        {
            foreach (IBuilderTask builder in Tasks)
            {
                activeTask = builder;
                builder.State = BuilderTaskState.Running;
                AnsiConsole.MarkupLine(
                    value: string.Format(format: Strings.Executing_task, arg0: builder.Name)
                );
                builder.Execute(project: project);
                builder.State = BuilderTaskState.Finished;
            }
        }
        catch
        {
            if (activeTask != null)
            {
                activeTask.State = BuilderTaskState.Failed;
            }
            for (var i = Tasks.Count - 1; i >= 0; i--)
            {
                RollbackTask(builderTask: Tasks[index: i], project: project);
            }
            throw;
        }
    }

    public void PrepareTasks(Project project)
    {
        Tasks.Add(item: printOrigamSettingsBuilderTask);
        Tasks.Add(item: downloadFileModelBuilderTask);
        Tasks.Add(item: createDatabaseBuilderTask);
        Tasks.Add(item: applyDatabasePermissionsBuilderTask);
        Tasks.Add(item: initFileModelBuilderTask);
        Tasks.Add(item: createDatabaseStructureBuilderTask);
        Tasks.Add(item: createNewPackageBuilderTask);
        Tasks.Add(item: createNewUserBuilderTask);
    }

    public List<IBuilderTask> GetTasks()
    {
        return Tasks;
    }

    private void RollbackTask(IBuilderTask builderTask, Project project)
    {
        try
        {
            if (builderTask.State == BuilderTaskState.Finished)
            {
                builderTask.State = BuilderTaskState.RollingBack;
                builderTask.Rollback(project: project);
                builderTask.State = BuilderTaskState.RolledBack;
            }
        }
        catch
        {
            builderTask.State = BuilderTaskState.RollbackFailed;
        }
    }
}
