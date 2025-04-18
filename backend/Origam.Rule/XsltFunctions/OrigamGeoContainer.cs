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
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Origam.Rule.XsltFunctions;

public class OrigamGeoContainer
{
    private static readonly Regex polygonCoordinatePair =
        new Regex("(-?\\d+\\.?\\d+)\\s(-?\\d+\\.?\\d+)");

    public static string PolygonFromJstk(string jstkPolygon)
    {
        MatchCollection matches = polygonCoordinatePair.Matches(jstkPolygon);

        if (matches.Count == 0)
        {
            return "";
        }

        IEnumerable<string> wgsCoordinates = matches
            .Cast<Match>()
            .Select(match =>
            {
                string xString = match.Groups[1].Value;
                string yString = match.Groups[2].Value;
                Coordinates wgs = ToWgsCoordinates(xString, yString);
                return
                    $"{XmlConvert.ToString(wgs.Longitude)} {XmlConvert.ToString(wgs.Latitude)}";
            });

        return $"POLYGON({string.Join(", ", wgsCoordinates)})";
    }

    private static Coordinates ToWgsCoordinates(string xString,
        string yString)
    {
        if (!double.TryParse(xString, out double x))
        {
            throw new Exception(
                $"Cannot parse polygon coordinate \"{xString}\" to double");
        }

        if (!double.TryParse(yString, out double y))
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