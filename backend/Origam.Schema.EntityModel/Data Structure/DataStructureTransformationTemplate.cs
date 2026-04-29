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
/// Summary description for DataStructureTransformationTemplate.
/// </summary>
[SchemaItemDescription(
    name: "Transformation Template",
    iconName: "icon_transformation-template.png"
)]
[HelpTopic(topic: "Template+Set+Template")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureTransformationTemplate : DataStructureTemplate
{
    public DataStructureTransformationTemplate()
        : base() { }

    public DataStructureTransformationTemplate(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public DataStructureTransformationTemplate(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid TransformationId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(TransformationConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "transformation", idField: "TransformationId")]
    public ITransformation Transformation
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.TransformationId;
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: key
                    ) as ITransformation;
        }
        set
        {
            this.TransformationId = (Guid)value.PrimaryKey[key: "Id"];
            this.Name = this.Transformation.Name;
        }
    }
    #endregion
    #region Overriden Members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.Transformation != null)
        {
            dependencies.Add(item: this.Transformation);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
}
