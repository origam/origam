#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Drawing;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.Schema.MenuModel
{
    /// <summary>
    /// Summary description for AbstractMenuItem.
    /// </summary>
    [XmlModelRoot(ItemTypeConst)]
    public abstract class AbstractMenuItem : AbstractSchemaItem, IAuthorizationContextContainer
	{
		public const string ItemTypeConst = "MenuItem";

		public AbstractMenuItem() : base() {}

		public AbstractMenuItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public AbstractMenuItem(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
		public override string ToString() => this.Path;

		[EntityColumn("ItemType")]
		public override string ItemType => ItemTypeConst;

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
		[EntityColumn("LS01")]
		[XmlAttribute ("roles")]
		[NotNullModelElementRule()]
		public virtual string Roles { get; set; }

		[Category("Menu Item")]
		[EntityColumn("SS02")]
		[XmlAttribute ("features")]
		public virtual string Features { get; set; }

		[DefaultValue(false)]
        [EntityColumn("B07")]
        [Description("When set to true it will be possible to execute this menu item only when other screens are closed.")]
        [XmlAttribute ("openExclusively")]
        public bool OpenExclusively { get; set; } = false;

		[Category("Menu Item")]
		[EntityColumn("SS01")]
		[Localizable(true)]
		[NotNullModelElementRule()]
		[XmlAttribute ("displayName")]
		public string DisplayName { get; set; }

		[EntityColumn("G01")]  
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
					return MenuIcon.GraphicsData;
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
			AbstractMenuItem item = obj as AbstractMenuItem;
            Submenu submenu = obj as Submenu;

            if (submenu != null)
            {
                return 1;
            }
            if (item != null)
			{
				return this.DisplayName.CompareTo(item.DisplayName);
			}
			else
			{
				return base.CompareTo(obj);
			}
		}


		public class MenuItemComparer : System.Collections.IComparer
		{
			public MenuItemComparer()
			{
			}

			#region IComparer Members

			public int Compare(object x, object y) => (x as AbstractMenuItem).DisplayName.CompareTo((y as AbstractMenuItem).DisplayName);
			#endregion
		}
	}
}
