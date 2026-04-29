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
        string hashPassword = adaptivePassword.HashPassword(password: project.WebUserPassword);
        _databaseType = project.DatabaseType;
        DataService(DatabaseType: _databaseType).DbUser = project.Name;
        DataService(DatabaseType: _databaseType).ConnectionString =
            project.BuilderDataConnectionString;
        QueryParameterCollection parameters = new QueryParameterCollection();
        parameters.Add(
            value: new QueryParameter(_parameterName: "Id", value: Guid.NewGuid().ToString())
        );
        parameters.Add(
            value: new QueryParameter(_parameterName: "UserName", value: project.WebUserName)
        );
        parameters.Add(value: new QueryParameter(_parameterName: "Password", value: hashPassword));
        parameters.Add(
            value: new QueryParameter(_parameterName: "FirstName", value: project.WebFirstName)
        );
        parameters.Add(
            value: new QueryParameter(_parameterName: "Name", value: project.WebSurname)
        );
        parameters.Add(value: new QueryParameter(_parameterName: "Email", value: project.WebEmail));
        parameters.Add(
            value: new QueryParameter(
                _parameterName: "RoleId",
                value: "E0AD1A0B-3E05-4B97-BE38-12FF63E7F2F2"
            )
        );
        parameters.Add(
            value: new QueryParameter(_parameterName: "RequestEmailConfirmation", value: "false")
        );
        DataService(DatabaseType: _databaseType).CreateFirstNewWebUser(parameters: parameters);
    }

    private string BuildConnectionString(Project project)
    {
        return DataService(DatabaseType: _databaseType)
            .BuildConnectionString(
                serverName: project.DatabaseServerName,
                port: project.DatabasePort,
                databaseName: project.DataDatabaseName,
                userName: project.DatabaseUserName,
                password: project.DatabasePassword,
                integratedAuthentication: project.DatabaseIntegratedAuthentication,
                pooling: false
            );
    }

    public override void Rollback() { }
}
