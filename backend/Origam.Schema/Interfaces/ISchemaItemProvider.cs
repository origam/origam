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
using System.Collections;
using System.Collections.Generic;
using Origam.UI;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema;
/// <summary>
/// Root of different types of schema items (e.g. Form provider, Entity provider, Roles provider).
/// </summary>
public interface ISchemaItemProvider : ISchemaItemFactory, IBrowserNode2, IProviderContainer
{
//		event EventHandler SchemaItemDeleted;
//		event EventHandler SchemaItemChanged;
	/// <summary>
	/// Gets all schema items.
	/// </summary>
	ISchemaItemCollection ChildItems{get;}
	List<Type> ChildItemTypes{get;}
	ISchemaItem GetChildByName(string name, string itemType);
	List<ISchemaItem> ChildItemsRecursive{get;}
	List<ISchemaItem> ChildItemsByType(string itemType);
	List<ISchemaItem> ChildItemsByGroup(SchemaItemGroup group);
	bool HasChildItems{get;}
	bool HasChildItemsByType(string itemType);
	bool HasChildItemsByGroup(SchemaItemGroup group);
	List<SchemaItemGroup> ChildGroups{get;}
	ISchemaItemProvider RootProvider{get; set;}
	bool AutoCreateFolder{get;}
    void ClearCache();
}
