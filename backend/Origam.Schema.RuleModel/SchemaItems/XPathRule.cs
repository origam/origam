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
#pragma warning disable IDE0005
using System.Drawing.Design;
#pragma warning restore IDE0005
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.Schema.EntityModel;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.RuleModel;

/// <summary>
/// Summary description for XPathRule.
/// </summary>
[SchemaItemDescription(name: "Start Rule", iconName: "icon_27_rules.png")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class XPathRule : AbstractRule
{
    public XPathRule()
        : base() { }

    public XPathRule(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public XPathRule(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        XsltDependencyHelper.GetDependencies(
            item: this,
            dependencies: dependencies,
            text: this.XPath
        );
        base.GetExtraDependencies(dependencies: dependencies);
    }

    #region Properties
    [XmlAttribute(attributeName: "xPath")]
#if !NETSTANDARD
    [Editor(type: typeof(MultiLineTextEditor), baseType: typeof(UITypeEditor))]
#endif
    public string XPath { get; set; } = "";

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "isPathRelative")]
    public override bool IsPathRelative { get; set; } = false;
    #endregion
}
