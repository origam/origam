namespace Origam.DA.Common.Interfaces;

public interface IDatabaseProfile
{
    public string CheckIdentifierLength(int length);
    public string CheckIndexNameLength(string indexName);
}
