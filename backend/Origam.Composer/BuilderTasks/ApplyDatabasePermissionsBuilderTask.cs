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
using Origam.Composer.Services;
using static Origam.DA.Common.Enums;

namespace Origam.Composer.ProjectBuilderTasks;

public class ApplyDatabasePermissionsBuilderTask : AbstractDatabaseBuilderTask
{
    public override string Name => "Apply database permissions";

    private DatabaseType DatabaseType;
    private string LoginName;
    private bool IsIntegratedAuthentication;

    public override void Execute(Project project)
    {
        DatabaseType = project.DatabaseType;

        DataService(DatabaseType).DbUser = project.Name;
        LoginName = DataService(DatabaseType).DbUser;
        IsIntegratedAuthentication = project.DatabaseIntegratedAuthentication;

        DataService(DatabaseType).ConnectionString = BuildConnectionString(project);
        DataService(DatabaseType)
            .CreateDatabaseUser(
                LoginName,
                project.UserPassword,
                project.DatabaseName,
                project.DatabaseIntegratedAuthentication
            );
    }

    private string BuildConnectionString(Project project)
    {
        var dataService = DataService(DatabaseType)
            .BuildConnectionString(
                project.DatabaseHost,
                project.DatabasePort,
                project.DatabaseName,
                project.DatabaseUserName,
                project.DatabasePassword,
                project.DatabaseIntegratedAuthentication,
                false
            );

        return dataService;
    }

    public override void Rollback()
    {
        DataService(DatabaseType).DeleteUser(LoginName, IsIntegratedAuthentication);
    }
}
