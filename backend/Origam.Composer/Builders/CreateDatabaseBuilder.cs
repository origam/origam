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
using Spectre.Console;
using static Origam.DA.Common.Enums;

namespace Origam.Composer.Builders;

public class CreateDatabaseBuilder : AbstractDatabaseBuilder
{
    public override string Name => "Create database (new empty)";

    private string _databaseName;
    private DatabaseType _databaseType;

    public override void Execute(Project project)
    {
        AnsiConsole.MarkupLine($"[orange1][bold]Executing:[/][/] {Name}");

        _databaseType = project.DatabaseType;
        _databaseName = project.DatabaseName;

        CreateDatabase(project);
        CreateSchema(project);
        DataService(_databaseType).ConnectionString = BuildConnectionStringCreateDatabase(
            project,
            ""
        );
    }

    private void CreateDatabase(Project project)
    {
        DataService(_databaseType).ConnectionString = BuildConnectionStringCreateDatabase(
            project,
            ""
        );
        DataService(_databaseType).CreateDatabase(_databaseName);
    }

    private void CreateSchema(Project project)
    {
        DataService(_databaseType).ConnectionString = BuildConnectionStringCreateDatabase(
            project,
            project.DatabaseName
        );
        DataService(_databaseType).CreateSchema(_databaseName);
    }

    private string BuildConnectionStringCreateDatabase(Project project, string creatingDatabase)
    {
        return DataService(_databaseType)
            .BuildConnectionString(
                project.DatabaseHost,
                project.DatabasePort,
                creatingDatabase,
                project.DatabaseUserName,
                project.DatabasePassword,
                project.DatabaseIntegratedAuthentication,
                false
            );
    }

    public void ResetDataservice()
    {
        DataService();
    }

    private string BuildConnectionString(Project project, bool pooling)
    {
        _databaseType = project.DatabaseType;

        var dataService = DataService(project.DatabaseType)
            .BuildConnectionString(
                project.DatabaseHost,
                project.DatabasePort,
                project.DatabaseName,
                project.DatabaseUserName,
                project.DatabasePassword,
                project.DatabaseIntegratedAuthentication,
                pooling
            );

        return dataService;
    }

    public string BuildConnectionStringArchitect(Project project, bool pooling)
    {
        _databaseType = project.DatabaseType;
        if (_databaseType == DatabaseType.MsSql)
        {
            return BuildConnectionString(project, pooling);
        }

        if (_databaseType == DatabaseType.PgSql)
        {
            DataService(_databaseType).DbUser = project.Name;
            var dataService = DataService(project.DatabaseType)
                .BuildConnectionString(
                    project.DatabaseHost,
                    project.DatabasePort,
                    project.DatabaseName,
                    DataService(_databaseType).DbUser,
                    project.UserPassword,
                    project.DatabaseIntegratedAuthentication,
                    pooling
                );

            return dataService;
        }

        return null;
    }

    public override void Rollback()
    {
        OrigamUserContext.Reset();
        DataService(_databaseType).DeleteDatabase(_databaseName);
    }
}
