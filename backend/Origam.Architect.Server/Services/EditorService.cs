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

    private readonly List<ISchemaItem> editorSchemaItems = new();

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
        editorSchemaItems.Add(item);
        return item;
    }
    
    public ISchemaItem OpenEditor(Guid schemaItemId)
    {
        var editorItem = editorSchemaItems.FirstOrDefault(x => x.Id == schemaItemId);
        if (editorItem == null)
        {
            editorItem = persistenceService.SchemaProvider
                .RetrieveInstance<ISchemaItem>(schemaItemId, false);
            editorSchemaItems.Add(editorItem);
        }

        return editorItem;
    }

    public void CloseEditor(Guid schemaItemId)
    {
        var editorItem = editorSchemaItems.FirstOrDefault(x => x.Id == schemaItemId);
        if (editorItem == null)
        {
            return;
        }
        
        if(editorItem.IsPersisted)
        {
            // in case we edited some of the children we invalidate their cache
            editorItem.InvalidateChildrenPersistenceCache();
            editorItem.ClearCache();
        }
        else
        {
            ISchemaItemProvider provider = editorItem.ParentItem ?? editorItem.RootProvider ;
            if (provider != null)
            {
                if (provider.ChildItems.Contains(editorItem))
                {
                    provider.ChildItems.Remove(editorItem);
                }
            }
        }
    }

    public ISchemaItem ChangesToSchemaItem(ChangesModel input)
    {
        // We should probably make sure that a single item is not edited in multiple editors. Put the item to unsavedSchemaItems and rename it?
        ISchemaItem item =
            editorSchemaItems.FirstOrDefault(x => x.Id == input.SchemaItemId) ??
            persistenceService.SchemaProvider
                .RetrieveInstance<ISchemaItem>(input.SchemaItemId, false);
        
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
}