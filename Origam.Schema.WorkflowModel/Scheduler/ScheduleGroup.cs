#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using Origam.DA.Common;
using System;
using Schedule;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for ScheduleGroup.
	/// </summary>
	[SchemaItemDescription("Schedule Group", "schedule-group.png")]
    [ClassMetaVersion("6.0.0")]
	public class ScheduleGroup : AbstractScheduleTime
	{
		public ScheduleGroup() : base() {}

		public ScheduleGroup(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public ScheduleGroup(Key primaryKey) : base(primaryKey)	{}

		#region Overriden Members
		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}

		public override IScheduledItem GetScheduledTime()
		{
			EventQueue q = new EventQueue();
			
			foreach(AbstractScheduleTime sch in this.ChildItems)
			{
				q.Add(sch.GetScheduledTime());
			}

			return q;
		}
		#endregion

		#region ISchemaItemFactory Members
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(SimpleScheduleTime),
									  typeof(ScheduleGroup)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(SimpleScheduleTime))
			{
				item = new SimpleScheduleTime(schemaExtensionId);
				item.Name = "NewSimpleScheduleTime";
			}
			else if(type == typeof(ScheduleGroup))
			{
				item = new ScheduleGroup(schemaExtensionId);
				item.Name = "NewScheduleGroup";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorWorkflowSchedulerModelUnknownType"));

			item.RootProvider = this;
			item.PersistenceProvider = this.PersistenceProvider;
			item.Group = group;
			this.ChildItems.Add(item);

			return item;
		}

		#endregion
	}
}
