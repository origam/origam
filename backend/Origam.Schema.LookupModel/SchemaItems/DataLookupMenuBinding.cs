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
using Origam.Schema.ItemCollection;
using Origam.Schema.MenuModel;

namespace Origam.Schema.LookupModel;

/// <summary>
/// Summary description for DataConstantReferenceMenuItem.
/// </summary>
[SchemaItemDescription(name: "Menu Binding", iconName: "icon_menu-binding.png")]
[HelpTopic(topic: "Menu+Bindings")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "MenuItem")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataLookupMenuBinding : AbstractSchemaItem, IAuthorizationContextContainer, IComparable
{
    public const string CategoryConst = "DataLookupMenuBinding";

    public DataLookupMenuBinding()
        : base() { }

    public DataLookupMenuBinding(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataLookupMenuBinding(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.MenuItem);
        ISchemaItem menu = this.MenuItem;
        while (menu.ParentItem != null)
        {
            menu = menu.ParentItem;
            dependencies.Add(item: menu);
        }
        if (this.SelectionConstant != null)
        {
            dependencies.Add(item: this.SelectionConstant);
        }
        if (this.SelectionLookup != null)
        {
            dependencies.Add(item: this.SelectionLookup);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
    {
        return newNode is AbstractDataLookup;
    }
    #endregion
    #region Properties
    public Guid MenuItemId;

    [Category(category: "Menu Reference")]
    [TypeConverter(type: typeof(MenuItemConverter))]
    [NotNullModelElementRule()]
    [NotNullMenuRecordEditMethod()]
    [XmlReference(attributeName: "menuItem", idField: "MenuItemId")]
    public AbstractMenuItem MenuItem
    {
        get
        {
            return (AbstractMenuItem)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.MenuItemId)
                );
        }
        set { this.MenuItemId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }

    private string _roles;

    [Category(category: "Security")]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }

    public Guid SelectionLookupId;

    [Category(category: "Selection")]
    [TypeConverter(type: typeof(DataLookupConverter))]
    [Description(
        description: "Choose lookup that returns a value you want to use for deciding whether the menu binding will be applied. Such a lookup should expect (as an input value) the same entity column (entity id in most cases) as original value lookup to which the current menu binding is bound. Example of use: We need for each type of actuarial document another form to edit. So we create parameter mappings for all types of documents with diferent selection constants and same selection lookup that returns a type of an actuarial document."
    )]
    [XmlReference(attributeName: "selectionLookup", idField: "SelectionLookupId")]
    public AbstractDataLookup SelectionLookup
    {
        get
        {
            return this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.SelectionLookupId)
                ) as AbstractDataLookup;
        }
        set { this.SelectionLookupId = (Guid)value.PrimaryKey[key: "Id"]; }
    }

    public Guid SelectionConstantId;

    [Category(category: "Selection")]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [Description(
        description: "If SelectionLookup return value will be equal to provided SelectionConstant, the current menu binding will be applied on the current record."
    )]
    [XmlReference(attributeName: "selectionConstant", idField: "SelectionConstantId")]
    public DataConstant SelectionConstant
    {
        get
        {
            return this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.SelectionConstantId)
                ) as DataConstant;
        }
        set { this.SelectionConstantId = (Guid)value.PrimaryKey[key: "Id"]; }
    }

    public Guid _selectionPanelId;

    [Category(category: "Menu Reference")]
    [XmlReference(attributeName: "selectionSectionId", idField: "_selectionPanelId")]
    public string SelectionPanelId
    {
        get
        {
            if (_selectionPanelId.Equals(g: Guid.Empty))
            {
                return null;
            }

            return _selectionPanelId.ToString();
        }
        set
        {
            if (value == null)
            {
                _selectionPanelId = Guid.Empty;
            }
            else
            {
                _selectionPanelId = new Guid(g: value);
            }
        }
    }
    private int _level = 100;

    [Category(category: "Selection")]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "level")]
    public int Level
    {
        get { return _level; }
        set { _level = value; }
    }
    #endregion
    #region IAuthorizationContextContainer Members
    [Browsable(browsable: false)]
    public string AuthorizationContext
    {
        get { return this.Roles; }
    }
    #endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        DataLookupMenuBinding compared = obj as DataLookupMenuBinding;
        if (compared != null)
        {
            // then by level
            return this.Level.CompareTo(value: compared.Level);
        }

        return base.CompareTo(obj: obj);
    }
    #endregion
}
