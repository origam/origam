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

using System.Collections.Generic;
using Origam.DA.Common.DatabasePlatform;
using Origam.ProjectAutomation.Builders;

namespace Origam.ProjectAutomation;

public class ProjectBuilder
{
    private readonly List<IProjectBuilder> tasks = new();
    private readonly SettingsBuilder settingsBuilder = new();
    private readonly DataDatabaseBuilder dataDatabaseBuilder = new();
    private readonly DockerBuilder dockerBuilder;

    public ProjectBuilder()
    {
        dockerBuilder = new DockerBuilder(dataDatabaseBuilder: dataDatabaseBuilder);
    }

    public void Create(Project project)
    {
        dataDatabaseBuilder.ResetDataservice();
        project.DataConnectionString = dataDatabaseBuilder.BuildConnectionString(
            project: project,
            pooling: true
        );
        project.BuilderDataConnectionString = dataDatabaseBuilder.BuildConnectionStringArchitect(
            project: project,
            pooling: false
        );
        project.BaseUrl = dockerBuilder.WebSiteUrl(project: project);
        IProjectBuilder activeTask = null;
        try
        {
            foreach (IProjectBuilder builder in tasks)
            {
                activeTask = builder;
                builder.State = TaskState.Running;
                builder.Execute(project: project);
                builder.State = TaskState.Finished;
            }
        }
        catch
        {
            activeTask.State = TaskState.Failed;
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                Rollback(builder: tasks[index: i]);
            }
            throw;
        }
    }

    public void CreateTasks(Project project)
    {
        tasks.Clear();
        if (project.DatabaseType == DatabaseType.MsSql)
        {
            tasks.Add(item: settingsBuilder);
            tasks.Add(item: dataDatabaseBuilder);
            tasks.Add(item: new FileModelImportBuilder());
            tasks.Add(item: new FileModelInitBuilder());
            tasks.Add(item: new DataDatabaseStructureBuilder());
            tasks.Add(item: new ApplyDatabasePermissionsBuilder());
            tasks.Add(item: new NewPackageBuilder());
        }
        if (project.DatabaseType == DatabaseType.PgSql)
        {
            tasks.Add(item: new FileModelImportBuilder());
            tasks.Add(item: settingsBuilder);
            tasks.Add(item: dataDatabaseBuilder);
            tasks.Add(item: new ApplyDatabasePermissionsBuilder());
            tasks.Add(item: new FileModelInitBuilder());
            tasks.Add(item: new DataDatabaseStructureBuilder());
            tasks.Add(item: new NewPackageBuilder());
        }
        tasks.Add(item: new NewUserBuilder());
        tasks.Add(item: dockerBuilder);
        AddGitTasks(project: project);
    }

    private void AddGitTasks(Project project)
    {
        if (project.GitRepository)
        {
            tasks.Add(item: new CreateGitRepository());
        }
    }

    #region Properties
    public List<IProjectBuilder> Tasks => tasks;

    #endregion
    private void Rollback(IProjectBuilder builder)
    {
        try
        {
            if (builder.State == TaskState.Finished)
            {
                builder.State = TaskState.RollingBack;
                builder.Rollback();
                builder.State = TaskState.RolledBack;
            }
        }
        catch
        {
            builder.State = TaskState.RollbackFailed;
        }
    }
}
