using System;

namespace Origam.Schema;

class ServerSchemItemCollection : CheckedList<AbstractSchemaItem>, IDisposable
{
    private bool disposing;
    private bool clearing;
    public bool DeleteItemsOnClear { get; set; } = true;
    public bool RemoveDeletedItems { get; set; } = true;
    public bool UpdateParentItem { get; set; } = true;
    public AbstractSchemaItem ParentSchemaItem { get; set;}
    public void Add(AbstractSchemaItem value)
    {
        base.Add(value);
        if (value.IsAbstract)
        {
            SetDerivedFrom(value);
        }
    }
    
    private void SetDerivedFrom(AbstractSchemaItem item)
    {
        if (item.ParentItem != null)
        {
            // If we assign derived items, we mark them
            if (!item.ParentItem.PrimaryKey.Equals(ParentSchemaItem
                    .PrimaryKey))
            {
                item.DerivedFrom = item.ParentItem;
                item.ParentItem = ParentSchemaItem;
            }
        }
    }

    protected override void OnSet(int index, AbstractSchemaItem oldItem,
        AbstractSchemaItem newItem)
    {
        if (UpdateParentItem)
        {
            newItem.ParentItem = ParentSchemaItem;
            oldItem.ParentItem = null;
        }
    }
 
    protected override void OnInsert(int index, AbstractSchemaItem item)
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
            foreach (AbstractSchemaItem item in this)
            {
                if (DeleteItemsOnClear)
                {
                    item.IsDeleted = true;
                }
            }
            clearing = false;
        }
    }

    protected override void OnRemove(int index, AbstractSchemaItem item)
    {
        if (item != null)
        {
            item.ParentItem = null;
        }
    }
    
    public void Dispose()
    {
        disposing = true;
        Clear();
    }
}