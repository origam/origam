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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for ControlPropertyItem.
/// </summary>
///
public enum ControlPropertyValueType
{
    Integer = 0,
    Boolean,
    String,
    Xml,
    UniqueIdentifier,
}

[SchemaItemDescription("Property", "Properties", "icon_property.png")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ControlPropertyItem : AbstractSchemaItem
{
    public const string CategoryConst = "ControlPropertyItem";

    public ControlPropertyItem()
        : base() { }

    public ControlPropertyItem(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public ControlPropertyItem(Key primaryKey)
        : base(primaryKey) { }

    #region Properties
    private ControlPropertyValueType _propertyType;

    [XmlAttribute("propertyType")]
    public ControlPropertyValueType PropertyType
    {
        get { return _propertyType; }
        set { _propertyType = value; }
    }
    private bool _isBindOnly;

    [XmlAttribute("bindOnly")]
    public bool IsBindOnly
    {
        get { return _isBindOnly; }
        set { _isBindOnly = value; }
    }
    private bool _isLocalizable;

    [XmlAttribute("localizable")]
    public bool IsLocalizable
    {
        get { return _isLocalizable; }
        set { _isLocalizable = value; }
    }
    #endregion

    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return ControlPropertyItem.CategoryConst; }
    }
    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }

    [Browsable(false)]
    public Type SystemType
    {
        get
        {
            switch (PropertyType)
            {
                case ControlPropertyValueType.Integer:
                    return typeof(int);
                case ControlPropertyValueType.Boolean:
                    return typeof(bool);
                case ControlPropertyValueType.Xml:
                case ControlPropertyValueType.String:
                    return typeof(string);
                case ControlPropertyValueType.UniqueIdentifier:
                    return typeof(Guid);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    #endregion
}
