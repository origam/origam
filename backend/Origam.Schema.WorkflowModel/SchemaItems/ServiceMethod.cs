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
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.WorkflowModel;
[SchemaItemDescription("Method", "Methods", "method-1.png")]
[HelpTopic("Service+Method")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ServiceMethod : AbstractSchemaItem, IServiceMethod, ISchemaItemFactory
{
	public const string CategoryConst = "ServiceMethod";
	public ServiceMethod() {}
	public ServiceMethod(Guid schemaExtensionId) 
		: base(schemaExtensionId) {}
	public ServiceMethod(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	private OrigamDataType _returnValueDataType;
	[XmlAttribute("returnValueDataType")]
	public OrigamDataType ReturnValueDataType
	{
		get => _returnValueDataType;
		set => _returnValueDataType = value;
	}
	#endregion
	#region Overriden ISchemaItem Members
	
	public override string ItemType => CategoryConst;
	public override bool UseFolders => false;
	#endregion
	#region ISchemaItemFactory Members
	[Browsable(false)]
	public override Type[] NewItemTypes => new[]
	{
		typeof(ServiceMethodParameter)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		return base.NewItem<T>(schemaExtensionId, group, 
			typeof(T) == typeof(ServiceMethodParameter) ?
				"NewParameter" : null);
	}
	#endregion
}
