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

using Origam.DA.Common.DatabasePlatform;

namespace Origam.ProjectAutomation;

public class ApplyDatabasePermissionsBuilder : AbstractDatabaseBuilder
{
    string _loginName;
    bool _integratedAuthentication = false;
    DatabaseType _databaseType;
    public override string Name
    {
        get { return "Apply Database Permissions"; }
    }

    public override void Execute(Project project)
    {
        _databaseType = project.DatabaseType;
        DataService(DatabaseType: _databaseType).DbUser = project.Name;
        _loginName = DataService(DatabaseType: _databaseType).DbUser;
        _integratedAuthentication = project.DatabaseIntegratedAuthentication;
        DataService(DatabaseType: _databaseType).ConnectionString = BuildConnectionString(
            project: project
        );
        DataService(DatabaseType: _databaseType)
            .CreateDatabaseUser(
                user: _loginName,
                password: project.UserPassword,
                name: project.DataDatabaseName,
                databaseIntegratedAuthentication: project.DatabaseIntegratedAuthentication
            );
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

    public override void Rollback()
    {
        DataService(DatabaseType: _databaseType)
            .DeleteUser(user: _loginName, _integratedAuthentication: _integratedAuthentication);
    }
}
