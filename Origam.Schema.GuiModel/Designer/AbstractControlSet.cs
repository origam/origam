#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for AbstractControlSet.
	/// </summary>
	public abstract class AbstractControlSet : AbstractSchemaItem, IControlSet, ISchemaItemFactory 
	{
		public AbstractControlSet() : base() {}
		public AbstractControlSet(Guid schemaExtensionId) : base(schemaExtensionId) {}
		public AbstractControlSet(Key primaryKey) : base(primaryKey) {}

		public BrowserNodeCollection Alternatives
		{
			get {
				BrowserNodeCollection result = new BrowserNodeCollection ();
				foreach (ControlSetItem item in ChildItemsByType(ControlSetItem.ItemTypeConst)) {
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
				foreach (ControlSetItem item in ChildItemsByType(ControlSetItem.ItemTypeConst)) {
					if (! item.IsAlternative) {
						return item;
					}
				}
				throw new Exception ("Main item was not found for a control set " + this.Path);
			}
		}

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {typeof(ControlSetItem)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(ControlSetItem))
			{
				ControlSetItem item = new ControlSetItem(schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewComponent";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorFormControlSetUnknownType"));
		}

	
		#endregion
	}

	
}
