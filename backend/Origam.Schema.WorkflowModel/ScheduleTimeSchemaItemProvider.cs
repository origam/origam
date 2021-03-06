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

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkflowSchemaItemProvide.
	/// </summary>
	public class ScheduleTimeSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public ScheduleTimeSchemaItemProvider()
		{
		}

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return AbstractScheduleTime.CategoryConst;
			}
		}
		public override string Group
		{
			get
			{
				return "BL";
			}
		}
		#endregion

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
				return "icon_28_schedule-times.png";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Schedule Times";
			}
			set
			{
				base.NodeText = value;
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
				return null;
			}
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
