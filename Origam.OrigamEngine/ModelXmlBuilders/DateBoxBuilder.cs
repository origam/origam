#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Xml;
using System.Globalization;

namespace Origam.OrigamEngine.ModelXmlBuilders
{
	/// <summary>
	/// Summary description for DateBoxBuilder.
	/// </summary>
	public class DateBoxBuilder
	{
		public static void Build(XmlElement propertyElement, string format, string customFormat)
		{
			string pattern;
			CultureInfo culture = CultureInfo.CurrentCulture;

			if(customFormat != null && customFormat != String.Empty)
			{
				switch(customFormat)
				{
                    case "dd. MM. yyyy":
                        pattern = ConvertToFlexPattern(culture.DateTimeFormat.ShortDatePattern);
                        break;
                    case "dd. MM. yyyy HH:mm":
						pattern = ConvertToFlexPattern(culture.DateTimeFormat.ShortDatePattern + " " + culture.DateTimeFormat.ShortTimePattern);
						break;
                    case "dd. MM. yyyy HH:mm:ss":
						pattern = ConvertToFlexPattern(culture.DateTimeFormat.ShortDatePattern + " " + culture.DateTimeFormat.LongTimePattern);
						break;
                    case "dd. MM. yyyy HH:mm:ss.fff":
                        pattern = ConvertToFlexPattern(culture.DateTimeFormat.ShortDatePattern + " " + culture.DateTimeFormat.LongTimePattern + ".fff");
                        break;
                    case "ddd d. MMMM yyyy":
                        pattern = ConvertToFlexPattern(AddDayName(culture.DateTimeFormat.LongDatePattern));
                        break;
                    case "ddd d. MMMM yyyy HH:mm":
						pattern = ConvertToFlexPattern(AddDayName(culture.DateTimeFormat.LongDatePattern) + " " + culture.DateTimeFormat.ShortTimePattern);
						break;
					default:
						pattern = ConvertToFlexPattern(customFormat);
						break;
				}
			}
			else
			{
				switch(format)
				{
					case "Long":
						pattern = ConvertToFlexPattern(culture.DateTimeFormat.ShortDatePattern);
						break;
					default:
						pattern = "EEE DD. MM YYYY JJ:NN";
						break;
				}
			}

			propertyElement.SetAttribute("Entity", "Date");
			propertyElement.SetAttribute("Column", "Date");
			propertyElement.SetAttribute("FormatterPattern", pattern);
		}

		private static string ConvertToFlexPattern(string netPattern)
		{
			string result = netPattern;
            result = result.Replace("dddd", "EEEE");
            result = result.Replace("ddd", "EEE");
			result = result.Replace("d", "D");
			result = result.Replace("y", "Y");
			result = result.Replace("H", "J");
			result = result.Replace("h", "K");
			result = result.Replace("m", "N");
			result = result.Replace("s", "S");
			result = result.Replace("f", "Q");
			result = result.Replace("tt", "A");
			result = result.Replace("t", "A");

			return result;
		}

		private static string AddDayName(string netPattern)
		{
			if(netPattern.IndexOf("ddd") == -1)
			{
				return "ddd " + netPattern;
			}

			return netPattern;
		}
	}
}
