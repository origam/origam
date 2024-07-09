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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using Origam.Schema.GuiModel;

namespace Origam.Schema.WorkflowModel;
/// <summary>
/// Summary description for WorkQueueWorkflowCommand.
/// </summary>
[SchemaItemDescription("Workflow Command", "Commands", "workflow-command.png")]
[ClassMetaVersion("6.0.0")]
public class WorkQueueWorkflowCommand : EntityUIAction
{
	public new const string CategoryConst = "WorkQueueCommand";
	public WorkQueueWorkflowCommand() : base() {Init();}
	public WorkQueueWorkflowCommand(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public WorkQueueWorkflowCommand(Key primaryKey) : base(primaryKey)	{Init();}

	private void Init()
	{
		this.ChildItemTypes.Remove(typeof(EntityUIActionParameterMapping));
		this.ChildItemTypes.Add(typeof(WorkQueueWorkflowCommandParameterMapping));
	}
	public new List<ISchemaItem> ParameterMappings
	{
		get
		{
			return this.ChildItemsByType(WorkQueueWorkflowCommandParameterMapping.CategoryConst);
		}
	}
	#region Overriden AbstractDataEntityColumn Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.Workflow);
		base.GetExtraDependencies (dependencies);
	}
	#endregion
	#region Properties
	public Guid WorkflowId;
	[Category("References")]
	[TypeConverter(typeof(WorkflowConverter)), NotNullModelElementRule()]
    [XmlReference("workflow", "WorkflowId")]
	public IWorkflow Workflow
	{
		get
		{
			return (IWorkflow)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(this.WorkflowId));
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
