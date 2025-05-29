using System.Collections.Concurrent;
using System.Reflection;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Controllers;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.GuiModel;
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

    private readonly ConcurrentDictionary<Guid, EditorData> editorSchemaItems =
        new();

    public EditorData OpenEditorWithNewItem(string parentId, string fullTypeName)
    {
        ISchemaItemFactory factory = GetParentItemFactory(parentId);

        Type newItemType = Reflector.GetTypeByName(fullTypeName);
        object result = factory
            .GetType()
            .GetMethod("NewItem")
            .MakeGenericMethod(newItemType)
            .Invoke(factory,
                new object[] { schemaService.ActiveSchemaExtensionId, null });

        if (result is FormControlSet formControlSet)
        {
            var rootControl = formControlSet.NewItem<ControlSetItem>(
                schemaService.ActiveSchemaExtensionId, null);
            ControlItem controlItem = GetControlByType("Origam.Gui.Win.AsForm");
            SetControlItem(rootControl, controlItem);
            rootControl.GetProperty("Height").Value = "500";
            rootControl.GetProperty("Width").Value = "500";
            rootControl.GetProperty("Top").Value = "15";
            rootControl.GetProperty("Left").Value = "15";
        } 
        else if (result is PanelControlSet panelControlSet)
        {
            var rootControl = panelControlSet.NewItem<ControlSetItem>(
                schemaService.ActiveSchemaExtensionId, null);
            ControlItem controlItem = GetControlByType("Origam.Gui.Win.AsPanel");
            SetControlItem(rootControl, controlItem);
            rootControl.GetProperty("Height").Value = "500";
            rootControl.GetProperty("Width").Value = "500";
            rootControl.GetProperty("Top").Value = "15";
            rootControl.GetProperty("Left").Value = "15";
        }

        ISchemaItem item = (ISchemaItem)result;
        return editorSchemaItems
            .GetOrAdd(item.Id, id => new EditorData(item));
    }

    private ISchemaItemFactory GetParentItemFactory(string parentId)
    {
        if (Guid.TryParse(parentId, out Guid parentGuid))
        {
            IBrowserNode2 parentItem = persistenceProvider
                .RetrieveInstance<IBrowserNode2>(parentGuid);
            return (ISchemaItemFactory)parentItem;
        }

        ISchemaItemProvider provider = schemaService.Providers
            .FirstOrDefault(provider => provider.GetType().FullName == parentId);
        if (provider == null)
        {
            throw new Exception("Unable to find schema item provider " + parentId);
        }
        return provider;
    }

    private void SetControlItem (ControlSetItem controlSetItem, ControlItem controlItem)
    {
        controlSetItem.ControlItem = controlItem;
        List<ControlPropertyItem> controlProperties = controlItem
            .ChildItemsByType<ControlPropertyItem>(ControlPropertyItem.CategoryConst);
        
        foreach (ControlPropertyItem controlProperty in controlProperties)
        {
            PropertyValueItem valueItem = controlSetItem.NewItem<PropertyValueItem>(
                schemaService.ActiveSchemaExtensionId, null);
            valueItem.ControlPropertyItem = controlProperty;
            valueItem.Name = controlProperty.Name;
        }
    }

    public ControlItem GetControlByType(string fullTypeName)
    {
        var items = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems
            .OfType<ControlItem>();
        foreach (ControlItem item in items)
        {
           // When we decide to implement the plugin support we have to extend this method.
           // The original is here should look here for more 
           // https://github.com/origam/origam/blob/b5f3cc1dfe853de4082e41c594eb8f6c59451a9c/backend/Origam.Gui.Designer/ControlSetEditor.cs#L940

            if (item.ControlType == fullTypeName)
            {
                return item;
            }
        }

        return null;
    }

    public EditorData OpenEditor(Guid schemaItemId)
    {
        return editorSchemaItems.GetOrAdd(
            schemaItemId,
            id =>
            {
                ISchemaItem item = persistenceService.SchemaProvider
                    .RetrieveInstance<ISchemaItem>(id, false);
                return new EditorData(item);
            });
    }

    public void CloseEditor(Guid schemaItemId)
    {
        bool success =
            editorSchemaItems.TryRemove(schemaItemId,
                out EditorData removedData);
        if (!success)
        {
            return;
        }

        ISchemaItem removedItem = removedData.Item;
        if (removedItem.IsPersisted)
        {
            // in case we edited some of the children we invalidate their cache
            removedItem.InvalidateChildrenPersistenceCache();
            removedItem.ClearCache();
        }
        else
        {
            ISchemaItemProvider provider =
                removedItem.ParentItem ?? removedItem.RootProvider;
            if (provider != null)
            {
                if (provider.ChildItems.Contains(removedItem))
                {
                    provider.ChildItems.Remove(removedItem);
                }
            }
        }
    }

    public EditorData ChangesToEditorData(ChangesModel input)
    {
        EditorData editor = OpenEditor(input.SchemaItemId);
        PropertyInfo[] properties = editor.Item.GetType().GetProperties();
        foreach (var change in input.Changes)
        {
            PropertyInfo propertyToChange = properties
                .FirstOrDefault(prop => prop.Name == change.Name);

            if (propertyToChange == null)
            {
                throw new Exception(
                    $"Property {change.Name} not found on type {editor.GetType().Name}");
            }

            object newValue =
                propertyParser.Parse(propertyToChange, change.Value);
            object oldValue = propertyToChange.GetValue(editor.Item);
            if (oldValue != newValue)
            {
                editor.IsDirty = true;
            }

            propertyToChange.SetValue(editor.Item, newValue);
        }

        return editor;
    }

    public IEnumerable<EditorData> GetOpenEditors()
    {
        return editorSchemaItems.Values
            .OrderBy(x => x.OpenedAt);
    }
}

public class EditorData(ISchemaItem item)
{
    public ISchemaItem Item { get; } = item;
    public DateTime OpenedAt { get; } = DateTime.Now;

    public bool IsDirty { get; set; }
}