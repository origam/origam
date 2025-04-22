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
using Origam.UI; 

namespace Origam.Schema.GuiModel;
public abstract class AbstractControlSet : AbstractSchemaItem, IControlSet 
{
	public AbstractControlSet() {}
	public AbstractControlSet(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public AbstractControlSet(Key primaryKey) : base(primaryKey) {}
	public Guid DataSourceId;
	public BrowserNodeCollection Alternatives
	{
		get {
			var result = new BrowserNodeCollection ();
			foreach (var item 
			         in ChildItemsByType<ControlSetItem>(ControlSetItem.CategoryConst)) {
				if (item.IsAlternative) {
					result.Add (item);
				}
			}
			return result;
		}
	}
	public ControlSetItem MainItem
	{
		get {
			foreach (var item 
			         in ChildItemsByType<ControlSetItem>(ControlSetItem.CategoryConst)) {
				if(!item.IsAlternative) {
					return item;
				}
			}
			throw new Exception (
				$"Main item was not found for a control set {Path}");
		}
	}
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes => new[]
	{
		typeof(ControlSetItem)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		return base.NewItem<T>(schemaExtensionId, group, 
			typeof(T) == typeof(ControlSetItem) ?
				"NewComponent" : null);
	}

	#endregion
}

