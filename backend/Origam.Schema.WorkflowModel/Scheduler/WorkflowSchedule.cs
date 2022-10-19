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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkflowSchedule.
	/// </summary>
	[SchemaItemDescription("Workflow Schedule", "workflow-schedule.png")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public class WorkflowSchedule : AbstractSchemaItem
	{
		public const string CategoryConst = "WorkflowSchedule";

		public WorkflowSchedule() : base() {}

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
				ModelElementKey key = new ModelElementKey();
				key.Id = this.WorkflowId;

				return (IWorkflow)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.WorkflowId = Guid.Empty;
				}
				else
				{
					this.WorkflowId = (Guid)value.PrimaryKey["Id"];
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
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ScheduleTimeId;

				return (AbstractScheduleTime)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.ScheduleTimeId = Guid.Empty;
				}
				else
				{
					this.ScheduleTimeId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}
		#endregion

		#region Overriden AbstractSchemaItem Members
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Workflow);
			dependencies.Add(this.ScheduleTime);

			base.GetExtraDependencies (dependencies);
		}

		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}


		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(DataConstantReference),
									  typeof(SystemFunctionCall),
									  typeof(ReportReference)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(DataConstantReference))
			{
				item = new DataConstantReference(schemaExtensionId);
				item.Name = "NewDataConstantReference";
			}
			else if(type == typeof(ReportReference))
			{
				item = new ReportReference(schemaExtensionId);
				item.Name = "NewReportReference";
			}
			else if(type == typeof(SystemFunctionCall))
			{
				item = new SystemFunctionCall(schemaExtensionId);
				item.Name = "NewSystemFunctionCall";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorWorkflowUknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion

	}
}
