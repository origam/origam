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
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.GuiModel;
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ColumnParameterMapping : AbstractSchemaItem
{
	public const string CategoryConst = "ColumnParameterMapping";
	public ColumnParameterMapping() : base() {}
	
	public ColumnParameterMapping(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public ColumnParameterMapping(Key primaryKey) : base(primaryKey) {}
	#region Properties
	private string _columnName;
	
    [XmlAttribute("field")]
	public string ColumnName
	{
		get
		{
			return _columnName;
		}
		set
		{
			_columnName = value;
            				
		}
	}
	//Schema item Name stores ParameterName
	
	#endregion
       
	#region Overriden ISchemaItem Members
	public override string Icon
	{
		get
		{
			return "3";
		}
	}
	public override string ItemType
	{
		get
		{
			return ColumnParameterMapping.CategoryConst;
		}
	}
	public override ISchemaItemCollection ChildItems
	{
		get
		{
			return SchemaItemCollection.Create();
		}
	}
	#endregion			

}

