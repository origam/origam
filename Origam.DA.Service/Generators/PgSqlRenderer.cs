using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using Origam.Schema;
using Origam.Schema.EntityModel;
using ResourceUtils = Origam.Schema.ResourceUtils;

class PgSqlRenderer : SqlRenderer
{
    public override string NameLeftBracket => "\"";

    public override string NameRightBracket => "\"";

    public override string ParameterDeclarationChar => "";

    internal override string DeclareAsSql()
    {
        return "";
    }

    internal override string FunctionPrefix()
    {
        return "";
    }

    internal override string VarcharSql()
    {
        return "VARCHAR";
    }

    internal override string Length(string expresion)
    {
        return string.Format("LENGTH({0})", expresion);
    }

    internal override string Text(string expresion)
    {
        return string.Format("CAST ({0} AS {1} )", expresion, "TEXT");
    }

    internal override string DatePart(string datetype, string expresion)
    {
        return string.Format("DATE_PART('{0}',{1})", datetype, expresion);
    }

    internal override string DateAdd(DateTypeSql datepart, string number,
        string date)
    {
        return string.Format("({0} + ( {1} || '{2}')::interval)", date, number,
            GetAddDateSql(datepart));
    }
    
    private string GetAddDateSql(DateTypeSql datepart)
    {
        switch (datepart)
        {
            case DateTypeSql.Second:
                return "second";
            case DateTypeSql.Minute:
                return "minute";
            case DateTypeSql.Hour:
                return "hour";
            case DateTypeSql.Day:
                return "day";
            case DateTypeSql.Month:
                return "month";
            case DateTypeSql.Year:
                return "year";

            default:
                throw new NotSupportedException("Unsuported in AddDateSql " + datepart.ToString());
        }
    }

    internal override string DateDiff(DateTypeSql datepart, string startdate,
        string enddate)
    {
        StringBuilder stringBuilder = new StringBuilder();
        switch (datepart)
        {
            case DateTypeSql.Day:
                stringBuilder.Append(
                    "DATE_PART('day', {0}::timestamp - {1}::timestamp) ");
                break;
            case DateTypeSql.Hour:
                stringBuilder.Append(
                    "DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + ");
                stringBuilder.Append(
                    "DATE_PART('hour', {0}::timestamp - {1}::timestamp) ");
                break;
            case DateTypeSql.Minute:
                stringBuilder.Append(
                    "(DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + ");
                stringBuilder.Append(
                    "DATE_PART('hour', {0}::timestamp - {1}::timestamp)) * 60 + ");
                stringBuilder.Append(
                    "DATE_PART('minute', {0}::timestamp - {1}::timestamp)");
                break;
            case DateTypeSql.Second:
                stringBuilder.Append(
                    "(((DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + ");
                stringBuilder.Append(
                    "DATE_PART('hour', {0}::timestamp - {1}::timestamp)) * 60 + ");
                stringBuilder.Append(
                    "DATE_PART('minute', {0}::timestamp - {1}::timestamp)) *60 ");
                stringBuilder.Append(
                    "DATE_PART('second', {0}::timestamp - {1}::timestamp)");
                break;
            default:
                throw new NotSupportedException("Unsuported DateDiffSql " +
                                                datepart.ToString());
        }

        return string.Format(stringBuilder.ToString(), enddate, startdate);
    }

    internal override string STDistance(string point1, string point2)
    {
        return string.Format(
            "ST_Distance(('SRID=4326;' || {0})::geography,('SRID=4326;' || {1})::geography)",
            ConvertGeoToTextClause(point1),
            ConvertGeoToTextClause(point2));
    }

    internal override string Now()
    {
        return "NOW()";
    }

    internal override string FreeText(string columnsForSeach,
        string freetext_string, string languageForFullText)
    {
        return string.Format("{0} @@ to_tsquery({1},{2})", columnsForSeach,
            languageForFullText, freetext_string);
    }

    internal override string Contains(string columnsForSeach,
        string freetext_string, string languageForFullText)
    {
        return string.Format("levenshtein({0},{1})", columnsForSeach,
            freetext_string);
    }

    internal override string LatLon(geoLatLonSql latLon, string expresion)
    {
        switch (latLon)
        {
            case geoLatLonSql.Lat:
                return string.Format("st_y({0})", expresion);
            case geoLatLonSql.Lon:
                return string.Format("st_x({0})", expresion);
            default:
                throw new NotSupportedException(
                    "Unsuported in Latitude or Longtitude " +
                    latLon.ToString());
        }
    }

    internal override string Array(string expresion1, string expresion2)
    {
        return string.Format("{0}::text = ANY ({1})", expresion1, expresion2);
    }

    internal override string CreateDataStructureHead()
    {
        return "DO $$";
    }

    internal override string DeclareBegin()
    {
        return "BEGIN";
    }
    
    internal override string SetParameter(string name)
    {
        return string.Format("{0} = NULL;{1}", name, Environment.NewLine);
    }

    public override string SelectClause(string finalQuery, int top)
    {
        if (top == 0)
        {
            return "SELECT" + finalQuery;
        }
        else
        {
            return "SELECT" + finalQuery + " LIMIT " + top.ToString();
        }
    }

    public override string ConvertGeoFromTextClause(string argument)
    {
        return "ST_GeomFromText(" + argument + ", 4326)";
    }

    public override string ConvertGeoToTextClause(string argument)
    {
        return "ST_AsText(" + argument + ")";
    }

    internal override string Sequence(string entityName, string primaryKeyName)
    {
        StringBuilder actualsequence = new StringBuilder();
        actualsequence.Append(entityName);
        actualsequence.Append("_");
        actualsequence.Append(primaryKeyName);
        actualsequence.Append("_seq");
        return "; SELECT currval(" + actualsequence + ")";
    }
    
    public override string FunctionDefinitionDdl(Function function)
    {
        if (function.FunctionType == OrigamFunctionType.Database)
        {
            StringBuilder builder = new StringBuilder("CREATE FUNCTION ");
            builder.Append(function.Name + "(");
            int i = 0;
            foreach (FunctionParameter parameter in function.ChildItems)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(ParameterDeclarationChar + parameter.Name +
                               " ?");
                i++;
            }

            builder.Append(")" + Environment.NewLine);
            builder.Append("RETURNS " +
                           DdlDataType(function.DataType, 0, null) +
                           Environment.NewLine);
            builder.Append("AS $$" + Environment.NewLine);
            builder.Append("DECLARE " + ParameterDeclarationChar + " "
                           + DdlDataType(function.DataType, 0, null) +
                           Environment.NewLine);
            builder.Append("BEGIN" + Environment.NewLine);

            builder.Append("RETURN " + ParameterReferenceChar +
                           Environment.NewLine);
            builder.Append("END;" + Environment.NewLine);
            builder.Append("$$ LANGUAGE plpgsql;");
            return builder.ToString();
        }
        else
        {
            throw new InvalidOperationException(
                ResourceUtils.GetString("DDLForFunctionsOnly"));
        }
    }
    public override string ParameterReferenceChar => GenerateConsoleUseSyntax ? "" : ":";

    public override string StringConcatenationChar => "||";

    private NpgsqlDbType ConvertDataType(OrigamDataType columnType,
        DatabaseDataType dbDataType)
    {
        if (dbDataType != null)
        {
            return (NpgsqlDbType)Enum.Parse(typeof(NpgsqlDbType),
                dbDataType.MappedDatabaseTypeName);
        }
        switch (columnType)
        {
            case OrigamDataType.Blob:
                return NpgsqlDbType.Bytea;
            case OrigamDataType.Boolean:
                return NpgsqlDbType.Boolean;
            case OrigamDataType.Byte:
                //TODO: check right 
                return NpgsqlDbType.Smallint;
            case OrigamDataType.Currency:
                return NpgsqlDbType.Money;
            case OrigamDataType.Date:
                return NpgsqlDbType.Timestamp;
            case OrigamDataType.Long:
                return NpgsqlDbType.Bigint;
            case OrigamDataType.Xml:
            case OrigamDataType.Geography:
            case OrigamDataType.Memo:
                return NpgsqlDbType.Text;
            case OrigamDataType.Array:
                return NpgsqlDbType.Array;
            case OrigamDataType.Integer: 
                return NpgsqlDbType.Integer;
            case OrigamDataType.Float:
                return NpgsqlDbType.Numeric;
            case OrigamDataType.Object:
            case OrigamDataType.String:
                return NpgsqlDbType.Varchar;
            case OrigamDataType.UniqueIdentifier:
                return NpgsqlDbType.Uuid;
            default:
                throw new NotSupportedException(ResourceUtils.GetString("UnsupportedType"));
        }
    }

    public override string DefaultDdlDataType(OrigamDataType columnType)
    {
        switch (columnType)
        {
            case OrigamDataType.Date:
                return string.Format("{0}(3)",
                    ConvertDataType(columnType, null).ToString());
            default:
                return ConvertDataType(columnType, null).ToString();
        }
    }

    public override IDbDataParameter BuildParameter(string paramName,
        string sourceColumn, OrigamDataType dataType,
        DatabaseDataType dbDataType,
        int dataLength, bool allowNulls)
    {
        NpgsqlDbType convDataType = ConvertDataType(dataType, dbDataType);
        if (dataType == OrigamDataType.Array)
        {
            convDataType = ConvertDataType(dataType, dbDataType) |
                           NpgsqlDbType.Text;
        }

        NpgsqlParameter sqlParam = new NpgsqlParameter(
            paramName,
            convDataType,
            dataLength,
            sourceColumn
        )
        {
            IsNullable = allowNulls
        };

        if (sqlParam.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Numeric)
        {
            sqlParam.Precision = 18;
            sqlParam.Scale = 10;
        }

        if (sqlParam.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Timestamp)
        {
            sqlParam.Precision = 3;
        }

        return sqlParam;
    }
    
    internal override string IsNull()
    {
        return "COALESCE";
    }
    internal override string CountAggregate()
    {
        return "COUNT";
    }
}