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

namespace Origam.Schema.MenuModel;

/// <summary>
/// Summary description for Graphics.
/// </summary>
[SchemaItemDescription(name: "Search Data Source", icon: 9)]
[HelpTopic(topic: "Search Data Sources")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class SearchDataSource : AbstractSchemaItem, IDataStructureReference
{
    public const string CategoryConst = "SearchDataSource";

    public SearchDataSource()
        : base()
    {
        Init();
    }

    public SearchDataSource(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public SearchDataSource(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    #region Properties
    public Guid DataStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [NotNullModelElementRule()]
    [Category(category: "Data Source")]
    [DisplayName(displayName: "Data Structure")]
    [Description(
        description: "Data structure that will be used to retrieve search results. Must contain columns: Name, ReferenceId; optional column: Description."
    )]
    [XmlReference(attributeName: "dataStructure", idField: "DataStructureId")]
    public DataStructure DataStructure
    {
        get
        {
            return (DataStructure)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructure),
                    primaryKey: new ModelElementKey(id: this.DataStructureId)
                );
        }
        set { this.DataStructureId = (Guid)value.PrimaryKey[key: "Id"]; }
    }
    public Guid DataStructureMethodId;

    [TypeConverter(type: typeof(DataStructureReferenceMethodConverter))]
    [Category(category: "Data Source")]
    [DisplayName(displayName: "Data Structure Method")]
    [Description(
        description: "Method that will accept a string parameter to return search results."
    )]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "method", idField: "DataStructureMethodId")]
    public DataStructureMethod Method
    {
        get
        {
            return (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: DataStructureMethodId)
                );
        }
        set
        {
            DataStructureMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    private string _groupLabel;

    [Category(category: "Results")]
    [DisplayName(displayName: "Group Label")]
    [Description(description: "A text under which the search results will be grouped.")]
    [NotNullModelElementRule()]
    [Localizable(isLocalizable: true)]
    [XmlAttribute(attributeName: "groupLabel")]
    public string GroupLabel
    {
        get { return _groupLabel; }
        set { _groupLabel = value; }
    }
    private string _filterParameter;

    [Category(category: "Data Source")]
    [DisplayName(displayName: "Filter Parameter")]
    [Description(description: "String parameter that will accept the searched text.")]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "filterParameter")]
    public string FilterParameter
    {
        get { return _filterParameter; }
        set { _filterParameter = value; }
    }
    public Guid LookupId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(DataLookupConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "lookup", idField: "LookupId")]
    public IDataLookup Lookup
    {
        get
        {
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.LookupId)
                );
        }
        set { this.LookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }
    private string _roles = "*";

    [Category(category: "Security")]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.DataStructure);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override bool UseFolders
    {
        get { return false; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override string Icon
    {
        get { return "9"; }
    }
    #endregion
}
