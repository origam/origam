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
}