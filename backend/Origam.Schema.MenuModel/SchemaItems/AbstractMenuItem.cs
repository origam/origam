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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.Schema.MenuModel;

/// <summary>
/// Summary description for AbstractMenuItem.
/// </summary>
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.2")]
public abstract class AbstractMenuItem : AbstractSchemaItem, IAuthorizationContextContainer
{
	public const string CategoryConst = "MenuItem";

	public AbstractMenuItem() : base() {}

	public AbstractMenuItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public AbstractMenuItem(Key primaryKey) : base(primaryKey)	{}

	#region Overriden AbstractSchemaItem Members
		
	public override string ToString() => this.Path;
		
	public override string ItemType => CategoryConst;

	public override string NodeText
	{
		get => this.DisplayName;
		set
		{
				this.DisplayName = value;
				this.Persist();
			}
	}

	public override bool CanMove(Origam.UI.IBrowserNode2 newNode) => newNode is Submenu | newNode is Menu;

	[Browsable(false)]
	public override bool UseFolders => false;

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			if(this.MenuIcon != null) dependencies.Add(this.MenuIcon);

			base.GetExtraDependencies (dependencies);
		}
	#endregion

	#region Properties
	[Category("Security")]
	[XmlAttribute ("roles")]
	[NotNullModelElementRule()]
	public virtual string Roles { get; set; }

	[Category("Menu Item")]
	[XmlAttribute ("features")]
	public virtual string Features { get; set; }

	[DefaultValue(false)]
	[Description("When set to true it will be possible to execute this menu item only when other screens are closed.")]
	[XmlAttribute ("openExclusively")]
	public bool OpenExclusively { get; set; } = false;

	[DefaultValue(false)]
	[Description("When set to true it will always open a new tab.")]
	[XmlAttribute("alwaysOpenNew")]
	public bool AlwaysOpenNew { get; set; } = false;

	[Category("Menu Item")]
	[Localizable(true)]
	[NotNullModelElementRule()]
	[XmlAttribute ("displayName")]
	public string DisplayName { get; set; }
		
	public Guid GraphicsId;

	[Category("Menu Item")]
	[TypeConverter(typeof(GuiModel.GraphicsConverter))]
	[XmlReference("menuIcon", "GraphicsId")]
	public GuiModel.Graphics MenuIcon
	{
		get => (GuiModel.Graphics)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.GraphicsId));
		set
		{
				this.GraphicsId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
	}

	private int order = 100;
	[DefaultValue(100)]
	[Category("Menu Item")]
	[Localizable(false)]
	[NotNullModelElementRule]
	[Description("Primary attribute to use in sorting of menu items, secondary is Name.")]
	[XmlAttribute ("order")]
	public int Order
	{
		get => order;
		set => order = value;
	}
		
	public override byte[] NodeImage
	{
		get
		{
				if(this.MenuIcon == null)
				{
					return base.NodeImage;
				}
				else
				{
					return MenuIcon.GraphicsData.ToByteArray();
				}
			}
	}


	#endregion

	#region IAuthorizationContextContainer Members

	[Browsable(false)]
	public string AuthorizationContext => this.Roles;
	#endregion

	public override int CompareTo(object obj)
	{
			var item = obj as AbstractMenuItem;
			if (obj is Submenu submenu)
            {
                return 1;
            }
            if (item != null)
            {
	            var orderComparison = Order.CompareTo(item.Order);
	            return orderComparison == 0 
		            ? DisplayName.CompareTo(item.DisplayName) 
		            : orderComparison;
            }
            return base.CompareTo(obj);
		}

	public class MenuItemComparer : System.Collections.IComparer
	{
		#region IComparer Members
		public int Compare(object x, object y)
		{
	            var orderComparison = (x as AbstractMenuItem).Order
		            .CompareTo((y as AbstractMenuItem).Order);
	            return orderComparison == 0 
		            ? (x as AbstractMenuItem).DisplayName
		            .CompareTo((y as AbstractMenuItem).DisplayName) 
		            : orderComparison;
			}

		#endregion
	}
}