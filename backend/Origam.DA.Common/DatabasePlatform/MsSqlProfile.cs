namespace Origam.DA.Common.DatabasePlatform;

public class MsSqlProfile : IDatabaseProperties
{
    private readonly int maxIdentifierLength = 128;

    public string CheckIdentifierLength(int length)
    {
        return length > maxIdentifierLength
            ? $"Length limit exceeded. Max Postgre SQL entity name length is {maxIdentifierLength} characters."
            : null;
    }

    public string CheckIndexNameLength(int length)
    {
        return CheckIdentifierLength(length);
    }
}
