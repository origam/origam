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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for WorkflowTaskDependency.
/// </summary>
[SchemaItemDescription(
    name: "Dependency",
    folderName: "Dependencies",
    iconName: "dependency-blm.png"
)]
[HelpTopic(topic: "Workflow+Task+Dependency")]
[DefaultProperty(name: "Task")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkflowTaskDependency : AbstractSchemaItem
{
    public const string CategoryConst = "WorkflowTaskDependency";

    public WorkflowTaskDependency()
        : base() { }

    public WorkflowTaskDependency(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public WorkflowTaskDependency(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Task);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Task.PrimaryKey))
                {
                    this.Task = item as IWorkflowStep;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override ISchemaItemCollection ChildItems => SchemaItemCollection.Create();
    #endregion
    #region Properties
    public Guid WorkflowTaskId;

    [NotNullModelElementRule]
    [NoParentDependenciesRule]
    [TypeConverter(type: typeof(WorkflowStepFilteredConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "task", idField: "WorkflowTaskId")]
    public IWorkflowStep Task
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.WorkflowTaskId;
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: key
                    ) as IWorkflowStep;
        }
        set
        {
            this.WorkflowTaskId = (Guid)value.PrimaryKey[key: "Id"];
            this.Name = "After_" + this.Task.Name;
        }
    }

    [DefaultValue(value: WorkflowStepStartEvent.Success)]
    [XmlAttribute(attributeName: "startEvent")]
    public WorkflowStepStartEvent StartEvent { get; set; } = WorkflowStepStartEvent.Success;
    #endregion
}

[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class NoParentDependenciesRule : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        if (!(instance is WorkflowTaskDependency taskDependency))
        {
            throw new Exception(
                message: $"{nameof(NoParentDependenciesRule)} can be only applied to type {nameof(WorkflowTaskDependency)}"
            );
        }
        if (taskDependency.Task == null)
        {
            return null;
        }
        ISchemaItem workflowStep = taskDependency.ParentItem;
        var parentInDependencies = workflowStep
            .Parents.OfType<IWorkflowStep>()
            .Any(predicate: parent => taskDependency.Task == parent);
        return parentInDependencies
            ? new Exception(
                message: $"Invalid dependency detected. Workflow step cannot depend on one of its parents."
            )
            : null;
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        return CheckRule(instance: instance);
    }
}
