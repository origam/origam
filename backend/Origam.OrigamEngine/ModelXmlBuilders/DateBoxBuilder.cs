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
using System.Globalization;
using System.Xml;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for DateBoxBuilder.
/// </summary>
public class DateBoxBuilder
{
    public static void Build(XmlElement propertyElement, string format, string customFormat)
    {
        string pattern;
        CultureInfo culture = CultureInfo.CurrentCulture;
        if (format == "Custom" && !string.IsNullOrEmpty(customFormat))
        {
            switch (customFormat)
            {
                case "dd. MM. yyyy":
                {
                    pattern = culture.DateTimeFormat.ShortDatePattern;
                    break;
                }

                case "dd. MM. yyyy HH:mm":
                {
                    pattern =
                        culture.DateTimeFormat.ShortDatePattern
                        + " "
                        + culture.DateTimeFormat.ShortTimePattern;
                    break;
                }

                case "dd. MM. yyyy HH:mm:ss":
                {
                    pattern =
                        culture.DateTimeFormat.ShortDatePattern
                        + " "
                        + culture.DateTimeFormat.LongTimePattern;
                    break;
                }

                case "dd. MM. yyyy HH:mm:ss.fff":
                {
                    pattern =
                        culture.DateTimeFormat.ShortDatePattern
                        + " "
                        + culture.DateTimeFormat.LongTimePattern
                        + ".fff";
                    break;
                }

                case "ddd d. MMMM yyyy":
                {
                    pattern = AddDayName(culture.DateTimeFormat.LongDatePattern);
                    break;
                }

                case "ddd d. MMMM yyyy HH:mm":
                {
                    pattern =
                        AddDayName(culture.DateTimeFormat.LongDatePattern)
                        + " "
                        + culture.DateTimeFormat.ShortTimePattern;
                    break;
                }

                default:
                {
                    pattern = customFormat;
                    break;
                }
            }
        }
        else
        {
            switch (format)
            {
                case "Long":
                {
                    pattern = "long";
                    break;
                }

                case "Short":
                {
                    pattern = "short";
                    break;
                }

                case "Time":
                {
                    pattern = "time";
                    break;
                }

                default:
                    throw new NotImplementedException("Unknown option " + format);
            }
        }
        propertyElement.SetAttribute("Entity", "Date");
        propertyElement.SetAttribute("Column", "Date");
        propertyElement.SetAttribute("FormatterPattern", pattern);
    }

    private static string AddDayName(string netPattern)
    {
        if (netPattern.IndexOf("ddd") == -1)
        {
            return "ddd " + netPattern;
        }
        return netPattern;
    }
}
