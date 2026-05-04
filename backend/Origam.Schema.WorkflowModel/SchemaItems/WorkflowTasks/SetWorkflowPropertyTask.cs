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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel;

public enum WorkflowProperty
{
    Title,
    Notification,
    ResultMessage,
}

public enum SetWorkflowPropertyMethod
{
    Overwrite,
    Add,
}

[SchemaItemDescription(
    name: "(Task) Set Workflow Property",
    folderName: "Tasks",
    iconName: "task-set-workflow-property.png"
)]
[HelpTopic(topic: "Set+Workflow+Property+Task")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class SetWorkflowPropertyTask : AbstractWorkflowStep
{
    public SetWorkflowPropertyTask() { }

    public SetWorkflowPropertyTask(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public SetWorkflowPropertyTask(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        XsltDependencyHelper.GetDependencies(item: this, dependencies: dependencies, text: XPath);
        dependencies.Add(item: this.ContextStore);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey?.Equals(obj: ContextStore.PrimaryKey) == true)
            {
                ContextStore = item as IContextStore;
                break;
            }
        }
        base.UpdateReferences();
    }
    #endregion
    #region Properties
    public Guid ContextStoreId;

    [TypeConverter(type: typeof(ContextStoreConverter))]
    [XmlReference(attributeName: "contextStore", idField: "ContextStoreId")]
    public IContextStore ContextStore
    {
        get
        {
            var key = new ModelElementKey { Id = ContextStoreId };
            return (IContextStore)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            if (value == null)
            {
                ContextStoreId = Guid.Empty;
            }
            else
            {
                ContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    public Guid TransformationId;

    [TypeConverter(type: typeof(TransformationConverter))]
    [XmlReference(attributeName: "transformation", idField: "TransformationId")]
    public ITransformation Transformation
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: TransformationId)
                ) as ITransformation;
        set => TransformationId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    string _xpath;

    [XmlAttribute(attributeName: "xPath")]
    public string XPath
    {
        get => _xpath;
        set => _xpath = value;
    }
    string _delimiter = "";

    [XmlAttribute(attributeName: "delimiter")]
    public string Delimiter
    {
        get => _delimiter;
        set => _delimiter = value;
    }
    WorkflowProperty _workflowProperty;

    [XmlAttribute(attributeName: "workflowProperty")]
    public WorkflowProperty WorkflowProperty
    {
        get => _workflowProperty;
        set => _workflowProperty = value;
    }
    SetWorkflowPropertyMethod _setWorkflowPropertyMethod;

    [XmlAttribute(attributeName: "method")]
    public SetWorkflowPropertyMethod Method
    {
        get => _setWorkflowPropertyMethod;
        set => _setWorkflowPropertyMethod = value;
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes => new[] { typeof(WorkflowTaskDependency) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: typeof(T) == typeof(WorkflowTaskDependency)
                ? "NewWorkflowTaskDependency"
                : null
        );
    }
    #endregion
}
