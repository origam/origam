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

using System;
using System.Collections;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using Origam.Schema.GuiModel;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkQueueWorkflowCommand.
	/// </summary>
	[SchemaItemDescription("Workflow Command", "Commands", "workflow-command.png")]
	public class WorkQueueWorkflowCommand : EntityUIAction
	{
		public new const string ItemTypeConst = "WorkQueueCommand";

		public WorkQueueWorkflowCommand() : base() {Init();}

		public WorkQueueWorkflowCommand(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

		public WorkQueueWorkflowCommand(Key primaryKey) : base(primaryKey)	{Init();}
	
		private void Init()
		{
			this.ChildItemTypes.Remove(typeof(EntityUIActionParameterMapping));
			this.ChildItemTypes.Add(typeof(WorkQueueWorkflowCommandParameterMapping));
		}

		public ArrayList ParameterMappings
		{
			get
			{
				return this.ChildItemsByType(WorkQueueWorkflowCommandParameterMapping.ItemTypeConst);
			}
		}

		#region Overriden AbstractDataEntityColumn Members
		
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Workflow);

			base.GetExtraDependencies (dependencies);
		}

		#endregion

		#region Properties
		[EntityColumn("G05")]  
		public Guid WorkflowId;

		[Category("References")]
		[TypeConverter(typeof(WorkflowConverter)), NotNullModelElementRule()]
        [XmlReference("workflow", "WorkflowId")]
		public IWorkflow Workflow
		{
			get
			{
				return (IWorkflow)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.WorkflowId));
			}
			set
			{
				this.WorkflowId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		#endregion

		#region IComparable Members
		public override int CompareTo(object obj)
		{
			WorkQueueWorkflowCommand compared = obj as WorkQueueWorkflowCommand;

			if(compared != null)
			{
				return this.Order.CompareTo(compared.Order);
			}
            return base.CompareTo(obj);
		}
		#endregion
	}
}
