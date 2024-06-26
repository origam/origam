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

using Origam.Extensions;
using Origam.ProjectAutomation.Builders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Origam.DA.Common.Enums;
using static Origam.NewProjectEnums;

namespace Origam.ProjectAutomation;
public class ProjectBuilder
{
    private readonly List<IProjectBuilder> tasks = new List<IProjectBuilder>();
    private readonly SettingsBuilder settingsBuilder = new SettingsBuilder();
    private readonly DataDatabaseBuilder dataDatabaseBuilder = new DataDatabaseBuilder();
    private readonly ConfigureWebServerBuilder configureWebServerBuilder = new ConfigureWebServerBuilder();
    private readonly DockerBuilder dockerBuilder = new DockerBuilder();
    public ProjectBuilder()
    {           
        
    }
    public void Create(Project project)
    {
        dataDatabaseBuilder.ResetDataservice();
        //Wizard connection
        project.DataConnectionString =
        dataDatabaseBuilder.BuildConnectionString(project, true);
        //OrigamSettings
        project.BuilderDataConnectionString =
        dataDatabaseBuilder.BuildConnectionStringArchitect(project, false);
        switch (project.Deployment)
        {
            case DeploymentType.Local:
                project.BaseUrl =
                    configureWebServerBuilder.WebSiteUrl(project.WebRootName);
                break;
            case DeploymentType.Docker:
                project.BaseUrl =
                    dockerBuilder.WebSiteUrl(project);
                break;
            case DeploymentType.DockerPostgres:
                project.BaseUrl =
                    dockerBuilder.WebSiteUrl(project);
                break;
        }
        IProjectBuilder activeTask = null;
        try
        {
            foreach (IProjectBuilder builder in tasks)
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
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                Rollback(tasks[i]);
            }
            throw;
        }
    }
    public void CreateTasks(Project _project)
    {
        tasks.Clear();
        if (_project.DatabaseType == DatabaseType.MsSql)
        {
            tasks.Add(settingsBuilder);
            tasks.Add(dataDatabaseBuilder);
            tasks.Add(new FileModelImportBuilder());
            tasks.Add(new FileModelInitBuilder());
            tasks.Add(new DataDatabaseStructureBuilder());
            if (_project.Deployment == DeploymentType.Local)
            {
                tasks.Add(new ModifyConfigurationFilesBuilder());
                tasks.Add(configureWebServerBuilder);
            }
            tasks.Add(new ApplyDatabasePermissionsBuilder());
            tasks.Add(new NewPackageBuilder());
        }
        if (_project.DatabaseType == DatabaseType.PgSql)
        {
            tasks.Add(new FileModelImportBuilder());
            if (_project.Deployment == DeploymentType.DockerPostgres)
            {
                tasks.Add(new DockerBuilder());
                tasks.Add(new DockerCreator("master-latest".GetAssemblyVersion(),_project.DockerApiAddress));
            }
            tasks.Add(settingsBuilder);
            tasks.Add(dataDatabaseBuilder);
            tasks.Add(new ApplyDatabasePermissionsBuilder());
            tasks.Add(new FileModelInitBuilder());
            tasks.Add(new DataDatabaseStructureBuilder());
            if (_project.Deployment == DeploymentType.Local)
            {
                tasks.Add(new ModifyConfigurationFilesBuilder());
                tasks.Add(configureWebServerBuilder);
            }
            tasks.Add(new NewPackageBuilder());
        }
        tasks.Add(new NewUserBuilder());
        if (_project.Deployment == DeploymentType.Docker)
        {
            tasks.Add(new DockerBuilder());
        }
        AddGitTasks(_project);
    }
    private void AddGitTasks(Project _project)
    {
        switch (_project.TypeTemplate)
        {
            case TypeTemplate.Default:
                CreateGit(_project);
                break;
            case TypeTemplate.Template:
                tasks.Add(new DropGitRepository());
                CreateGit(_project);
                break;
            case TypeTemplate.Open:
                if (_project.TypeDoTemplate == TypeDoTemplate.Clone &&
                    _project.GitRepository)
                {
                    break;
                }
                tasks.Add(new DropGitRepository());
                if (_project.TypeDoTemplate == TypeDoTemplate.Copy)
                {
                    CreateGit(_project);
                }
                break;
        }
    }
    private void CreateGit(Project _project)
    {
        if (_project.GitRepository)
        {
            tasks.Add(new CreateGitRepository());
        }
    }
    #region Properties
    public List<IProjectBuilder> Tasks => tasks;
    public string[] WebSites() => configureWebServerBuilder.WebSites();
    #endregion
    private void Rollback(IProjectBuilder builder)
    {
        try
        {
            if(builder.State == TaskState.Finished)
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
