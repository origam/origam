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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;
using System.IO;

namespace Origam.JSON;

public class JsonUtils
{
	public static void SerializeToJson(TextWriter textWriter, object value, bool omitRootElement)
	{
            SerializeToJson(textWriter, value, omitRootElement, false);
        }

	public static void SerializeToJson(TextWriter textWriter, object value,
		bool omitRootElement, bool omitMainElement)
	{
            JsonSerializer serializer = new JsonSerializer();
            // remove standard DataSet and XML converters
            RemoveJsonConverter(serializer, typeof(DataSetConverter));
            RemoveJsonConverter(serializer, typeof(XmlNodeConverter));
            // add our custom converters


            if (value is DataSet)
            {
                DataSetConverter datasetConverter = new DataSetConverter(
                    omitRootElement: omitRootElement, 
                    omitMainElement: omitMainElement);
                serializer.Converters.Add(datasetConverter);
                serializer.Converters.Add(new DataRowConverter());
            }
            else
            {
                XmlNodeConverter xmlConverter = new XmlNodeConverter();
                xmlConverter.OmitRootObject = omitRootElement;
                serializer.Converters.Add(xmlConverter);
            }
            JsonWriter writer = new JsonTextWriter(textWriter);
            //JsonWriter writer = new JsonTextWriter(textWriter);
            serializer.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            serializer.Serialize(writer, value);
        }

	public static void RemoveJsonConverter(JsonSerializer serializer, Type type)
	{
			JsonConverter converter = GetJsonConverter(serializer, type);
			if (converter != null)
			{
				serializer.Converters.Remove(converter);
			}
		}

	public static JsonConverter GetJsonConverter(JsonSerializer serializer, Type type)
	{
			JsonConverter result = null;
			foreach (JsonConverter converter in serializer.Converters)
			{
				if (type.Equals(converter.GetType()))
				{
					result = converter;
					break;
				}
			}

			return result;
		}

}