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
using newton = Newtonsoft.Json.Converters;
using System.Data;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Origam.JSON
{
    class DataRowConverter : JsonConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, 
            Newtonsoft.Json.JsonSerializer serializer)
        {
            DataRow row = (DataRow)value;
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            writer.WriteStartObject();
            foreach (DataColumn column in row.Table.Columns)
            {
                if (column.ColumnMapping == MappingType.Hidden)
                {
                    continue;
                }

                if (serializer.NullValueHandling == NullValueHandling.Ignore 
                    && (row[column] == null || row[column] == DBNull.Value))
                    continue;

                writer.WritePropertyName((resolver != null) 
                    ? resolver.GetResolvedPropertyName(column.ColumnName) 
                    : column.ColumnName);
                serializer.Serialize(writer, row[column]);
            }
            foreach (DataRelation relation in row.Table.ChildRelations)
            {
                if (relation.Nested)
                {
                    string childTableName = relation.ChildTable.TableName;
                    writer.WritePropertyName((resolver != null) 
                        ? resolver.GetResolvedPropertyName(childTableName) 
                        : childTableName);
                    writer.WriteStartArray();
                    foreach (DataRow childRow in row.GetChildRows(relation))
                    {
                        this.WriteJson(writer, childRow, serializer);
                    }
                    writer.WriteEndArray();
                }
            }            
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(DataRow).IsAssignableFrom(objectType);
        }
    }
}
