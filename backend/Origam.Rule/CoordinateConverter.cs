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
    public static Coordinates JtskToWgs(double X, double Y)
    {
        return JtskToWgs(X, Y, 0);
    }

    public static Coordinates JtskToWgs(double x, double y, double h)
    {
        // Make sure the value signs and order is correct
        if (x < 0 && y < 0)
        {
            x=-x;
            y=-y;
        }
        if (y > x) {
            (x, y) = (y, x);
        };
        return JtskToWgsInternal(x, y, h);
    }

    private static Coordinates JtskToWgsInternal(double X, double Y, double H)
    {
        var coord = new Coordinates();

        // Constants
        double a = 6377397.15508;
        double e = 0.081696831215303;
        double n = 0.97992470462083;
        double konst_u_ro = 12310230.12797036;
        double sinUQ = 0.863499969506341;
        double cosUQ = 0.504348889819882;
        double sinVQ = 0.420215144586493;
        double cosVQ = 0.907424504992097;
        double alfa = 1.000597498371542;
        double k = 1.003419163966575;

        // Step 1: Convert planar to geographic coordinates
        double ro = Math.Sqrt(X * X + Y * Y);
        double epsilon = 2 * Math.Atan2(Y, ro + X);
        double D = epsilon / n;
        double S = 2 * Math.Atan(Math.Exp(1 / n * Math.Log(konst_u_ro / ro))) - Math.PI / 2;
        double sinS = Math.Sin(S);
        double cosS = Math.Cos(S);
        double sinU = sinUQ * sinS - cosUQ * cosS * Math.Cos(D);
        double cosU = Math.Sqrt(1 - sinU * sinU);
        double sinDV = Math.Sin(D) * cosS / cosU;
        double cosDV = Math.Sqrt(1 - sinDV * sinDV);
        double sinV = sinVQ * cosDV - cosVQ * sinDV;
        double cosV = cosVQ * cosDV + sinVQ * sinDV;
        double Ljtsk = 2 * Math.Atan2(sinV, 1 + cosV) / alfa;
        double t = Math.Exp(2 / alfa * Math.Log((1 + sinU) / (cosU * k)));
        double pom = (t - 1) / (t + 1);

        double sinB;
        do
        {
            sinB = pom;
            pom = t * Math.Exp(e * Math.Log((1 + e * sinB) / (1 - e * sinB)));
            pom = (pom - 1) / (pom + 1);
        } while (Math.Abs(pom - sinB) > 1e-14);

        double Bjtsk = Math.Atan(pom / Math.Sqrt(1 - pom * pom));

        // Step 2: Cartesian coordinates in S-JTSK
        double f_1 = 299.152812853;
        double e2 = 1 - Math.Pow(1 - 1 / f_1, 2);
        ro = a / Math.Sqrt(1 - e2 * Math.Sin(Bjtsk) * Math.Sin(Bjtsk));
        double x = (ro + H) * Math.Cos(Bjtsk) * Math.Cos(Ljtsk);
        double y = (ro + H) * Math.Cos(Bjtsk) * Math.Sin(Ljtsk);
        double z = ((1 - e2) * ro + H) * Math.Sin(Bjtsk);

        // Step 3: Transform to WGS-84 Cartesian coordinates
        double dx = 570.69, dy = 85.69, dz = 462.84;
        double wz = -5.2611 / 3600 * Math.PI / 180;
        double wy = -1.58676 / 3600 * Math.PI / 180;
        double wx = -4.99821 / 3600 * Math.PI / 180;
        double m = 3.543e-6;

        double xn = dx + (1 + m) * (x + wz * y - wy * z);
        double yn = dy + (1 + m) * (-wz * x + y + wx * z);
        double zn = dz + (1 + m) * (wy * x - wx * y + z);

        // Step 4: Convert to geodetic coordinates in WGS-84
        a = 6378137.0;
        f_1 = 298.257223563;
        double a_b = f_1 / (f_1 - 1);
        double p = Math.Sqrt(xn * xn + yn * yn);
        e2 = 1 - Math.Pow(1 - 1 / f_1, 2);

        double theta = Math.Atan(zn * a_b / p);
        double st = Math.Sin(theta);
        double ct = Math.Cos(theta);
        t = (zn + e2 * a_b * a * Math.Pow(st, 3)) / (p - e2 * a * Math.Pow(ct, 3));
        double B = Math.Atan(t);
        double L = 2 * Math.Atan2(yn, p + xn);
        H = Math.Sqrt(1 + t * t) * (p - a / Math.Sqrt(1 + (1 - e2) * t * t));

        // Final formatting
        B = B * 180 / Math.PI;
        coord.Latitude = Math.Round(B, 12); // Rounding makes the results consistent, 12 decimal places corresponds to about 1.1132e-7 meters.
        string latitudeDir = B < 0 ? "S" : "N";
        B = Math.Abs(B);
        int degLat = (int)Math.Floor(B);
        B = (B - degLat) * 60;
        int minLat = (int)Math.Floor(B);
        double secLat = Math.Round((B - minLat) * 60 * 1000) / 1000;
        coord.Wgs84Latitude = $"{degLat}°{minLat}'{secLat}{latitudeDir}";

        L = L * 180 / Math.PI;
        coord.Longitude = Math.Round(L, 12);
        string longitudeDir = L < 0 ? "W" : "E";
        L = Math.Abs(L);
        int degLon = (int)Math.Floor(L);
        L = (L - degLon) * 60;
        int minLon = (int)Math.Floor(L);
        double secLon = Math.Round((L - minLon) * 60 * 1000) / 1000;
        coord.Wgs84Longitude = $"{degLon}°{minLon}'{secLon}{longitudeDir}";

        coord.Height = Math.Round(H * 100) / 100;

        return coord;
    }
}
