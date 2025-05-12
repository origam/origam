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
    private static readonly Regex Polygon =
        new Regex(
            @"^(?<prefix>[^0-9-]*)
              (?:
                (?<x>-?\d+\.?\d+)   
                \s+
                (?<y>-?\d+\.?\d+)  
                [,\s]*              
              )+
              (?<suffix>.*)$", 
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    public static string PolygonFromJstk(string jstkPolygon)
    {
        var match = Polygon.Match(jstkPolygon);
        if (!match.Success)
        {
            return "";
        }

        var xCoordinates = match.Groups["x"].Captures;
        var yCoordinates = match.Groups["y"].Captures;

        var stringBuilder = new StringBuilder(match.Groups["prefix"].Value);
        for (int i = 0; i < xCoordinates.Count; i++)
        {
            Coordinates wgs = ToWgsCoordinates(
                xCoordinates[i].Value, 
                yCoordinates[i].Value);
            stringBuilder.Append(XmlConvert.ToString(wgs.Longitude));
            stringBuilder.Append(" ");
            stringBuilder.Append(XmlConvert.ToString(wgs.Latitude));
            if (i != xCoordinates.Count - 1)
            {
                stringBuilder.Append(", ");
            }
        }

        stringBuilder.Append(match.Groups["suffix"].Value);
        return stringBuilder.ToString();
    }

    private static Coordinates ToWgsCoordinates(string xString,
        string yString)
    {
        if (!double.TryParse(xString, NumberStyles.Float,
                CultureInfo.InvariantCulture, out double x))
        {
            throw new Exception(
                $"Cannot parse polygon coordinate \"{xString}\" to double");
        }

        if (!double.TryParse(yString, NumberStyles.Float,
                CultureInfo.InvariantCulture, out double y))
        {
            throw new Exception(
                $"Cannot parse polygon coordinate \"{yString}\" to double");
        }

        return CoordinateConverter.JtskToWgs(x, y);
    }

    public static string PointFromJtsk(double x, double y)
    {
        return LegacyXsltFunctionContainer.PointFromJtsk(x, y);
    }
}