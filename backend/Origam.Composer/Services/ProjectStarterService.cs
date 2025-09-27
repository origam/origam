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

using Origam.Composer.Builders;
using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.Services;
using static Origam.DA.Common.Enums;

namespace Origam.Composer.Services;

public class ProjectStarterService : IProjectStarterService
{
    private readonly List<IProjectBuilder> Tasks = [];
    private readonly CreateDatabaseBuilder CreateDatabaseBuilder = new();
    private readonly DockerBuilder DockerBuilder = new();

    public ProjectStarterService()
    {
        SecurityManager.SetServerIdentity();
    }

    public void Create(Project project)
    {
        CreateDatabaseBuilder.ResetDataservice();
        project.BuilderDataConnectionString = CreateDatabaseBuilder.BuildConnectionStringArchitect(
            project,
            false
        );
        project.BaseUrl = DockerBuilder.WebSiteUrl(project);

        var settings = new OrigamSettings
        {
            Name = project.Name,
            TitleText = project.Name,
            DataConnectionString = project.BuilderDataConnectionString,
            ServerUrl = project.BaseUrl,
            DataDataService = project.GetDataDataService,
            SchemaDataService = project.GetDataDataService,
            ModelSourceControlLocation = project.ModelFolder,
        };
        ConfigurationManager.SetActiveConfiguration(settings);

        IProjectBuilder activeTask = null;
        try
        {
            foreach (IProjectBuilder builder in Tasks)
            {
                activeTask = builder;
                builder.State = TaskState.Running;
                builder.Execute(project);
                builder.State = TaskState.Finished;
            }
        }
        catch
        {
            activeTask.State = TaskState.Failed;
            for (var i = Tasks.Count - 1; i >= 0; i--)
            {
                Rollback(Tasks[i]);
            }
            throw;
        }
    }

    public void CreateTasks(Project project)
    {
        switch (project.DatabaseType)
        {
            case DatabaseType.MsSql:
                Tasks.Add(CreateDatabaseBuilder);
                Tasks.Add(new DownloadFileModelBuilder());
                // Tasks.Add(new ApplyDatabasePermissionsBuilder());
                break;
            case DatabaseType.PgSql:
                Tasks.Add(new DownloadFileModelBuilder());
                Tasks.Add(CreateDatabaseBuilder);
                // Tasks.Add(new ApplyDatabasePermissionsBuilder());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(project));
        }

        Tasks.Add(new InitFileModelBuilder());
        Tasks.Add(new CreateDatabaseStructureBuilder());
        Tasks.Add(new CreateNewPackageBuilder());
        Tasks.Add(new CreateNewUserBuilder());
        Tasks.Add(new DockerBuilder());

        if (project.IsGitInit)
        {
            Tasks.Add(new CreateGitRepositoryBuilder());
        }
    }

    public List<IProjectBuilder> GetTasks()
    {
        return Tasks;
    }

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
