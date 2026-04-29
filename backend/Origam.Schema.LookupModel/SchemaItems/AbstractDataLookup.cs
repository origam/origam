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

namespace Origam.Schema.LookupModel;

[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public abstract class AbstractDataLookup : AbstractSchemaItem, IDataLookup
{
    public const string CategoryConst = "DataLookup";

    public AbstractDataLookup() { }

    public AbstractDataLookup(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public AbstractDataLookup(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members

    public override bool UseFolders => false;
    public override string ItemType => CategoryConst;

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        base.GetParameterReferences(parentItem: ListMethod, list: list);
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: ListDataStructure);
        dependencies.Add(item: ValueDataStructure);
        if (ListMethod != null)
        {
            dependencies.Add(item: ListMethod);
        }
        if (ValueMethod != null)
        {
            dependencies.Add(item: ValueMethod);
        }
        if (ValueSortSet != null)
        {
            dependencies.Add(item: ValueSortSet);
        }
        if (ListSortSet != null)
        {
            dependencies.Add(item: ListSortSet);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    [Browsable(browsable: false)]
    public List<DataLookupMenuBinding> MenuBindings =>
        ChildItemsByType<DataLookupMenuBinding>(itemType: DataLookupMenuBinding.CategoryConst);

    [Browsable(browsable: false)]
    public bool HasTooltip
    {
        get
        {
            var tooltips = ChildItemsByType<AbstractDataTooltip>(
                itemType: AbstractDataTooltip.CategoryConst
            );
            return tooltips.Count > 0;
        }
    }

    [Browsable(browsable: false)]
    public List<AbstractDataTooltip> Tooltips =>
        ChildItemsByType<AbstractDataTooltip>(itemType: AbstractDataTooltip.CategoryConst);
    #region List
    private string _listValueMember;

    [Category(category: "List")]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "listValueMember")]
    public string ListValueMember
    {
        get => _listValueMember;
        set => _listValueMember = value;
    }
    private string _listDisplayMember;

    [Category(category: "List")]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "listDisplayMember")]
    public string ListDisplayMember
    {
        get => _listDisplayMember;
        set => _listDisplayMember = value;
    }
    private bool _isTree;

    [Category(category: "List")]
    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "isTree")]
    public bool IsTree
    {
        get => _isTree;
        set => _isTree = value;
    }
    private string _treeParentMember = "";

    [Category(category: "List")]
    [XmlAttribute(attributeName: "treeParentMember")]
    public string TreeParentMember
    {
        get => _treeParentMember;
        set => _treeParentMember = value;
    }

    public Guid ListDataStructureId;

    [Category(category: "List")]
    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "listDataStructure", idField: "ListDataStructureId")]
    public DataStructure ListDataStructure
    {
        get
        {
            var key = new ModelElementKey { Id = ListDataStructureId };
            return (ISchemaItem)
                    PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key)
                as DataStructure;
        }
        set
        {
            ListDataStructureId = (Guid)value.PrimaryKey[key: "Id"];
            ListMethod = null;
            ListSortSet = null;
        }
    }
    private bool _suppressEmptyColumns;

    [Category(category: "List")]
    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "suppressEmptyColumns")]
    public bool SuppressEmptyColumns
    {
        get => _suppressEmptyColumns;
        set => _suppressEmptyColumns = value;
    }
    private bool _alwaysAllowReturnToForm;

    [Category(category: "List")]
    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "alwaysAllowReturnToForm")]
    public bool AlwaysAllowReturnToForm
    {
        get => _alwaysAllowReturnToForm;
        set => _alwaysAllowReturnToForm = value;
    }
    private bool _isFilteredServerside;

    [Category(category: "List")]
    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "isFilteredServerside")]
    public bool IsFilteredServerside
    {
        get => _isFilteredServerside;
        set => _isFilteredServerside = value;
    }
    private string _serversideFilterParameter;

    [Category(category: "List")]
    [LookupServerSideFilterModelElementRule()]
    [XmlAttribute(attributeName: "serversideFilterParameter")]
    public string ServersideFilterParameter
    {
        get => _serversideFilterParameter;
        set => _serversideFilterParameter = value;
    }
    private bool _searchByFirstColumnOnly;

    [Category(category: "List")]
    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "searchByFirstColumnOnly")]
    public bool SearchByFirstColumnOnly
    {
        get => _searchByFirstColumnOnly;
        set => _searchByFirstColumnOnly = value;
    }
    #endregion
    #region Value
    private string _valueValueMember;

    [Category(category: "Value")]
    [NotNullModelElementRule(conditionField: "ValueDataStructure")]
    [XmlAttribute(attributeName: "valueValueMember")]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    public string ValueValueMember
    {
        get => _valueValueMember;
        set => _valueValueMember = value;
    }
    private string _valueDisplayMember;

    [Category(category: "Value")]
    [NotNullModelElementRule(conditionField: "ValueDataStructure")]
    [XmlAttribute(attributeName: "valueDisplayMember")]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    public string ValueDisplayMember
    {
        get => _valueDisplayMember;
        set => _valueDisplayMember = value;
    }
    public Guid ValueDataStructureId;

    [Category(category: "Value")]
    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "valueDataStructure", idField: "ValueDataStructureId")]
    public DataStructure ValueDataStructure
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ValueDataStructureId)
                ) as DataStructure;
        set
        {
            ValueDataStructureId = (Guid)value.PrimaryKey[key: "Id"];
            ValueMethod = null;
            ValueSortSet = null;
        }
    }

    [Browsable(browsable: false)]
    public DataStructureEntity ValueEntity =>
        ValueDataStructure?.Entities[index: 0] as DataStructureEntity;
    public DataStructureColumn ValueColumn => ValueEntity?.Column(name: ValueValueMember);
    public DataStructureColumn ValueDisplayColumn => ValueEntity?.Column(name: ValueDisplayMember);
    #endregion
    #region Filters
    private string _roleFilterMember;

    [Category(category: "Filter")]
    [XmlAttribute(attributeName: "roleFilterMember")]
    public string RoleFilterMember
    {
        get => _roleFilterMember;
        set => _roleFilterMember = value;
    }
    private string _featureFilterMember;

    [Category(category: "Filter")]
    [XmlAttribute(attributeName: "featureFilterMember")]
    public string FeatureFilterMember
    {
        get => _featureFilterMember;
        set => _featureFilterMember = value;
    }
    #endregion

    public Guid ListDataStructureMethodId;

    [TypeConverter(type: typeof(DataServiceDataLookupListMethodConverter))]
    [LookupServerSideFilterModelElementRule()]
    [Category(category: "List")]
    [XmlReference(attributeName: "listMethod", idField: "ListDataStructureMethodId")]
    public DataStructureMethod ListMethod
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ListDataStructureMethodId)
                );
        set =>
            ListDataStructureMethodId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid ValueDataStructureMethodId;

    [TypeConverter(type: typeof(DataServiceDataLookupValueFilterConverter))]
    [Category(category: "Value")]
    [XmlReference(attributeName: "valueMethod", idField: "ValueDataStructureMethodId")]
    public DataStructureMethod ValueMethod
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ValueDataStructureMethodId)
                );
        set =>
            ValueDataStructureMethodId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid ValueDataStructureSortSetId;

    [TypeConverter(type: typeof(DataServiceDataLookupValueSortSetConverter))]
    [Category(category: "Value")]
    [XmlReference(attributeName: "valueSortSet", idField: "ValueDataStructureSortSetId")]
    public DataStructureSortSet ValueSortSet
    {
        get =>
            (DataStructureSortSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ValueDataStructureSortSetId)
                );
        set =>
            ValueDataStructureSortSetId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid ListDataStructureSortSetId;

    [TypeConverter(type: typeof(DataServiceDataLookupListSortSetConverter))]
    [Category(category: "List")]
    [XmlReference(attributeName: "listSortSet", idField: "ListDataStructureSortSetId")]
    public DataStructureSortSet ListSortSet
    {
        get =>
            (DataStructureSortSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ListDataStructureSortSetId)
                );
        set =>
            ListDataStructureSortSetId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[]
        {
            typeof(DataLookupMenuBinding),
            typeof(DataServiceDataTooltip),
            typeof(NewRecordScreenBinding),
        };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(DataLookupMenuBinding))
        {
            itemName = "MenuBinding";
        }
        else if (typeof(T) == typeof(DataServiceDataTooltip))
        {
            itemName = "NewTooltip";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion
}
