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

[SchemaItemDescription(
    name: "(Task) Update context by Xpath",
    folderName: "Tasks",
    iconName: "task-update-context-by-xpath.png"
)]
[HelpTopic(topic: "Update+Context+Task")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class UpdateContextTask : AbstractWorkflowStep
{
    public UpdateContextTask() { }

    public UpdateContextTask(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public UpdateContextTask(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        XsltDependencyHelper.GetDependencies(
            item: this,
            dependencies: dependencies,
            text: ValueXPath
        );
        dependencies.Add(item: OutputContextStore);
        dependencies.Add(item: XPathContextStore);
        dependencies.Add(item: Entity);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey?.Equals(obj: OutputContextStore.PrimaryKey) == true)
            {
                OutputContextStore = item as IContextStore;
            }
            if (item.OldPrimaryKey?.Equals(obj: XPathContextStore.PrimaryKey) == true)
            {
                XPathContextStore = item as IContextStore;
            }
        }
        base.UpdateReferences();
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
    #region Properties
    public Guid DataStructureEntityId;

    [TypeConverter(type: typeof(ContextStoreEntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Category(category: "Output")]
    [Description(
        description: "A data structure entity within `OutputContextStore' which is to be updated. `Entity' is only applicable if an `OutputContextStore'"
            + " is a data structure context store."
    )]
    [XmlReference(attributeName: "entity", idField: "DataStructureEntityId")]
    public DataStructureEntity Entity
    {
        get
        {
            var key = new ModelElementKey { Id = DataStructureEntityId };
            return (DataStructureEntity)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                DataStructureEntityId = Guid.Empty;
            }
            else
            {
                DataStructureEntityId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    [UpdateContextTaskValidModelElementRuleAttribute()]
    [Category(category: "Output"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Description(
        description: "A Name of a field (column) within `Entity' within an `OutputContextStore' which is to be updated for all rows of `Entity'"
            + " with a return value of `ValueXPath'. `FieldName' is applicable only if an OutputContextStore is a data struture context store."
    )]
    [XmlAttribute(attributeName: "fieldName")]
    public string FieldName { get; set; }

    public Guid OutputContextStoreId;

    [TypeConverter(type: typeof(ContextStoreConverter))]
    [Description(
        description: "Context store to be updated. In case of simple scalar context the value of context is updated with a result value of `ValueXPath'."
            + " In case of data structure context store `FieldName' is updated for all columns of `Entity'."
    )]
    [Category(category: "Output")]
    [NotNullModelElementRuleAttribute()]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "outputContextStore", idField: "OutputContextStoreId")]
    public IContextStore OutputContextStore
    {
        get
        {
            var key = new ModelElementKey { Id = OutputContextStoreId };
            return (IContextStore)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            if (value == null)
            {
                OutputContextStoreId = Guid.Empty;
            }
            else
            {
                OutputContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            }
            // clear entity and field properties only if copying
            // of whole workflow is not in progress (caller is UpdateReferences())
            if (OldPrimaryKey == null)
            {
                ClearEntityAndField();
            }
        }
    }
    public Guid XPathContextStoreId;

    [TypeConverter(type: typeof(ContextStoreConverter))]
    [Category(category: "Input")]
    [Description(description: "Contextstore to perform a ValueXPath expression on.")]
    [NotNullModelElementRuleAttribute()]
    [XmlReference(attributeName: "xPathContextStore", idField: "XPathContextStoreId")]
    public IContextStore XPathContextStore
    {
        get
        {
            var key = new ModelElementKey { Id = XPathContextStoreId };
            return (IContextStore)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            if (value == null)
            {
                XPathContextStoreId = Guid.Empty;
            }
            else
            {
                XPathContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    [Category(category: "Input")]
    [Description(
        description: "Result of this XPath is a value which is to be used fo updating a OutputContextStore."
    )]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "valueXPath")]
    public string ValueXPath { get; set; }
    #endregion
    #region Private Methods

    private void ClearEntityAndField()
    {
        FieldName = null;
        Entity = null;
    }

    public DataStructureColumn GetFieldSchemaItem()
    {
        if (Entity == null)
        {
            return null;
        }
        foreach (
            DataStructureColumn dataStructureColumn in Entity.ChildItemsByType<DataStructureColumn>(
                itemType: DataStructureColumn.CategoryConst
            )
        )
        {
            if (dataStructureColumn.Name == FieldName)
            {
                return dataStructureColumn;
            }
        }
        foreach (var dataStructureColumn in Entity.GetColumnsFromEntity())
        {
            if (dataStructureColumn.Name == FieldName)
            {
                return dataStructureColumn;
            }
        }
        return null;
    }
    #endregion
}
