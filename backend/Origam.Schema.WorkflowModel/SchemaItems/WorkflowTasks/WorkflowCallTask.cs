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

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkflowCallTask.
	/// </summary>
	[SchemaItemDescription("(Task) Workflow Call", "Tasks", "task-workflow-call.png")]
    [HelpTopic("Workflow+Call+Task")]
    [ClassMetaVersion("6.0.0")]
	public class WorkflowCallTask : WorkflowTask, ISchemaItemFactory
	{
		public WorkflowCallTask() : base() {}

		public WorkflowCallTask(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public WorkflowCallTask(Key primaryKey) : base(primaryKey)	{}

		#region Override AbstractSchemaItem Members
		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Workflow);

			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region Properties
		public Guid WorkflowId;

		[TypeConverter(typeof(WorkflowConverter))]
        [NotNullModelElementRule()]
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
				// We delete any current parameters
				foreach(ISchemaItem child in this.ChildItems)
				{
					if(child is ContextStoreLink)
					{
						child.IsDeleted = true;
					}
				}
				
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
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(WorkflowTaskDependency),
									  typeof(ContextStoreLink)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(WorkflowTaskDependency))
			{
				item = new WorkflowTaskDependency(schemaExtensionId);
				item.Name = "NewWorkflowTaskDependency";
			}
			else if(type == typeof(ContextStoreLink))
			{
				item = new ContextStoreLink(schemaExtensionId);
				item.Name = "newContextStoreLink";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorUIFormTaskUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}
		#endregion

	}
}
