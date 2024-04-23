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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.RuleModel;

/// <summary>
/// Summary description for XPathRule.
/// </summary>
[SchemaItemDescription("Start Rule", "icon_27_rules.png")]
[ClassMetaVersion("6.0.0")]
public class XPathRule : AbstractRule
{
	public XPathRule() : base() {}

	public XPathRule(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public XPathRule(Key primaryKey) : base(primaryKey)	{}

	public override SchemaItemCollection ChildItems
	{
		get
		{
				return new SchemaItemCollection();
			}
	}

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			XsltDependencyHelper.GetDependencies(this, dependencies, this.XPath);

			base.GetExtraDependencies (dependencies);
		}

	#region Properties
	[XmlAttribute("xPath")]
#if !NETSTANDARD
        [Editor(typeof(MultiLineTextEditor), typeof(UITypeEditor))]
#endif
	public string XPath { get; set; } = "";

	[DefaultValue(false)] 
	[XmlAttribute("isPathRelative")]
	public override bool IsPathRelative { get; set; } = false;
	#endregion

}