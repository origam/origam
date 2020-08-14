using System;
using Origam.Schema;

namespace Origam.DA.Service
{
    public class SQLValueFormatter
    {
        private readonly string trueValue;
        private readonly string falseValue;
        private readonly Func<string, string> escapeLikeInput;

        public SQLValueFormatter(string trueValue, string falseValue, Func<string, string> escapeLikeInput)
        {
            this.trueValue = trueValue;
            this.falseValue = falseValue;
            this.escapeLikeInput = escapeLikeInput;
        }
            
        internal string RenderString(string text, string sqlOperator=null)
        {
            string escapedValue;
            switch (sqlOperator)
            {
                case "like":
                    escapedValue = escapeLikeInput(text);
                    break;
                default:
                    escapedValue = text;
                    break;
            }

            return "'" + escapedValue.Replace("'", "''") + "'";
        }

        public string Format(OrigamDataType dataType, object value, string sqlOperator=null)
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
                    return "'" + value + "'";

                case OrigamDataType.Xml:
                case OrigamDataType.Memo:
                case OrigamDataType.String:
                    return value.ToString() == "null" ? "NULL" : RenderString(value.ToString(), sqlOperator);

                case OrigamDataType.Date:
                    if (value == null || 
                        value.Equals("null") || 
                        value is string strValue1 && string.IsNullOrWhiteSpace(strValue1)) return "null";
                    DateTime date;
                    if (value is string strValue)
                    {
                        bool success = DateTime.TryParse(strValue, out var parsedDate);
                        if (!success)
                        {
                            throw new ArgumentException($"Cannot parse \"{value}\" to date"); 
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