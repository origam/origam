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
using Origam.Composer.Interfaces.Services;
using Origam.DA.Common.DatabasePlatform;

namespace Origam.Composer.Services;

public class ConnectionStringService : IConnectionStringService
{
    public string GetConnectionString(Project project)
    {
        string dbHost = GetDbHost(project);

        if (project.DatabaseType == DatabaseType.MsSql)
        {
            string dbUserName = project.DatabaseUserName;
            string dbPassword = project.DatabasePassword;

            return $"Encrypt=False;Data Source={dbHost},{project.DatabasePort};"
                + $"User ID={dbUserName};Password={dbPassword};"
                + $"Initial Catalog={project.DatabaseName};";
        }

        if (project.DatabaseType == DatabaseType.PgSql)
        {
            string dbUserName = project.DatabaseInternalUserName;
            string dbPassword = project.DatabaseInternalUserPassword;

            return $"sslmode=disable;Application Name=Origam;"
                + $"Host={dbHost};Port={project.DatabasePort};"
                + $"Username={dbUserName};Password={dbPassword};"
                + $"Database={project.DatabaseName};Pooling=True;"
                + $"Search Path={project.DatabaseName},public";
        }

        throw new NotSupportedException("Unsupported database type.");
    }

    private string GetDbHost(Project project)
    {
        if (
            project.DatabaseHost.Equals("localhost")
            || project.DatabaseHost.Equals(".")
            || project.DatabaseHost.Equals("127.0.0.1")
        )
        {
            return "host.docker.internal";
        }
        return project.DatabaseHost;
    }
}
