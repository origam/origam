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
using System.Data;
using Newtonsoft.Json.Serialization;
using newton = Newtonsoft.Json.Converters;

namespace Origam.JSON;

class DataSetConverter : newton.DataSetConverter
{
    private readonly bool omitRootElement;
    private readonly bool omitMainElement;

    public DataSetConverter(bool omitRootElement, bool omitMainElement)
    {
        this.omitRootElement = omitRootElement;
        this.omitMainElement = omitMainElement;
    }

    public override bool CanConvert(Type valueType)
    {
        return typeof(DataSet).IsAssignableFrom(c: valueType);
    }

    public override void WriteJson(
        Newtonsoft.Json.JsonWriter writer,
        object value,
        Newtonsoft.Json.JsonSerializer serializer
    )
    {
        DataSet dataSet = (DataSet)value;
        DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;
        DataTableConverter converter = new DataTableConverter();
        if (!omitRootElement)
        {
            string name = dataSet.DataSetName;
            writer.WriteStartObject();
            writer.WritePropertyName(
                name: (resolver != null)
                    ? resolver.GetResolvedPropertyName(propertyName: name)
                    : name
            );
        }
        if (!omitMainElement)
        {
            writer.WriteStartObject();
        }
        foreach (DataTable table in dataSet.Tables)
        {
            if (IsRoot(table: table))
            {
                if (!omitMainElement)
                {
                    writer.WritePropertyName(
                        name: (resolver != null)
                            ? resolver.GetResolvedPropertyName(propertyName: table.TableName)
                            : table.TableName
                    );
                }
                converter.WriteJson(writer: writer, value: table, serializer: serializer);
            }
        }
        if (!omitMainElement)
        {
            writer.WriteEndObject();
        }
        if (!omitRootElement)
        {
            writer.WriteEndObject();
        }
    }

    private bool IsRoot(DataTable table)
    {
        foreach (DataRelation relation in table.DataSet.Relations)
        {
            if (relation.Nested && relation.ChildTable.Equals(obj: table))
            {
                return false;
            }
        }
        return true;
    }
}
