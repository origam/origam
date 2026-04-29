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
using Origam.Extensions;

namespace Origam.Schema.MenuModel;

/// <summary>
/// Summary description for AbstractMenuItem.
/// </summary>
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.2")]
public abstract class AbstractMenuItem : AbstractSchemaItem, IAuthorizationContextContainer
{
    public const string CategoryConst = "MenuItem";

    public AbstractMenuItem()
        : base() { }

    public AbstractMenuItem(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public AbstractMenuItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members

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

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode) =>
        newNode is Submenu | newNode is Menu;

    [Browsable(browsable: false)]
    public override bool UseFolders => false;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.MenuIcon != null)
        {
            dependencies.Add(item: this.MenuIcon);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    [Category(category: "Security")]
    [XmlAttribute(attributeName: "roles")]
    [NotNullModelElementRule()]
    public virtual string Roles { get; set; }

    [Category(category: "Menu Item")]
    [XmlAttribute(attributeName: "features")]
    public virtual string Features { get; set; }

    [DefaultValue(value: false)]
    [Description(
        description: "When set to true it will be possible to execute this menu item only when other screens are closed."
    )]
    [XmlAttribute(attributeName: "openExclusively")]
    public bool OpenExclusively { get; set; } = false;

    [DefaultValue(value: false)]
    [Description(description: "When set to true it will always open a new tab.")]
    [XmlAttribute(attributeName: "alwaysOpenNew")]
    public bool AlwaysOpenNew { get; set; } = false;

    [Category(category: "Menu Item")]
    [Localizable(isLocalizable: true)]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "displayName")]
    public string DisplayName { get; set; }

    public Guid GraphicsId;

    [Category(category: "Menu Item")]
    [TypeConverter(type: typeof(GuiModel.GraphicsConverter))]
    [XmlReference(attributeName: "menuIcon", idField: "GraphicsId")]
    public GuiModel.Graphics MenuIcon
    {
        get =>
            (GuiModel.Graphics)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.GraphicsId)
                );
        set { this.GraphicsId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }
    private int order = 100;

    [DefaultValue(value: 100)]
    [Category(category: "Menu Item")]
    [Localizable(isLocalizable: false)]
    [NotNullModelElementRule]
    [Description(
        description: "Primary attribute to use in sorting of menu items, secondary is Name."
    )]
    [XmlAttribute(attributeName: "order")]
    public int Order
    {
        get => order;
        set => order = value;
    }

    public override byte[] NodeImage
    {
        get
        {
            if (this.MenuIcon == null)
            {
                return base.NodeImage;
            }

            return MenuIcon.GraphicsData.ToByteArray();
        }
    }
    #endregion
    #region IAuthorizationContextContainer Members
    [Browsable(browsable: false)]
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
            var orderComparison = Order.CompareTo(value: item.Order);
            return orderComparison == 0
                ? DisplayName.CompareTo(strB: item.DisplayName)
                : orderComparison;
        }
        return base.CompareTo(obj: obj);
    }

    public class MenuItemComparer : IComparer<ISchemaItem>
    {
        #region IComparer Members
        public int Compare(ISchemaItem x, ISchemaItem y)
        {
            var orderComparison = (x as AbstractMenuItem).Order.CompareTo(
                value: (y as AbstractMenuItem).Order
            );
            return orderComparison == 0
                ? (x as AbstractMenuItem).DisplayName.CompareTo(
                    strB: (y as AbstractMenuItem).DisplayName
                )
                : orderComparison;
        }
        #endregion
    }
}
