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
using Origam.DA.Service;

public class MsSqlRenderer : SqlRenderer
{
    public override string NameLeftBracket => "[";

    public override string NameRightBracket => "]";

    public override string ParameterDeclarationChar => "@";

    public override string ParameterReferenceChar => "@";

    public override string StringConcatenationChar => "+";

    public override string SelectClause(string finalQuery, int top)
    {
        return top == 0 ? "SELECT" + finalQuery : "SELECT TOP " + top + finalQuery;
    }

    public override string ConvertGeoFromTextClause(string argument)
    {
        return "geography::STGeomFromText(" + argument + ", 4326)";
    }

    public override string ConvertGeoToTextClause(string argument)
    {
        return argument + ".STAsText()";
    }

    internal override string Sequence(string entityName, string primaryKeyName)
    {
        return "; SELECT @@IDENTITY AS " + primaryKeyName;
    }

    internal override string IsNull()
    {
        return "ISNULL";
    }

    internal override string Format(string date, string culture)
    {
        return @$" FORMAT({date}, IIF (FORMAT({date}, 'HH:mm:ss tt', 'en-US' ) = '00:00:00 AM', 'd', ''), '{culture}') ";
    }

    internal override string CountAggregate()
    {
        return "COUNT_BIG";
    }

    internal override string DeclareAsSql()
    {
        return "AS";
    }

    internal override string FunctionPrefix()
    {
        return "dbo.";
    }

    internal override string VarcharSql()
    {
        return "NVARCHAR";
    }

    internal override string Length(string expresion)
    {
        return string.Format("LEN({0})", expresion);
    }

    internal override string Text(string expresion)
    {
        return string.Format("CAST ({0} AS {1} )", expresion, "NVARCHAR(MAX)");
    }

    internal override string DatePart(string datetype, string expresion)
    {
        return string.Format("DATEPART({0},{1})", datetype, expresion);
    }

    internal override string DateAdd(DateTypeSql datepart, string number, string date)
    {
        return string.Format("DATEADD({0},{1},{2})", GetAddDateSql(datepart), number, date);
    }

    private string GetAddDateSql(DateTypeSql datepart)
    {
        switch (datepart)
        {
            case DateTypeSql.Second:
                return "s";
            case DateTypeSql.Minute:
                return "mi";
            case DateTypeSql.Hour:
                return "hh";
            case DateTypeSql.Day:
                return "dd";
            case DateTypeSql.Month:
                return "m";
            case DateTypeSql.Year:
                return "yy";
            default:
                throw new NotSupportedException("Unsuported in AddDateSql " + datepart.ToString());
        }
    }

    internal override string DateDiff(DateTypeSql datepart, string startdate, string enddate)
    {
        return string.Format(
            "DATEDIFF({0}, {1}, {2})",
            GetAddDateSql(datepart),
            startdate,
            enddate
        );
    }

    internal override string STDistance(string point1, string point2)
    {
        return string.Format("{0}.STDistance({1})", point1, point2);
    }

    internal override string Now()
    {
        return "GETDATE()";
    }

    internal override string FreeText(
        string columnsForSeach,
        string freetext_string,
        string languageForFullText
    )
    {
        if (string.IsNullOrEmpty(languageForFullText))
        {
            return string.Format("FREETEXT({0},{1})", columnsForSeach, freetext_string);
        }
        return string.Format(
            "FREETEXT({0},{1},{2})",
            columnsForSeach,
            freetext_string,
            languageForFullText
        );
    }

    internal override string Contains(
        string columnsForSeach,
        string freetext_string,
        string languageForFullText
    )
    {
        if (string.IsNullOrEmpty(languageForFullText))
        {
            return string.Format("CONTAINS({0},{1})", columnsForSeach, freetext_string);
        }
        return string.Format(
            "CONTAINS({0},{1},{2})",
            columnsForSeach,
            freetext_string,
            languageForFullText
        );
    }

    internal override string LatLon(geoLatLonSql latLon, string expresion)
    {
        switch (latLon)
        {
            case geoLatLonSql.Lat:
                return string.Format("{0}.Lat", expresion);
            case geoLatLonSql.Lon:
                return string.Format("{0}.Long", expresion);
            default:
                throw new NotSupportedException(
                    "Unsuported in Latitude or Longtitude " + latLon.ToString()
                );
        }
    }

    internal override string Array(string expresion1, string expresion2)
    {
        return string.Format(
            "{0} IN (SELECT ListValue FROM {1} origamListValue)",
            expresion1,
            expresion2
        );
    }

    internal override string CreateDataStructureHead()
    {
        return "";
    }

    internal override string DeclareBegin()
    {
        return "";
    }

    internal override string SetParameter(string name)
    {
        return string.Format("SET {0} = NULL{1}", name, Environment.NewLine);
    }

    internal override string Char(int number)
    {
        return "CHAR(" + number + ")";
    }
}
