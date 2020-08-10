using System;
using Origam.Schema;

namespace Origam.DA.Service
{
    public class SQLValueFormatter
    {
        private readonly string trueValue;
        private readonly string falseValue;
            
        public SQLValueFormatter(string trueValue, string falseValue)
        {
            this.trueValue = trueValue;
            this.falseValue = falseValue;
        }
            
        internal string RenderString(string text)
        {
            return "'" + text.Replace("'", "''") + "'";
        }
        public string Format(OrigamDataType dataType, object value)
        {
            switch (dataType)
            {
                case OrigamDataType.Integer:
                case OrigamDataType.Float:
                case OrigamDataType.Currency:
                    return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);

                case OrigamDataType.Boolean:
                    if ((bool)value)
                    {
                        return trueValue;
                    }
                    else
                    {
                        return falseValue;
                    }

                case OrigamDataType.UniqueIdentifier:
                    return "'" + value is string strGuid ? strGuid.Replace("'","''") : value  + "'";

                case OrigamDataType.Xml:
                case OrigamDataType.Memo:
                case OrigamDataType.String:
                    return value.ToString() == "null" ? "NULL" : RenderString(value.ToString());

                case OrigamDataType.Date:
                    if (value == null || (value is string strValue1 && string.IsNullOrWhiteSpace(strValue1))) return "null";
                    DateTime date;
                    if (value is string strValue)
                    {
                        bool success = DateTime.TryParse(strValue, out var parsedDate);
                        if (!success)
                        {
                            throw new ArgumentException($"Cannot parse \"value\" to date"); 
                        }
                        date = parsedDate;
                    }
                    else
                    {
                        date = (DateTime) value;
                    }

                    return date.ToString(@" \'yyyy-MM-dd HH:mm:ss\' ");

                default:
                    throw new NotImplementedException(ResourceUtils.GetString("TypeNotImplementedByDatabase", dataType.ToString()));
            }
        }
    }
}