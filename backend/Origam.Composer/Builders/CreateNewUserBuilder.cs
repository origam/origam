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
using Origam.Composer.Services;
using Origam.DA;
using Origam.Security.Common;
using Spectre.Console;
using static Origam.DA.Common.Enums;

namespace Origam.Composer.Builders;

public class CreateNewUserBuilder : AbstractDatabaseBuilder
{
    public override string Name => "Create new web user";

    private DatabaseType _databaseType;

    public override void Execute(Project project)
    {
        AnsiConsole.MarkupLine($"[orange1][bold]Executing:[/][/] {Name}");

        var adaptivePassword = new InternalPasswordHasherWithLegacySupport();
        string hashPassword = adaptivePassword.HashPassword(project.WebUserPassword);

        _databaseType = project.DatabaseType;
        DataService(_databaseType).DbUser = project.Name;
        DataService(_databaseType).ConnectionString = project.BuilderDataConnectionString;

        var parameters = new QueryParameterCollection
        {
            new QueryParameter("Id", Guid.NewGuid().ToString()),
            new QueryParameter("UserName", project.WebUserName),
            new QueryParameter("Password", hashPassword),
            new QueryParameter("FirstName", project.WebFirstName),
            new QueryParameter("Name", project.WebSurname),
            new QueryParameter("Email", project.WebEmail),
            new QueryParameter("RoleId", "E0AD1A0B-3E05-4B97-BE38-12FF63E7F2F2"),
            new QueryParameter("RequestEmailConfirmation", "false"),
        };

        DataService(_databaseType).CreateFirstNewWebUser(parameters);
    }

    public override void Rollback() { }
}
