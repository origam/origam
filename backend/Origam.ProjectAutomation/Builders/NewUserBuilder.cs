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

using System;
using Origam.DA;
using Origam.DA.Common.DatabasePlatform;
using Origam.Security.Common;

namespace Origam.ProjectAutomation.Builders;

public class NewUserBuilder : AbstractDatabaseBuilder
{
    DatabaseType _databaseType;
    public override string Name => "Create Web User";

    public override void Execute(Project project)
    {
        var adaptivePassword = new InternalPasswordHasherWithLegacySupport();
        string hashPassword = adaptivePassword.HashPassword(project.WebUserPassword);
        _databaseType = project.DatabaseType;
        DataService(_databaseType).DbUser = project.Name;
        DataService(_databaseType).ConnectionString = project.BuilderDataConnectionString;
        QueryParameterCollection parameters = new QueryParameterCollection();
        parameters.Add(new QueryParameter("Id", Guid.NewGuid().ToString()));
        parameters.Add(new QueryParameter("UserName", project.WebUserName));
        parameters.Add(new QueryParameter("Password", hashPassword));
        parameters.Add(new QueryParameter("FirstName", project.WebFirstName));
        parameters.Add(new QueryParameter("Name", project.WebSurname));
        parameters.Add(new QueryParameter("Email", project.WebEmail));
        parameters.Add(new QueryParameter("RoleId", "E0AD1A0B-3E05-4B97-BE38-12FF63E7F2F2"));
        parameters.Add(new QueryParameter("RequestEmailConfirmation", "false"));
        DataService(_databaseType).CreateFirstNewWebUser(parameters);
    }

    private string BuildConnectionString(Project project)
    {
        return DataService(_databaseType)
            .BuildConnectionString(
                project.DatabaseServerName,
                project.DatabasePort,
                project.DataDatabaseName,
                project.DatabaseUserName,
                project.DatabasePassword,
                project.DatabaseIntegratedAuthentication,
                false
            );
    }

    public override void Rollback() { }
}
