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
        return top == 0 ? $"SELECT{finalQuery}" : $"SELECT TOP {top}{finalQuery}";
    }

    public override string ConvertGeoFromTextClause(string argument)
    {
        return $"geography::STGeomFromText({argument}, 4326)";
    }

    public override string ConvertGeoToTextClause(string argument)
    {
        return $"{argument}.STAsText()";
    }

    internal override string Sequence(string entityName, string primaryKeyName)
    {
        return $"; SELECT @@IDENTITY AS {primaryKeyName}";
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

    internal override string Length(string expression)
    {
        return $"LEN({expression})";
    }

    internal override string Text(string expression)
    {
        return $"CAST ({expression} AS NVARCHAR(MAX) )";
    }

    internal override string DatePart(string dateType, string expression)
    {
        return $"DATEPART({dateType},{expression})";
    }

    internal override string DateAdd(DateTypeSql datepart, string number, string date)
    {
        return $"DATEADD({GetAddDateSql(datepart)},{number},{date})";
    }

    private string GetAddDateSql(DateTypeSql datepart)
    {
        return datepart switch
        {
            DateTypeSql.Second => "s",
            DateTypeSql.Minute => "mi",
            DateTypeSql.Hour => "hh",
            DateTypeSql.Day => "dd",
            DateTypeSql.Month => "m",
            DateTypeSql.Year => "yy",
            _ => throw new NotSupportedException($"Unsupported in AddDateSql {datepart}"),
        };
    }

    internal override string DateDiff(DateTypeSql datepart, string startDate, string endDate)
    {
        return $"DATEDIFF({GetAddDateSql(datepart)}, {startDate}, {endDate})";
    }

    internal override string STDistance(string point1, string point2)
    {
        return $"{point1}.STDistance({point2})";
    }

    internal override string Now()
    {
        return "GETDATE()";
    }

    internal override string FreeText(
        string columnsForSearch,
        string freetext,
        string languageForFullText
    )
    {
        if (string.IsNullOrEmpty(languageForFullText))
        {
            return $"FREETEXT({columnsForSearch},{freetext})";
        }
        return $"FREETEXT({columnsForSearch},{freetext},{languageForFullText})";
    }

    internal override string Contains(
        string columnsForSearch,
        string freetext,
        string languageForFullText
    )
    {
        if (string.IsNullOrEmpty(languageForFullText))
        {
            return $"CONTAINS({columnsForSearch},{freetext})";
        }
        return $"CONTAINS({columnsForSearch},{freetext},{languageForFullText})";
    }

    internal override string LatLon(geoLatLonSql latLon, string expression)
    {
        return latLon switch
        {
            geoLatLonSql.Lat => $"{expression}.Lat",
            geoLatLonSql.Lon => $"{expression}.Long",
            _ => throw new NotSupportedException($"Unsupported in Latitude or Longitude {latLon}"),
        };
    }

    internal override string Array(string expression1, string expression2)
    {
        return $"{expression1} IN (SELECT ListValue FROM {expression2} origamListValue)";
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
        return $"SET {name} = NULL{Environment.NewLine}";
    }

    internal override string Char(int number)
    {
        return $"CHAR({number})";
    }
}
