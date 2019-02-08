#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkflowTaskDependency.
	/// </summary>
	[SchemaItemDescription("Dependency", "Dependencies", 3)]
    [HelpTopic("Workflow+Task+Dependency")]
    [DefaultProperty("Task")]
	[XmlModelRoot(ItemTypeConst)]
	public class WorkflowTaskDependency : AbstractSchemaItem
	{
		public const string ItemTypeConst = "WorkflowTaskDependency";

		public WorkflowTaskDependency() : base() {}

		public WorkflowTaskDependency(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public WorkflowTaskDependency(Key primaryKey) : base(primaryKey)	{}
	
		#region Overriden AbstractDataEntityColumn Members
		
		[EntityColumn("ItemType")]
		public override string ItemType => ItemTypeConst;

		public override string Icon => "3";

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Task);

			base.GetExtraDependencies (dependencies);
		}

		public override void UpdateReferences()
		{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(item.OldPrimaryKey.Equals(this.Task.PrimaryKey))
					{
						this.Task = item as IWorkflowStep;
						break;
					}
				}
			}

			base.UpdateReferences ();
		}

		public override SchemaItemCollection ChildItems => new SchemaItemCollection();
		#endregion

		#region Properties
		[EntityColumn("G01")]  
		public Guid WorkflowTaskId;
        [NotNullModelElementRule]
        [TypeConverter(typeof(WorkflowStepConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("task", "WorkflowTaskId")]
		public IWorkflowStep Task
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.WorkflowTaskId;

				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as IWorkflowStep;
			}
			set
			{
				this.WorkflowTaskId = (Guid)value.PrimaryKey["Id"];

				this.Name = "After_" + this.Task.Name;
			}
		}

		[EntityColumn("I01")] 	
        [DefaultValue(WorkflowStepStartEvent.Success)]
		[XmlAttribute ("startEvent")]
		public WorkflowStepStartEvent StartEvent { get; set; } = WorkflowStepStartEvent.Success;
		#endregion
	}
}
