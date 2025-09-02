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

    public override string ParameterDeclarationChar => ":";

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

    internal override string DateAdd(DateTypeSql datepart, string number, string date)
    {
        return string.Format(
            "({0}::timestamp + ( {1} || '{2}')::interval)",
            date,
            number,
            GetAddDateSql(datepart)
        );
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

    internal override string DateDiff(DateTypeSql datepart, string startdate, string enddate)
    {
        StringBuilder stringBuilder = new StringBuilder();
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
                throw new NotSupportedException("Unsuported DateDiffSql " + datepart.ToString());
        }

        return string.Format(stringBuilder.ToString(), enddate, startdate);
    }

    internal override string STDistance(string point1, string point2)
    {
        return string.Format(
            "ST_Distance(('SRID=4326;' || {0})::geography,('SRID=4326;' || {1})::geography)",
            ConvertGeoToTextClause(point1),
            ConvertGeoToTextClause(point2)
        );
    }

    internal override string Now()
    {
        return "NOW()";
    }

    internal override string FreeText(
        string columnsForSeach,
        string freetext_string,
        string languageForFullText
    )
    {
        return string.Format(
            "{0} @@ to_tsquery({1},{2})",
            columnsForSeach,
            languageForFullText,
            freetext_string
        );
    }

    internal override string Contains(
        string columnsForSeach,
        string freetext_string,
        string languageForFullText
    )
    {
        return string.Format("levenshtein({0},{1})", columnsForSeach, freetext_string);
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
                    "Unsuported in Latitude or Longtitude " + latLon.ToString()
                );
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

        return "SELECT" + finalQuery + " LIMIT " + top.ToString();
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
        return "CHR(" + number + ")";
    }
}
