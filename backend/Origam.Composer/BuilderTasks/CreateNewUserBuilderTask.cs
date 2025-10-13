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
using Origam.DA;

namespace Origam.Composer.BuilderTasks;

public class CreateNewUserBuilderTask : AbstractDatabaseBuilderTask, ICreateNewUserBuilderTask
{
    public override string Name => "Create new Admin user (Client web application)";

    public override void Execute(Project project)
    {
        var adaptivePassword = new Origam.Security.Common.InternalPasswordHasherWithLegacySupport();

        DataService(project.DatabaseType).DbUser = project.DatabaseInternalUserName;
        DataService(project.DatabaseType).ConnectionString = BuildConnectionStringArchitect(
            project
        );

        var parameters = new QueryParameterCollection
        {
            new QueryParameter("Id", Guid.NewGuid().ToString()),
            new QueryParameter("UserName", project.WebAdminUsername),
            new QueryParameter("Password", adaptivePassword.HashPassword(project.WebAdminPassword)),
            new QueryParameter("FirstName", project.WebAdminUsername),
            new QueryParameter("Name", project.WebAdminUsername),
            new QueryParameter("Email", project.WebAdminEmail),
            new QueryParameter("RoleId", Common.Constants.OrigamRoleSuperUserId),
            new QueryParameter("RequestEmailConfirmation", "false"),
        };
        DataService(project.DatabaseType).CreateFirstNewWebUser(parameters);
    }

    public override void Rollback(Project project) { }

    private string? BuildConnectionStringArchitect(Project project)
    {
        if (project.DatabaseType == DA.Common.Enums.DatabaseType.MsSql)
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

        if (project.DatabaseType == DA.Common.Enums.DatabaseType.PgSql)
        {
            return DataService(project.DatabaseType)
                .BuildConnectionString(
                    project.DatabaseHost,
                    project.DatabasePort,
                    project.DatabaseName,
                    project.DatabaseInternalUserName,
                    project.DatabaseInternalUserPassword,
                    project.DatabaseIntegratedAuthentication,
                    false
                );
        }

        return null;
    }
}
