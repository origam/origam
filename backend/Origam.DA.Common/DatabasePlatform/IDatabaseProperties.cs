namespace Origam.DA.Common.DatabasePlatform;

public interface IDatabaseProperties
{
    public string CheckIdentifierLength(int length);
    public string CheckIndexNameLength(int length);
}
