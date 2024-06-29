namespace Origam.Schema.ItemCollection;

class ServerSchemaItemCollection : SchemaItemCollectionBase<AbstractSchemaItem>,
    ISchemaItemCollection
{
    
    public ServerSchemaItemCollection()
    {
    }
    
    public ServerSchemaItemCollection(AbstractSchemaItem parentSchemaItem)
    {
        ParentSchemaItem = parentSchemaItem;
    }
    
    public new void Add(AbstractSchemaItem value)
    {
        base.Add(value);
        if (value.IsAbstract)
        {
            SetDerivedFrom(value);
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
    
}