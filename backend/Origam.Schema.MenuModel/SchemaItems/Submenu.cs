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

namespace Origam.Schema.MenuModel;

[SchemaItemDescription(name: "Submenu", iconName: "menu_folder.png")]
[HelpTopic(topic: "Submenu")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class Submenu : AbstractMenuItem
{
    public Submenu() { }

    public Submenu(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public Submenu(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    [Browsable(browsable: false)]
    public override string Roles
    {
        get
        {
            var children = ChildItemsRecursive;
            var roles = new List<string>();
            foreach (var child in children)
            {
                if (child is AbstractMenuItem menuItem && !(menuItem is Submenu))
                {
                    if (menuItem.Roles == "*")
                    {
                        return "*";
                    }
                    var childRoles = menuItem.Roles.Split(separator: ";".ToCharArray());
                    foreach (var role in childRoles)
                    {
                        if (!roles.Contains(item: role))
                        {
                            roles.Add(item: role);
                        }
                    }
                }
            }
            var stringBuilder = new StringBuilder();
            foreach (string role in roles)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(value: ";");
                }
                stringBuilder.Append(value: role);
            }
            return stringBuilder.ToString();
        }
        set => base.Roles = value;
    }

    [Browsable(browsable: false)]
    public override string Features
    {
        get
        {
            var children = ChildItemsRecursive;
            var features = new List<string>();
            foreach (var child in children)
            {
                if (child is AbstractMenuItem menuItem && !(menuItem is Submenu))
                {
                    if ((menuItem.Features == "") || (menuItem.Features == null))
                    {
                        return "";
                    }
                    var childFeatures = menuItem.Features.Split(separator: ";".ToCharArray());
                    foreach (var feature in childFeatures)
                    {
                        if (!features.Contains(item: feature))
                        {
                            features.Add(item: feature);
                        }
                    }
                }
            }
            var stringBuilder = new StringBuilder();
            foreach (string feature in features)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(value: ";");
                }

                stringBuilder.Append(value: feature);
            }
            return stringBuilder.ToString();
        }
        set => base.Features = value;
    }

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "isHidden")]
    public bool IsHidden { get; set; }

    [Browsable(browsable: false)]
    public new bool OpenExclusively
    {
        get => base.OpenExclusively;
        set => base.OpenExclusively = value;
    }
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[]
        {
            typeof(Submenu),
            typeof(FormReferenceMenuItem),
            typeof(DataConstantReferenceMenuItem),
            typeof(WorkflowReferenceMenuItem),
            typeof(ReportReferenceMenuItem),
            typeof(DashboardMenuItem),
            typeof(DynamicMenu),
        };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(WorkflowReferenceMenuItem))
        {
            itemName = "<SequentialWorkflowReference_name>";
        }
        else if (typeof(T) == typeof(FormReferenceMenuItem))
        {
            itemName = "<ScreenReference_name>";
        }
        else if (typeof(T) == typeof(ReportReferenceMenuItem))
        {
            itemName = "<ReportReference_name>";
        }
        else if (typeof(T) == typeof(DataConstantReferenceMenuItem))
        {
            itemName = "<DataConstantReference_name>";
        }
        else if (typeof(T) == typeof(Submenu))
        {
            itemName = "<Submenu_name>";
        }
        else if (typeof(T) == typeof(DashboardMenuItem))
        {
            itemName = "<Dashboard_name>";
        }
        else if (typeof(T) == typeof(DynamicMenu))
        {
            itemName = "<DynamicMenu_name>";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }

    public override int CompareTo(object obj)
    {
        var abstractMenuItem = obj as AbstractMenuItem;
        if (obj is Submenu)
        {
            return DisplayName.CompareTo(strB: abstractMenuItem.DisplayName);
        }
        if (abstractMenuItem != null)
        {
            return -1;
        }
        throw new InvalidCastException();
    }
    #endregion
}
