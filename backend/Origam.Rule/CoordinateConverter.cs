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

namespace Origam.Rule;

using System;

public class Coordinates
{
    public string Wgs84Latitude { get; set; }
    public string Wgs84Longitude { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Height { get; set; }
}

public static class CoordinateConverter
{
    public static Coordinates JtskToWgs(double x, double y)
    {
        return JtskToWgs(x, y, 0);
    }

    public static Coordinates JtskToWgs(double x, double y, double h)
    {
        // Make sure the value signs and order is correct
        if (x < 0 && y < 0)
        {
            x = -x;
            y = -y;
        }
        if (y > x)
        {
            (x, y) = (y, x);
        }
        ;
        return JtskToWgsInternal(x, y, h);
    }

    private static Coordinates JtskToWgsInternal(double x, double y, double h)
    {
        var coordinates = new Coordinates();

        // Constants
        double a = 6377397.15508;
        double e = 0.081696831215303;
        double n = 0.97992470462083;
        double constURo = 12310230.12797036;
        double sinUq = 0.863499969506341;
        double cosUq = 0.504348889819882;
        double sinVq = 0.420215144586493;
        double cosVq = 0.907424504992097;
        double alfa = 1.000597498371542;
        double k = 1.003419163966575;

        // Step 1: Convert planar to geographic coordinates
        double ro = Math.Sqrt((x * x) + (y * y));
        double epsilon = 2 * Math.Atan2(y, ro + x);
        double d = epsilon / n;
        double s = (2 * Math.Atan(Math.Exp(1 / n * Math.Log(constURo / ro)))) - (Math.PI / 2);
        double sinS = Math.Sin(s);
        double cosS = Math.Cos(s);
        double sinU = (sinUq * sinS) - (cosUq * cosS * Math.Cos(d));
        double cosU = Math.Sqrt(1 - (sinU * sinU));
        double sinDv = Math.Sin(d) * cosS / cosU;
        double cosDv = Math.Sqrt(1 - (sinDv * sinDv));
        double sinV = (sinVq * cosDv) - (cosVq * sinDv);
        double cosV = (cosVq * cosDv) + (sinVq * sinDv);
        double ljtsk = 2 * Math.Atan2(sinV, 1 + cosV) / alfa;
        double t = Math.Exp(2 / alfa * Math.Log((1 + sinU) / (cosU * k)));
        double pom = (t - 1) / (t + 1);

        double sinB;
        do
        {
            sinB = pom;
            pom = t * Math.Exp(e * Math.Log((1 + (e * sinB)) / (1 - (e * sinB))));
            pom = (pom - 1) / (pom + 1);
        } while (Math.Abs(pom - sinB) > 1e-14);

        double bjtsk = Math.Atan(pom / Math.Sqrt(1 - (pom * pom)));

        // Step 2: Cartesian coordinates in S-JTSK
        double f1 = 299.152812853;
        double e2 = 1 - Math.Pow(1 - (1 / f1), 2);
        ro = a / Math.Sqrt(1 - (e2 * Math.Sin(bjtsk) * Math.Sin(bjtsk)));
        double xCartesian = (ro + h) * Math.Cos(bjtsk) * Math.Cos(ljtsk);
        double yCartesian = (ro + h) * Math.Cos(bjtsk) * Math.Sin(ljtsk);
        double zCartesian = (((1 - e2) * ro) + h) * Math.Sin(bjtsk);

        // Step 3: Transform to WGS-84 Cartesian coordinates
        double dx = 570.69,
            dy = 85.69,
            dz = 462.84;
        double wz = -5.2611 / 3600 * Math.PI / 180;
        double wy = -1.58676 / 3600 * Math.PI / 180;
        double wx = -4.99821 / 3600 * Math.PI / 180;
        double m = 3.543e-6;

        double xn = dx + ((1 + m) * (xCartesian + (wz * yCartesian) - (wy * zCartesian)));
        double yn = dy + ((1 + m) * ((-wz * xCartesian) + yCartesian + (wx * zCartesian)));
        double zn = dz + ((1 + m) * ((wy * xCartesian) - (wx * yCartesian) + zCartesian));

        // Step 4: Convert to geodetic coordinates in WGS-84
        a = 6378137.0;
        f1 = 298.257223563;
        double aB = f1 / (f1 - 1);
        double p = Math.Sqrt((xn * xn) + (yn * yn));
        e2 = 1 - Math.Pow(1 - (1 / f1), 2);

        double theta = Math.Atan(zn * aB / p);
        double sinTheta = Math.Sin(theta);
        double cosTheta = Math.Cos(theta);
        t = (zn + (e2 * aB * a * Math.Pow(sinTheta, 3))) / (p - (e2 * a * Math.Pow(cosTheta, 3)));
        double b = Math.Atan(t);
        double l = 2 * Math.Atan2(yn, p + xn);
        h = Math.Sqrt(1 + (t * t)) * (p - (a / Math.Sqrt(1 + ((1 - e2) * t * t))));

        // Final formatting
        b = b * 180 / Math.PI;
        coordinates.Latitude = Math.Round(b, 12); // Rounding makes the results consistent, 12 decimal places corresponds to about 1.1132e-7 meters.
        string latitudeDir = b < 0 ? "S" : "N";
        b = Math.Abs(b);
        int degLat = (int)Math.Floor(b);
        b = (b - degLat) * 60;
        int minLat = (int)Math.Floor(b);
        double secLat = Math.Round((b - minLat) * 60 * 1000) / 1000;
        coordinates.Wgs84Latitude = $"{degLat}°{minLat}'{secLat}{latitudeDir}";

        l = l * 180 / Math.PI;
        coordinates.Longitude = Math.Round(l, 12);
        string longitudeDir = l < 0 ? "W" : "E";
        l = Math.Abs(l);
        int degLon = (int)Math.Floor(l);
        l = (l - degLon) * 60;
        int minLon = (int)Math.Floor(l);
        double secLon = Math.Round((l - minLon) * 60 * 1000) / 1000;
        coordinates.Wgs84Longitude = $"{degLon}°{minLon}'{secLon}{longitudeDir}";

        coordinates.Height = Math.Round(h * 100) / 100;

        return coordinates;
    }
}
