using Origam.DA.Common.Interfaces;

namespace Origam.DA.Common.DatabasePlatform;

public class PostgresProfile : IDatabaseProfile
{
    private readonly int maxIdentifierLength = 63;

    public string CheckIdentifierLength(int length)
    {
        return length > maxIdentifierLength
            ? $"Length limit exceeded. Max SQL Server entity name length is {maxIdentifierLength} characters."
            : null;
    }

    public string CheckIndexNameLength(int length)
    {
        string lengthErrorMessage = CheckIdentifierLength(length);
        if (lengthErrorMessage != null)
        {
            return lengthErrorMessage
                + " "
                + "Be careful, index names must be unique within schema in Postgre SQL.";
        }
        return null;
    }
}
