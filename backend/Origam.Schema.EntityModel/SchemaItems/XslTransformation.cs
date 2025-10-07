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

[SchemaItemDescription("XSL Transformation (XSLT)", "xsl-transformation.png")]
[HelpTopic("Transformations")]
[ClassMetaVersion("6.0.1")]
public class XslTransformation : AbstractTransformation
{
    public XslTransformation()
        : base()
    {
        InitializeProperyContainers();
    }

    public XslTransformation(Guid schemaExtensionId)
        : base(schemaExtensionId)
    {
        InitializeProperyContainers();
    }

    public XslTransformation(Key primaryKey)
        : base(primaryKey)
    {
        InitializeProperyContainers();
    }

    private void InitializeProperyContainers()
    {
        text = new PropertyContainer<string>(containerName: nameof(text), containingObject: this);
    }

    public override object Clone()
    {
        var clone = (XslTransformation)base.Clone();
        clone.TextStore = TextStore;
        return clone;
    }

    #region Overriden ISchemaItem members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        XsltDependencyHelper.GetDependencies(this, dependencies, this.TextStore);
        base.GetExtraDependencies(dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    private PropertyContainer<string> text;

    [StringNotEmptyModelElementRule()]
    [XmlExternalFileReference(containerName: nameof(text), extension: ExternalFileExtension.Xslt)]
    public string TextStore
    {
        get => text.Get();
        set => text.Set(value);
    }
    #endregion
}
