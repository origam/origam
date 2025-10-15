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
using Origam.Composer.Interfaces.BuilderTasks;

namespace Origam.Composer.BuilderTasks;

public class CreateDatabaseBuilderTask : AbstractDatabaseBuilderTask, ICreateDatabaseBuilderTask
{
    public override string Name => Strings.BuilderTask_Create_database_new_empty;

    public override void Execute(Project project)
    {
        CreateDatabase(project: project);
        CreateSchema(project: project);

        DataService(databaseType: project.DatabaseType).DbUser = project.DatabaseInternalUserName;
        DataService(databaseType: project.DatabaseType).ConnectionString = BuildConnectionString(
            project: project,
            databaseName: ""
        );
    }

    private void CreateDatabase(Project project)
    {
        DataService(databaseType: project.DatabaseType).ConnectionString = BuildConnectionString(
            project: project,
            databaseName: ""
        );
        DataService(databaseType: project.DatabaseType).CreateDatabase(name: project.DatabaseName);
    }

    private void CreateSchema(Project project)
    {
        DataService(databaseType: project.DatabaseType).ConnectionString = BuildConnectionString(
            project: project,
            databaseName: project.DatabaseName
        );
        DataService(databaseType: project.DatabaseType)
            .CreateSchema(databaseName: project.DatabaseName);
    }

    private string BuildConnectionString(Project project, string databaseName)
    {
        return DataService(project.DatabaseType)
            .BuildConnectionString(
                serverName: project.DatabaseHost,
                port: project.DatabasePort,
                databaseName: databaseName,
                userName: project.DatabaseUserName,
                password: project.DatabasePassword,
                integratedAuthentication: project.DatabaseIntegratedAuthentication,
                pooling: false
            );
    }

    public override void Rollback(Project project)
    {
        OrigamUserContext.Reset();
        DataService(databaseType: project.DatabaseType).DeleteDatabase(name: project.DatabaseName);
    }
}
