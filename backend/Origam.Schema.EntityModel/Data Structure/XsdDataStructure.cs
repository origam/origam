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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence; 

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for XsdDataStructure.
/// </summary>
[SchemaItemDescription("XSD Data Structure", "icon_xsd-data-structure.png")]
[HelpTopic("Data+Structures")]
public class XsdDataStructure : AbstractDataStructure
{
	public XsdDataStructure() : base(){}
		
	public XsdDataStructure(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public XsdDataStructure(Key primaryKey) : base(primaryKey)	{}

	public override SchemaItemCollection ChildItems
	{
		get
		{
				return new SchemaItemCollection();
			}
	}

	#region Properties
	private string _xsd = "";
		
	[XmlAttribute("xsd")]
	public string Xsd
	{
		get
		{
				return _xsd;
			}
		set
		{
				_xsd = value;
			}
	}
	#endregion
}