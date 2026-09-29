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

using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services.SectionEditor;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class DesignerEditorService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    IDocumentationService documentationService,
    ControlAdapterFactory adapterFactory,
    PanelControlFactory panelControlFactory,
    ApiControlFactory apiControlFactory
)
{
    public bool Update(AbstractControlSet screenSection, SectionEditorChangesModel input)
    {
        bool editorIsDirty = false;
        if (input.Name != null && screenSection.Name != input.Name)
        {
            screenSection.Name = input.Name;
            editorIsDirty = true;
        }

        if (
            input.SelectedDataSourceId is { } selectedDataSourceId
            && screenSection.DataSourceId != selectedDataSourceId
        )
        {
            screenSection.DataSourceId = selectedDataSourceId;
            editorIsDirty = true;
        }

        foreach (var changes in input.ModelChanges)
        {
            ControlSetItem itemToUpdate =
                screenSection.GetChildByIdRecursive(changes.SchemaItemId) as ControlSetItem;
            if (itemToUpdate == null)
            {
                throw new Exception(
                    string.Format(
                        Strings.DesignerEditor_ChildNotFound,
                        changes.SchemaItemId,
                        screenSection.Id
                    )
                );
            }

            if (
                changes.ParentSchemaItemId is { } newParentId
                && itemToUpdate.Id != screenSection.MainItem.Id
                && itemToUpdate.ParentItemId != newParentId
            )
            {
                ISchemaItem newParent = screenSection.GetChildByIdRecursive(newParentId);
                itemToUpdate.ParentItem.ChildItems.Remove(itemToUpdate);
                newParent.ChildItems.Add(itemToUpdate);
            }

            ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(itemToUpdate);
            editorIsDirty |= controlAdapter.UpdateProperties(changes);
        }

        return editorIsDirty;
    }

    public SectionEditorData GetSectionEditorData(ISchemaItem editedItem)
    {
        if (editedItem is PanelControlSet screenSection)
        {
            var entityProvider =
                schemaService.GetProvider(typeof(EntityModelSchemaItemProvider))
                as EntityModelSchemaItemProvider;
            var dataSources = entityProvider
                .ChildItems.Select(x => new DataSource { Name = x.Name, SchemaItemId = x.Id })
                .OrderBy(x => x.Name)
                .ToList();
            dataSources.Insert(index: 0, DataSource.Empty);

            ApiControl apiControl = apiControlFactory.CreateWithChildren(
                screenSection.MainItem,
                ScreenSectionBindings.GetFieldDropDownValues(screenSection)
            );
            return new SectionEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screenSection.DataEntity?.Id ?? Guid.Empty,
                Fields = ScreenSectionBindings.GetFields(screenSection),
                Warnings = ScreenSectionWarningFinder.FindWarnings(screenSection),
            };
        }

        return null;
    }

    public void DeleteItem(List<Guid> schemaItemIds, ISchemaItem rootItem)
    {
        foreach (var schemaItemId in schemaItemIds)
        {
            ISchemaItem schemaItem = rootItem.GetChildByIdRecursive(schemaItemId);
            if (schemaItem is ControlSetItem itemToUpdate)
            {
                itemToUpdate.IsDeleted = true;
            }
        }
    }

    public bool SaveScreenSection(PanelControlSet screenSection)
    {
        ScreenSectionValidator.ValidateForRuntime(screenSection);
        try
        {
            bool createWidget = !screenSection.IsPersisted;
            persistenceService.SchemaListProvider.BeginTransaction();
            screenSection.ClearCacheOnPersist = false;
            screenSection.Persist();
            // If the controlset was cloned, we clone its documentation, too.
            if (screenSection.OldPrimaryKey != null)
            {
                List<ISchemaItem> items = screenSection.ChildItemsRecursive;
                items.Add(screenSection);
                documentationService.CloneDocumentation(items);
            }

            screenSection.OldPrimaryKey = null;
            if (createWidget)
            {
                panelControlFactory.Create(screenSection, schemaService.ActiveSchemaExtensionId);
                return true;
            }

            RenamePanelControlIfNameDiffers(screenSection);
        }
        finally
        {
            persistenceService.SchemaListProvider.EndTransaction();
        }

        return false;
    }

    private static void RenamePanelControlIfNameDiffers(PanelControlSet screenSection)
    {
        ControlItem panelControl = screenSection.PanelControl;
        if (panelControl == null || panelControl.Name == screenSection.Name)
        {
            return;
        }

        panelControl.Name = screenSection.Name;
        panelControl.ThrowEventOnPersist = false;
        panelControl.Persist();
        panelControl.ThrowEventOnPersist = true;
    }
}
