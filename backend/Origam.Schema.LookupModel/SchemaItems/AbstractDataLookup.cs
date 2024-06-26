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

using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using System.Xml.Serialization;
using Origam.DA.Common;

namespace Origam.Schema.LookupModel;
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public abstract class AbstractDataLookup : AbstractSchemaItem, IDataLookup
{
	public const string CategoryConst = "DataLookup";
	public AbstractDataLookup() {}
	public AbstractDataLookup(Guid schemaExtensionId) 
		: base(schemaExtensionId) {}
	public AbstractDataLookup(Key primaryKey) : base(primaryKey) {}
	#region Overriden AbstractSchemaItem Members
	
	public override bool UseFolders => false;
	public override string ItemType => CategoryConst;
	public override void GetParameterReferences(
		AbstractSchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		base.GetParameterReferences(ListMethod, list);
	}
	public override void GetExtraDependencies(ArrayList dependencies)
	{
		dependencies.Add(ListDataStructure);
		dependencies.Add(ValueDataStructure);
		if(ListMethod != null)
		{
			dependencies.Add(ListMethod);
		}
		if(ValueMethod != null)
		{
			dependencies.Add(ValueMethod);
		}
		if(ValueSortSet != null)
		{
			dependencies.Add(ValueSortSet);
		}
		if(ListSortSet != null)
		{
			dependencies.Add(ListSortSet);
		}
		base.GetExtraDependencies (dependencies);
	}
	#endregion
	#region Properties
	[Browsable(false)]
	public ArrayList MenuBindings => ChildItemsByType(
		DataLookupMenuBinding.CategoryConst);
	[Browsable(false)]
	public bool HasTooltip
	{
		get
		{
			var tooltips = ChildItemsByType(
				AbstractDataTooltip.CategoryConst);
			return tooltips.Count > 0;
		}
	}
	[Browsable(false)]
	public ArrayList Tooltips => ChildItemsByType(
		AbstractDataTooltip.CategoryConst);
	#region List
	private string _listValueMember;
	[Category("List")]
	[NotNullModelElementRule()]
    [XmlAttribute("listValueMember")]
	public string ListValueMember
	{
		get => _listValueMember;
		set => _listValueMember = value;
	}
	private string _listDisplayMember;
	[Category("List")]
	[NotNullModelElementRule()]
    [XmlAttribute("listDisplayMember")]
    public string ListDisplayMember
	{
		get => _listDisplayMember;
		set => _listDisplayMember = value;
	}
	private bool _isTree;
	[Category("List")]
	[DefaultValue(false)]
    [XmlAttribute("isTree")]
    public bool IsTree
	{
		get => _isTree;
		set => _isTree = value;
	}
	private string _treeParentMember = "";
	[Category("List")]
	[XmlAttribute("treeParentMember")]
    public string TreeParentMember
	{
		get => _treeParentMember;
		set => _treeParentMember = value;
	}
    
	public Guid ListDataStructureId;
	[Category("List")]
	[TypeConverter(typeof(DataStructureConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
    [XmlReference("listDataStructure", "ListDataStructureId")]
	public DataStructure ListDataStructure
	{
		get
		{
			var key = new ModelElementKey
			{
				Id = ListDataStructureId
			};
			return (AbstractSchemaItem)PersistenceProvider.RetrieveInstance(
				typeof(AbstractSchemaItem), key) as DataStructure;
		}
		set
		{
			ListDataStructureId = (Guid)value.PrimaryKey["Id"];
			ListMethod = null;
			ListSortSet = null;
		}
	}
	private bool _suppressEmptyColumns;
	[Category("List")]
	[DefaultValue(false)]
    [XmlAttribute("suppressEmptyColumns")]
    public bool SuppressEmptyColumns
	{
		get => _suppressEmptyColumns;
		set => _suppressEmptyColumns = value;
	}
	private bool _alwaysAllowReturnToForm;
	[Category("List")]
	[DefaultValue(false)]
    [XmlAttribute("alwaysAllowReturnToForm")]
    public bool AlwaysAllowReturnToForm
	{
		get => _alwaysAllowReturnToForm;
		set => _alwaysAllowReturnToForm = value;
	}
	private bool _isFilteredServerside;
	[Category("List")]
	[DefaultValue(false)]
    [XmlAttribute("isFilteredServerside")]
    public bool IsFilteredServerside
	{
		get => _isFilteredServerside;
		set => _isFilteredServerside = value;
	}
    private string _serversideFilterParameter;
    [Category("List")]
    [LookupServerSideFilterModelElementRule()]
    [XmlAttribute("serversideFilterParameter")]
    public string ServersideFilterParameter
    {
        get => _serversideFilterParameter;
        set => _serversideFilterParameter = value;
    }
	private bool _searchByFirstColumnOnly;
	[Category("List")]
	[DefaultValue(false)]
    [XmlAttribute("searchByFirstColumnOnly")]
    public bool SearchByFirstColumnOnly
	{
		get => _searchByFirstColumnOnly;
		set => _searchByFirstColumnOnly = value;
	}
	#endregion
	#region Value
	private string _valueValueMember;
	[Category("Value")]
	[NotNullModelElementRule("ValueDataStructure")]
    [XmlAttribute("valueValueMember")]
    [RefreshProperties(RefreshProperties.Repaint)]
    public string ValueValueMember
	{
		get => _valueValueMember;
		set => _valueValueMember = value;
	}
	private string _valueDisplayMember;
	[Category("Value")]
	[NotNullModelElementRule("ValueDataStructure")]
    [XmlAttribute("valueDisplayMember")]
    [RefreshProperties(RefreshProperties.Repaint)]
    public string ValueDisplayMember
	{
		get => _valueDisplayMember;
		set => _valueDisplayMember = value;
	}
	public Guid ValueDataStructureId;
	[Category("Value")]
	[TypeConverter(typeof(DataStructureConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("valueDataStructure", "ValueDataStructureId")]
    public DataStructure ValueDataStructure
	{
		get => (AbstractSchemaItem)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(ValueDataStructureId)) as DataStructure;
		set
		{
			ValueDataStructureId = (Guid)value.PrimaryKey["Id"];
			ValueMethod = null;
			ValueSortSet = null;
		}
	}
	[Browsable(false)]
	public DataStructureEntity ValueEntity => 
		ValueDataStructure?.Entities[0] as DataStructureEntity;
	public DataStructureColumn ValueColumn => 
		ValueEntity?.Column(ValueValueMember);
	public DataStructureColumn ValueDisplayColumn =>
		ValueEntity?.Column(ValueDisplayMember);
	#endregion
	#region Filters
	private string _roleFilterMember;
	[Category("Filter")]
	[XmlAttribute("roleFilterMember")]
    public string RoleFilterMember
	{
		get => _roleFilterMember;
		set => _roleFilterMember = value;
	}
	private string _featureFilterMember;
	[Category("Filter")]
	[XmlAttribute("featureFilterMember")]
    public string FeatureFilterMember
	{
		get => _featureFilterMember;
		set => _featureFilterMember = value;
	}
	#endregion
	
	public Guid ListDataStructureMethodId;
	[TypeConverter(typeof(DataServiceDataLookupListMethodConverter))]
    [LookupServerSideFilterModelElementRule()]
    [Category("List")]
    [XmlReference("listMethod", "ListDataStructureMethodId")]
    public DataStructureMethod ListMethod
	{
		get => (DataStructureMethod)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(ListDataStructureMethodId));
		set => ListDataStructureMethodId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	public Guid ValueDataStructureMethodId;
	[TypeConverter(typeof(DataServiceDataLookupValueFilterConverter))]
	[Category("Value")]
    [XmlReference("valueMethod", "ValueDataStructureMethodId")]
    public DataStructureMethod ValueMethod
	{
		get => (DataStructureMethod)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(ValueDataStructureMethodId));
		set => ValueDataStructureMethodId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	public Guid ValueDataStructureSortSetId;
	[TypeConverter(typeof(DataServiceDataLookupValueSortSetConverter))]
	[Category("Value")]
    [XmlReference("valueSortSet", "ValueDataStructureSortSetId")]
    public DataStructureSortSet ValueSortSet
	{
		get => (DataStructureSortSet)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(ValueDataStructureSortSetId));
		set => ValueDataStructureSortSetId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	public Guid ListDataStructureSortSetId;
	[TypeConverter(typeof(DataServiceDataLookupListSortSetConverter))]
	[Category("List")]
    [XmlReference("listSortSet", "ListDataStructureSortSetId")]
    public DataStructureSortSet ListSortSet
	{
		get => (DataStructureSortSet)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(ListDataStructureSortSetId));
		set => ListDataStructureSortSetId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	#endregion
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes => new[] 
	{ 
		typeof(DataLookupMenuBinding),
		typeof(DataServiceDataTooltip),
		typeof(NewRecordScreenBinding)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		string itemName = null;
		if(typeof(T) == typeof(DataLookupMenuBinding))
		{
			itemName = "MenuBinding";
		}
		else if(typeof(T) == typeof(DataServiceDataTooltip))
		{
			itemName = "NewTooltip";
		}
		return base.NewItem<T>(schemaExtensionId, group, itemName);
	}
	#endregion
}
