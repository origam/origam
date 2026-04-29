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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for EntityColumnReference.
/// </summary>
[SchemaItemDescription(name: "Data Structure Reference", iconName: "data-structure-reference.png")]
[HelpTopic(topic: "Data+Structure+Reference")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "DataStructure")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureReference : AbstractSchemaItem, IDataStructureReference
{
    public const string CategoryConst = "DataStructureReference";

    public DataStructureReference()
        : base() { }

    public DataStructureReference(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureReference(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.DataStructure != null)
        {
            base.GetParameterReferences(parentItem: DataStructure, list: list);
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.DataStructure);
        if (this.Method != null)
        {
            dependencies.Add(item: this.Method);
        }

        if (this.SortSet != null)
        {
            dependencies.Add(item: this.SortSet);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    public Guid DataStructureId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "dataStructure", idField: "DataStructureId")]
    [NotNullModelElementRule()]
    public DataStructure DataStructure
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataStructureId;
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: key
                    ) as DataStructure;
        }
        set
        {
            this.DataStructureId = (Guid)value.PrimaryKey[key: "Id"];
            this.Method = null;
            this.SortSet = null;
            this.Name = this.DataStructure.Name;
        }
    }
    public Guid DataStructureMethodId;

    [TypeConverter(type: typeof(DataStructureReferenceMethodConverter))]
    [Category(category: "Reference")]
    [XmlReference(attributeName: "method", idField: "DataStructureMethodId")]
    public DataStructureMethod Method
    {
        get
        {
            return (DataStructureMethod)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: DataStructureMethodId)
                );
        }
        set
        {
            this.DataStructureMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid DataStructureSortSetId;

    [TypeConverter(type: typeof(DataStructureReferenceSortSetConverter))]
    [Category(category: "Reference")]
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
