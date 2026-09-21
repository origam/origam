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

using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using static Origam.Architect.Server.Services.SectionEditor.ScreenSectionBindings;

namespace Origam.Architect.Server.Services.SectionEditor;

public static class ScreenSectionValidator
{
    public static void ValidateForRuntime(PanelControlSet screenSection)
    {
        foreach (ControlSetItem item in GetLiveControls(screenSection))
        {
            if (!IsBoundToField(item))
            {
                continue;
            }
            string controlName = item.ControlItem.Name;
            if (controlName == "RadioButton")
            {
                RequireGuidProperty(
                    item,
                    propertyName: "DataConstantId",
                    Strings.SectionEditor_RadioButtonValueConstantMissing
                );
            }
            if (controlName == GuiHelper.CONTROL_NAME_MULTICOLUMNADAPTERFIELD)
            {
                RequireChildRenderedAsProperty(item);
            }
            if (controlName is GuiHelper.CONTROL_NAME_COMBOBOX or "TagInput" or "Checklist")
            {
                RequireGuidProperty(
                    item,
                    propertyName: "LookupId",
                    Strings.SectionEditor_DropdownLookupMissing
                );
            }
            if (controlName is "TagInput" or "Checklist")
            {
                RequireArrayField(screenSection, item);
            }
            if (controlName == "ColorPicker")
            {
                RequireIntegerField(screenSection, item);
            }
        }
        ValidateFieldsBoundOnce(screenSection);
    }

    private static void RequireGuidProperty(
        ControlSetItem item,
        string propertyName,
        string messageTemplate
    )
    {
        if ((FindValueItem(item, propertyName)?.GuidValue ?? Guid.Empty) == Guid.Empty)
        {
            throw new UserOrigamException(string.Format(messageTemplate, item.Name));
        }
    }

    private static void RequireChildRenderedAsProperty(ControlSetItem wrapper)
    {
        if (
            !wrapper
                .ChildItemsByType<ControlSetItem>(ControlSetItem.CategoryConst)
                .Any(child => !child.IsDeleted && RendersAsProperty(child))
        )
        {
            throw new UserOrigamException(
                string.Format(Strings.SectionEditor_WrapperHasNoBoundWidgets, wrapper.Name)
            );
        }
    }

    private static void RequireArrayField(PanelControlSet screenSection, ControlSetItem item)
    {
        string fieldName = BoundFieldName(item);
        IDataEntityColumn field = BoundField(screenSection, fieldName);
        if (field != null && field.DataType != OrigamDataType.Array)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.SectionEditor_TagInputFieldNotArray,
                    item.Name,
                    fieldName,
                    field.DataType
                )
            );
        }
    }

    private static void RequireIntegerField(PanelControlSet screenSection, ControlSetItem item)
    {
        string fieldName = BoundFieldName(item);
        IDataEntityColumn field = BoundField(screenSection, fieldName);
        if (field != null && field.DataType != OrigamDataType.Integer)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.SectionEditor_ColorPickerFieldNotInteger,
                    item.Name,
                    fieldName
                )
            );
        }
    }

    private static void ValidateFieldsBoundOnce(PanelControlSet screenSection)
    {
        var fieldBoundTwice = GetLiveControls(screenSection)
            .Where(item => IsBoundToField(item) && item.ControlItem.Name != "RadioButton")
            .GroupBy(BoundFieldName)
            .FirstOrDefault(group => group.Count() > 1);
        if (fieldBoundTwice == null)
        {
            return;
        }
        throw new UserOrigamException(
            string.Format(
                Strings.SectionEditor_FieldBoundTwice,
                fieldBoundTwice.Key,
                string.Join(separator: ", ", fieldBoundTwice.Select(item => item.Name))
            )
        );
    }

    private static bool RendersAsProperty(ControlSetItem item)
    {
        return IsBoundToField(item)
            && item.ControlItem.Name != "RadioButton"
            && !(FindValueItem(item, propertyName: "HideOnForm")?.BoolValue ?? false);
    }
}
