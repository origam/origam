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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for EntityFilter.
/// </summary>
[SchemaItemDescription(
    name: "Tree Structure",
    folderName: "Tree Structures",
    iconName: "icon_tree-structures.png"
)]
[HelpTopic(topic: "Tree+Structures")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class TreeStructure : AbstractSchemaItem, ISchemaItemFactory
{
    public const string CategoryConst = "TreeStructure";

    public TreeStructure()
        : base()
    {
        Init();
    }

    public TreeStructure(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public TreeStructure(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.Add(item: typeof(TreeStructureNode));
    }

    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override bool UseFolders
    {
        get { return false; }
    }
    #endregion
    #region Properties
    public string _rootNodeLabel;

    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "rootNodeLabel")]
    public string RootNodeLabel
    {
        get { return _rootNodeLabel; }
        set { _rootNodeLabel = value; }
    }
    #endregion
}
