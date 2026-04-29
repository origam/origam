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
/// Summary description for DataEntityIndexField.
/// </summary>
[SchemaItemDescription(name: "Index Field", folderName: "Fields", iconName: "icon_index-field.png")]
[HelpTopic(topic: "Index+Field")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "Field")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataEntityIndexField : AbstractSchemaItem
{
    public DataEntityIndexField()
        : base() { }

    public DataEntityIndexField(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataEntityIndexField(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public const string CategoryConst = "DataEntityIndexField";
    #region Properties
    private DataEntityIndexSortOrder _sortOrder = DataEntityIndexSortOrder.Ascending;

    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "sortOrder")]
    public DataEntityIndexSortOrder SortOrder
    {
        get { return _sortOrder; }
        set { _sortOrder = value; }
    }
    private int _ordinalPosition;

    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "ordinalPosition")]
    public int OrdinalPosition
    {
        get { return _ordinalPosition; }
        set
        {
            _ordinalPosition = value;
            UpdateName();
        }
    }

    public Guid ColumnId;

    [TypeConverter(type: typeof(EntityColumnReferenceConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "field", idField: "ColumnId")]
    public IDataEntityColumn Field
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ColumnId;
            return (IDataEntityColumn)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            this.ColumnId = (Guid)value.PrimaryKey[key: "Id"];
            UpdateName();
        }
    }
    #endregion
    #region Overriden ISchemaItem Members
    [Browsable(browsable: false)]
    public override bool UseFolders
    {
        get { return false; }
    }

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Field);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Field.PrimaryKey))
                {
                    this.Field = item as IDataEntityColumn;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }
    #endregion
    private void UpdateName()
    {
        this.Name =
            this.OrdinalPosition.ToString()
            + "_"
            + (this.ColumnId == Guid.Empty ? "?" : this.Field.Name);
    }

    #region IComparable Members
    public override int CompareTo(object obj)
    {
        if (obj is DataEntityIndexField)
        {
            DataEntityIndexField f = obj as DataEntityIndexField;

            return this.OrdinalPosition.CompareTo(value: f.OrdinalPosition);
        }

        return base.CompareTo(obj: obj);
    }
    #endregion
}
