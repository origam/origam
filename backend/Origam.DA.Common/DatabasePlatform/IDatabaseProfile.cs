namespace Origam.DA.Common.DatabasePlatform;

public interface IDatabaseProfile
{
    public string CheckIdentifierLength(int length);
    public string CheckIndexNameLength(int length);
}
