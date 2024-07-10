#region license
/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;
[SchemaItemDescription("XsltFunctionCollection", "service.png")]
[HelpTopic("Services")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class XsltFunctionCollection : AbstractSchemaItem
{
	public const string CategoryConst = "XsltFunctionCollection";
	public override string ItemType => CategoryConst;
	public XsltFunctionCollection()
	{
	}
	public XsltFunctionCollection(Guid schemaExtensionId) : base(schemaExtensionId)
	{
	}
	public XsltFunctionCollection(Key primaryKey) : base(primaryKey)
	{
	}
	[StringNotEmptyModelElementRule]
	[Description("C# namespace followed by a \".\" and a class name.")]
	[XmlAttribute("fullClassName")]
	public string FullClassName { get; set; }
	
	[StringNotEmptyModelElementRule]
	[Description("Assembly name (without extension) where the class is defined.")]
	[XmlAttribute("assemblyName")]
	public string AssemblyName { get; set; }
	
	[StringNotEmptyModelElementRule]
	[Description("Xslt functions found in the provided class will be defined in this xslt namespace.")]
	[XmlAttribute("xslNameSpaceUri")]
	public string XslNameSpaceUri { get; set; }	
	
	[StringNotEmptyModelElementRule]
	[Description("Xslt namespace prefix in xpath. Prefix in Xslt transformations will be declared in the Xslt templates.")]
	[XmlAttribute("xslNameSpacePrefix")]
	public string XslNameSpacePrefix { get; set; }
}
