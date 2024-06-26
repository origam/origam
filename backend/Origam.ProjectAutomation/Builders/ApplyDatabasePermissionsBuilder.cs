#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using static Origam.DA.Common.Enums;

namespace Origam.ProjectAutomation;
public class ApplyDatabasePermissionsBuilder : AbstractDatabaseBuilder
{
    string _loginName;
    bool _integratedAuthentication = false;
    DatabaseType _databaseType;
    public override string Name
    {
        get
        {
            return "Apply Database Permissions";
        }
    }
    public override void Execute(Project project)
    {
        _databaseType = project.DatabaseType;
        DataService(_databaseType).DbUser = project.Name;
        _loginName = DataService(_databaseType).DbUser;
        _integratedAuthentication = project.DatabaseIntegratedAuthentication;
        DataService(_databaseType).ConnectionString = BuildConnectionString(project);
        DataService(_databaseType)
            .CreateDatabaseUser(
                _loginName,
                project.UserPassword,
                project.DataDatabaseName,
                project.DatabaseIntegratedAuthentication
            );
    }
    private string BuildConnectionString(Project project)
    {
        return DataService(_databaseType).BuildConnectionString(project.DatabaseServerName, project.Port,
        project.DataDatabaseName, project.DatabaseUserName,
        project.DatabasePassword, project.DatabaseIntegratedAuthentication, false);
    }
    public override void Rollback()
    {
        DataService(_databaseType).DeleteUser(_loginName, _integratedAuthentication);
    }
}
