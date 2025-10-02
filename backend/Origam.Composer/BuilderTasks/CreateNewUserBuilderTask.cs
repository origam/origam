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
using Origam.DA;
using Origam.Security.Common;

namespace Origam.Composer.BuilderTasks;

public class CreateNewUserBuilderTask : AbstractDatabaseBuilderTask
{
    public override string Name => "Create new Admin user (Client web application)";

    public override void Execute(Project project)
    {
        var adaptivePassword = new InternalPasswordHasherWithLegacySupport(); // TODO: DI

        DataService(project.DatabaseType).DbUser = project.Name;
        DataService(project.DatabaseType).ConnectionString = project.BuilderDataConnectionString; // TODO: Refactor

        var parameters = new QueryParameterCollection
        {
            new QueryParameter("Id", Guid.NewGuid().ToString()),
            new QueryParameter("UserName", project.WebUserName),
            new QueryParameter("Password", adaptivePassword.HashPassword(project.WebUserPassword)),
            new QueryParameter("FirstName", project.WebFirstName),
            new QueryParameter("Name", project.WebSurname),
            new QueryParameter("Email", project.WebEmail),
            new QueryParameter("RoleId", Common.Constants.OrigamRoleSuperUserId),
            new QueryParameter("RequestEmailConfirmation", "false"),
        };
        DataService(project.DatabaseType).CreateFirstNewWebUser(parameters);
    }

    public override void Rollback(Project project) { }
}
