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

using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence.Attributes;
using System.Collections;
using System.Collections.Generic;
using Origam.Schema.Attributes;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;
public enum DataStructureColumnSortDirection
{
	Ascending = 1,
	Descending = 2
}
/// <summary>
/// Summary description for DataStructureColumn.
/// </summary>
[SchemaItemDescription("Field", "Fields", 22)]
[HelpTopic("Data+Structure+Field")]
[ExpressionBrowserTreeSortAtribute(typeof(ComparerSortByName))]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Field")]
[ClassMetaVersion("6.0.1")]
public class DataStructureColumn : AbstractSchemaItem
{
	public const string CategoryConst = "DataStructureColumn";
	public DataStructureColumn() : base(){}
	
	public DataStructureColumn(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DataStructureColumn(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	public Guid DataStructureEntityId;
	[TypeConverter(typeof(DataQueryEntityConverterNoSelf))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("entity", "DataStructureEntityId")]
    public DataStructureEntity Entity
	{
		get
		{
			return (DataStructureEntity)this.PersistenceProvider.RetrieveInstance(typeof(DataStructureEntity), new ModelElementKey(this.DataStructureEntityId));
		}
		set
		{
			this.DataStructureEntityId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			this.Field = null;
		}
	}
	public bool IsFromParentEntity()
	{
		if(this.Entity == null)
		{
			return false;
		}
		DataStructureEntity parentEntity = this.ParentItem.ParentItem as DataStructureEntity;
		while(parentEntity != null)
		{
			if(parentEntity.PrimaryKey.Equals(this.Entity.PrimaryKey))
			{
				return true;
			}
			parentEntity = parentEntity.ParentItem as DataStructureEntity;
		}
		return false;
	}
	
	public Guid ColumnId;
	private IDataEntityColumn _column;
	[TypeConverter(typeof(DataStructureColumnConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("field", "ColumnId")]
    [NotNullModelElementRule()]
    public IDataEntityColumn Field
	{
		get
		{
			if(_column != null) return _column;
			try
			{
				_column = (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(this.ColumnId));
				return _column;
			}
			catch
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorColumnNotFound", this.Path));
			}
		}
		set
		{
			_column = value;
			if(value == null)
			{
				this.ColumnId = Guid.Empty;
				this.Name = "";
			}
			else
			{
				this.ColumnId = (Guid)value.PrimaryKey["Id"];
				if(this.Entity == null)
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
	[TypeConverter(typeof(DataLookupConverter))]
    [XmlReference("lookup", "DefaultLookupId")]
    public IDataLookup DefaultLookup
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.DefaultLookupId;
			return (IDataLookup)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
		}
		set
		{
			LookupField lookupField = this.Field as LookupField;
			if(lookupField != null)
			{
				throw new NotSupportedException();
			}
			else if(value == null)
			{
				this.DefaultLookupId = Guid.Empty;
			}
			else
			{
				this.DefaultLookupId = (Guid)value.PrimaryKey["Id"];
			}
		}
	}
	private string _caption = "";
	[XmlAttribute("label")]
	[Localizable(true)]
    public string Caption
	{
		get
		{
			return _caption;
		}
		set
		{
			_caption = value;
		}
	}
	private bool _useLookupValue = false;
    [XmlAttribute("useLookupValue")]
    [DefaultValue(false)]
	public bool UseLookupValue
	{
		get
		{
			// try-catch when source field is deleted so we can delete
			// the data-structure-column without errors
			try
			{
				LookupField lookupField = this.Field as LookupField;
				if(lookupField != null)
				{
					return true;
				}
				else
				{
					return _useLookupValue;
				}
			}
			catch
			{
				return _useLookupValue;
			}
	}
		set
		{
			_useLookupValue = value;
		}
	}
	private bool _useCopiedValue = false;
    [XmlAttribute("useCopiedValue")]
    [DefaultValue(false)]
	public bool UseCopiedValue
	{
		get
		{
			return _useCopiedValue;
		}
		set
		{
			_useCopiedValue = value;
		}
	}
	private bool _isWriteOnly = false;
    [XmlAttribute("writeOnly")]
    [DefaultValue(false)]
	public bool IsWriteOnly
	{
		get
		{
			return _isWriteOnly;
		}
		set
		{
			_isWriteOnly = value;
		}
	}
	[Browsable(false)]
	private DataStructureColumn LookedUpDisplayColumn
	{
		get
		{
			if(!this.UseLookupValue) throw new InvalidOperationException(ResourceUtils.GetString("ErrorUseLookupValueTrue", this.Path));
			IDataLookup lookup = this.FinalLookup;
			foreach(DataStructureColumn column in LookupEntity.Columns)
			{
				if(column.Name == lookup.ValueDisplayMember)
				{
					// possible recursion
					return column.FinalColumn;
				}
			}
			throw new InvalidOperationException(ResourceUtils.GetString("ErrorValueDisplayMemberNotFound", lookup.Name));
		}
	}
	[Browsable(false)]
	public DataStructureColumn LookedUpValueColumn
	{
		get
		{
			if(!this.UseLookupValue) throw new InvalidOperationException(ResourceUtils.GetString("ErrorUseLookupValueTrue", this.Path));
			IDataLookup lookup = this.FinalLookup;
			foreach(DataStructureColumn column in LookupEntity.Columns)
			{
				if(column.Name == lookup.ValueValueMember)
				{
					return column;
				}
			}
			throw new InvalidOperationException(ResourceUtils.GetString("ErrorValueDisplayMemberNotFound", lookup.Name));
		}
	}
	[Browsable(false)]
	public IDataLookup FinalLookup
	{
		get
		{
			return (this.DefaultLookup == null ? this.Field.DefaultLookup : this.DefaultLookup);
		}
	}
	public DataStructureColumn FinalColumn
	{
		get
		{
			return this.UseLookupValue ? this.LookedUpDisplayColumn : this;
		}
	}
	[Browsable(false)]
	public OrigamDataType DataType
	{
		get
		{
			OrigamDataType finalDataType;
			if(this.FinalColumn.Aggregation == AggregationType.Count)
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
	[Browsable(false)]
	public DataStructureEntity LookupEntity
	{
		get
		{
			if(this.FinalLookup == null) throw new InvalidOperationException(ResourceUtils.GetString("ErrorNoLookupDefined", this.Name, this.Path));
			return this.FinalLookup.ValueDataStructure.Entities[0] as DataStructureEntity;
		}
	}
	private AggregationType _aggregationType = AggregationType.None;
    [XmlAttribute("aggregation")]
    [DefaultValue(AggregationType.None)]
    public AggregationType Aggregation
	{
		get
		{
			return _aggregationType;
		}
		set
		{
			_aggregationType = value;
		}
	}
	
	private int _order = 0;
	[XmlAttribute("order")]
    public int Order
	{
		get
		{
			return _order;
		}
		set
		{
			_order = value;
		}
	}
	private DataStructureColumnXmlMappingType _xmlMappingType = DataStructureColumnXmlMappingType.Default;
	[Category("Entity Column"), DefaultValue(DataStructureColumnXmlMappingType.Default)]
    [XmlAttribute("xmlMappingType")]
	public DataStructureColumnXmlMappingType XmlMappingType 
	{
		get
		{
			return _xmlMappingType;
		}
		set
		{
			_xmlMappingType = value;
		}
	}
	[Category("Entity Column"), DefaultValue(false)]
	[XmlAttribute("hideInOutput")]
	[Description("Will remove the column from json and xml representations when requested through an api call.")]
	public bool HideInOutput { get; set; }
	private UpsertType _upsertType = UpsertType.Replace;
	[Category("Update"), DefaultValue(UpsertType.Replace)]
    [XmlAttribute("upsertType")]
	public UpsertType UpsertType 
	{
		get
		{
			return _upsertType;
		}
		set
		{
			_upsertType = value;
		}
	}
	#endregion
	#region Overriden ISchemaItem Members
	public override string Icon
	{
		get
		{
			try
			{
				if(this.UseLookupValue)
				{
					return "icon_lookup-field.png";
				}
				else
				{
					if(this.Field == null)
					{
						return "icon_field.png";
					}
					else if(this.Field.IsPrimaryKey)
					{
						return "icon_key.png";
					}
					else
					{
						if(this.Field is FieldMappingItem)
						{
							return "icon_database-field.png";
						}
						else
						{
							return "icon_field.png";
						}
					}
				}
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
		get
		{
			return CategoryConst;
		}
	}
	public override void GetParameterReferences(ISchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		if(this.Field != null)
			base.GetParameterReferences(this.Field as ISchemaItem, list);
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
		dependencies.Add(this.Field);
		if(this.DefaultLookup != null) dependencies.Add(this.DefaultLookup);
		if(this.Entity != null) dependencies.Add(this.Entity);
		base.GetExtraDependencies (dependencies);
	}
    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null && this.Entity != null)
            {
                if (item.OldPrimaryKey.Equals(this.Entity.PrimaryKey))
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
		get
		{
			return SchemaItemCollection.Create();
		}
	}
	public override string NodeText
	{
		get
		{
			try
			{
				if(this.Field != null && this.Field.Name != this.Name)
				{
					return this.Name + " [" + this.Field.Name + "]";
				}
			}
			catch
			{
			}
			return base.NodeText;
		}
		set
		{
			base.NodeText = value;
		}
	}
	#endregion
	#region Public Methods
	public bool IsColumnSorted(DataStructureSortSet sortSet)
	{
		if(sortSet == null) return false;
		foreach(DataStructureSortSetItem item in sortSet.ChildItems)
		{
			if(item.FieldName == this.Name & item.DataStructureEntityId.Equals(this.ParentItemId)) return true;
		}
		return false;
	}
	public DataStructureColumnSortDirection SortDirection(DataStructureSortSet sortSet)
	{
		if(sortSet == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorNoSortSet"));
		foreach(DataStructureSortSetItem item in sortSet.ChildItems)
		{
			if(item.FieldName == this.Name & item.DataStructureEntityId.Equals(this.ParentItemId)) return item.SortDirection;
		}
		throw new ArgumentOutOfRangeException("sortSet", sortSet, ResourceUtils.GetString("ErrorNoSortDirection"));
	}
	public int SortOrder(DataStructureSortSet sortSet)
	{
		if(sortSet == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorNoSortSet"));
		foreach(DataStructureSortSetItem item in sortSet.ChildItems)
		{
			if(item.FieldName == this.Name & item.DataStructureEntityId.Equals(this.ParentItemId)) return item.SortOrder;
		}
		throw new ArgumentOutOfRangeException("sortSet", sortSet, ResourceUtils.GetString("ErrorNoSortDirection"));
	}
	#endregion
	#region IComparable Members
	public override int CompareTo(object obj)
	{
		DataStructureColumn compared = obj as DataStructureColumn;
		if(compared != null)
		{
			return this.Order.CompareTo(compared.Order);
		}
		else
		{
			return base.CompareTo(obj);
		}
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
			return compare.Name.CompareTo(compared.Name);
		}
		else
		{
			return 0;
		}
	}
}
