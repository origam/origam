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
[SchemaItemDescription("Transformation Template", "icon_transformation-template.png")]
[HelpTopic("Template+Set+Template")]
[ClassMetaVersion("6.0.0")]
public class DataStructureTransformationTemplate : DataStructureTemplate
{
    public DataStructureTransformationTemplate()
        : base() { }

    public DataStructureTransformationTemplate(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public DataStructureTransformationTemplate(Key primaryKey)
        : base(primaryKey) { }

    #region Properties
    public Guid TransformationId;

    [Category("Reference")]
    [TypeConverter(typeof(TransformationConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("transformation", "TransformationId")]
    public ITransformation Transformation
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.TransformationId;
            return (ISchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key)
                as ITransformation;
        }
        set
        {
            this.TransformationId = (Guid)value.PrimaryKey["Id"];
            this.Name = this.Transformation.Name;
        }
    }
    #endregion
    #region Overriden Members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.Transformation != null)
        {
            dependencies.Add(this.Transformation);
        }

        base.GetExtraDependencies(dependencies);
    }
    #endregion
}
