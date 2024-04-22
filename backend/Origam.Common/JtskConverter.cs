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

namespace Origam.Geo;

/// <summary>
/// Summary description for GeoTools.
/// </summary>
public class JtskConverter
{
	private const double EPS = 1e-4; // relative accuracy

	struct BLH
	{
		public double B;
		public double L;
		public double H;
	}

	struct GeoCoords
	{
		public double X;
		public double Y;
		public double Z;
	}

	public struct Jtsk
	{
		public double X;
		public double Y;
	}

	public struct Wgs84
	{
		public Wgs84(double latitude, double longitude)
		{
				Latitude = latitude;
				Longitude = longitude;
				Altitude = 0;
			}

		public double Latitude;
		public double Longitude;
		public double Altitude;
	}

	struct Bessel
	{
		public double Latitude;
		public double Longitude;
	}

	/**
	 * Calculate distance between two points
	 * @param x1
	 * @param y1
	 * @param x2
	 * @param y2
	 * @returns {*}
	 */
	private static double distPoints (double x1, double y1, double x2, double y2)
	{
			double dist = hypot(x1 - x2, y1 - y2);
			if (dist < EPS) 
			{
				return 0;
			}

			return dist;
		}

	/**
	 * Coordinates transformation
	 * @param xs
	 * @param ys
	 * @param zs
	 * @returns {Array}
	 */
	private static GeoCoords transformCoords (GeoCoords s)
	{
			// coeficients of transformation from WGS-84 to JTSK
			double dx = -570.69;
			double dy = -85.69;
			double dz = -462.84; // shift
			double wx = 4.99821/3600 * Math.PI / 180;
			double wy = 1.58676/3600 * Math.PI / 180;
			double wz = 5.2611/3600 * Math.PI / 180; // rotation
			double m  = -3.543e-6; // scale

			GeoCoords result = new GeoCoords();
			result.X = dx + (1 + m) * (+s.X + wz * s.Y - wy * s.Z);
			result.Y = dy + (1 + m) * (-wz * s.X + s.Y + wx * s.Z);
			result.Z = dz + (1 + m) * (+wy * s.X - wx * s.Y + s.Z);

			return result;
		}

	// helper Math functions
	private static double deg2rad (double deg) 
	{
			return (deg / 180) * Math.PI;
		}

	private static double rad2deg (double rad) 
	{
			return rad / Math.PI * 180;
		}

	private static double hypot (double x, double y) 
	{
			try
			{
				return Math.Sqrt(x * x + y * y);
			}
			catch
			{
				return 0.0;
			}
		}

	/**
	 * Conversion from JTSK to WGS-84 (by iteration)
	 * @param x
	 * @param y
	 * @returns {{lat: number, lon: number}}
	 */
	public static Wgs84 JTSKtoWGS84 (Jtsk source)
	{
			Wgs84 result = new Wgs84();

			if (source.X == 0 && source.Y == 0) 
			{
				return result;
			}

			double delta = 5;
			double latitude = 49;
			double longitude = 14;
			double steps = 0;

			Jtsk jtsk;
			double v1, v2, v3, v4;

			do 
			{
				jtsk = WGS84toJTSK(new Wgs84(latitude - delta, longitude - delta));
				if (jtsk.X != 0 && jtsk.Y != 0) 
				{
					v1 = distPoints(jtsk.X, jtsk.Y, source.X, source.Y);
				} 
				else 
				{
					v1 = 1e32;
				}

				jtsk = WGS84toJTSK(new Wgs84(latitude - delta, longitude + delta));
				if (jtsk.X != 0 && jtsk.Y != 0) 
				{
					v2 = distPoints(jtsk.X, jtsk.Y, source.X, source.Y);
				} 
				else 
				{
					v2 = 1e32;
				}

				jtsk = WGS84toJTSK(new Wgs84(latitude + delta, longitude - delta));
				if (jtsk.X != 0 && jtsk.Y != 0) 
				{
					v3 = distPoints(jtsk.X, jtsk.Y, source.X, source.Y);
				} 
				else 
				{
					v3 = 1e32;
				}

				jtsk = WGS84toJTSK(new Wgs84(latitude + delta, longitude + delta));
				if (jtsk.X != 0 && jtsk.Y != 0) 
				{
					v4 = distPoints(jtsk.X, jtsk.Y, source.X, source.Y);
				} 
				else 
				{
					v4 = 1e32;
				}

				if ((v1 <= v2) && (v1 <= v3) && (v1 <= v4)) 
				{
					latitude = latitude - delta / 2;
					longitude = longitude - delta / 2;
				}

				if ((v2 <= v1) && (v2 <= v3) && (v2 <= v4)) 
				{
					latitude = latitude - delta / 2;
					longitude = longitude + delta / 2;
				}

				if ((v3 <= v1) && (v3 <= v2) && (v3 <= v4)) 
				{
					latitude = latitude + delta / 2;
					longitude = longitude - delta / 2;
				}

				if ((v4 <= v1) && (v4 <= v2) && (v4 <= v3)) 
				{
					latitude = latitude + delta / 2;
					longitude = longitude + delta / 2;
				}

				delta *= 0.55;
				steps += 4;

			} while (!((delta < 0.00001) || (steps > 1000)));

			result.Latitude = latitude;
			result.Longitude = longitude;
			return result;
		}

	/**
	 * Conversion from WGS-84 to JTSK
	 * @param latitude
	 * @param longitude
	 * @returns {{x: number, y: number}}
	 */
	public static Jtsk WGS84toJTSK (Wgs84 wgs)
	{
			Jtsk result = new Jtsk();

			if ((wgs.Latitude < 40) || (wgs.Latitude > 60) || (wgs.Longitude < 5) || (wgs.Longitude > 25)) 
			{
				result.X = 0;
				result.Y = 0;
				return result;
			}
			else 
			{
				Bessel lonlat = WGS84toBessel(wgs);
				return BesseltoJTSK(lonlat);
			}
		}

	/**
	 * Conversion from ellipsoid WGS-84 to Bessel's ellipsoid
	 * @param latitude
	 * @param longitude
	 * @param altitude
	 * @returns {Array}
	 */
	private static Bessel WGS84toBessel (Wgs84 wgs)
	{
			BLH blh1 = new BLH();
			blh1.B = deg2rad(wgs.Latitude);
			blh1.L = deg2rad(wgs.Longitude);
			blh1.H = wgs.Altitude;

			GeoCoords xyz1 = BLHToGeoCoords(blh1);
			GeoCoords xyz2 = transformCoords(xyz1);
			BLH blh2 = GeoCoordsToBLH(xyz2);

			Bessel result = new Bessel();
			result.Latitude = rad2deg(blh2.B);
			result.Longitude = rad2deg(blh2.L);
			//Altitude = H;

			return result;
		}

	/**
	 * Conversion from Bessel's lat/lon to WGS-84
	 * @param latitude
	 * @param longitude
	 * @returns {{x: number, y: number}}
	 */
	private static Jtsk BesseltoJTSK (Bessel bessel)
	{
			double e     = 0.081696831215303;
			double n     = 0.97992470462083;
			double rho_0 = 12310230.12797036;
			double sinUQ = 0.863499969506341;
			double cosUQ = 0.504348889819882;
			double sinVQ = 0.420215144586493;
			double cosVQ = 0.907424504992097;
			double alfa  = 1.000597498371542;
			double k_2   = 1.00685001861538;

			double B = deg2rad(bessel.Latitude);
			double L = deg2rad(bessel.Longitude);

			double sinB = Math.Sin(B);
			double t = (1 - e * sinB) / (1 + e * sinB);
			t = Math.Pow(1 + sinB, 2) / (1 - Math.Pow(sinB, 2)) * Math.Exp(e * Math.Log(t));
			t = k_2 * Math.Exp(alfa * Math.Log(t));

			double sinU  = (t - 1) / (t + 1);
			double cosU  = Math.Sqrt(1 - sinU * sinU);
			double V     = alfa * L;
			double sinV  = Math.Sin(V);
			double cosV  = Math.Cos(V);
			double cosDV = cosVQ * cosV + sinVQ * sinV;
			double sinDV = sinVQ * cosV - cosVQ * sinV;
			double sinS  = sinUQ * sinU + cosUQ * cosU * cosDV;
			double cosS  = Math.Sqrt(1 - sinS * sinS);
			double sinD  = sinDV * cosU / cosS;
			double cosD  = Math.Sqrt(1 - sinD * sinD);

			double eps = n * Math.Atan(sinD / cosD);
			double rho = rho_0 * Math.Exp(-n * Math.Log((1 + sinS) / cosS));

			Jtsk result = new Jtsk();
			result.X = rho * Math.Cos(eps);
			result.Y = rho * Math.Sin(eps);
			return result;
		}

	/**
	 * Conversion from geodetic coordinates to Cartesian coordinates
	 * @param B
	 * @param L
	 * @param H
	 * @returns {Array}
	 */
	private static GeoCoords BLHToGeoCoords(BLH blh)
	{
			//  WGS-84 ellipsoid parameters
			double a   = 6378137.0;
			double f_1 = 298.257223563;
			double e2  = 1 - Math.Pow(1 - 1 / f_1, 2);
			double rho = a / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(blh.B), 2));

			GeoCoords result = new GeoCoords();
			result.X = (rho + blh.H) * Math.Cos(blh.B) * Math.Cos(blh.L);
			result.Y = (rho + blh.H) * Math.Cos(blh.B) * Math.Sin(blh.L);
			result.Z = ((1 - e2) * rho + blh.H) * Math.Sin(blh.B);

			return result;
		}

	/**
	 * Conversion from Cartesian coordinates to geodetic coordinates
	 * @param x
	 * @param y
	 * @param z
	 * @returns {Array}
	 */
	private static BLH GeoCoordsToBLH (GeoCoords coords)
	{
			// Bessel's ellipsoid parameters
			double a   = 6377397.15508;
			double f_1 = 299.152812853;
			double a_b = f_1 / (f_1-1);
			double p   = Math.Sqrt(Math.Pow(coords.X, 2) + Math.Pow(coords.Y, 2));
			double e2  = 1 - Math.Pow(1 - 1 / f_1, 2);
			double th  = Math.Atan(coords.Z * a_b / p);
			double st  = Math.Sin(th);
			double ct  = Math.Cos(th);
			double t   = (coords.Z + e2 * a_b * a * Math.Pow(st, 3)) / (p - e2 * a * Math.Pow(ct, 3));

			BLH result = new BLH();
			result.B = Math.Atan(t);
			result.H = Math.Sqrt(1 + t * t) * (p - a / Math.Sqrt(1 + (1 - e2) * t * t));
			result.L = 2 * Math.Atan(coords.Y / (p + coords.X));

			return result;
		}	
}