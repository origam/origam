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

namespace Origam.Schema.EntityModel;

public enum DataEntityIndexSortOrder
{
    Ascending,
    Descending,
}

[SchemaItemDescription("Index", "Indexes", "icon_index.png")]
[HelpTopic("Indexes")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class DataEntityIndex : AbstractSchemaItem
{
    public DataEntityIndex() { }

    public DataEntityIndex(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public DataEntityIndex(Key primaryKey)
        : base(primaryKey) { }

    public const string CategoryConst = "DataEntityIndex";
    #region Properties
    private bool _isUnique = false;

    public override void OnNameChanged(string originalName)
    {
        string ToMappedName(string name)
        {
            return ParentItem.Name + "_" + name;
        }

        bool mappedNameNotEditedByUser = MappedObjectName == ToMappedName(originalName);
        if (string.IsNullOrEmpty(MappedObjectName) || mappedNameNotEditedByUser)
        {
            MappedObjectName = ToMappedName(Name);
        }
    }

    [DefaultValue(false)]
    [XmlAttribute("unique")]
    public bool IsUnique
    {
        get => _isUnique;
        set => _isUnique = value;
    }
    private bool _generateDeploymentScript = true;

    [Category("Mapping"), DefaultValue(true)]
    [Description(
        "Indicates if deployment script will be generated for this index. If set to false, this index will be skipped from the deployment scripts generator."
    )]
    [XmlAttribute("generateDeploymentScript")]
    public bool GenerateDeploymentScript
    {
        get { return _generateDeploymentScript; }
        set { _generateDeploymentScript = value; }
    }

    [PostgresLengthLimit("Be careful, index names must be unique within schema in Postgre SQL.")]
    [Category("Mapping")]
    [StringNotEmptyModelElementRule()]
    [Description("Name of the index in the database.")]
    [XmlAttribute("mappedObjectName")]
    public string MappedObjectName { get; set; }
    #endregion
    #region Overriden ISchemaItem Members
    public override bool UseFolders
    {
        get { return false; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
    #region ISchemaItemFactory Members
    [Browsable(false)]
    public override Type[] NewItemTypes => new[] { typeof(DataEntityIndexField) };

    public override SchemaItemGroup NewGroup(Guid schemaExtensionId, string groupName)
    {
        return null;
    }

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        return base.NewItem<T>(
            schemaExtensionId,
            group,
            typeof(T) == typeof(DataEntityIndexField)
                ? Name + "Field" + (ChildItems.Count + 1)
                : null
        );
    }
    #endregion
}
