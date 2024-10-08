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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

using Schedule;

namespace Origam.Schema.WorkflowModel;
public enum ScheduleIntervalType
{
	BySecond = 1,
	ByMinute = 2,
	Hourly = 3,
	Daily = 4,
	Weekly = 5,
	Monthly = 6,
}
/// <summary>
/// Summary description for AbstractScheduleTime.
/// </summary>
[XmlModelRoot(CategoryConst)]
public abstract class AbstractScheduleTime : AbstractSchemaItem
{
	public const string CategoryConst = "ScheduleTime";
	public AbstractScheduleTime() : base() {}
	public AbstractScheduleTime(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public AbstractScheduleTime(Key primaryKey) : base(primaryKey)	{}
	#region Abstract Members
	public abstract IScheduledItem GetScheduledTime();
	#endregion
	#region Properties
	public string NextScheduleTime
	{
		get
		{
			return GetScheduledTime().NextRunTime(DateTime.Now, true).ToString();
		}
	}
	#endregion
	#region Overriden ISchemaItem Members
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	#endregion
}
