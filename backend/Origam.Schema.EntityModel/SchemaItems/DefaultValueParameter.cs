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

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for DeaultValueParameter.
/// </summary>
[SchemaItemDescription(
    name: "Default Value Parameter",
    folderName: "Parameters",
    iconName: "icon_default-value-parameter.png"
)]
[HelpTopic(topic: "Default+Value+Parameter")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DefaultValueParameter : SchemaItemParameter
{
    public DefaultValueParameter()
        : base() { }

    public DefaultValueParameter(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public DefaultValueParameter(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.DefaultValue);
    }

    #region Properties
    public Guid DefaultValueId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "defaultValue", idField: "DefaultValueId")]
    public DataConstant DefaultValue
    {
        get
        {
            try
            {
                return (ISchemaItem)
                        this.PersistenceProvider.RetrieveInstance(
                            type: typeof(ISchemaItem),
                            primaryKey: new ModelElementKey(id: this.DefaultValueId)
                        ) as DataConstant;
            }
            catch
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "ErrorDataConstantNotFound",
                        args: this.Name
                    )
                );
            }
        }
        set { this.DefaultValueId = (Guid)value.PrimaryKey[key: "Id"]; }
    }
    #endregion
}
