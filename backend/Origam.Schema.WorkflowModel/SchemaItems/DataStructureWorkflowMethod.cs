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
using System.Collections;
using System.Collections.Generic;

namespace Origam.Schema.WorkflowModel;
/// <summary>
/// Summary description for DataStructureWorkflowMethod.
/// </summary>
[SchemaItemDescription("Workflow Method", "Workflow Methods",
    "icon_workflow-method.png")]
[HelpTopic("Data+Structure+Workflow+Method")]
[DefaultProperty("LoadWorkflow")]
[ClassMetaVersion("6.0.0")]
public class DataStructureWorkflowMethod : DataStructureMethod
{
	public DataStructureWorkflowMethod() : base() {}
	public DataStructureWorkflowMethod(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DataStructureWorkflowMethod(Key primaryKey) : base(primaryKey)	{}

	// with workflow method we consider all the workflows
	// as input parameters except context stores marked with `IsReturnValue'
	public override void GetParameterReferences(AbstractSchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		foreach (ContextStore context in LoadWorkflow.ChildItemsByType(ContextStore.CategoryConst))
		{
			if(context.IsReturnValue == false && context.isScalar())
			{
				if (!list.ContainsKey(context.Name))
				{
					ParameterReference pr = new ParameterReference();
				
					pr.PersistenceProvider = this.PersistenceProvider;
					pr.Name = context.Name;
					list.Add(context.Name, pr);
				}
			}
		}
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.LoadWorkflow);
		base.GetExtraDependencies (dependencies);
	}
	
	#region Properties
	
	public Guid LoadWorkflowId;
	[Category("Reference")]
	[TypeConverter(typeof(WorkflowConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Description("Select a workflow to load data into the structure. This schema item will Be extended later with `SaveWorkflow' and `SaveWorkflowInputContext' properties to be able to save the datastructure with a workflow.")]
	[NotNullModelElementRule()]
    [XmlReference("loadWorkflow", "LoadWorkflowId")]
	public Workflow LoadWorkflow
	{
		get
		{
			return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.LoadWorkflowId)) as Workflow;
		}
		set
		{
			this.LoadWorkflowId = (Guid)value.PrimaryKey["Id"];
		}
	}
	#endregion
}
