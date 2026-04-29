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

/// <summary>
/// This class will be used to implement a filter that enables a repeating window of activity.  For cases where you want to
/// run every 15 minutes between 6:00 AM and 5:00 PM.  Or just on weekdays or weekends.
/// </summary>
public class BlockWrapper : IScheduledItem
{
    public BlockWrapper(IScheduledItem item, string StrBase, string BeginOffset, string EndOffset)
    {
        _Item = item;
        _Begin = new ScheduledTime(StrBase: StrBase, StrOffset: BeginOffset);
        _End = new ScheduledTime(StrBase: StrBase, StrOffset: EndOffset);
    }

    public void AddEventsInInterval(DateTime Begin, DateTime End, List<DateTime> List)
    {
        DateTime Next = NextRunTime(time: Begin, AllowExact: true);
        while (Next < End)
        {
            List.Add(item: Next);
            Next = NextRunTime(time: Next, AllowExact: false);
        }
    }

    public DateTime NextRunTime(DateTime time, bool AllowExact)
    {
        return NextRunTime(time: time, count: 100, AllowExact: AllowExact);
    }

    DateTime NextRunTime(DateTime time, int count, bool AllowExact)
    {
        if (count == 0)
        {
            throw new Exception(message: "Invalid block wrapper combination.");
        }

        DateTime temp = _Item.NextRunTime(time: time, IncludeStartTime: AllowExact),
            begin = _Begin.NextRunTime(time: time, AllowExact: true),
            end = _End.NextRunTime(time: time, AllowExact: true);
        System.Diagnostics.Debug.WriteLine(
            message: string.Format(
                format: "{0} {1} {2} {3}",
                args: new object[] { time, begin, end, temp }
            )
        );
        bool A = temp > end,
            B = temp < begin,
            C = end < begin;
        System.Diagnostics.Debug.WriteLine(
            message: string.Format(format: "{0} {1} {2}", arg0: A, arg1: B, arg2: C)
        );
        if (C)
        {
            if (A && B)
            {
                return NextRunTime(time: begin, count: --count, AllowExact: false);
            }

            return temp;
        }

        if (!A && !B)
        {
            return temp;
        }

        if (!A)
        {
            return NextRunTime(time: begin, count: --count, AllowExact: false);
        }

        return NextRunTime(time: end, count: --count, AllowExact: false);
    }

    private IScheduledItem _Item;
    private ScheduledTime _Begin,
        _End;
}
