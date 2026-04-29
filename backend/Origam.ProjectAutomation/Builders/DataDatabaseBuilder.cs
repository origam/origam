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

public class DataDatabaseBuilder : AbstractDatabaseBuilder
{
    string _databaseName;
    DatabaseType _databaseType;
    public override string Name
    {
        get { return "Create Data Database"; }
    }

    public override void Execute(Project project)
    {
        _databaseType = project.DatabaseType;
        _databaseName = project.DataDatabaseName;
        CreateDatabase(project: project);
        CreateSchema(project: project);
        DataService(DatabaseType: _databaseType).ConnectionString =
            BuildConnectionStringCreateDatabase(project: project, databaseName: "");
    }

    public string BuildConnectionStringCreateDatabase(
        IConnectionStringData project,
        string databaseName
    )
    {
        return DataService(DatabaseType: _databaseType)
            .BuildConnectionString(
                serverName: project.DatabaseServerName,
                port: project.DatabasePort,
                databaseName: databaseName,
                userName: project.DatabaseUserName,
                password: project.DatabasePassword,
                integratedAuthentication: project.DatabaseIntegratedAuthentication,
                pooling: false
            );
    }

    public void ResetDataservice()
    {
        DataService();
    }

    public string BuildConnectionString(IConnectionStringData project, bool pooling)
    {
        _databaseType = project.DatabaseType;
        return DataService(DatabaseType: project.DatabaseType)
            .BuildConnectionString(
                serverName: project.DatabaseServerName,
                port: project.DatabasePort,
                databaseName: project.DataDatabaseName,
                userName: project.DatabaseUserName,
                password: project.DatabasePassword,
                integratedAuthentication: project.DatabaseIntegratedAuthentication,
                pooling: pooling
            );
    }

    public string BuildConnectionStringArchitect(Project project, bool pooling)
    {
        _databaseType = project.DatabaseType;
        if (_databaseType == DatabaseType.MsSql)
        {
            return BuildConnectionString(project: project, pooling: pooling);
        }
        if (_databaseType == DatabaseType.PgSql)
        {
            this.DataService(DatabaseType: _databaseType).DbUser = project.Name;
            return DataService(DatabaseType: project.DatabaseType)
                .BuildConnectionString(
                    serverName: project.DatabaseServerName,
                    port: project.DatabasePort,
                    databaseName: project.DataDatabaseName,
                    userName: DataService(DatabaseType: _databaseType).DbUser,
                    password: project.UserPassword,
                    integratedAuthentication: project.DatabaseIntegratedAuthentication,
                    pooling: pooling
                );
        }
        return null;
    }

    private void CreateSchema(Project project)
    {
        DataService(DatabaseType: _databaseType).ConnectionString =
            BuildConnectionStringCreateDatabase(
                project: project,
                databaseName: project.DataDatabaseName
            );
        DataService(DatabaseType: _databaseType).CreateSchema(databaseName: _databaseName);
    }

    private void CreateDatabase(Project project)
    {
        DataService(DatabaseType: _databaseType).ConnectionString =
            BuildConnectionStringCreateDatabase(project: project, databaseName: "");
        DataService(DatabaseType: _databaseType).CreateDatabase(name: _databaseName);
    }

    public override void Rollback()
    {
        OrigamUserContext.Reset();
        DataService(DatabaseType: _databaseType).DeleteDatabase(name: _databaseName);
    }
}
