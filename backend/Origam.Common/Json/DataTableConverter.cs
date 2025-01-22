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

using newton = Newtonsoft.Json.Converters;
using System.Data;
using Newtonsoft.Json.Serialization;
using Origam.Extensions;
using System.Linq;
using Origam.Service.Core;

namespace Origam.JSON;
class DataTableConverter : newton.DataTableConverter
{
    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, 
        Newtonsoft.Json.JsonSerializer serializer)
    {
        DataTable table = (DataTable)value;        
        DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;
        bool serializeAsJsonObject = table.ExtendedProperties
            .Contains(Constants.SerializeAsJsonObject)
            ? table.ExtendedProperties[Constants.SerializeAsJsonObject]
                .ToString().Equals("True")
            : false;
        writer.WriteComment(table.ExtendedProperties.Get<string>(Constants.SerializeAsJsonObject));
        writer.WriteComment("ggg");
        writer.WriteComment(table.ExtendedProperties[Constants.SerializeAsJsonObject].ToString());
        writer.WriteComment("hhh");
        writer.WriteComment((string)table.ExtendedProperties[Constants.SerializeAsJsonObject]);
        if (serializeAsJsonObject && table.Rows.Count > 1)
        {
            throw new OrigamException("JSON Serialization failed. "
                + $"Table '{table.TableName}' is defined to serialize to a "
                + $"single object, but multiple objects came ({table.Rows.Count}).");
        }
        if (!serializeAsJsonObject)
        {
            writer.WriteStartArray();
        } 
        foreach (DataRow row in table.Rows)
        {
            serializer.Serialize(writer, row);
        }
        if (!serializeAsJsonObject)
        {
            writer.WriteEndArray();
        }
    }
}
