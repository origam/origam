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
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for WorkflowTaskDependency.
/// </summary>
[SchemaItemDescription("Dependency", "Dependencies", "dependency-blm.png")]
[HelpTopic("Workflow+Task+Dependency")]
[DefaultProperty("Task")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class WorkflowTaskDependency : AbstractSchemaItem
{
	public const string CategoryConst = "WorkflowTaskDependency";

	public WorkflowTaskDependency() : base() {}

	public WorkflowTaskDependency(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public WorkflowTaskDependency(Key primaryKey) : base(primaryKey)	{}
	
	#region Overriden AbstractDataEntityColumn Members
		
	public override string ItemType => CategoryConst;

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
	public Guid WorkflowTaskId;
	[NotNullModelElementRule]
	[NoParentDependenciesRule]
	[TypeConverter(typeof(WorkflowStepFilteredConverter))]
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

	[DefaultValue(WorkflowStepStartEvent.Success)]
	[XmlAttribute ("startEvent")]
	public WorkflowStepStartEvent StartEvent { get; set; } = WorkflowStepStartEvent.Success;
	#endregion
}
	
[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
public class NoParentDependenciesRule : AbstractModelElementRuleAttribute 
{
	public override Exception CheckRule(object instance)
	{
			if (!(instance is WorkflowTaskDependency taskDependency))
			{
				throw new Exception(
					$"{nameof(NoParentDependenciesRule)} can be only applied to type {nameof(WorkflowTaskDependency)}");  
			}
			if (taskDependency.Task == null)
			{
				return null;
			}
			AbstractSchemaItem workflowStep = taskDependency.ParentItem;
			var parentInDependencies = workflowStep.Parents
				.OfType<IWorkflowStep>()
				.Any(parent => taskDependency.Task == parent);
			return parentInDependencies
				? new Exception($"Invalid dependency detected. Workflow step cannot depend on one of its parents.")
				: null;
		}
		
	public override Exception CheckRule(object instance, string memberName)
	{
			return CheckRule(instance);
		}
}