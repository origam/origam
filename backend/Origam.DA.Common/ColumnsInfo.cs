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

using System.Collections.Generic;
using System.Linq;

namespace Origam.DA;

public class ColumnsInfo
{
    public static readonly ColumnsInfo Empty = new ColumnsInfo();
    public bool RenderSqlForDetachedFields { get; }

    private ColumnsInfo() { }

    public ColumnsInfo(string columnName)
        : this(columnName, false) { }

    public ColumnsInfo(string columnName, bool renderSqlForDetachedFields)
    {
        RenderSqlForDetachedFields = renderSqlForDetachedFields;
        if (columnName == null)
        {
            Columns = new List<ColumnData>();
            return;
        }
        Columns = columnName
            .Split(';')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new ColumnData(x))
            .ToList();
    }

    public ColumnsInfo(List<ColumnData> columns, bool renderSqlForDetachedFields)
    {
        Columns = columns.ToList();
        RenderSqlForDetachedFields = renderSqlForDetachedFields;
    }

    public List<ColumnData> Columns { get; } = new List<ColumnData>();
    public int Count => Columns.Count;
    public bool IsEmpty => Count == 0;
    public List<string> ColumnNames => Columns.Select(x => x.Name).ToList();

    public override string ToString()
    {
        return string.Join(";", ColumnNames);
    }
}

public class ColumnData
{
    public string Name { get; }
    public bool IsVirtual { get; }
    public object DefaultValue { get; }
    public bool HasRelation { get; }
    public static readonly ColumnData GroupByCountColumn = new ColumnData("groupCount");
    public static readonly ColumnData GroupByCaptionColumn = new ColumnData("groupCaption");

    public ColumnData(string name, bool isVirtual, object defaultValue, bool hasRelation)
    {
        Name = name;
        IsVirtual = isVirtual;
        DefaultValue = defaultValue;
        HasRelation = hasRelation;
    }

    public ColumnData(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
