using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

namespace Origam.Extensions;

public static class DataSetExtensions
{
    public static XDocument ToXDocument(this DataSet dataSet)
    {
        using (var stream = new MemoryStream())
        {
            using (
                var xmlTextWriter = new XmlTextWriter(w: stream, encoding: Encoding.UTF8)
                {
                    Formatting = Formatting.None,
                }
            )
            {
                dataSet.WriteXml(writer: xmlTextWriter);
                stream.Position = 0;
                var xmlReader = XmlReader.Create(input: stream);
                xmlReader.MoveToContent();
                return XDocument.Load(reader: xmlReader);
            }
        }
    }

    public static void ReEnableNullConstraints(this DataSet data)
    {
        foreach (DataTable table in data.Tables)
        {
            foreach (DataColumn col in table.Columns)
            {
                if (
                    col.ExtendedProperties.Contains(key: "AllowNulls")
                    && (bool)col.ExtendedProperties[key: "AllowNulls"] == false
                )
                {
                    col.AllowDBNull = false;
                }
            }
        }
    }

    public static void RemoveNullConstraints(this DataSet data)
    {
        foreach (DataTable table in data.Tables)
        {
            foreach (DataColumn col in table.Columns)
            {
                if (col.AllowDBNull == false & IsKey(column: col) == false)
                {
                    col.AllowDBNull = true;
                }
            }
        }
    }

    private static bool IsKey(DataColumn column)
    {
        // primary key
        bool found = IsInColumns(searchedColumn: column, columns: column.Table.PrimaryKey);
        if (found)
        {
            return true;
        }

        // parent relations
        found = IsInRelations(column: column, relations: column.Table.ParentRelations);
        if (found)
        {
            return true;
        }

        // child relations
        return IsInRelations(column: column, relations: column.Table.ChildRelations);
    }

    private static bool IsInRelations(DataColumn column, DataRelationCollection relations)
    {
        foreach (DataRelation relation in relations)
        {
            if (IsRelationKey(column: column, relation: relation))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRelationKey(DataColumn column, DataRelation relation)
    {
        // parent columns
        bool found = IsInColumns(searchedColumn: column, columns: relation.ParentColumns);

        if (found)
        {
            return true;
        }

        // child columns
        return IsInColumns(searchedColumn: column, columns: relation.ChildColumns);
    }

    private static bool IsInColumns(DataColumn searchedColumn, DataColumn[] columns)
    {
        foreach (DataColumn col in columns)
        {
            if (col.Equals(obj: searchedColumn))
            {
                return true;
            }
        }

        return false;
    }
}
