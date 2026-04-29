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
[SchemaItemDescription(name: "Field Reference", iconName: "icon_field-reference.png")]
[HelpTopic(topic: "Field+Reference")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "Field")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class EntityColumnReference : AbstractSchemaItem
{
    public const string CategoryConst = "EntityColumnReference";

    public EntityColumnReference()
        : base() { }

    public EntityColumnReference(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public EntityColumnReference(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override string Icon
    {
        get
        {
            try
            {
                return this.Field.Icon;
            }
            catch
            {
                return "icon_field-reference.png";
            }
        }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.Field != null)
        {
            base.GetParameterReferences(parentItem: Field, list: list);
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Field);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Field.PrimaryKey))
                {
                    this.Field = item as IDataEntityColumn;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    public Guid FieldId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(EntityColumnReferenceConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "field", idField: "FieldId")]
    public IDataEntityColumn Field
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.FieldId;
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: key
                    ) as IDataEntityColumn;
        }
        set
        {
            this.FieldId = (Guid)value.PrimaryKey[key: "Id"];
            if (this.Name == null)
            {
                this.Name = this.Field.Name;
            }
        }
    }
    #endregion
}
