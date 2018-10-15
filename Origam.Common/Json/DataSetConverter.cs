#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

namespace Origam.JSON
{
    class DataSetConverter : newton.DataSetConverter
    {
        private bool _omitRootObject = false;
        public bool OmitRootObject
        {
            get
            {
                return _omitRootObject;
            }
            set
            {
                _omitRootObject = value;
            }
        }
        public override bool CanConvert(Type valueType)
        {
            return typeof(DataSet).IsAssignableFrom(valueType);
        }
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, 
            Newtonsoft.Json.JsonSerializer serializer)
        {
            DataSet dataSet = (DataSet)value;
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            DataTableConverter converter = new DataTableConverter();

            if (!OmitRootObject)
            {
                string name = dataSet.DataSetName;
                writer.WriteStartObject();
                writer.WritePropertyName((resolver != null) 
                    ? resolver.GetResolvedPropertyName(name) 
                    : name);
            }

            writer.WriteStartObject();

            foreach (DataTable table in dataSet.Tables)
            {
                if (IsRoot(table))
                {
                    writer.WritePropertyName((resolver != null) 
                        ? resolver.GetResolvedPropertyName(table.TableName) 
                        : table.TableName);

                    converter.WriteJson(writer, table, serializer);
                }
            }

            writer.WriteEndObject();

            if (!OmitRootObject)
            {
                writer.WriteEndObject();
            }
        }

        private bool IsRoot(DataTable table)
        {
            foreach (DataRelation relation in table.DataSet.Relations)
            {
                if (relation.Nested && relation.ChildTable.Equals(table))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
