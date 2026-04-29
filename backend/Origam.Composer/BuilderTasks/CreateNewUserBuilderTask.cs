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

using BrockAllen.IdentityReboot;
using Origam.Composer.DTOs;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.DA;
using Origam.DA.Common.DatabasePlatform;

namespace Origam.Composer.BuilderTasks;

public class CreateNewUserBuilderTask : AbstractDatabaseBuilderTask, ICreateNewUserBuilderTask
{
    public override string Name => Strings.BuilderTask_Create_new_Admin_user;

    public override void Execute(Project project)
    {
        var adaptivePassword = new AdaptivePasswordHasher();

        DataService(databaseType: project.DatabaseType).DbUser = project.DatabaseInternalUserName;
        DataService(databaseType: project.DatabaseType).ConnectionString =
            BuildConnectionStringArchitect(project: project);

        var parameters = new QueryParameterCollection
        {
            new QueryParameter(_parameterName: "Id", value: Guid.NewGuid().ToString()),
            new QueryParameter(_parameterName: "UserName", value: project.WebAdminUsername),
            new QueryParameter(
                _parameterName: "Password",
                value: adaptivePassword.HashPassword(password: project.WebAdminPassword)
            ),
            new QueryParameter(_parameterName: "FirstName", value: project.WebAdminUsername),
            new QueryParameter(_parameterName: "Name", value: project.WebAdminUsername),
            new QueryParameter(_parameterName: "Email", value: project.WebAdminEmail),
            new QueryParameter(
                _parameterName: "RoleId",
                value: Common.Constants.OrigamRoleSuperUserId
            ),
            new QueryParameter(_parameterName: "RequestEmailConfirmation", value: "false"),
        };
        DataService(databaseType: project.DatabaseType)
            .CreateFirstNewWebUser(parameters: parameters);
    }

    public override void Rollback(Project project) { }

    private string BuildConnectionStringArchitect(Project project)
    {
        if (project.DatabaseType == DatabaseType.MsSql)
        {
            return DataService(databaseType: project.DatabaseType)
                .BuildConnectionString(
                    serverName: project.DatabaseHost,
                    port: project.DatabasePort,
                    databaseName: project.DatabaseName,
                    userName: project.DatabaseUserName,
                    password: project.DatabasePassword,
                    integratedAuthentication: project.DatabaseIntegratedAuthentication,
                    pooling: false
                );
        }

        if (project.DatabaseType == DatabaseType.PgSql)
        {
            return DataService(databaseType: project.DatabaseType)
                .BuildConnectionString(
                    serverName: project.DatabaseHost,
                    port: project.DatabasePort,
                    databaseName: project.DatabaseName,
                    userName: project.DatabaseInternalUserName,
                    password: project.DatabaseInternalUserPassword,
                    integratedAuthentication: project.DatabaseIntegratedAuthentication,
                    pooling: false
                );
        }

        return null;
    }
}
