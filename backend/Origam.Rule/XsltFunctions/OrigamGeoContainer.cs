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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Origam.Rule.XsltFunctions;

public class OrigamGeoContainer
{
    public static string PolygonFromJstk(string jstkPolygon)
    {
        if (string.IsNullOrWhiteSpace(jstkPolygon))
        {
            return "";
        }

        var emptyRegex = new Regex(@"POLYGON\s+EMPTY",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        if (emptyRegex.Match(jstkPolygon).Success)
        {
            return jstkPolygon;
        }

        var numberRegex = new Regex(@"-?\d+\.?\d+", RegexOptions.Compiled);
        var matches = numberRegex.Matches(jstkPolygon);
        if (matches.Count == 0 || matches.Count % 2 != 0)
        {
            return "";
        }
        var converted = new List<string>(matches.Count);
        for (int i = 0; i < matches.Count; i += 2)
        {
            var xString = matches[i].Value;
            var yString = matches[i + 1].Value;
            if (!double.TryParse(xString, NumberStyles.Float, CultureInfo.InvariantCulture, out double x) ||
                !double.TryParse(yString, NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
            {
                return "";
            }

            Coordinates wgs = CoordinateConverter.JtskToWgs(x, y);
            converted.Add(XmlConvert.ToString(wgs.Longitude));
            converted.Add(XmlConvert.ToString(wgs.Latitude));
        }
        var stringBuilder = new StringBuilder();
        int lastIndex = 0;
        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            stringBuilder.Append(jstkPolygon, lastIndex, match.Index - lastIndex);
            stringBuilder.Append(converted[i]);
            lastIndex = match.Index + match.Length;
        }
        stringBuilder.Append(jstkPolygon, lastIndex, jstkPolygon.Length - lastIndex);
        return stringBuilder.ToString();
    }

    public static string PointFromJtsk(double x, double y)
    {
        return LegacyXsltFunctionContainer.PointFromJtsk(x, y);
    }
}
