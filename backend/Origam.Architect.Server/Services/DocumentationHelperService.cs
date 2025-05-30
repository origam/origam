using Origam.Architect.Server.Controllers;
using Origam.Architect.Server.ReturnModels;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class DocumentationHelperService(IDocumentationService documentationService)
{
    
    public List<EditorProperty> GetData(DocumentationComplete documentationComplete)
    {
        var entries = Enum.GetValues(typeof(DocumentationType))
            .Cast<DocumentationType>()
            .Select(docType => new EditorProperty(
                name: docType.ToString(),
                type:"string",
                category:null, 
                controlPropertyId: null, 
                description: "",
                dropDownValues: [],
                readOnly: false, 
                value: null )
            )
            .ToDictionary(prop => prop.Name, prop => prop);

        foreach (DocumentationComplete.DocumentationRow row in documentationComplete.Documentation.Rows)
        {
            entries[row.Category] = new EditorProperty(
                name: row.Category,
                type:"string",
                category:null, 
                controlPropertyId: null, 
                description: "",
                dropDownValues: [],
                readOnly: false, 
                value: row.Data);
        }

        return entries.Values.ToList();
    }

    public void Update(ChangesModel changes, EditorData editor)
    {
        foreach (PropertyChange propertyChange in changes.Changes)
        {
            DocumentationComplete.DocumentationDataTable table = editor.DocumentationData.Documentation;
            DocumentationComplete.DocumentationRow row = table.Rows
                .Cast<DocumentationComplete.DocumentationRow>()
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