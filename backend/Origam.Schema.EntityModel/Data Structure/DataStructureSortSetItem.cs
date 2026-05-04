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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for DataStructureFilterSetFilter.
/// </summary>
[SchemaItemDescription(name: "Sort Field", iconName: "icon_sort-field.png")]
[HelpTopic(topic: "Sort+Field")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureSortSetItem : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureSortSetItem";

    public DataStructureSortSetItem()
        : base() { }

    public DataStructureSortSetItem(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureSortSetItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid DataStructureEntityId;

    [TypeConverter(type: typeof(DataQueryEntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Category(category: "Sorting")]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "entity", idField: "DataStructureEntityId")]
    public DataStructureEntity Entity
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataStructureEntityId;
            return (DataStructureEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.DataStructureEntityId = Guid.Empty;
            }
            else
            {
                this.DataStructureEntityId = (Guid)value.PrimaryKey[key: "Id"];
            }
            UpdateName();
        }
    }
    private string _fieldName;

    [SortSetItemValidModelElementRuleAttribute()]
    [TypeConverter(type: typeof(DataStructureColumnStringConverter))]
    [Category(category: "Sorting"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlAttribute(attributeName: "fieldName")]
    public string FieldName
    {
        get { return _fieldName; }
        set
        {
            _fieldName = value;
            this.UpdateName();
        }
    }
    private int _sortOrder = 0;

    [
        Category(category: "Sorting"),
        DefaultValue(value: 0),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [XmlAttribute(attributeName: "sortOrder")]
    public int SortOrder
    {
        get { return _sortOrder; }
        set { _sortOrder = value; }
    }
    private DataStructureColumnSortDirection _sortDirection =
        DataStructureColumnSortDirection.Ascending;

    [Category(category: "Sorting"), DefaultValue(value: DataStructureColumnSortDirection.Ascending)]
    [XmlAttribute(attributeName: "sortDirection")]
    public DataStructureColumnSortDirection SortDirection
    {
        get { return _sortDirection; }
        set { _sortDirection = value; }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Entity);
        /* return a column used in a sort set */
        /* firstly look at columns defined on datastructure level */
        foreach (
            var dsColumn in Entity.ChildItemsByType<DataStructureColumn>(
                itemType: DataStructureColumn.CategoryConst
            )
        )
        {
            if (FieldName == dsColumn.Name)
            {
                // return data structure entity
                dependencies.Add(item: dsColumn);
            }
        }
        /* look at columns defined on entity level (AllFields = true) */
        foreach (DataStructureColumn dsColumn in this.Entity.GetColumnsFromEntity())
        {
            // check whether the name is referenced in fieldName
            if (FieldName == dsColumn.Name)
            {
                // data entity level - return data entity
                dependencies.Add(item: dsColumn.Field);
            }
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Entity.PrimaryKey))
                {
                    this.Entity = item as DataStructureEntity;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Private Methods
    private void UpdateName()
    {
        string entity = this.Entity == null ? "" : this.Entity.Name;
        string field = this.FieldName == null ? "" : this.FieldName;
        this.Name = entity + "_" + this.SortOrder.ToString() + "_" + field;
    }
    #endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        DataStructureSortSetItem compareItem = obj as DataStructureSortSetItem;
        if (compareItem == null)
        {
            return base.CompareTo(obj: obj);
        }
        return this.SortOrder.CompareTo(value: compareItem.SortOrder);
    }
    #endregion
}
