namespace Origam.Architect.Server.ReturnModels;

public class XsltEditorData (List<EditorProperty> properties, string xslt)
{
    public List<EditorProperty> Properties { get; } = properties;
    public string Xslt { get; } = xslt;
}