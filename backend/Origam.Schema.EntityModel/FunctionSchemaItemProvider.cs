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

namespace Origam.Schema.EntityModel;

public class FunctionSchemaItemProvider : AbstractSchemaItemProvider
{
	public FunctionSchemaItemProvider() {}

	#region ISchemaItemProvider Members
	public override string RootItemType => Function.CategoryConst;

	public override string Group => "DATA";

	#endregion

	#region IBrowserNode Members

	public override string Icon =>
		// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
		"icon_10_functions.png";

	public override string NodeText
	{
		get => "Functions";
		set => base.NodeText = value;
	}

	public override string NodeToolTipText =>
		// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
		null;

	#endregion

	#region ISchemaItemFactory Members

	public override Type[] NewItemTypes => new[] {typeof(Function)};

	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
			return base.NewItem<T>(schemaExtensionId, group, 
				typeof(T) == typeof(Function) ?
				"NewFunction" : null);
		}

	#endregion
}