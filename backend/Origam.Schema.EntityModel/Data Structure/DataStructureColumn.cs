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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Attributes;
using Origam.Schema.Attributes;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

public enum DataStructureColumnSortDirection
{
    Ascending = 1,
    Descending = 2,
}

/// <summary>
/// Summary description for DataStructureColumn.
/// </summary>
[SchemaItemDescription(name: "Field", folderName: "Fields", icon: 22)]
[HelpTopic(topic: "Data+Structure+Field")]
[ExpressionBrowserTreeSortAtribute(t: typeof(ComparerSortByName))]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "Field")]
[ClassMetaVersion(versionStr: "6.0.1")]
public class DataStructureColumn : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureColumn";

    public DataStructureColumn()
        : base() { }

    public DataStructureColumn(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureColumn(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid DataStructureEntityId;

    [TypeConverter(type: typeof(DataQueryEntityConverterNoSelf))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "entity", idField: "DataStructureEntityId")]
    public DataStructureEntity Entity
    {
        get
        {
            return (DataStructureEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: new ModelElementKey(id: this.DataStructureEntityId)
                );
        }
        set
        {
            this.DataStructureEntityId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
            this.Field = null;
        }
    }

    public bool IsFromParentEntity()
    {
        if (this.Entity == null)
        {
            return false;
        }
        DataStructureEntity parentEntity = this.ParentItem.ParentItem as DataStructureEntity;
        while (parentEntity != null)
        {
            if (parentEntity.PrimaryKey.Equals(obj: this.Entity.PrimaryKey))
            {
                return true;
            }
            parentEntity = parentEntity.ParentItem as DataStructureEntity;
        }
        return false;
    }

    public Guid ColumnId;
    private IDataEntityColumn _column;

    [TypeConverter(type: typeof(DataStructureColumnConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "field", idField: "ColumnId")]
    [NotNullModelElementRule()]
    public IDataEntityColumn Field
    {
        get
        {
            if (_column != null)
            {
                return _column;
            }

            try
            {
                _column = (IDataEntityColumn)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: this.ColumnId)
                    );
                return _column;
            }
            catch
            {
                throw new InvalidOperationException(
                    message: ResourceUtils.GetString(key: "ErrorColumnNotFound", args: this.Path)
                );
            }
        }
        set
        {
            _column = value;
            if (value == null)
            {
                this.ColumnId = Guid.Empty;
                this.Name = "";
            }
            else
            {
                this.ColumnId = (Guid)value.PrimaryKey[key: "Id"];
                if (this.Entity == null)
                {
                    this.Name = this.Field.Name;
                }
                else
                {
                    this.Name = this.Entity.Name + "_" + this.Field.Name;
                }
            }
        }
    }

    public Guid DefaultLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [XmlReference(attributeName: "lookup", idField: "DefaultLookupId")]
    public IDataLookup DefaultLookup
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DefaultLookupId;
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            LookupField lookupField = this.Field as LookupField;
            if (lookupField != null)
            {
                throw new NotSupportedException();
            }

            if (value == null)
            {
                this.DefaultLookupId = Guid.Empty;
            }
            else
            {
                this.DefaultLookupId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    private string _caption = "";

    [XmlAttribute(attributeName: "label")]
    [Localizable(isLocalizable: true)]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
    private bool _useLookupValue = false;

    [XmlAttribute(attributeName: "useLookupValue")]
    [DefaultValue(value: false)]
    public bool UseLookupValue
    {
        get
        {
            // try-catch when source field is deleted so we can delete
            // the data-structure-column without errors
            try
            {
                LookupField lookupField = this.Field as LookupField;
                if (lookupField != null)
                {
                    return true;
                }

                return _useLookupValue;
            }
            catch
            {
                return _useLookupValue;
            }
        }
        set { _useLookupValue = value; }
    }
    private bool _useCopiedValue = false;

    [XmlAttribute(attributeName: "useCopiedValue")]
    [DefaultValue(value: false)]
    public bool UseCopiedValue
    {
        get { return _useCopiedValue; }
        set { _useCopiedValue = value; }
    }
    private bool _isWriteOnly = false;

    [XmlAttribute(attributeName: "writeOnly")]
    [DefaultValue(value: false)]
    public bool IsWriteOnly
    {
        get { return _isWriteOnly; }
        set { _isWriteOnly = value; }
    }

    [Browsable(browsable: false)]
    private DataStructureColumn LookedUpDisplayColumn
    {
        get
        {
            if (!this.UseLookupValue)
            {
                throw new InvalidOperationException(
                    message: ResourceUtils.GetString(
                        key: "ErrorUseLookupValueTrue",
                        args: this.Path
                    )
                );
            }

            IDataLookup lookup = this.FinalLookup;
            foreach (DataStructureColumn column in LookupEntity.Columns)
            {
                if (column.Name == lookup.ValueDisplayMember)
                {
                    // possible recursion
                    return column.FinalColumn;
                }
            }
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(
                    key: "ErrorValueDisplayMemberNotFound",
                    args: lookup.Name
                )
            );
        }
    }

    [Browsable(browsable: false)]
    public DataStructureColumn LookedUpValueColumn
    {
        get
        {
            if (!this.UseLookupValue)
            {
                throw new InvalidOperationException(
                    message: ResourceUtils.GetString(
                        key: "ErrorUseLookupValueTrue",
                        args: this.Path
                    )
                );
            }

            IDataLookup lookup = this.FinalLookup;
            foreach (DataStructureColumn column in LookupEntity.Columns)
            {
                if (column.Name == lookup.ValueValueMember)
                {
                    return column;
                }
            }
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(
                    key: "ErrorValueDisplayMemberNotFound",
                    args: lookup.Name
                )
            );
        }
    }

    [Browsable(browsable: false)]
    public IDataLookup FinalLookup
    {
        get { return (this.DefaultLookup == null ? this.Field.DefaultLookup : this.DefaultLookup); }
    }
    public DataStructureColumn FinalColumn
    {
        get { return this.UseLookupValue ? this.LookedUpDisplayColumn : this; }
    }

    [Browsable(browsable: false)]
    public OrigamDataType DataType
    {
        get
        {
            OrigamDataType finalDataType;
            if (this.FinalColumn.Aggregation == AggregationType.Count)
            {
                finalDataType = OrigamDataType.Long;
            }
            else
            {
                finalDataType = this.FinalColumn.Field.DataType;
            }
            return finalDataType;
        }
    }

    [Browsable(browsable: false)]
    public DataStructureEntity LookupEntity
    {
        get
        {
            if (this.FinalLookup == null)
            {
                throw new InvalidOperationException(
                    message: ResourceUtils.GetString(
                        key: "ErrorNoLookupDefined",
                        args: new object[] { this.Name, this.Path }
                    )
                );
            }

            return this.FinalLookup.ValueDataStructure.Entities[index: 0] as DataStructureEntity;
        }
    }
    private AggregationType _aggregationType = AggregationType.None;

    [XmlAttribute(attributeName: "aggregation")]
    [DefaultValue(value: AggregationType.None)]
    public AggregationType Aggregation
    {
        get { return _aggregationType; }
        set { _aggregationType = value; }
    }

    private int _order = 0;

    [XmlAttribute(attributeName: "order")]
    public int Order
    {
        get { return _order; }
        set { _order = value; }
    }
    private DataStructureColumnXmlMappingType _xmlMappingType =
        DataStructureColumnXmlMappingType.Default;

    [
        Category(category: "Entity Column"),
        DefaultValue(value: DataStructureColumnXmlMappingType.Default)
    ]
    [XmlAttribute(attributeName: "xmlMappingType")]
    public DataStructureColumnXmlMappingType XmlMappingType
    {
        get { return _xmlMappingType; }
        set { _xmlMappingType = value; }
    }

    [Category(category: "Entity Column"), DefaultValue(value: false)]
    [XmlAttribute(attributeName: "hideInOutput")]
    [Description(
        description: "Will remove the column from json and xml representations when requested through an api call."
    )]
    public bool HideInOutput { get; set; }
    private UpsertType _upsertType = UpsertType.Replace;

    [Category(category: "Update"), DefaultValue(value: UpsertType.Replace)]
    [XmlAttribute(attributeName: "upsertType")]
    public UpsertType UpsertType
    {
        get { return _upsertType; }
        set { _upsertType = value; }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override string Icon
    {
        get
        {
            try
            {
                if (this.UseLookupValue)
                {
                    return "icon_lookup-field.png";
                }

                if (this.Field == null)
                {
                    return "icon_field.png";
                }

                if (this.Field.IsPrimaryKey)
                {
                    return "icon_key.png";
                }

                if (this.Field is FieldMappingItem)
                {
                    return "icon_database-field.png";
                }

                return "icon_field.png";
            }
            catch
            {
                return null;
            }
        }
    }

    [RelationTypeParentModelElementRule()]
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.Field != null)
        {
            base.GetParameterReferences(parentItem: Field, list: list);
        }
    }

    //		public override void Persist()
    //		{
    //			if(this.IsPersistable)
    //			{
    //				if(this.ParentItem != null && (this.ParentItem as DataStructureEntity).AllFields & this.IsDeleted == false)
    //				{
    //					throw new InvalidOperationException("Cannot persist column. Parent data structure entity has AllColumns=true.");
    //				}
    //				else
    //				{
    //					base.Persist ();
    //				}
    //			}
    //		}
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Field);
        if (this.DefaultLookup != null)
        {
            dependencies.Add(item: this.DefaultLookup);
        }

        if (this.Entity != null)
        {
            dependencies.Add(item: this.Entity);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null && this.Entity != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Entity.PrimaryKey))
                {
                    this.DataStructureEntityId = (item as DataStructureEntity).Id;
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
    public override string NodeText
    {
        get
        {
            try
            {
                if (this.Field != null && this.Field.Name != this.Name)
                {
                    return this.Name + " [" + this.Field.Name + "]";
                }
            }
            catch { }
            return base.NodeText;
        }
        set { base.NodeText = value; }
    }
    #endregion
    #region Public Methods
    public bool IsColumnSorted(DataStructureSortSet sortSet)
    {
        if (sortSet == null)
        {
            return false;
        }

        foreach (DataStructureSortSetItem item in sortSet.ChildItems)
        {
            if (
                item.FieldName == this.Name
                & item.DataStructureEntityId.Equals(g: this.ParentItemId)
            )
            {
                return true;
            }
        }
        return false;
    }

    public DataStructureColumnSortDirection SortDirection(DataStructureSortSet sortSet)
    {
        if (sortSet == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorNoSortSet")
            );
        }

        foreach (DataStructureSortSetItem item in sortSet.ChildItems)
        {
            if (
                item.FieldName == this.Name
                & item.DataStructureEntityId.Equals(g: this.ParentItemId)
            )
            {
                return item.SortDirection;
            }
        }
        throw new ArgumentOutOfRangeException(
            paramName: "sortSet",
            actualValue: sortSet,
            message: ResourceUtils.GetString(key: "ErrorNoSortDirection")
        );
    }

    public int SortOrder(DataStructureSortSet sortSet)
    {
        if (sortSet == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorNoSortSet")
            );
        }

        foreach (DataStructureSortSetItem item in sortSet.ChildItems)
        {
            if (
                item.FieldName == this.Name
                & item.DataStructureEntityId.Equals(g: this.ParentItemId)
            )
            {
                return item.SortOrder;
            }
        }
        throw new ArgumentOutOfRangeException(
            paramName: "sortSet",
            actualValue: sortSet,
            message: ResourceUtils.GetString(key: "ErrorNoSortDirection")
        );
    }
    #endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        DataStructureColumn compared = obj as DataStructureColumn;
        if (compared != null)
        {
            return this.Order.CompareTo(value: compared.Order);
        }

        return base.CompareTo(obj: obj);
    }
    #endregion
}

public class ComparerSortByName : IComparer
{
    public int Compare(object a, object obj)
    {
        DataStructureColumn compared = obj as DataStructureColumn;
        DataStructureColumn compare = a as DataStructureColumn;
        if (compared != null && compare != null)
        {
            return compare.Name.CompareTo(strB: compared.Name);
        }

        return 0;
    }
}
