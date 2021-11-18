// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// ExtendedFormatHelper.cs
//
// Provides an extended format helper to specify concurrent expirations.
// 
//
// For more information see the Configuration Management Application Block Implementation Overview. 
// 
// ===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
// ==============================================================================

using System;
using System.Collections;
using System.Configuration;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
	#region Extended Format class

	/// <summary>
	/// This class represents a extended format 
	/// </summary>
	internal class ExtendedFormat
	{
		private static readonly char ARGUMENT_DELIMITER = Convert.ToChar(",", System.Globalization.CultureInfo.CurrentUICulture);
		private static readonly char WILDCARD_ALL = Convert.ToChar("*", System.Globalization.CultureInfo.CurrentUICulture);
		private readonly static char REFRESH_DELIMITER = Convert.ToChar(" ", System.Globalization.CultureInfo.CurrentUICulture); 

		private int[] _minutes;
		private int[] _hours;
		private int[] _days;
		private int[] _months;
		private int[] _daysOfWeek;
        
		public ExtendedFormat( string format )
		{
			string[] parsedFormat = format.Trim().Split( REFRESH_DELIMITER );
            
			if( parsedFormat.Length != 5 )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidExtendedFormatArguments" ] );

			_minutes = ParseValueToInt( parsedFormat[ 0 ] );
			foreach( int minute in _minutes )
				if( minute > 59 )
					throw new ArgumentOutOfRangeException( "format", Resource.ResourceManager[ "RES_ExceptionExtendedFormatIncorrectMinutePart" ] );
			_hours = ParseValueToInt( parsedFormat[ 1 ] );
			foreach( int hour in _hours )
				if( hour > 23 )
					throw new ArgumentOutOfRangeException( "format", Resource.ResourceManager[ "RES_ExceptionExtendedFormatIncorrectHourPart" ] );
			_days = ParseValueToInt( parsedFormat[ 2 ] );
			foreach( int day in _days )
				if( day > 31 )
					throw new ArgumentOutOfRangeException( "format", Resource.ResourceManager[ "RES_ExceptionExtendedFormatIncorrectDayPart" ] );
			_months = ParseValueToInt( parsedFormat[ 3 ] );
			foreach( int month in _months )
				if( month > 12 )
					throw new ArgumentOutOfRangeException( "format", Resource.ResourceManager[ "RES_ExceptionExtendedFormatIncorrectMonthPart" ] );
			_daysOfWeek = ParseValueToInt( parsedFormat[ 4 ] );
			foreach( int dayOfWeek in _daysOfWeek )
				if( dayOfWeek > 6 )
					throw new ArgumentOutOfRangeException( "format", Resource.ResourceManager[ "RES_ExceptionExtendedFormatIncorrectDayOfWeekPart" ] );
		}

		public int[] Minutes
		{
			get { return _minutes; }
		}

		public int[] Hours
		{
			get { return _hours; }
		}

		public int[] Days
		{
			get { return _days; }
		}

		public int[] Months
		{
			get { return _months; }
		}

		public int[] DaysOfWeek
		{
			get { return _daysOfWeek; }
		}

		public bool ExpireEveryMinute
		{
			get { return _minutes[ 0 ] == -1; }
		}

		public bool ExpireEveryDay
		{
			get { return _days[ 0 ] == -1; }
		}

		public bool ExpireEveryHour
		{
			get { return _hours[ 0 ] == -1; }
		}

		public bool ExpireEveryMonth
		{
			get { return _months[ 0 ] == -1; }
		}

		public bool ExpireEveryDayOfWeek
		{
			get { return _daysOfWeek[ 0 ] == -1; }
		}

		private int[] ParseValueToInt( string value )
		{
			int[] result;

			if( value.IndexOf( WILDCARD_ALL ) != -1 )
			{
				result = new int[1];
				result[0] = -1;
			}
			else
			{
				string[] values = value.Split( ARGUMENT_DELIMITER );
				result = new int[ values.Length ]; 
				for( int i = 0; i < values.Length; i++ )
				{
					result[ i ] =  int.Parse( values[ i ], System.Globalization.CultureInfo.CurrentUICulture );
				}
			}

			return result;
		}
	}
	#endregion

	#region Extended Format Helper class
	/// <summary>
	/// This class tests if a item was expired using a extended format
	/// </summary>
	/// <remarks>
	/// Extended format syntax : <br/><br/>
	/// 
	/// Minute       - 0-59 <br/>
	/// Hour         - 0-23 <br/>
	/// Day of month - 1-31 <br/>
	/// Month        - 1-12 <br/>
	/// Day of week  - 0-6 (Sunday is 0) <br/>
	/// Wildcards    - * means run every <br/>
	/// Examples: <br/>
	/// * * * * *    - expires every minute<br/>
	/// 5 * * * *    - expire 5th minute of every hour <br/>
	/// * 21 * * *   - expire every minute of the 21st hour of every day <br/>
	/// 31 15 * * *  - expire 3:31 PM every day <br/>
	/// 7 4 * * 6    - expire Saturday 4:07 AM <br/>
	/// 15 21 4 7 *  - expire 9:15 PM on 4 July <br/>
	///	Therefore 6 6 6 6 1 means:
	///	•	have we crossed/entered the 6th minute AND
	///	•	have we crossed/entered the 6th hour AND 
	///	•	have we crossed/entered the 6th day AND
	///	•	have we crossed/entered the 6th month AND
	///	•	have we crossed/entered A MONDAY?
	///
	///	Therefore these cases should exhibit these behaviors:
	///
	///	getTime = DateTime.Parse( "02/20/2003 04:06:55 AM" );
	///	nowTime = DateTime.Parse( "06/07/2003 07:07:00 AM" );
	///	isExpired = ExtendedFormatHelper.IsExtendedExpired( "6 6 6 6 1", getTime, nowTime );
	///	TRUE, ALL CROSSED/ENTERED
	///			
	///	getTime = DateTime.Parse( "02/20/2003 04:06:55 AM" );
	///	nowTime = DateTime.Parse( "06/07/2003 07:07:00 AM" );
	///	isExpired = ExtendedFormatHelper.IsExtendedExpired( "6 6 6 6 5", getTime, nowTime );
	///	TRUE
	///			
	///	getTime = DateTime.Parse( "02/20/2003 04:06:55 AM" );
	///	nowTime = DateTime.Parse( "06/06/2003 06:06:00 AM" );
	///	isExpired = ExtendedFormatHelper.IsExtendedExpired( "6 6 6 6 *", getTime, nowTime );
	///	TRUE
	///	
	///			
	///	getTime = DateTime.Parse( "06/05/2003 04:06:55 AM" );
	///	nowTime = DateTime.Parse( "06/06/2003 06:06:00 AM" );
	///	isExpired = ExtendedFormatHelper.IsExtendedExpired( "6 6 6 6 5", getTime, nowTime );
	///	TRUE
	///						
	///	getTime = DateTime.Parse( "06/05/2003 04:06:55 AM" );
	///	nowTime = DateTime.Parse( "06/06/2005 05:06:00 AM" );
	///	isExpired = ExtendedFormatHelper.IsExtendedExpired( "6 6 6 6 1", getTime, nowTime );
	///	TRUE
	///						
	///	getTime = DateTime.Parse( "06/05/2003 04:06:55 AM" );
	///	nowTime = DateTime.Parse( "06/06/2003 05:06:00 AM" );
	///	isExpired = ExtendedFormatHelper.IsExtendedExpired( "6 6 6 6 1", getTime, nowTime );
	///	FALSE:  we did not cross 6th hour, nor did we cross Monday
	///						
	///	getTime = DateTime.Parse( "06/05/2003 04:06:55 AM" );
	///	nowTime = DateTime.Parse( "06/06/2003 06:06:00 AM" );
	///	isExpired = ExtendedFormatHelper.IsExtendedExpired( "6 6 6 6 5", getTime, nowTime );
	///	TRUE, we cross/enter Friday
	///
	///
	///	getTime = DateTime.Parse( "06/05/2003 04:06:55 AM" );
	///	nowTime = DateTime.Parse( "06/06/2003 06:06:00 AM" );
	///	isExpired = ExtendedFormatHelper.IsExtendedExpired( "6 6 6 6 1", getTime, nowTime );
	///	FALSE:  we don’t cross Monday but all other conditions satisfied
	/// </remarks>
	internal class ExtendedFormatHelper
	{
		private readonly static char REFRESH_DELIMITER = Convert.ToChar(" ", System.Globalization.CultureInfo.CurrentUICulture); 
		private static readonly char WILDCARD_ALL = Convert.ToChar("*", System.Globalization.CultureInfo.CurrentUICulture);
		
		private static Hashtable _parsedFormatCache = new Hashtable();
 
		/// <summary>
		/// Test the extended format with a given date.
		/// </summary>
		/// <param name="format">The extended format arguments.</param>
		/// <param name="getTime">The time when the item has been refreshed.</param>
		/// <param name="nowTime">Always DateTime.Now, or the date to test with.</param>
		/// <returns>true if the item was expired, otherwise false</returns>
		public static bool IsExtendedExpired( string format, DateTime getTime, DateTime nowTime )
		{
			// Validate arguments
			if(format == null)
				throw new ArgumentNullException("format");

			// Remove the seconds to provide better precission on calculations
			getTime = getTime.AddSeconds( getTime.Second * -1 );
			nowTime = nowTime.AddSeconds( nowTime.Second * -1 );

			ExtendedFormat parsedFormat = (ExtendedFormat)_parsedFormatCache[ format ];
			if( parsedFormat == null )
			{
				parsedFormat = new ExtendedFormat( format );
				lock(_parsedFormatCache.SyncRoot)
					_parsedFormatCache[ format ] = parsedFormat;
			}

			if( nowTime.Subtract( getTime ).TotalMinutes < 1 )
				return false;

			// Validate the format arguments
			foreach( int minute in parsedFormat.Minutes )
			{
				foreach( int hour in parsedFormat.Hours )
				{
					foreach( int day in parsedFormat.Days )
					{
						foreach( int month in parsedFormat.Months )
						{
							// Set the expiration date parts
							int expirMinute = minute == -1 ? getTime.Minute : minute;
							int expirHour = hour == -1 ? getTime.Hour : hour;
							int expirDay = day == -1 ? getTime.Day : day;
							int expirMonth = month == -1 ? getTime.Month : month;
							int expirYear = getTime.Year;
							
							// Adjust when wildcards are set
							if( minute == -1 && hour != -1 )
								expirMinute = 0;
							if( hour == -1 && day != -1 )
								expirHour = 0;
							if( minute == -1 && day != -1 )
								expirMinute = 0;
							if( day == -1 && month != -1 )
								expirDay = 1;
							if( hour == -1 && month != -1 )
								expirHour = 0;
							if( minute == -1 && month != -1 )
								expirMinute = 0;

							if( DateTime.DaysInMonth( expirYear, expirMonth ) < expirDay )
							{
								if( expirMonth == 12 )
								{
									expirMonth = 1;
									expirYear++;
								}
								else
									expirMonth++;
								expirDay = 1;
							}

							// Create the date with the adjusted parts
							DateTime expTime = 
								new DateTime( 
								expirYear, expirMonth, expirDay, 
								expirHour, expirMinute, 0 );

							// Adjust when expTime is before getTime
							if( expTime < getTime )
							{
								if( month != -1 && getTime.Month >= month )
									expTime = expTime.AddYears(1);
								else if( day != -1 && getTime.Day >= day )
									expTime = expTime.AddMonths(1);
								else if( hour != -1 && getTime.Hour >= hour )
									expTime = expTime.AddDays(1);
								else if( minute != -1 && getTime.Minute >= minute )
									expTime = expTime.AddHours(1);
							}

							// Is Expired?
							if( parsedFormat.ExpireEveryDayOfWeek )
							{
								if(nowTime >= expTime)
									return true;
							}
							else
							{
								// Validate WeekDay
								foreach( int weekDay in parsedFormat.DaysOfWeek )
								{
									DateTime tmpTime = getTime;
									tmpTime = tmpTime.AddHours( -1 * tmpTime.Hour );
									tmpTime = tmpTime.AddMinutes( -1 * tmpTime.Minute );
									while( (int)tmpTime.DayOfWeek != weekDay )
									{
										tmpTime = tmpTime.AddDays(1);
									}
									if(nowTime >= tmpTime && nowTime >= expTime)
										return true;
								}
							}
						}
					}
				}
			}
			return false;
		}
	}
	#endregion
}
