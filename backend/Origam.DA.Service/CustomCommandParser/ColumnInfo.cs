using Origam.Schema;

namespace Origam.DA.Service.CustomCommandParser;

public class ColumnInfo
{
    public string Name { get; set; }
    public OrigamDataType DataType { get; set; }
    public bool IsNullable { get; set; }
}