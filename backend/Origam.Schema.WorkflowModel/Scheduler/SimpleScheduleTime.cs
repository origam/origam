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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Schedule;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription("Simple Schedule", "simple-schedule-1.png")]
[ClassMetaVersion("6.0.0")]
public class SimpleScheduleTime : AbstractScheduleTime
{
    public SimpleScheduleTime() { }

    public SimpleScheduleTime(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public SimpleScheduleTime(Key primaryKey)
        : base(primaryKey) { }

    #region Override AbstractScheduleTime Members
    public override bool CanConvertTo(Type type) => type == typeof(ScheduleGroup);

    protected override ISchemaItem ConvertTo<T>()
    {
        if (typeof(T) != typeof(ScheduleGroup))
        {
            return base.ConvertTo<T>();
        }
        var converted = RootProvider.NewItem<T>(SchemaExtensionId, Group);
        converted.PrimaryKey["Id"] = PrimaryKey["Id"];
        converted.Name = Name;
        converted.IsAbstract = IsAbstract;
        // we have to delete first (also from the cache)
        DeleteChildItems = false;
        IsDeleted = true;
        Persist();
        converted.Persist();
        return converted;
    }

    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        return ((ISchemaItem)newNode).RootProvider == RootProvider;
    }
    #endregion
    #region Properties
    private ScheduleIntervalType _intervalType = ScheduleIntervalType.Daily;

    [Category("Schedule Interval"), RefreshProperties(RefreshProperties.Repaint)]
    [XmlAttribute("intervalType")]
    public ScheduleIntervalType IntervalType
    {
        get => _intervalType;
        set
        {
            switch (value)
            {
                case ScheduleIntervalType.Monthly:
                {
                    if (_days < 1)
                    {
                        _days = 1;
                    }
                    break;
                }
                case ScheduleIntervalType.Hourly:
                {
                    _days = 0;
                    break;
                }
                case ScheduleIntervalType.ByMinute:
                {
                    _days = 0;
                    _hours = 0;
                    break;
                }
                case ScheduleIntervalType.BySecond:
                {
                    _days = 0;
                    _hours = 0;
                    _minutes = 0;
                    break;
                }
            }
            _intervalType = value;
        }
    }
    private int _milliseconds = 0;

    [Browsable(false)]
    [XmlAttribute("milliseconds")]
    public int Milliseconds
    {
        get => _milliseconds;
        set
        {
            if ((value < 0) || (value > 999))
            {
                throw new ArgumentOutOfRangeException(
                    "Milliseconds",
                    value,
                    ResourceUtils.GetString("EnterNumberBetween0")
                );
            }
            _milliseconds = value;
        }
    }
    private int _seconds = 0;

    [Category("Schedule"), RefreshProperties(RefreshProperties.Repaint)]
    [XmlAttribute("seconds")]
    public int Seconds
    {
        get => _seconds;
        set
        {
            if ((value < 0) || (value > 59))
            {
                throw new ArgumentOutOfRangeException(
                    "Seconds",
                    value,
                    ResourceUtils.GetString("EnterNumberBetween1")
                );
            }
            _seconds = value;
        }
    }
    private int _minutes = 0;

    [Category("Schedule"), RefreshProperties(RefreshProperties.Repaint)]
    [XmlAttribute("minutes")]
    public int Minutes
    {
        get => _minutes;
        set
        {
            if ((value < 0) || (value > 59))
            {
                throw new ArgumentOutOfRangeException(
                    "Minutes",
                    value,
                    ResourceUtils.GetString("EnterNumberBetween1")
                );
            }
            _minutes = value;
        }
    }
    private int _hours = 0;

    [Category("Schedule"), RefreshProperties(RefreshProperties.Repaint)]
    [XmlAttribute("hours")]
    public int Hours
    {
        get => _hours;
        set
        {
            if ((value < 0) || (value > 23))
            {
                throw new ArgumentOutOfRangeException(
                    "Hours",
                    value,
                    ResourceUtils.GetString("EnterNumberBetween2")
                );
            }
            _hours = value;
        }
    }
    private int _days = 0;

    [Category("Schedule"), RefreshProperties(RefreshProperties.Repaint)]
    [XmlAttribute("days")]
    public int Days
    {
        get => _days;
        set
        {
            if (IntervalType == ScheduleIntervalType.Monthly)
            {
                if ((value < 1) || (value > 31))
                {
                    throw new ArgumentOutOfRangeException(
                        "Days",
                        value,
                        ResourceUtils.GetString("EnterNumberBetween3")
                    );
                }
            }
            else
            {
                if ((value < 0) || (value > 6))
                {
                    throw new ArgumentOutOfRangeException(
                        "Days",
                        value,
                        ResourceUtils.GetString("EnterNumberBetween4")
                    );
                }
            }
            _days = value;
        }
    }
    #endregion
    #region Public Methods
    public override IScheduledItem GetScheduledTime()
    {
        var days = Days;
        if (IntervalType == ScheduleIntervalType.Monthly)
        {
            days--;
        }
        var span = new TimeSpan(days, Hours, Minutes, Seconds, Milliseconds);
        return new ScheduledTime(SchedulerEventTimeBase(IntervalType), span);
    }
    #endregion
    #region Private Methods
    private EventTimeBase SchedulerEventTimeBase(ScheduleIntervalType intervalType)
    {
        switch (intervalType)
        {
            case ScheduleIntervalType.ByMinute:
            {
                return EventTimeBase.ByMinute;
            }
            case ScheduleIntervalType.BySecond:
            {
                return EventTimeBase.BySecond;
            }
            case ScheduleIntervalType.Daily:
            {
                return EventTimeBase.Daily;
            }
            case ScheduleIntervalType.Hourly:
            {
                return EventTimeBase.Hourly;
            }
            case ScheduleIntervalType.Monthly:
            {
                return EventTimeBase.Monthly;
            }
            case ScheduleIntervalType.Weekly:
            {
                return EventTimeBase.Weekly;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    "intervalType",
                    intervalType,
                    ResourceUtils.GetString("ErrorUknownIntervalType")
                );
            }
        }
    }
    #endregion
}
