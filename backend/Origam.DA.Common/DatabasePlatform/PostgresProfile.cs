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

    public string CheckIndexNameLength(int length)
    {
        string lengthErrorMessage = CheckIdentifierLength(length);
        if (lengthErrorMessage != null)
        {
            return lengthErrorMessage + " " + string.Format(Strings.PostgresIndexNameLength);
        }
        return null;
    }
}
