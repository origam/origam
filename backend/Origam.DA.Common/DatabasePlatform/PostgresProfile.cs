using Origam.DA.Common.Interfaces;

namespace Origam.DA.Common.DatabasePlatform;

public class PostgresProfile : IDatabaseProfile
{
    private readonly int maxIdentifierLength = 63;

    public string CheckIdentifierLength(int length)
    {
        return length > maxIdentifierLength
            ? string.Format(Strings.IdentifierMaxLength, "SQL Server", maxIdentifierLength)
            : null;
    }

    public string CheckIndexNameLength(string indexName)
    {
        return indexName.Length > maxIdentifierLength
            ? string.Format(Strings.IndexMaxLength, indexName, "Postgre SQL", maxIdentifierLength)
                + " "
                + string.Format(Strings.PostgresIndexNameLength)
            : null;
    }
}
