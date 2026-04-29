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

namespace Origam.Schema;

/// <summary>
/// Summary description for ParameterReference.
/// </summary>
[SchemaItemDescription(name: "Parameter Reference", iconName: "icon_parameter-reference.png")]
[HelpTopic(topic: "Parameter+Reference")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "Parameter")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ParameterReference : AbstractSchemaItem
{
    public const string CategoryConst = "ParameterReference";

    public ParameterReference()
        : base() { }

    public ParameterReference(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public ParameterReference(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Parameter);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Parameter.PrimaryKey))
                {
                    this.Parameter = item as SchemaItemParameter;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }
    #endregion
    #region Properties
    public Guid ParameterId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(ParameterReferenceConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "parameter", idField: "ParameterId")]
    public SchemaItemParameter Parameter
    {
        get
        {
            return (SchemaItemParameter)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(SchemaItemParameter),
                        primaryKey: new ModelElementKey(id: this.ParameterId)
                    ) as SchemaItemParameter;
        }
        set
        {
            this.ParameterId = (Guid)value.PrimaryKey[key: "Id"];
            if (this.Name == null)
            {
                this.Name = this.Parameter.Name;
            }
        }
    }
    #endregion
}
