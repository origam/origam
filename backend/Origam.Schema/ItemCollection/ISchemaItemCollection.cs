using System;
using System.Collections.Generic;

namespace Origam.Schema.ItemCollection;

interface ISchemaItemCollection : IList<AbstractSchemaItem>, IDisposable
{
    public bool DeleteItemsOnClear { get; set; }
    public bool RemoveDeletedItems { get; set; }
    public bool UpdateParentItem { get; set; }
    public AbstractSchemaItem ParentSchemaItem { get; set;}
}