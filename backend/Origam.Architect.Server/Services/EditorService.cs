using System.Reflection;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Controllers;
using Origam.Architect.Server.ReturnModels;
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

    private readonly List<ISchemaItem> unsavedSchemaItems = new();

    public ISchemaItem CreateSchemaItem(Guid parentId, string fullTypeName)
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
        unsavedSchemaItems.Add(item);
        return item;
    }

    public void CloseEditor(Guid itemId)
    {
        // The old architect removes the items at AbstractEditor_Closing method of
        // AbstractEditor class. This will have to happen here
        
        // ISchemaItemProvider parentProvider = (ISchemaItemProvider)parentItem;
        // if (parentProvider.ChildItems.Contains(item))
        // {
        //     parentProvider.ChildItems.Remove(item);
        // }  
    }

    public ISchemaItem ChangesToSchemaItem(ChangesModel input)
    {
        // We should probably make sure that a single item is not edited in multiple editors. Put the item to unsavedSchemaItems and rename it?
        ISchemaItem item =
            unsavedSchemaItems.FirstOrDefault(x => x.Id == input.SchemaItemId) ??
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