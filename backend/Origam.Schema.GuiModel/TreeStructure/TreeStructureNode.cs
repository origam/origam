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
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for EntityFilter.
/// </summary>
[SchemaItemDescription(name: "Tree Node", folderName: "Nodes", iconName: "icon_tree-node.png")]
[HelpTopic(topic: "Tree+Node")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class TreeStructureNode : AbstractSchemaItem, ISchemaItemFactory, IDataStructureReference
{
    public const string CategoryConst = "TreeStructure";

    public TreeStructureNode()
        : base()
    {
        Init();
    }

    public TreeStructureNode(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public TreeStructureNode(Key primaryKey)
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
    private string _label;

    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "label")]
    public string Label
    {
        get { return _label; }
        set { _label = value; }
    }
    public Guid NodeIconId;

    [Category(category: "Menu Item")]
    [TypeConverter(type: typeof(GuiModel.GraphicsConverter))]
    [XmlReference(attributeName: "icon", idField: "NodeIconId")]
    public Graphics NodeIcon
    {
        get
        {
            return (GuiModel.Graphics)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.NodeIconId)
                );
        }
        set { this.NodeIconId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }
    public Guid DataStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "dataStructure", idField: "DataStructureId")]
    public DataStructure DataStructure
    {
        get
        {
            return (DataStructure)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DataStructureId)
                );
        }
        set
        {
            this.Method = null;
            this.SortSet = null;
            this.DataStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
        }
    }
    public Guid LoadByParentMethodId;

    [TypeConverter(type: typeof(DataStructureReferenceMethodConverter))]
    [XmlReference(attributeName: "loadByParentMethod", idField: "LoadByParentMethodId")]
    public DataStructureMethod LoadByParentMethod
    {
        get
        {
            return (DataStructureMethod)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.LoadByParentMethodId)
                );
        }
        set
        {
            this.LoadByParentMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    [Browsable(browsable: false)]
    public DataStructureMethod Method
    {
        get { return LoadByParentMethod; }
        set { LoadByParentMethod = value; }
    }

    [NotNullModelElementRule()]
    public Guid LoadByPrimaryKeyMethodId;

    [TypeConverter(type: typeof(DataStructureReferenceMethodConverter))]
    [XmlReference(attributeName: "loadByPrimaryKeyMethod", idField: "LoadByPrimaryKeyMethodId")]
    public DataStructureMethod LoadByPrimaryKeyMethod
    {
        get
        {
            return (DataStructureMethod)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.LoadByPrimaryKeyMethodId)
                );
        }
        set
        {
            this.LoadByPrimaryKeyMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public Guid DataStructureSortSetId;

    [TypeConverter(type: typeof(DataStructureReferenceSortSetConverter))]
    [XmlReference(attributeName: "sortSet", idField: "DataStructureSortSetId")]
    public DataStructureSortSet SortSet
    {
        get
        {
            return (DataStructureSortSet)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DataStructureSortSetId)
                );
        }
        set
        {
            this.DataStructureSortSetId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    #endregion
}
