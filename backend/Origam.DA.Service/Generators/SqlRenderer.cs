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

using System.Configuration;
using Origam.DA.Service;

public abstract class SqlRenderer
{
    public abstract string NameLeftBracket { get; }
    public abstract string NameRightBracket { get; }
    public abstract string ParameterDeclarationChar { get; }
    public abstract string ParameterReferenceChar { get; }
    public abstract string StringConcatenationChar { get; }
    public bool GenerateConsoleUseSyntax { get; set; }
    public abstract string SelectClause(string finalQuery, int top);

    public string SelectClauseWithDistinct(string finalQuery) => $"SELECT DISTINCT{finalQuery}";

    public abstract string ConvertGeoFromTextClause(string argument);
    public abstract string ConvertGeoToTextClause(string argument);
    internal abstract string Sequence(string entityName, string primaryKeyName);
    internal abstract string IsNull();

    // Will omit the time part if the time is 00:00:00.
    // The result should be the same as the the result of C# DateTime.ToString() / DateTime.ToShortDateString()
    // Because that is what is used in C# code in the UIService/GetLookupLabelsEx endpoint
    internal abstract string Format(string date, string culture);
    internal abstract string CountAggregate();
    internal abstract string DeclareAsSql();
    internal abstract string FunctionPrefix();
    internal abstract string VarcharSql();
    internal abstract string Length(string expresion);
    internal abstract string Text(string expresion);
    internal abstract string DatePart(string datetype, string expresion);
    internal abstract string DateAdd(DateTypeSql addDateSql, string number, string date);
    internal abstract string LatLon(geoLatLonSql latLon, string expresion);
    internal abstract string Contains(
        string columnsForSeach,
        string freetext_string,
        string languageForFullText
    );
    internal abstract string FreeText(
        string columnsForSeach,
        string freetext_string,
        string languageForFullText
    );
    internal abstract string Now();
    internal abstract string STDistance(string point1, string point2);
    internal abstract string DateDiff(DateTypeSql addDateSql, string startdate, string enddate);
    internal abstract string Array(string expresion1, string expresion2);
    internal abstract string CreateDataStructureHead();
    internal abstract string DeclareBegin();
    internal abstract string SetParameter(string name);
    internal abstract string Char(int number);
}
