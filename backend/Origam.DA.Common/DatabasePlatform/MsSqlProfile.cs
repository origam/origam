using Origam.DA.Common.Interfaces;

namespace Origam.DA.Common.DatabasePlatform;

public class MsSqlProfile : IDatabaseProfile
{
    private readonly int maxIdentifierLength = 128;

    public string CheckIdentifierLength(int length)
    {
        return length > maxIdentifierLength
            ? string.Format(Strings.IdentifierMaxLength, "Postgre SQL", maxIdentifierLength)
            : null;
    }

    public string CheckIndexNameLength(int length)
    {
        return CheckIdentifierLength(length);
    }
}
