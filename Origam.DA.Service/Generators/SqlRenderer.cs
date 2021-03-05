using System.Data;
using Origam.Schema;
using Origam.Schema.EntityModel;

public abstract class SqlRenderer
{
    public abstract string NameLeftBracket { get; }
    public abstract string NameRightBracket { get; }
    public abstract string ParameterDeclarationChar { get; }
    public abstract string ParameterReferenceChar { get; }
    public abstract string StringConcatenationChar { get; }
    public bool GenerateConsoleUseSyntax { get; set; }
    public abstract string SelectClause(string finalQuery, int top);
    public abstract string ConvertGeoFromTextClause(string argument);
    public abstract string ConvertGeoToTextClause(string argument);
    internal abstract string SequenceSql(string entityName, string primaryKeyName);
    internal abstract string IsNullSql();
    internal abstract string CountAggregateSql();
    internal abstract string DeclareAsSql();
    internal abstract string FunctionPrefixSql();
    internal abstract string VarcharSql();
    internal abstract string LengthSql(string expresion);
    internal abstract string TextSql(string expresion);
    internal abstract string DatePartSql(string datetype, string expresion);
    internal abstract string DateAddSql(DateTypeSql addDateSql, string number, string date);
    internal abstract string LatLonSql(geoLatLonSql latLon, string expresion);
    internal abstract string ContainsSql(string columnsForSeach, string freetext_string, string languageForFullText);
    internal abstract string FreeTextSql(string columnsForSeach, string freetext_string, string languageForFullText);
    internal abstract string NowSql();
    internal abstract string STDistanceSql(string point1, string point2);
    internal abstract string DateDiffSql(DateTypeSql addDateSql, string startdate, string enddate);
    internal abstract string ArraySql(string expresion1, string expresion2);
    internal abstract string CreateDataStructureHeadSql();
    internal abstract string DeclareBegin();
    internal abstract string SetParameterSql(string name);
    public abstract string FunctionDefinitionDdl(Function function);
    
    public abstract string DefaultDdlDataType(OrigamDataType columnType);
    
    public abstract IDbDataParameter BuildParameter(string paramName,
        string sourceColumn, OrigamDataType dataType, DatabaseDataType dbDataType,
        int dataLength, bool allowNulls);
    
    public string DdlDataType(OrigamDataType columnType, int dataLenght,
        DatabaseDataType dbDataType)
    {
        switch (columnType)
        {
            case OrigamDataType.String:
                return DdlDataType(columnType, dbDataType)
                       + "(" + dataLenght + ")";

            case OrigamDataType.Xml:
                return DdlDataType(columnType, dbDataType);

            case OrigamDataType.Float:
                return DdlDataType(columnType, dbDataType) + "(28,10)";

            default:
                return DdlDataType(columnType, dbDataType);
        }
    }
    
    public string DdlDataType(OrigamDataType columnType,
        DatabaseDataType dbDataType)
    {
        if (dbDataType != null)
        {
            return dbDataType.MappedDatabaseTypeName;
        }
        else
        {
            return DefaultDdlDataType(columnType);
        }
    }
}