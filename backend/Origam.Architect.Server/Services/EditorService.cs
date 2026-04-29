#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.Collections.Concurrent;
using System.Reflection;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Models;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class EditorService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    PropertyParser propertyParser
)
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    private readonly ConcurrentDictionary<EditorId, EditorData> editorSchemaItems = new();

    public EditorData OpenEditorWithNewItem(string parentId, string fullTypeName)
    {
        ISchemaItemFactory factory = GetParentItemFactory(parentId: parentId);

        Type newItemType = Reflector.GetTypeByName(typeName: fullTypeName);
        object result = factory
            .GetType()
            .GetMethod(name: "NewItem")
            .MakeGenericMethod(typeArguments: newItemType)
            .Invoke(
                obj: factory,
                parameters: new object[] { schemaService.ActiveSchemaExtensionId, null }
            );

        if (result is FormControlSet formControlSet)
        {
            var rootControl = formControlSet.NewItem<ControlSetItem>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
            ControlItem controlItem = GetControlByType(fullTypeName: "Origam.Gui.Win.AsForm");
            SetControlItem(controlSetItem: rootControl, controlItem: controlItem);
            rootControl.GetProperty(propertyName: "Height").Value = "500";
            rootControl.GetProperty(propertyName: "Width").Value = "500";
            rootControl.GetProperty(propertyName: "Top").Value = "15";
            rootControl.GetProperty(propertyName: "Left").Value = "15";
        }
        else if (result is PanelControlSet panelControlSet)
        {
            var rootControl = panelControlSet.NewItem<ControlSetItem>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
            ControlItem controlItem = GetControlByType(fullTypeName: "Origam.Gui.Win.AsPanel");
            SetControlItem(controlSetItem: rootControl, controlItem: controlItem);
            rootControl.GetProperty(propertyName: "Height").Value = "500";
            rootControl.GetProperty(propertyName: "Width").Value = "500";
            rootControl.GetProperty(propertyName: "Top").Value = "15";
            rootControl.GetProperty(propertyName: "Left").Value = "15";
        }

        ISchemaItem item = (ISchemaItem)result;
        return editorSchemaItems.GetOrAdd(
            key: EditorId.Default(schemaItemId: item.Id),
            valueFactory: id => new EditorData(item: item, id: id)
        );
    }

    private ISchemaItemFactory GetParentItemFactory(string parentId)
    {
        if (Guid.TryParse(input: parentId, result: out Guid parentGuid))
        {
            IBrowserNode2 parentItem = persistenceProvider.RetrieveInstance<IBrowserNode2>(
                instanceId: parentGuid
            );
            return (ISchemaItemFactory)parentItem;
        }

        ISchemaItemProvider provider = schemaService.Providers.FirstOrDefault(predicate: provider =>
            provider.GetType().FullName == parentId
        );
        if (provider == null)
        {
            throw new Exception(message: "Unable to find schema item provider " + parentId);
        }
        return provider;
    }

    private void SetControlItem(ControlSetItem controlSetItem, ControlItem controlItem)
    {
        controlSetItem.ControlItem = controlItem;
        List<ControlPropertyItem> controlProperties =
            controlItem.ChildItemsByType<ControlPropertyItem>(
                itemType: ControlPropertyItem.CategoryConst
            );

        foreach (ControlPropertyItem controlProperty in controlProperties)
        {
            PropertyValueItem valueItem = controlSetItem.NewItem<PropertyValueItem>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
            valueItem.ControlPropertyItem = controlProperty;
            valueItem.Name = controlProperty.Name;
        }
    }

    public ControlItem GetControlByType(string fullTypeName)
    {
        var items = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems.OfType<ControlItem>();
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

    public EditorData OpenDefaultEditor(Guid schemaItemId)
    {
        return editorSchemaItems.GetOrAdd(
            key: EditorId.Default(schemaItemId: schemaItemId),
            valueFactory: editorId =>
            {
                ISchemaItem item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(
                    instanceId: editorId.SchemaItemId,
                    useCache: false
                );
                return new EditorData(item: item, id: editorId);
            }
        );
    }

    public EditorData OpenDocumentationEditor(Guid schemaItemId)
    {
        return editorSchemaItems.GetOrAdd(
            key: EditorId.Documentation(schemaItemId: schemaItemId),
            valueFactory: editorId =>
            {
                ISchemaItem item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(
                    instanceId: editorId.SchemaItemId,
                    useCache: false
                );
                return new EditorData(item: item, id: editorId);
            }
        );
    }

    public void CloseEditor(EditorId editorId)
    {
        bool success = editorSchemaItems.TryRemove(
            key: editorId,
            value: out EditorData removedData
        );
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
            ISchemaItemProvider provider = removedItem.ParentItem ?? removedItem.RootProvider;
            if (provider != null)
            {
                if (provider.ChildItems.Contains(item: removedItem))
                {
                    provider.ChildItems.Remove(item: removedItem);
                }
            }
        }
    }

    public EditorData ChangesToEditorData(ChangesModel input)
    {
        EditorData editor = OpenDefaultEditor(schemaItemId: input.SchemaItemId);
        PropertyInfo[] properties = editor.Item.GetType().GetProperties();
        foreach (var change in input.Changes)
        {
            PropertyInfo propertyToChange = properties.FirstOrDefault(predicate: prop =>
                prop.Name == change.Name
            );

            if (propertyToChange == null)
            {
                throw new Exception(
                    message: $"Property {change.Name} not found on type {editor.GetType().Name}"
                );
            }

            object newValue = propertyParser.Parse(property: propertyToChange, value: change.Value);
            object oldValue = propertyToChange.GetValue(obj: editor.Item);
            if (oldValue != newValue)
            {
                editor.IsDirty = true;
            }

            propertyToChange.SetValue(obj: editor.Item, value: newValue);
        }

        return editor;
    }

    public IEnumerable<EditorData> GetOpenEditors()
    {
        return editorSchemaItems.Values.OrderBy(keySelector: x => x.OpenedAt);
    }
}

public class EditorData(ISchemaItem item, EditorId id)
{
    public EditorId Id { get; } = id;
    public ISchemaItem Item { get; } = item;
    public DocumentationComplete DocumentationData { get; set; }
    public DateTime OpenedAt { get; } = DateTime.Now;

    public bool IsDirty { get; set; }
}
