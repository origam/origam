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
using System.Text;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for Graphics.
/// </summary>
[SchemaItemDescription("Style", "icon_style.png")]
[HelpTopic("Styles")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class UIStyle : AbstractSchemaItem
{
    public const string CategoryConst = "Style";

    public UIStyle()
        : base()
    {
        Init();
    }

    public UIStyle(Guid schemaExtensionId)
        : base(schemaExtensionId)
    {
        Init();
    }

    public UIStyle(Key primaryKey)
        : base(primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.Add(typeof(UIStyleProperty));
    }

    public string StyleDefinition()
    {
        StringBuilder result = new StringBuilder();
        foreach (var property in ChildItemsByType<UIStyleProperty>(UIStyleProperty.CategoryConst))
        {
            result.AppendFormat("{0}:{1};", property.Property.Name, property.Value);
        }
        return result.ToString();
    }

    #region Properties
    public Guid ControlId;

    [TypeConverter(typeof(ControlConverter))]
    [NotNullModelElementRule()]
    [XmlReference("widget", "ControlId")]
    public ControlItem Widget
    {
        get
        {
            return (ControlItem)
                this.PersistenceProvider.RetrieveInstance(
                    typeof(ControlItem),
                    new ModelElementKey(this.ControlId)
                );
        }
        set { this.ControlId = (Guid)value.PrimaryKey["Id"]; }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.Widget != null)
        {
            dependencies.Add(this.Widget);
        }
        base.GetExtraDependencies(dependencies);
    }

    public override bool UseFolders
    {
        get { return false; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
}
