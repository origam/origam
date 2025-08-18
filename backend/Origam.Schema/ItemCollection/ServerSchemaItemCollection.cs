#region license

/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

using System.Collections.Generic;

namespace Origam.Schema.ItemCollection;

class ServerSchemaItemCollection : SchemaItemCollectionBase<ISchemaItem>,
    ISchemaItemCollection
{
    
    public ServerSchemaItemCollection()
    {
    }
    
    public ServerSchemaItemCollection(ISchemaItem parentSchemaItem)
    {
        ParentSchemaItem = parentSchemaItem;
    }
    
    public override void Add(ISchemaItem value)
    {
        base.Add(value);
        if (value.IsAbstract)
        {
            SetDerivedFrom(value);
        }
    }

    public override void AddRange(IEnumerable<ISchemaItem> other)
    {
        foreach (var item in other)
        {
            Add(item);
        }
    }
    

    protected override void OnSet(int index, ISchemaItem oldItem,
        ISchemaItem newItem)
    {
        if (UpdateParentItem)
        {
            newItem.ParentItem = ParentSchemaItem;
            oldItem.ParentItem = null;
        }
    }
 
    protected override void OnInsert(int index, ISchemaItem item)
    {
        if (item.IsAbstract)
        {
            SetDerivedFrom(item);
        }
        if (UpdateParentItem)
        {
            item.ParentItem = ParentSchemaItem;
        }
    }

    protected override void OnClear()
    {
        if (!disposing)
        {
            clearing = true;
            foreach (ISchemaItem item in this)
            {
                if (DeleteItemsOnClear)
                {
                    item.IsDeleted = true;
                }
            }
            clearing = false;
        }
    }

    protected override void OnRemove(int index, ISchemaItem item)
    {
        if (item != null)
        {
            item.ParentItem = null;
        }
    }
    
}