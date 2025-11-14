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
using System.Collections.Generic;
using System.Linq;
using Origam.DA.Common.DatabasePlatform;
using Origam.DA.Common.Interfaces;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel;

public class DatabaseProfileService : IDatabaseProfile, IWorkbenchService
{
    private readonly List<IDatabaseProfile> includedDatabaseProfiles;

    private IDatabaseProfile ToDatabaseProperties(Platform platform)
    {
        return platform.Name switch
        {
            nameof(DatabaseType.PgSql) => new PostgresProfile(),
            nameof(DatabaseType.MsSql) => new MsSqlProfile(),
            _ => throw new Exception(string.Format(Strings.UnknownPlatform, platform)),
        };
    }

    public DatabaseProfileService(OrigamSettings settings)
    {
        includedDatabaseProfiles = settings.GetAllPlatforms().Select(ToDatabaseProperties).ToList();
    }

    public string CheckIdentifierLength(int length)
    {
        return includedDatabaseProfiles
            .Select(x => x.CheckIdentifierLength(length))
            .FirstOrDefault();
    }

    public string CheckIndexNameLength(string indexName)
    {
        return includedDatabaseProfiles
            .Select(x => x.CheckIndexNameLength(indexName))
            .FirstOrDefault();
    }

    public void InitializeService() { }

    public void UnloadService() { }
}
