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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for TransformationReference.
/// </summary>
[SchemaItemDescription("Transformation Reference", 16)]
[HelpTopic("Transformation+Reference")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Transformation")]
[ClassMetaVersion("6.0.0")]
public class TransformationReference : AbstractSchemaItem
{
    public const string CategoryConst = "TransformationReference";

    public TransformationReference()
        : base() { }

    public TransformationReference(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public TransformationReference(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override string Icon
    {
        get { return "16"; }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.Transformation != null)
            base.GetParameterReferences(Transformation, list);
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(this.Transformation);
        base.GetExtraDependencies(dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    public Guid TransformationId;

    [Category("Reference")]
    [TypeConverter(typeof(TransformationConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference("transformation", "TransformationId")]
    public ITransformation Transformation
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        typeof(ISchemaItem),
                        new ModelElementKey(this.TransformationId)
                    ) as ITransformation;
        }
        set
        {
            this.TransformationId = (Guid)value.PrimaryKey["Id"];
            this.Name = this.Transformation.Name;
        }
    }
    #endregion
}
