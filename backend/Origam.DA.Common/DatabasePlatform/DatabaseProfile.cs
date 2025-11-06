using System;
using System.Collections.Generic;
using System.Linq;

namespace Origam.DA.Common.DatabasePlatform;

public class DatabaseProfile : IDatabaseProperties
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

    private readonly List<IDatabaseProperties> includedDatabaseProperties;

    private static IDatabaseProperties ToDatabaseProperties(Platform platform)
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
        includedDatabaseProperties = settings
            .GetAllPlatforms()
            .Select(ToDatabaseProperties)
            .ToList();
    }

    public string CheckIdentifierLength(int length)
    {
        return includedDatabaseProperties
            .Select(x => x.CheckIdentifierLength(length))
            .FirstOrDefault();
    }

    public string CheckIndexNameLength(int length)
    {
        return includedDatabaseProperties
            .Select(x => x.CheckIndexNameLength(length))
            .FirstOrDefault();
    }
}
