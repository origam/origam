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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

namespace Origam.Schema.WorkflowModel
{
	[SchemaItemDescription("Workflow Schedule", "workflow-schedule.png")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public class WorkflowSchedule : AbstractSchemaItem
	{
		public const string CategoryConst = "WorkflowSchedule";

		public WorkflowSchedule() {}

		public WorkflowSchedule(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public WorkflowSchedule(Key primaryKey) : base(primaryKey)	{}

		#region Properties
		public Guid WorkflowId;

		[TypeConverter(typeof(WorkflowConverter))]
        [XmlReference("workflow", "WorkflowId")]
		public IWorkflow Workflow
		{
			get
			{
				var key = new ModelElementKey
				{
					Id = WorkflowId
				};
				return (IWorkflow)PersistenceProvider.RetrieveInstance(
					typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					WorkflowId = Guid.Empty;
				}
				else
				{
					WorkflowId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}
		
		public Guid ScheduleTimeId;

		[TypeConverter(typeof(ScheduleTimeConverter))]
        [XmlReference("scheduleTime", "ScheduleTimeId")]
		public AbstractScheduleTime ScheduleTime
		{
			get
			{
				var key = new ModelElementKey
				{
					Id = ScheduleTimeId
				};
				return (AbstractScheduleTime)PersistenceProvider
					.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					ScheduleTimeId = Guid.Empty;
				}
				else
				{
					ScheduleTimeId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}
		#endregion

		#region Overriden AbstractSchemaItem Members
		
		public override string ItemType => CategoryConst;

		public override void GetExtraDependencies(
			System.Collections.ArrayList dependencies)
		{
			dependencies.Add(Workflow);
			dependencies.Add(ScheduleTime);
			base.GetExtraDependencies (dependencies);
		}

		public override bool UseFolders => false;

		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes => new[]
		{
			typeof(DataConstantReference), 
			typeof(SystemFunctionCall), 
			typeof(ReportReference)
		};

		public override T NewItem<T>(
			Guid schemaExtensionId, SchemaItemGroup group)
		{
			string itemName = null;
			if(typeof(T) == typeof(DataConstantReference))
			{
				itemName = "NewDataConstantReference";
			}
			else if(typeof(T) == typeof(ReportReference))
			{
				itemName = "NewReportReference";
			}
			else if(typeof(T) == typeof(SystemFunctionCall))
			{
				itemName = "NewSystemFunctionCall";
			}
			return base.NewItem<T>(schemaExtensionId, group, itemName);
		}

		#endregion

	}
}
