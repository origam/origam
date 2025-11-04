#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

namespace Origam.Gui.Designer;

public struct ToolBoxConverter
{
    const string WIDGET_TEXTBOX = "Origam.Gui.Win.AsTextBox,Origam.Gui.Win";
    const string WIDGET_DROPDOWN = "Origam.Gui.Win.AsDropDown,Origam.Gui.Win";
    const string WIDGET_TAGINPUT = "Origam.Gui.Win.TagInput,Origam.Gui.Win";
    const string WIDGET_IMAGEBOX = "Origam.Gui.Win.ImageBox,Origam.Gui.Win";
    const string WIDGET_CHECKBOX = "Origam.Gui.Win.AsCheckBox,Origam.Gui.Win";
    const string WIDGET_DATEBOX = "Origam.Gui.Win.AsDateBox,Origam.Gui.Win";
    const string WIDGET_MULTICOLUMNWRAPPER =
        "Origam.Gui.Win.MultiColumnAdapterFieldWrapper,Origam.Gui.Win";

    public static string Convert(IDataEntityColumn field)
    {
        string result;
        if (field.DefaultLookup != null)
        {
            result = WIDGET_DROPDOWN;
        }
        else
        {
            switch (field.DataType)
            {
                case OrigamDataType.Array:
                {
                    result = WIDGET_TAGINPUT;
                    break;
                }

                case OrigamDataType.Blob:
                {
                    result = WIDGET_IMAGEBOX;
                    break;
                }

                case OrigamDataType.Boolean:
                {
                    result = WIDGET_CHECKBOX;
                    break;
                }

                case OrigamDataType.Date:
                {
                    result = WIDGET_DATEBOX;
                    break;
                }

                case OrigamDataType.Object:
                {
                    result = WIDGET_MULTICOLUMNWRAPPER;
                    break;
                }

                default:
                {
                    result = WIDGET_TEXTBOX;
                    break;
                }
            }
        }
        return result;
    }
}
