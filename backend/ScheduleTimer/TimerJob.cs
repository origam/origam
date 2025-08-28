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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Schedule;

/// <summary>
/// Timer job groups a schedule, syncronization data, a result filter, method information and an enabled state so that multiple jobs
/// can be managed by the same timer.  Each one operating independently of the others with different syncronization and recovery settings.
/// </summary>
public class TimerJob
{
    public TimerJob(IScheduledItem schedule, IMethodCall method)
    {
        Schedule = schedule;
        Method = method;
        _ExecuteHandler = new ExecuteHandler(ExecuteInternal);
    }

    public IScheduledItem Schedule;
    public bool SyncronizedEvent = true;
    public IResultFilter Filter;
    public IMethodCall Method;

    //		public IJobLog Log;
    public bool Enabled = true;
    private DateTime LastTime = DateTime.Now;

    public DateTime NextRunTime(DateTime time, bool IncludeStartTime)
    {
        if (Enabled == false)
        {
            return DateTime.MaxValue;
        }

        return Schedule.NextRunTime(time, IncludeStartTime);
    }

    public void Execute(object sender, DateTime Begin, DateTime End, ExceptionEventHandler Error)
    {
        if (Enabled == false)
        {
            return;
        }

        var EventList = new List<DateTime>();
        //			Schedule.AddEventsInInterval(Begin, End, EventList);
        Schedule.AddEventsInInterval(LastTime, End, EventList);
        if (Filter != null)
        {
            //				Filter.FilterResultsInInterval(Begin, End, EventList);
            Filter.FilterResultsInInterval(LastTime, End, EventList);
        }
        foreach (DateTime EventTime in EventList)
        {
            try
            {
                if (SyncronizedEvent)
                {
                    _ExecuteHandler(sender, EventTime, Error);
                }
                else
                {
                    _ExecuteHandler.BeginInvoke(sender, EventTime, Error, null, null);
                }
            }
            finally
            {
                this.LastTime = EventTime.AddMilliseconds(1);
            }
        }
    }

    private void ExecuteInternal(object sender, DateTime EventTime, ExceptionEventHandler Error)
    {
        try
        {
            TimerParameterSetter Setter = new TimerParameterSetter(EventTime, sender);
            Method.Execute(Setter);
        }
        catch (Exception ex)
        {
            if (Error != null)
            {
                try
                {
                    Error(this, new ExceptionEventArgs(EventTime, ex));
                }
                catch { }
            }
        }
    }

    private delegate void ExecuteHandler(
        object sender,
        DateTime EventTime,
        ExceptionEventHandler Error
    );
    private ExecuteHandler _ExecuteHandler;
}

/// <summary>
/// Timer job manages a group of timer jobs.
/// </summary>
public class TimerJobList
{
    public TimerJobList()
    {
        _List = new List<TimerJob>();
    }

    public void Add(TimerJob Event)
    {
        lock (_List)
        {
            _List.Add(Event);
        }
    }

    public void Clear()
    {
        lock (_List)
        {
            _List.Clear();
        }
    }

    public void RemoveJob(TimerJob job)
    {
        lock (_List)
        {
            _List.Remove(job);
        }
    }

    /// <summary>
    /// Gets the next time any of the jobs in the list will run.  Allows matching the exact start time.  If no matches are found the return
    /// is DateTime.MaxValue;
    /// </summary>
    /// <param name="time">The starting time for the interval being queried.  This time is included in the interval</param>
    /// <returns>The first absolute date one of the jobs will execute on.  If none of the jobs needs to run DateTime.MaxValue is returned.</returns>
    public DateTime NextRunTime(DateTime time)
    {
        DateTime next = DateTime.MaxValue;
        //Get minimum datetime from the list.
        int i = 0;
        while (true)
        {
            TimerJob job = null;
            lock (_List)
            {
                if (i >= _List.Count)
                {
                    break;
                }

                job = _List[i] as TimerJob;
                if (job == null)
                {
                    _List.RemoveAt(i);
                    continue;
                }
            }
            if (job.Enabled == false)
            {
                ++i;
                continue;
            }
            DateTime Proposed = job.NextRunTime(time, true);
            if (Proposed == DateTime.MaxValue)
            {
                lock (_List)
                {
                    _List.Remove(job);
                    continue;
                }
            }
            System.Diagnostics.Debug.WriteLine("Proposed Next Job: " + Proposed);
            next = (Proposed < next) ? Proposed : next;
            ++i;
        }
        System.Diagnostics.Debug.WriteLine("Next Job: " + next);
        return next;
    }

    public TimerJob[] Jobs
    {
        get
        {
            lock (_List)
            {
                return _List.ToArray();
            }
        }
    }
    private List<TimerJob> _List;
}

/// <summary>
/// The timer job allows delegates to be specified with unbound parameters.  This ParameterSetter assigns all unbound datetime parameters
/// with the specified time and all unbound object parameters with the calling object.
/// </summary>
public class TimerParameterSetter : IParameterSetter
{
    /// <summary>
    /// Initalize the ParameterSetter with the time to pass to unbound time parameters and object to pass to unbound object parameters.
    /// </summary>
    /// <param name="time">The time to pass to the unbound DateTime parameters</param>
    /// <param name="sender">The object to pass to the unbound object parameters</param>
    public TimerParameterSetter(DateTime time, object sender)
    {
        _time = time;
        _sender = sender;
    }

    public void reset() { }

    public bool GetParameterValue(ParameterInfo pi, int ParameterLoc, ref object parameter)
    {
        switch (pi.ParameterType.Name.ToLower())
        {
            case "datetime":
            {
                parameter = _time;
                return true;
            }

            case "object":
            {
                parameter = _sender;
                return true;
            }

            case "scheduledeventargs":
            {
                parameter = new ScheduledEventArgs(_time);
                return true;
            }

            case "eventargs":
            {
                parameter = new ScheduledEventArgs(_time);
                return true;
            }
        }
        return false;
    }

    DateTime _time;
    object _sender;
}
