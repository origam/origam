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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for DataStructureFilterSetFilter.
/// </summary>
[SchemaItemDescription("Filter", "icon_filter.png")]
[HelpTopic("Filter+Set+Filter")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Entity")]
[ClassMetaVersion("6.0.0")]
public class DataStructureFilterSetFilter : AbstractSchemaItem
{
	public const string CategoryConst = "DataStructureFilterSetFilter";
	public DataStructureFilterSetFilter() : base(){}
	
	public DataStructureFilterSetFilter(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DataStructureFilterSetFilter(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	public Guid DataStructureEntityId;
	[TypeConverter(typeof(DataQueryEntityConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Description("An entity from this data structure on which the filter will be applied.")]
	[NotNullModelElementRule()]
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
			this.Filter = null;
			UpdateName();
		}
	}
    
	public Guid FilterId;
	[TypeConverter(typeof(DataQueryEntityFilterConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Description("A filter that will be applied.")]
	[NotNullModelElementRule()]
    [XmlReference("filter", "FilterId")]
    public EntityFilter Filter
	{
		get
		{
			return (EntityFilter)this.PersistenceProvider.RetrieveInstance(typeof(EntityFilter), new ModelElementKey(this.FilterId));
		}
		set
		{
			this.FilterId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			UpdateName();
		}
	}
	public Guid IgnoreFilterConstantId;
	[TypeConverter(typeof(DataConstantConverter))]
	[Category("Condition")]
	[Description("This filter will be ignored (or not ignored if PassWhenParameterMatch = true) if a query parameter is equal to this value.\nWhen not set, it tests if the parameter is filled or not.")]
    [XmlReference("ignoreFilterConstant", "IgnoreFilterConstantId")]
    public DataConstant IgnoreFilterConstant
	{
		get
		{
			return (DataConstant)this.PersistenceProvider.RetrieveInstance(typeof(EntityFilter), new ModelElementKey(this.IgnoreFilterConstantId));
		}
		set
		{
			this.IgnoreFilterConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
	private string _ignoreFilterParameterName;
	[Category("Condition")]
	[Description("Name of the parameter that will be evaluated. If it matches with the IgnoreFilterConstant this filter will be ignored (or not ignored if PassWhenParameterMatch = true).")]
    [XmlAttribute("ignoreFilterParameterName")]
    public string IgnoreFilterParameterName
	{
		get
		{
			return _ignoreFilterParameterName;
		}
		set
		{
			if(value == "") value = null;
			_ignoreFilterParameterName = value;
		}
	}
	private bool _passWhenParameterMatch = false;
	[Category("Condition")]
	[DefaultValue(false)]
	[Description("Applies the filter condition instead of ignoring it (revert condition).")]
    [XmlAttribute("passWhenParameterMatch")]
    public bool PassWhenParameterMatch
	{
		get
		{
			return _passWhenParameterMatch;
		}
		set
		{
			_passWhenParameterMatch = value;
		}
	}
	private string _roles = "";
	[Category("Condition"), RefreshProperties(RefreshProperties.Repaint)]
	[Description("An Application role. This filter will be used only if a user has this role assigned. If * or empty the filter will be always applied.")]
    [XmlAttribute("roles")]
    public string Roles
	{
		get
		{
			return _roles;
		}
		set
		{
			_roles = value;
		}
	}
	#endregion
	#region Overriden AbstractSchemaItem Members
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override void GetParameterReferences(AbstractSchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		if(this.Filter != null)
		{
			var references = new Dictionary<string, ParameterReference>();
			base.GetParameterReferences(this.Filter, references);
			foreach(var entry in references)
			{
				string key = this.Entity.Name + "_" + (string)entry.Key;
				if(! list.ContainsKey(key))
				{
					list.Add(key, entry.Value);
				}
			}
			if(this.IgnoreFilterParameterName != null)
			{
				if(! list.ContainsKey(IgnoreFilterParameterName))
				{
					list.Add(IgnoreFilterParameterName, null);
				}
			}
		}
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.Entity);
		dependencies.Add(this.Filter);
		dependencies.Add(this.IgnoreFilterConstant);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.Entity.PrimaryKey))
				{
					// store the old filter because setting an entity will reset the filter
					EntityFilter oldFilter = this.Filter;
					this.Entity = item as DataStructureEntity;
					this.Filter = oldFilter;
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
	{
		if(newNode is DataStructureFilterSet)
		{
			// only inside the same data structure
			return this.RootItem.Equals((newNode as AbstractSchemaItem).RootItem);
		}
		else
		{
			return false;
		}
	}
	public override SchemaItemCollection ChildItems
	{
		get
		{
			return new SchemaItemCollection();
		}
	}
	#endregion
	#region Private Methods
	private void UpdateName()
	{
		string entity = this.Entity == null ? "" : this.Entity.Name;
		string filter = this.Filter == null ? "" : this.Filter.Name;
		this.Name = entity + "_" + filter;
	}
	#endregion
}
