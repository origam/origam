namespace Origam.Schema.EntityModel
{
    public interface IDatabaseDataTypeMapping
    {
        OrigamDataType DataType { get; set; }
        DatabaseDataType MappedDataType { get; set; }
    }
}
