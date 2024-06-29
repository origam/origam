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
using System.Collections.Generic;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using Origam.Schema.MenuModel;
using Origam.Schema.EntityModel;
using System.Xml.Serialization;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.LookupModel;
/// <summary>
/// Summary description for DataConstantReferenceMenuItem.
/// </summary>
[SchemaItemDescription("Menu Binding", "icon_menu-binding.png")]
[HelpTopic("Menu+Bindings")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("MenuItem")]
[ClassMetaVersion("6.0.0")]
public class DataLookupMenuBinding : AbstractSchemaItem, IAuthorizationContextContainer, IComparable
{
	public const string CategoryConst = "DataLookupMenuBinding";
	public DataLookupMenuBinding() : base() {}
	public DataLookupMenuBinding(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DataLookupMenuBinding(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.MenuItem);
		AbstractSchemaItem menu = this.MenuItem;
		while(menu.ParentItem != null)
		{
			menu = menu.ParentItem;
			dependencies.Add(menu);
		}
		if(this.SelectionConstant != null)
		{
			dependencies.Add(this.SelectionConstant);
		}
		if(this.SelectionLookup != null)
		{
			dependencies.Add(this.SelectionLookup);
		}
		base.GetExtraDependencies (dependencies);
	}
	public override SchemaItemCollection ChildItems
	{
		get
		{
			return new SchemaItemCollection();
		}
	}
	public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
	{
		return newNode is AbstractDataLookup;
	}
	#endregion
	#region Properties
	public Guid MenuItemId;
	[Category("Menu Reference")]
	[TypeConverter(typeof(MenuItemConverter))]
	[NotNullModelElementRule()]
	[NotNullMenuRecordEditMethod()]
    [XmlReference("menuItem", "MenuItemId")]
    public AbstractMenuItem MenuItem
	{
		get
		{
			return (AbstractMenuItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.MenuItemId));
		}
		set
		{
			this.MenuItemId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
	
	private string _roles;
	[Category("Security")]
	[NotNullModelElementRule()]
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
	
	public Guid SelectionLookupId;
	[Category("Selection")]
	[TypeConverter(typeof(DataLookupConverter))]
	[Description("Choose lookup that returns a value you want to use for deciding whether the menu binding will be applied. Such a lookup should expect (as an input value) the same entity column (entity id in most cases) as original value lookup to which the current menu binding is bound. Example of use: We need for each type of actuarial document another form to edit. So we create parameter mappings for all types of documents with diferent selection constants and same selection lookup that returns a type of an actuarial document.")]
    [XmlReference("selectionLookup", "SelectionLookupId")]
    public AbstractDataLookup SelectionLookup
	{
		get
		{
			return this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SelectionLookupId)) as AbstractDataLookup;
		}
		set
		{
			this.SelectionLookupId = (Guid)value.PrimaryKey["Id"];
		}
	}
    
	public Guid SelectionConstantId;
	[Category("Selection")]
	[TypeConverter(typeof(DataConstantConverter))]
	[Description("If SelectionLookup return value will be equal to provided SelectionConstant, the current menu binding will be applied on the current record.")]
    [XmlReference("selectionConstant", "SelectionConstantId")]
    public DataConstant SelectionConstant
	{
		get
		{
			return this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SelectionConstantId)) as DataConstant;
		}
		set
		{
			this.SelectionConstantId = (Guid)value.PrimaryKey["Id"];
		}
	}
    
	public Guid _selectionPanelId;
	[Category("Menu Reference")]
    [XmlReference("selectionSectionId", "_selectionPanelId")]
    public string SelectionPanelId
	{
		get
		{
			if(_selectionPanelId.Equals(Guid.Empty))
			{
				return null;
			}
			else
			{
				return _selectionPanelId.ToString();
			}
		}
		set
		{
			if(value == null)
			{
				_selectionPanelId = Guid.Empty;
			}
			else
			{
				_selectionPanelId = new Guid(value);
			}
		}
	}
    private int _level = 100;
    [Category("Selection")]
    [NotNullModelElementRule()]
    [XmlAttribute("level")]
    public int Level
    {
        get
        {
            return _level;
        }
        set
        {
            _level = value;
        }
    }
    #endregion
	#region IAuthorizationContextContainer Members
	[Browsable(false)]
	public string AuthorizationContext
	{
		get
		{
			return this.Roles;
		}
	}
	#endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        DataLookupMenuBinding compared = obj as DataLookupMenuBinding;
        if (compared != null)
        {
            // then by level
            return this.Level.CompareTo(compared.Level);
        }
        else
        {
            return base.CompareTo(obj);
        }
    }
    #endregion
}
