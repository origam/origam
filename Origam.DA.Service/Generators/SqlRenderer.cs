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
    internal abstract string Sequence(string entityName, string primaryKeyName);
    internal abstract string IsNull();
    internal abstract string CountAggregate();
    internal abstract string DeclareAsSql();
    internal abstract string FunctionPrefix();
    internal abstract string VarcharSql();
    internal abstract string Length(string expresion);
    internal abstract string Text(string expresion);
    internal abstract string DatePart(string datetype, string expresion);
    internal abstract string DateAdd(DateTypeSql addDateSql, string number, string date);
    internal abstract string LatLon(geoLatLonSql latLon, string expresion);
    internal abstract string Contains(string columnsForSeach, string freetext_string, string languageForFullText);
    internal abstract string FreeText(string columnsForSeach, string freetext_string, string languageForFullText);
    internal abstract string Now();
    internal abstract string STDistance(string point1, string point2);
    internal abstract string DateDiff(DateTypeSql addDateSql, string startdate, string enddate);
    internal abstract string Array(string expresion1, string expresion2);
    internal abstract string CreateDataStructureHead();
    internal abstract string DeclareBegin();
    internal abstract string SetParameter(string name);
}