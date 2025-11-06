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
            _ => throw new Exception("Unknown platform: " + platform),
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

    public string CheckIndexNameLength(int length)
    {
        return includedDatabaseProfiles
            .Select(x => x.CheckIndexNameLength(length))
            .FirstOrDefault();
    }

    public void InitializeService() { }

    public void UnloadService() { }
}
