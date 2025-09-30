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

namespace Origam.Composer.BuilderTasks;

public class CreateDatabaseBuilderTask : AbstractDatabaseBuilderTask
{
    public override string Name => "Create database (new empty)";

    private Project? Project;

    public override void Execute(Project project)
    {
        Project = project;

        CreateDatabase(project);
        CreateSchema(project);
        DataService(project.DatabaseType).ConnectionString = BuildConnectionString(project, "");
    }

    private void CreateDatabase(Project project)
    {
        DataService(project.DatabaseType).ConnectionString = BuildConnectionString(project, "");
        DataService(project.DatabaseType).CreateDatabase(project.DatabaseName);
    }

    private void CreateSchema(Project project)
    {
        DataService(project.DatabaseType).ConnectionString = BuildConnectionString(
            project,
            project.DatabaseName
        );
        DataService(project.DatabaseType).CreateSchema(project.DatabaseName);
    }

    private string BuildConnectionString(Project project, string databaseName)
    {
        return DataService(project.DatabaseType)
            .BuildConnectionString(
                project.DatabaseHost,
                project.DatabasePort,
                databaseName,
                project.DatabaseUserName,
                project.DatabasePassword,
                project.DatabaseIntegratedAuthentication,
                false
            );
    }

    public override void Rollback()
    {
        OrigamUserContext.Reset();
        if (Project != null)
        {
            DataService(Project.DatabaseType).DeleteDatabase(Project.DatabaseName);
        }
    }

    public string? BuildConnectionStringArchitect(Project project, bool pooling)
    {
        if (project.DatabaseType == DA.Common.Enums.DatabaseType.MsSql)
        {
            var dataService = DataService(project.DatabaseType)
                .BuildConnectionString(
                    project.DatabaseHost,
                    project.DatabasePort,
                    project.DatabaseName,
                    project.DatabaseUserName,
                    project.DatabasePassword,
                    project.DatabaseIntegratedAuthentication,
                    pooling
                );

            return dataService;
        }

        if (project.DatabaseType == DA.Common.Enums.DatabaseType.PgSql)
        {
            DataService(project.DatabaseType).DbUser = project.Name;
            var dataService = DataService(project.DatabaseType)
                .BuildConnectionString(
                    project.DatabaseHost,
                    project.DatabasePort,
                    project.DatabaseName,
                    DataService(project.DatabaseType).DbUser,
                    project.UserPassword,
                    project.DatabaseIntegratedAuthentication,
                    pooling
                );

            return dataService;
        }

        return null;
    }
}
