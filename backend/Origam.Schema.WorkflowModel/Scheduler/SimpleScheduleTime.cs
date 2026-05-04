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

[SchemaItemDescription(name: "Simple Schedule", iconName: "simple-schedule-1.png")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class SimpleScheduleTime : AbstractScheduleTime
{
    public SimpleScheduleTime() { }

    public SimpleScheduleTime(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public SimpleScheduleTime(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Override AbstractScheduleTime Members
    public override bool CanConvertTo(Type type) => type == typeof(ScheduleGroup);

    protected override ISchemaItem ConvertTo<T>()
    {
        if (typeof(T) != typeof(ScheduleGroup))
        {
            return base.ConvertTo<T>();
        }
        var converted = RootProvider.NewItem<T>(schemaExtensionId: SchemaExtensionId, group: Group);
        converted.PrimaryKey[key: "Id"] = PrimaryKey[key: "Id"];
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

    [Category(category: "Schedule Interval"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlAttribute(attributeName: "intervalType")]
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

    [Browsable(browsable: false)]
    [XmlAttribute(attributeName: "milliseconds")]
    public int Milliseconds
    {
        get => _milliseconds;
        set
        {
            if ((value < 0) || (value > 999))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Milliseconds",
                    actualValue: value,
                    message: ResourceUtils.GetString(key: "EnterNumberBetween0")
                );
            }
            _milliseconds = value;
        }
    }
    private int _seconds = 0;

    [Category(category: "Schedule"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlAttribute(attributeName: "seconds")]
    public int Seconds
    {
        get => _seconds;
        set
        {
            if ((value < 0) || (value > 59))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Seconds",
                    actualValue: value,
                    message: ResourceUtils.GetString(key: "EnterNumberBetween1")
                );
            }
            _seconds = value;
        }
    }
    private int _minutes = 0;

    [Category(category: "Schedule"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlAttribute(attributeName: "minutes")]
    public int Minutes
    {
        get => _minutes;
        set
        {
            if ((value < 0) || (value > 59))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Minutes",
                    actualValue: value,
                    message: ResourceUtils.GetString(key: "EnterNumberBetween1")
                );
            }
            _minutes = value;
        }
    }
    private int _hours = 0;

    [Category(category: "Schedule"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlAttribute(attributeName: "hours")]
    public int Hours
    {
        get => _hours;
        set
        {
            if ((value < 0) || (value > 23))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Hours",
                    actualValue: value,
                    message: ResourceUtils.GetString(key: "EnterNumberBetween2")
                );
            }
            _hours = value;
        }
    }
    private int _days = 0;

    [Category(category: "Schedule"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlAttribute(attributeName: "days")]
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
                        paramName: "Days",
                        actualValue: value,
                        message: ResourceUtils.GetString(key: "EnterNumberBetween3")
                    );
                }
            }
            else
            {
                if ((value < 0) || (value > 6))
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "Days",
                        actualValue: value,
                        message: ResourceUtils.GetString(key: "EnterNumberBetween4")
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
        var span = new TimeSpan(
            days: days,
            hours: Hours,
            minutes: Minutes,
            seconds: Seconds,
            milliseconds: Milliseconds
        );
        return new ScheduledTime(
            Base: SchedulerEventTimeBase(intervalType: IntervalType),
            Offset: span
        );
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
                    paramName: "intervalType",
                    actualValue: intervalType,
                    message: ResourceUtils.GetString(key: "ErrorUknownIntervalType")
                );
            }
        }
    }
    #endregion
}
