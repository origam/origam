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

public class ApplyDatabasePermissionsBuilderTask : AbstractDatabaseBuilderTask
{
    public override string Name => "Add new database user (with permissions)";

    public override void Execute(Project project)
    {
        DataService(project.DatabaseType).DbUser = project.DatabaseInternalUserName;
        DataService(project.DatabaseType).ConnectionString = BuildSuperAdminConnectionString(
            project
        );

        // MSSQL: User will be created only if project.DatabaseIntegratedAuthentication == true
        DataService(project.DatabaseType)
            .CreateDatabaseUser(
                user: project.DatabaseInternalUserName,
                password: project.DatabaseInternalUserPassword,
                name: project.DatabaseName,
                databaseIntegratedAuthentication: project.DatabaseIntegratedAuthentication
            );
    }

    private string BuildSuperAdminConnectionString(Project project)
    {
        return DataService(project.DatabaseType)
            .BuildConnectionString(
                project.DatabaseHost,
                project.DatabasePort,
                project.DatabaseName,
                project.DatabaseUserName,
                project.DatabasePassword,
                project.DatabaseIntegratedAuthentication,
                false
            );
    }

    public override void Rollback(Project project)
    {
        DataService(project.DatabaseType)
            .DeleteUser(project.DatabaseInternalUserName, project.DatabaseIntegratedAuthentication);
    }
}
