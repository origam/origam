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
using System.Collections.Generic;
using System.ComponentModel;

using Origam.Schema.EntityModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.RuleModel;
/// <summary>
/// Summary description for XslRule.
/// </summary>
[ClassMetaVersion("6.0.0")]
public class XslRule : AbstractRule
{
	public XslRule() : base()
    {
        InitializeProperyContainers();
    }
    public XslRule(Guid schemaExtensionId) : base(schemaExtensionId)
    {
        InitializeProperyContainers();
    }
    public XslRule(Key primaryKey) : base(primaryKey)
    {
        InitializeProperyContainers();
    }
    private void InitializeProperyContainers()
    {
        xsl = new PropertyContainer<string>(
            containerName: nameof(xsl),
            containingObject: this);
    }
    #region Overriden AbstractSchemaItem members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		XsltDependencyHelper.GetDependencies(this, dependencies, this.Xsl);
		base.GetExtraDependencies (dependencies);
	}
	public override SchemaItemCollection ChildItems
	{
		get
		{
			return new SchemaItemCollection();
		}
	}
	#endregion
	#region Properties
	internal PropertyContainer<string> xsl;
	
    [XmlExternalFileReference(containerName: nameof(xsl),
        extension: ExternalFileExtension.Xslt)]
    public string Xsl
	{
        get => xsl.Get();
        set => xsl.Set(value);
    }
    [Browsable(false)] 
	public override bool IsPathRelative
	{
		get
		{
			return false;
		}
		set
		{
			
		}
	}
	#endregion
}
