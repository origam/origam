/***************************************************************************
 * Copyright Andy Brummer 2004-2005
 *
 * This code is provided "as is", with absolutely no warranty expressed
 * or implied. Any use is at your own risk.
 *
 * This code may be used in compiled form in any way you desire. This
 * file may be redistributed unmodified by any means provided it is
 * not sold for profit without the authors written consent, and
 * providing that this notice and the authors name is included. If
 * the source code in  this file is used in any commercial application
 * then a simple email would be nice.
 *
 **************************************************************************/

using System;
using System.Collections.Generic;

namespace Schedule;

public enum EventTimeBase
{
    BySecond = 1,
    ByMinute = 2,
    Hourly = 3,
    Daily = 4,
    Weekly = 5,
    Monthly = 6,
}

/// <summary>
/// This class represents a simple schedule.  It can represent a repeating event that occurs anywhere from every
/// second to once a month.  It consists of an enumeration to mark the interval and an offset from that interval.
/// For example new ScheduledTime(Hourly, new TimeSpan(0, 15, 0)) would represent an event that fired 15 minutes
/// after the hour every hour.
/// </summary>
[Serializable]
public class ScheduledTime : IScheduledItem
{
    public ScheduledTime(EventTimeBase Base, TimeSpan Offset)
    {
        _Base = Base;
        _Offset = Offset;
    }

    /// <summary>
    /// intializes a simple scheduled time element from a pair of strings.
    /// Here are the supported formats
    ///
    /// BySecond - single integer representing the offset in ms value must be less then 1000
    /// ByMinute - A comma seperated list of integers representing the number of seconds and ms
    /// Hourly - A comma seperated list of integers representing the number of minutes, seconds and ms
    /// Daily - A time in hh:mm:ss AM/PM format
    /// Weekly - n, time where n represents an integer and time is a time in the Daily format
    /// Monthly - the same format as weekly.
    ///
    /// </summary>
    /// <param name="StrBase">A string representing the base enumeration for the scheduled time</param>
    /// <param name="StrOffset">A string representing the offset for the time.</param>
    public ScheduledTime(string StrBase, string StrOffset)
    {
        //TODO:Create an IScheduled time factory method.
        _Base = (EventTimeBase)
            Enum.Parse(enumType: typeof(EventTimeBase), value: StrBase, ignoreCase: true);
        Init(StrOffset: StrOffset);
    }

    public int ArrayAccess(string[] Arr, int i)
    {
        if (i >= Arr.Length)
        {
            return 0;
        }

        return int.Parse(s: Arr[i]);
    }

    public void AddEventsInInterval(DateTime Begin, DateTime End, List<DateTime> List)
    {
        DateTime Next = NextRunTime(time: Begin, AllowExact: true);

        System.Diagnostics.Debug.WriteLine(
            message: "Testing event. Next: " + Next.ToString() + ", Current: " + End.ToString()
        );

        while (Next < End)
        {
            List.Add(item: Next);
            Next = IncInterval(Last: Next);
        }
    }

    public DateTime NextRunTime(DateTime time, bool AllowExact)
    {
        DateTime NextRun = LastSyncForTime(time: time) + _Offset;
        if (NextRun == time && AllowExact)
        {
            return time;
        }

        if (NextRun > time)
        {
            return NextRun;
        }

        return IncInterval(Last: NextRun);
    }

    private DateTime LastSyncForTime(DateTime time)
    {
        switch (_Base)
        {
            case EventTimeBase.BySecond:
            {
                return new DateTime(
                    year: time.Year,
                    month: time.Month,
                    day: time.Day,
                    hour: time.Hour,
                    minute: time.Minute,
                    second: time.Second
                );
            }
            case EventTimeBase.ByMinute:
            {
                return new DateTime(
                    year: time.Year,
                    month: time.Month,
                    day: time.Day,
                    hour: time.Hour,
                    minute: time.Minute,
                    second: 0
                );
            }
            case EventTimeBase.Hourly:
            {
                return new DateTime(
                    year: time.Year,
                    month: time.Month,
                    day: time.Day,
                    hour: time.Hour,
                    minute: 0,
                    second: 0
                );
            }
            case EventTimeBase.Daily:
            {
                return new DateTime(year: time.Year, month: time.Month, day: time.Day);
            }
            case EventTimeBase.Weekly:
            {
                return (new DateTime(year: time.Year, month: time.Month, day: time.Day)).AddDays(
                    value: -(int)time.DayOfWeek
                );
            }
            case EventTimeBase.Monthly:
            {
                return new DateTime(year: time.Year, month: time.Month, day: 1);
            }
        }
        throw new Exception(message: "Invalid base specified for timer.");
    }

    private DateTime IncInterval(DateTime Last)
    {
        switch (_Base)
        {
            case EventTimeBase.BySecond:
            {
                return Last.AddSeconds(value: 1);
            }
            case EventTimeBase.ByMinute:
            {
                return Last.AddMinutes(value: 1);
            }
            case EventTimeBase.Hourly:
            {
                return Last.AddHours(value: 1);
            }
            case EventTimeBase.Daily:
            {
                return Last.AddDays(value: 1);
            }
            case EventTimeBase.Weekly:
            {
                return Last.AddDays(value: 7);
            }
            case EventTimeBase.Monthly:
            {
                return Last.AddMonths(months: 1);
            }
        }
        throw new Exception(message: "Invalid base specified for timer.");
    }

    private void Init(string StrOffset)
    {
        switch (_Base)
        {
            case EventTimeBase.BySecond:
            {
                {
                    int offset = int.Parse(s: StrOffset);
                    if (offset >= 1000 || offset < 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "offset",
                            actualValue: offset,
                            message: "millisecond offset must be between 0 and 1000.  If you need an event every n seconds use simpleinterval."
                        );
                    }

                    _Offset = new TimeSpan(
                        days: 0,
                        hours: 0,
                        minutes: 0,
                        seconds: 0,
                        milliseconds: int.Parse(s: StrOffset)
                    );
                }
                break;
            }

            case EventTimeBase.ByMinute:
            {
                {
                    string[] ArrMinute = StrOffset.Split(separator: ',');
                    if (
                        ArrayAccess(Arr: ArrMinute, i: 0) >= 60
                        || ArrayAccess(Arr: ArrMinute, i: 0) < 0
                    )
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "offset",
                            actualValue: ArrayAccess(Arr: ArrMinute, i: 0),
                            message: "second offset must be between 0 and 60.  If you need an event every n minutes use simpleinterval."
                        );
                    }

                    if (
                        ArrayAccess(Arr: ArrMinute, i: 1) >= 1000
                        || ArrayAccess(Arr: ArrMinute, i: 1) < 0
                    )
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "offset",
                            actualValue: ArrayAccess(Arr: ArrMinute, i: 1),
                            message: "millisecond offset must be between 0 and 1000."
                        );
                    }

                    _Offset = new TimeSpan(
                        days: 0,
                        hours: 0,
                        minutes: 0,
                        seconds: ArrayAccess(Arr: ArrMinute, i: 0),
                        milliseconds: ArrayAccess(Arr: ArrMinute, i: 1)
                    );
                }
                break;
            }

            case EventTimeBase.Hourly:
            {
                {
                    string[] ArrHour = StrOffset.Split(separator: ',');
                    if (
                        ArrayAccess(Arr: ArrHour, i: 0) >= 60
                        || ArrayAccess(Arr: ArrHour, i: 0) < 0
                    )
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "offset",
                            actualValue: ArrayAccess(Arr: ArrHour, i: 0),
                            message: "minute offset must be between 0 and 60.  If you need an event every n hours use simpleinterval."
                        );
                    }

                    if (
                        ArrayAccess(Arr: ArrHour, i: 1) >= 60
                        || ArrayAccess(Arr: ArrHour, i: 1) < 0
                    )
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "offset",
                            actualValue: ArrayAccess(Arr: ArrHour, i: 1),
                            message: "second offset must be between 0 and 60."
                        );
                    }

                    if (
                        ArrayAccess(Arr: ArrHour, i: 2) >= 1000
                        || ArrayAccess(Arr: ArrHour, i: 2) < 0
                    )
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "offset",
                            actualValue: ArrayAccess(Arr: ArrHour, i: 2),
                            message: "millisecond offset must be between 0 and 1000."
                        );
                    }

                    _Offset = new TimeSpan(
                        days: 0,
                        hours: 0,
                        minutes: ArrayAccess(Arr: ArrHour, i: 0),
                        seconds: ArrayAccess(Arr: ArrHour, i: 1),
                        milliseconds: ArrayAccess(Arr: ArrHour, i: 2)
                    );
                }
                break;
            }

            case EventTimeBase.Daily:
            {
                {
                    DateTime Daytime = DateTime.Parse(s: StrOffset);
                    _Offset = new TimeSpan(
                        days: 0,
                        hours: Daytime.Hour,
                        minutes: Daytime.Minute,
                        seconds: Daytime.Second,
                        milliseconds: Daytime.Millisecond
                    );
                }
                break;
            }

            case EventTimeBase.Weekly:
            {
                {
                    string[] ArrWeek = StrOffset.Split(separator: ',');
                    int offset = int.Parse(s: ArrWeek[0]);
                    if (ArrWeek.Length != 2 || offset < 0 || offset >= 7)
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "offset",
                            actualValue: offset,
                            message: "Weekly offset must be in the format n, time where n is the day of the week starting with 0 for sunday"
                        );
                    }

                    DateTime WeekTime = DateTime.Parse(s: ArrWeek[1]);
                    _Offset = new TimeSpan(
                        days: offset,
                        hours: WeekTime.Hour,
                        minutes: WeekTime.Minute,
                        seconds: WeekTime.Second,
                        milliseconds: WeekTime.Millisecond
                    );
                }
                break;
            }

            case EventTimeBase.Monthly:
            {
                {
                    string[] ArrMonth = StrOffset.Split(separator: ',');
                    int offset = int.Parse(s: ArrMonth[0]);
                    if (ArrMonth.Length != 2)
                    {
                        throw new Exception(
                            message: "Monthly offset must be in the format n, time where n is the day of the month starting with 1 for the first day of the month."
                        );
                    }

                    DateTime MonthTime = DateTime.Parse(s: ArrMonth[1]);
                    _Offset = new TimeSpan(
                        days: offset - 1,
                        hours: MonthTime.Hour,
                        minutes: MonthTime.Minute,
                        seconds: MonthTime.Second,
                        milliseconds: MonthTime.Millisecond
                    );
                }
                break;
            }

            default:
            {
                throw new Exception(message: "Invalid base specified for timer.");
            }
        }
    }

    private EventTimeBase _Base;
    private TimeSpan _Offset;
}
