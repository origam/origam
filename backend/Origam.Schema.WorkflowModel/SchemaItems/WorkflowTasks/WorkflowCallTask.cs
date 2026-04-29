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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(
    name: "(Task) Workflow Call",
    folderName: "Tasks",
    iconName: "task-workflow-call.png"
)]
[HelpTopic(topic: "Workflow+Call+Task")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkflowCallTask : WorkflowTask
{
    public WorkflowCallTask() { }

    public WorkflowCallTask(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public WorkflowCallTask(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Override ISchemaItem Members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Workflow);
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    public Guid WorkflowId;

    [TypeConverter(type: typeof(WorkflowConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "workflow", idField: "WorkflowId")]
    public IWorkflow Workflow
    {
        get
        {
            var key = new ModelElementKey { Id = WorkflowId };
            return (IWorkflow)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            // We delete any current parameters
            foreach (ISchemaItem child in ChildItems)
            {
                if (child is ContextStoreLink)
                {
                    child.IsDeleted = true;
                }
            }

            if (value == null)
            {
                WorkflowId = Guid.Empty;
            }
            else
            {
                WorkflowId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[] { typeof(WorkflowTaskDependency), typeof(ContextStoreLink) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(WorkflowTaskDependency))
        {
            itemName = "NewWorkflowTaskDependency";
        }
        else if (typeof(T) == typeof(ContextStoreLink))
        {
            itemName = "NewContextStoreLink";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion
}
