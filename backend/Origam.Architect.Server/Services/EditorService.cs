using System.Collections.Concurrent;
using System.Reflection;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Controllers;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class EditorService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    PropertyParser propertyParser)
{
    private readonly IPersistenceProvider persistenceProvider =
        persistenceService.SchemaProvider;

    private readonly ConcurrentDictionary<Guid, ISchemaItem> editorSchemaItems = new();

    public ISchemaItem OpenEditorWithNewItem(Guid parentId, string fullTypeName)
    {
        IBrowserNode2 parentItem = persistenceProvider
            .RetrieveInstance<IBrowserNode2>(parentId);
        var factory = (ISchemaItemFactory)parentItem;

        Type newItemType = Reflector.GetTypeByName(fullTypeName);
        object result = factory
            .GetType()
            .GetMethod("NewItem")
            .MakeGenericMethod(newItemType)
            .Invoke(factory,
                new object[] { schemaService.ActiveSchemaExtensionId, null });

        ISchemaItem item = (ISchemaItem)result;
        return editorSchemaItems.GetOrAdd(item.Id, id => item);
    }
    
    public ISchemaItem OpenEditor(Guid schemaItemId)
    {
        return editorSchemaItems.GetOrAdd(
            schemaItemId,
            id => persistenceService.SchemaProvider
                .RetrieveInstance<ISchemaItem>(id, false));
    }

    public void CloseEditor(Guid schemaItemId)
    {
        bool success = editorSchemaItems.TryRemove(schemaItemId, out ISchemaItem removedItem);
        if (!success)
        {
            return;
        }

        if(removedItem.IsPersisted)
        {
            // in case we edited some of the children we invalidate their cache
            removedItem.InvalidateChildrenPersistenceCache();
            removedItem.ClearCache();
        }
        else
        {
            ISchemaItemProvider provider = removedItem.ParentItem ?? removedItem.RootProvider ;
            if (provider != null)
            {
                if (provider.ChildItems.Contains(removedItem))
                {
                    provider.ChildItems.Remove(removedItem);
                }
            }
        }
    }

    public ISchemaItem ChangesToSchemaItem(ChangesModel input)
    {
        // We should probably make sure that a single item is not edited in multiple editors. Put the item to unsavedSchemaItems and rename it?
        ISchemaItem item = OpenEditor(input.SchemaItemId);
        PropertyInfo[] properties = item.GetType().GetProperties();
        foreach (var change in input.Changes)
        {
            PropertyInfo propertyToChange = properties
                .FirstOrDefault(prop => prop.Name == change.Name);

            if (propertyToChange == null)
            {
                throw new Exception(
                    $"Property {change.Name} not found on type {item.GetType().Name}");
            }

            object value = propertyParser.Parse(propertyToChange, change.Value);
            propertyToChange.SetValue(item, value);
        }

        return item;
    }

    public IEnumerable<ISchemaItem> GetOpenEditors()
    {
        return editorSchemaItems.Values;
    }
}