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

using Origam.Architect.Server.Controllers;
using Origam.Architect.Server.ReturnModels;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class DocumentationHelperService()
{
    public DocumentationEditorData GetData(
        DocumentationComplete documentationComplete,
        string label
    )
    {
        var entries = Enum.GetValues(typeof(DocumentationType))
            .Cast<DocumentationType>()
            .Select(docType => new EditorProperty(
                name: docType.ToString(),
                controlPropertyId: null,
                type: "string",
                value: null,
                dropDownValues: [],
                category: Strings.CategoryDocumentation,
                description: "",
                readOnly: false
            ))
            .ToDictionary(prop => prop.Name, prop => prop);

        foreach (
            DocumentationComplete.DocumentationRow row in documentationComplete.Documentation.Rows
        )
        {
            entries[row.Category] = new EditorProperty(
                name: row.Category,
                controlPropertyId: null,
                type: "string",
                value: row.Data,
                dropDownValues: [],
                category: Strings.CategoryDocumentation,
                description: "",
                readOnly: false
            );
        }

        return new DocumentationEditorData { Label = label, Properties = entries.Values.ToList() };
    }

    public void Update(ChangesModel changes, EditorData editor)
    {
        foreach (PropertyChange propertyChange in changes.Changes)
        {
            DocumentationComplete.DocumentationDataTable table = editor
                .DocumentationData
                .Documentation;
            DocumentationComplete.DocumentationRow row = table
                .Rows.Cast<DocumentationComplete.DocumentationRow>()
                .FirstOrDefault(row => row.Category == propertyChange.Name);
            if (string.IsNullOrEmpty(propertyChange.Value))
            {
                if (row != null)
                {
                    table.RemoveDocumentationRow(row);
                    editor.IsDirty = true;
                }

                continue;
            }

            if (row == null)
            {
                row = table.NewDocumentationRow();
                row.Category = propertyChange.Name;
                row.Data = propertyChange.Value;
                row.refSchemaItemId = changes.SchemaItemId;
                row.Id = Guid.NewGuid();
                table.Rows.Add(row);
            }
            else
            {
                row.Data = propertyChange.Value;
            }

            editor.IsDirty = true;
        }
    }
}
