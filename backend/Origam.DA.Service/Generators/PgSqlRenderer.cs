#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Text;
using Origam.DA.Service;

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

    internal override string Length(string expression)
    {
        return $"LENGTH({expression})";
    }

    internal override string Text(string expression)
    {
        return $"CAST ({expression} AS TEXT)";
    }

    internal override string DatePart(string dateType, string expression)
    {
        return $"DATE_PART('{dateType}',{expression})";
    }

    internal override string DateAdd(DateTypeSql datepart, string number, string date)
    {
        return $"({date}::timestamp + ( {number} || '{GetAddDateSql(datepart)}')::interval)";
    }

    private string GetAddDateSql(DateTypeSql datepart)
    {
        return datepart switch
        {
            DateTypeSql.Second => "second",
            DateTypeSql.Minute => "minute",
            DateTypeSql.Hour => "hour",
            DateTypeSql.Day => "day",
            DateTypeSql.Month => "month",
            DateTypeSql.Year => "year",
            _ => throw new NotSupportedException($"Unsupported in AddDateSql {datepart}"),
        };
    }

    internal override string DateDiff(DateTypeSql datepart, string startDate, string endDate)
    {
        var stringBuilder = new StringBuilder();
        switch (datepart)
        {
            case DateTypeSql.Day:
            {
                stringBuilder.Append("DATE_PART('day', {0}::timestamp - {1}::timestamp) ");
                break;
            }

            case DateTypeSql.Hour:
            {
                stringBuilder.Append("DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + ");
                stringBuilder.Append("DATE_PART('hour', {0}::timestamp - {1}::timestamp) ");
                break;
            }

            case DateTypeSql.Minute:
            {
                stringBuilder.Append("(DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + ");
                stringBuilder.Append("DATE_PART('hour', {0}::timestamp - {1}::timestamp)) * 60 + ");
                stringBuilder.Append("DATE_PART('minute', {0}::timestamp - {1}::timestamp)");
                break;
            }

            case DateTypeSql.Second:
            {
                stringBuilder.Append(
                    "(((DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + "
                );
                stringBuilder.Append("DATE_PART('hour', {0}::timestamp - {1}::timestamp)) * 60 + ");
                stringBuilder.Append("DATE_PART('minute', {0}::timestamp - {1}::timestamp)) *60 ");
                stringBuilder.Append("DATE_PART('second', {0}::timestamp - {1}::timestamp)");
                break;
            }

            default:
            {
                throw new NotSupportedException($"Unsupported DateDiffSql {datepart}");
            }
        }

        return string.Format(stringBuilder.ToString(), endDate, startDate);
    }

    internal override string STDistance(string point1, string point2)
    {
        return $"ST_Distance(('SRID=4326;' || {ConvertGeoToTextClause(point1)})::geography,('SRID=4326;' || {ConvertGeoToTextClause(point2)})::geography)";
    }

    internal override string Now()
    {
        return "NOW()";
    }

    internal override string FreeText(
        string columnsForSearch,
        string freetext,
        string languageForFullText
    )
    {
        return $"{columnsForSearch} @@ to_tsquery({languageForFullText},{freetext})";
    }

    internal override string Contains(
        string columnsForSearch,
        string freetext,
        string languageForFullText
    )
    {
        return $"levenshtein({columnsForSearch},{freetext})";
    }

    internal override string LatLon(geoLatLonSql latLon, string expression)
    {
        return latLon switch
        {
            geoLatLonSql.Lat => $"st_y({expression})",
            geoLatLonSql.Lon => $"st_x({expression})",
            _ => throw new NotSupportedException($"Unsupported in Latitude or Longitude {latLon}"),
        };
    }

    internal override string Array(string expression1, string expression2)
    {
        return $"{expression1}::text = ANY ({expression2})";
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
        return $"{name} = NULL;{Environment.NewLine}";
    }

    public override string SelectClause(string finalQuery, int top)
    {
        if (top == 0)
        {
            return "SELECT" + finalQuery;
        }

        return $"SELECT{finalQuery} LIMIT {top}";
    }

    public override string ConvertGeoFromTextClause(string argument)
    {
        return $"ST_GeomFromText({argument}, 4326)";
    }

    public override string ConvertGeoToTextClause(string argument)
    {
        return $"ST_AsText({argument})";
    }

    internal override string Sequence(string entityName, string primaryKeyName)
    {
        var actualSequence = new StringBuilder();
        actualSequence.Append(entityName);
        actualSequence.Append("_");
        actualSequence.Append(primaryKeyName);
        actualSequence.Append("_seq");
        return $"; SELECT currval({actualSequence})";
    }

    public override string ParameterReferenceChar => GenerateConsoleUseSyntax ? "" : ":";

    public override string StringConcatenationChar => "||";

    internal override string IsNull()
    {
        return "COALESCE";
    }

    internal override string Format(string date, string culture)
    {
        return @$" CASE when TO_CHAR({date} ,'HH24:MI:SS AM') = '00:00:00 AM' then CAST({date}::TIMESTAMP::DATE as TEXT) else CAST({date}::TIMESTAMP as TEXT) END ";
    }

    internal override string CountAggregate()
    {
        return "COUNT";
    }

    internal override string Char(int number)
    {
        return $"CHR({number})";
    }
}
