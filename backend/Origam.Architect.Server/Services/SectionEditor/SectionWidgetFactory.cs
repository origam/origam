#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Data;
using System.Xml;
using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.Controls;
using Origam.Architect.Server.Models;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;
using static Origam.Architect.Server.Services.SectionEditor.ScreenSectionBindings;

namespace Origam.Architect.Server.Services.SectionEditor;

public class SectionWidgetFactory(
    SchemaService schemaService,
    ControlAdapterFactory adapterFactory,
    ApiControlFactory apiControlFactory
)
{
    private const int PanelGrowMargin = 20;

    public ApiControl Create(SectionEditorItemModel itemModelData, PanelControlSet screenSection)
    {
        ISchemaItem parent = FindParent(screenSection, itemModelData.ParentControlSetItemId);
        ControlItem controlItem = FindSectionWidget(itemModelData.ComponentType);
        IDataEntityColumn boundColumn = string.IsNullOrEmpty(itemModelData.FieldName)
            ? null
            : FindBoundColumn(screenSection, itemModelData.FieldName);
        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId,
            group: null
        );
        newItem.ControlItem = controlItem;
        newItem.Name = boundColumn == null ? controlItem.Name : itemModelData.FieldName;

        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(newItem);
        string caption =
            boundColumn == null
                ? null
                : BindToColumn(newItem, controlAdapter.Control, screenSection, boundColumn);

        controlAdapter.InitializeProperties(top: itemModelData.Top, left: itemModelData.Left);
        PropertyValueItem textValueItem = newItem
            .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
            .FirstOrDefault(x => x.Name == "Text");
        if (textValueItem != null && caption != null)
        {
            textValueItem.Value = caption;
        }
        if (controlAdapter.Control is Label && string.IsNullOrEmpty(itemModelData.FieldName))
        {
            newItem.Name = GetUniqueControlName(screenSection, newItem);
            if (textValueItem != null)
            {
                textValueItem.Value = newItem.Name;
            }
        }

        GrowRootPanel(screenSection.MainItem, newItem);

        ApiControl createdControl = apiControlFactory.Create(
            newItem,
            GetFieldDropDownValues(screenSection)
        );
        if (boundColumn != null)
        {
            createdControl.Warnings = ScreenSectionWarningFinder.FindWarnings(
                screenSection,
                [boundColumn]
            );
        }
        return createdControl;
    }

    private static ISchemaItem FindParent(PanelControlSet screenSection, Guid parentId)
    {
        ISchemaItem parent =
            parentId == Guid.Empty || parentId == screenSection.Id
                ? screenSection.MainItem
                : screenSection.GetChildByIdRecursive(parentId);
        if (parent == null)
        {
            throw new UserOrigamException(
                string.Format(Strings.DesignerEditor_ParentControlNotFound, parentId)
            );
        }
        return parent;
    }

    private string BindToColumn(
        ControlSetItem newItem,
        IControl control,
        PanelControlSet screenSection,
        IDataEntityColumn boundColumn
    )
    {
        DataSet dataSet = new DatasetGenerator(userDefinedParameters: false).CreateDataSet(
            screenSection.DataEntity
        );
        string caption = dataSet.Tables[0].Columns[boundColumn.Name]?.Caption;

        if (control is IAsControl asControl)
        {
            string boundPropertyName = asControl.DefaultBindableProperty;
            ControlPropertyItem propertyItem = FindPropertyItem(newItem, boundPropertyName);
            PropertyBindingInfo propertyBinding = FindOrMakeBindingInfo(newItem, propertyItem);
            propertyBinding.ControlPropertyItem = propertyItem;
            propertyBinding.Name = boundPropertyName;
            propertyBinding.Value = boundColumn.Name;
            propertyBinding.DesignDataSetPath =
                dataSet.Tables[0].TableName + "." + boundColumn.Name;
        }

        if (control is ILookupBoundControl lookupControl && boundColumn.DefaultLookup != null)
        {
            lookupControl.LookupId = (Guid)boundColumn.DefaultLookup.PrimaryKey["Id"];
        }
        return caption;
    }

    private ControlItem FindSectionWidget(string controlType)
    {
        List<ControlItem> controlItems = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems.OfType<ControlItem>()
            .ToList();
        ControlItem controlItem = controlItems.FirstOrDefault(item =>
            item.ControlType == controlType
        );
        if (controlItem != null)
        {
            return controlItem;
        }
        IEnumerable<string> sectionWidgetTypes = controlItems
            .Where(item =>
                item.ControlToolBoxVisibility
                    is ControlToolBoxVisibility.PanelDesigner
                        or ControlToolBoxVisibility.PanelAndFormDesigner
            )
            .Select(item => item.ControlType)
            .Where(type => type is not ("Origam.Gui.Win.AsForm" or "Origam.Gui.Win.AsPanel"))
            .OrderBy(type => type);
        throw new UserOrigamException(
            string.Format(
                Strings.SectionEditor_UnknownWidgetType,
                controlType,
                string.Join(separator: ", ", sectionWidgetTypes)
            )
        );
    }

    private static IDataEntityColumn FindBoundColumn(
        PanelControlSet screenSection,
        string fieldName
    )
    {
        if (screenSection.DataEntity == null)
        {
            throw new UserOrigamException(Strings.SectionEditor_NoDataSource);
        }
        return BoundField(screenSection, fieldName)
            ?? throw new UserOrigamException(
                string.Format(
                    Strings.SectionEditor_FieldNotFound,
                    fieldName,
                    screenSection.DataEntity.Name,
                    string.Join(
                        separator: ", ",
                        GetFields(screenSection).Select(field => field.Name)
                    )
                )
            );
    }

    private void GrowRootPanel(ControlSetItem rootPanel, ControlSetItem newItem)
    {
        int right = IntValue(newItem, propertyName: "Width");
        int bottom = IntValue(newItem, propertyName: "Height");
        for (
            ISchemaItem item = newItem;
            item is ControlSetItem control && control.Id != rootPanel.Id;
            item = item.ParentItem
        )
        {
            right += IntValue(control, propertyName: "Left");
            bottom += IntValue(control, propertyName: "Top");
        }

        var changes = new List<PropertyChange>();
        if (right > IntValue(rootPanel, propertyName: "Width"))
        {
            changes.Add(
                new PropertyChange
                {
                    Name = "Width",
                    Value = XmlConvert.ToString(right + PanelGrowMargin),
                }
            );
        }
        if (bottom > IntValue(rootPanel, propertyName: "Height"))
        {
            changes.Add(
                new PropertyChange
                {
                    Name = "Height",
                    Value = XmlConvert.ToString(bottom + PanelGrowMargin),
                }
            );
        }
        if (changes.Count > 0)
        {
            adapterFactory
                .Create(rootPanel)
                .UpdateProperties(
                    new ChangesModel { SchemaItemId = rootPanel.Id, Changes = changes }
                );
        }
    }

    private static string GetUniqueControlName(
        PanelControlSet screenSection,
        ControlSetItem newItem
    )
    {
        HashSet<string> usedNames = screenSection
            .ChildItemsRecursive.OfType<ControlSetItem>()
            .Where(item => item != newItem)
            .Select(item => item.Name)
            .ToHashSet();
        int index = 1;
        while (usedNames.Contains(newItem.ControlItem.Name + index))
        {
            index++;
        }
        return newItem.ControlItem.Name + index;
    }

    private PropertyBindingInfo FindOrMakeBindingInfo(
        ControlSetItem controlSetItem,
        ControlPropertyItem propertyToFind
    )
    {
        PropertyBindingInfo result = controlSetItem
            .ChildItemsByType<PropertyBindingInfo>(PropertyBindingInfo.CategoryConst)
            .FirstOrDefault(item =>
                Equals(item.ControlPropertyItem?.PrimaryKey, propertyToFind.PrimaryKey)
            );

        if (result == null)
        {
            result = controlSetItem.NewItem<PropertyBindingInfo>(
                schemaService.ActiveSchemaExtensionId,
                group: null
            );
            result.ControlPropertyItem = propertyToFind;
            result.Name = propertyToFind.Name;
        }

        return result;
    }

    private static ControlPropertyItem FindPropertyItem(
        ControlSetItem controlSetItem,
        string propertyName
    )
    {
        ControlPropertyItem propertyItem = controlSetItem
            .ControlItem.ChildItemsByType<ControlPropertyItem>(ControlPropertyItem.CategoryConst)
            .FirstOrDefault(propItem =>
                string.Equals(
                    propItem.Name,
                    propertyName,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );

        if (propertyItem == null)
        {
            throw new Exception(
                $"Property {propertyName} was not found on ControlItem {controlSetItem.ControlItem.Id}"
            );
        }

        return propertyItem;
    }
}
