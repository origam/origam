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

namespace Origam.DA.Service.Generators;
public class PgSqlFilterRenderer : AbstractFilterRenderer
{
    public override string StringConcatenationChar => "||";
    protected override string LikeOperator()
    {
        //for support case insensitive in PostgreSQL
        return "ILIKE";
    }
    protected override string ColumnArray(string columnName, string operand, string[] rightValues)
    {
        return "\0" + columnName + " ::text " + operand + " (" + string.Join(", ", rightValues) + ")\0";
    }
}
