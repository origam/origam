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

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Origam.Rule.XsltFunctions;

public class OrigamGeoContainer
{
    public static string PolygonFromJstk(string jstkPolygon)
    {
        if (string.IsNullOrWhiteSpace(value: jstkPolygon))
        {
            return "";
        }
        var emptyRegex = new Regex(
            pattern: @"POLYGON\s+EMPTY",
            options: RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        if (emptyRegex.Match(input: jstkPolygon).Success)
        {
            return jstkPolygon;
        }
        var numberRegex = new Regex(pattern: @"-?\d+\.?\d+", options: RegexOptions.Compiled);
        var matches = numberRegex.Matches(input: jstkPolygon);
        if (matches.Count == 0 || matches.Count % 2 != 0)
        {
            return "";
        }
        var converted = new List<string>(capacity: matches.Count);
        for (int i = 0; i < matches.Count; i += 2)
        {
            var xString = matches[i: i].Value;
            var yString = matches[i: i + 1].Value;
            if (
                !double.TryParse(
                    s: xString,
                    style: NumberStyles.Float,
                    provider: CultureInfo.InvariantCulture,
                    result: out double x
                )
                || !double.TryParse(
                    s: yString,
                    style: NumberStyles.Float,
                    provider: CultureInfo.InvariantCulture,
                    result: out double y
                )
            )
            {
                return "";
            }
            Coordinates wgs = CoordinateConverter.JtskToWgs(x: x, y: y);
            converted.Add(item: XmlConvert.ToString(value: wgs.Longitude));
            converted.Add(item: XmlConvert.ToString(value: wgs.Latitude));
        }
        var stringBuilder = new StringBuilder();
        int lastIndex = 0;
        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i: i];
            stringBuilder.Append(
                value: jstkPolygon,
                startIndex: lastIndex,
                count: match.Index - lastIndex
            );
            stringBuilder.Append(value: converted[index: i]);
            lastIndex = match.Index + match.Length;
        }
        stringBuilder.Append(
            value: jstkPolygon,
            startIndex: lastIndex,
            count: jstkPolygon.Length - lastIndex
        );
        return stringBuilder.ToString();
    }

    public static string PointFromJtsk(double x, double y)
    {
        return LegacyXsltFunctionContainer.PointFromJtsk(x: x, y: y);
    }
}
