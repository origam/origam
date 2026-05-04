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

namespace Origam.Schema.MenuModel;

/// <summary>
/// Summary description for Menu.
/// </summary>
[SchemaItemDescription(name: "Menu", iconName: "home.png")]
[HelpTopic(topic: "Menu")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class Menu : AbstractSchemaItem, ISchemaItemFactory
{
    public const string CategoryConst = "Menu";

    public Menu()
        : base()
    {
        Init();
    }

    public Menu(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public Menu(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        ChildItemTypes.Add(item: typeof(Submenu));
        ChildItemTypes.Add(item: typeof(FormReferenceMenuItem));
        ChildItemTypes.Add(item: typeof(DataConstantReferenceMenuItem));
        ChildItemTypes.Add(item: typeof(WorkflowReferenceMenuItem));
        ChildItemTypes.Add(item: typeof(ReportReferenceMenuItem));
        ChildItemTypes.Add(item: typeof(DashboardMenuItem));
        ChildItemTypes.Add(item: typeof(DynamicMenu));
    }

    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    [Browsable(browsable: false)]
    public override bool UseFolders
    {
        get { return false; }
    }
    #endregion
    #region Properties
    [Category(category: "Menu Item")]
    [XmlAttribute(attributeName: "displayName")]
    public string DisplayName { get; set; }
    #endregion
}
