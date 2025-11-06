using System;
using System.Collections.Generic;
using System.Linq;

namespace Origam.DA.Common.DatabasePlatform;

public class DatabaseProfile : IDatabaseProfile
{
    private static DatabaseProfile instance;

    public static DatabaseProfile GetInstance()
    {
        if (instance == null)
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            instance = new DatabaseProfile(settings);
        }
        return instance;
    }

    private readonly List<IDatabaseProfile> includedDatabaseProfiles;

    private static IDatabaseProfile ToDatabaseProperties(Platform platform)
    {
        return platform.Name switch
        {
            nameof(DatabaseType.PgSql) => new PostgresProfile(),
            nameof(DatabaseType.MsSql) => new MsSqlProfile(),
            _ => throw new Exception("Unknown platform: " + platform),
        };
    }

    private DatabaseProfile(OrigamSettings settings)
    {
        includedDatabaseProfiles = settings
            .GetAllPlatforms()
            .Select(ToDatabaseProperties)
            .ToList();
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
}
